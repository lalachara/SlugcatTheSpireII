﻿using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Rainworld.Scripts;
using Rainworld.Scripts.Powers;

namespace Rainworld.Scripts.Card.Liver.Attack;

public class Rainworld_Liver_Spearfire : LiverCardModel

{
    // 基础耗能
    private const int energyCost = 1;

    // 卡牌类型
    private const CardType type = CardType.Attack;

    // 卡牌稀有度
    private const CardRarity rarity = CardRarity.Ancient;

    // 目标类型（AnyEnemy表示任意敌人）
    private const TargetType targetType = TargetType.AnyEnemy;

    // 是否在卡牌图鉴中显示
    private const bool shouldShowInCardLibrary = true;
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromPower<ChuangPower>() };

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        new[] { RainworldKeywords.Spear };

    //public override IEnumerable<CardKeyword> CanonicalKeywords => new []{CardKeyword.Exhaust};
    private const string _increaseKey = "Increase";

    private const int _baseDamage = 16;

    private int _currentDamage = 16;

    private int _increasedDamage;

    [SavedProperty]
    public int CurrentDamage
    {
        get => _currentDamage;
        set
        {
            AssertMutable();
            _currentDamage = value;
            DynamicVars.Damage.BaseValue = _currentDamage;
        }
    }

    [SavedProperty]
    public int IncreasedDamage
    {
        get => _increasedDamage;
        set
        {
            AssertMutable();
            _increasedDamage = value;
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(CurrentDamage, ValueProp.Move),
        new("Chuang", 5m),
        new IntVar("Increase", 3m)
    ];

    // 升级后的效果逻辑
    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(6m);
        DynamicVars["Chuang"].UpgradeValueBy(2m);
        DynamicVars["Increase"].UpgradeValueBy(1m);
    }

    public Rainworld_Liver_Spearfire() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    // 打出时的效果逻辑
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var shouldTriggerFatal = cardPlay.Target.Powers.All((PowerModel p) => p.ShouldOwnerDeathTriggerFatal());
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        var attackCommand = await DamageCmd
            .Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx(sfx:"event:/Rainworld/sfx/spear")
            .Execute(choiceContext);
        await PowerCmd.Apply<ChuangPower>(choiceContext, cardPlay.Target, DynamicVars["Chuang"].BaseValue,
            Owner.Creature, this);
        if (!shouldTriggerFatal || !attackCommand.Results
                .SelectMany<List<DamageResult>, DamageResult>(
                    (Func<List<DamageResult>, IEnumerable<DamageResult>>)(r => (IEnumerable<DamageResult>)r))
                .Any<DamageResult>((Func<DamageResult, bool>)(r => r.WasTargetKilled)))
            return;

        var intValue = DynamicVars["Increase"].IntValue;
        BuffFromPlay(intValue);
        (DeckVersion as Rainworld_Liver_Spearfire)?.BuffFromPlay(intValue);
    }


    protected override void AfterDowngraded()
    {
        UpdateDamage();
    }

    private void BuffFromPlay(int extraDamage)
    {
        IncreasedDamage += extraDamage;
        UpdateDamage();
    }

    private void UpdateDamage()
    {
        CurrentDamage = _baseDamage + IncreasedDamage;
    }
}