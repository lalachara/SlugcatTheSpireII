﻿using Godot;
 using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Rainworld.Scripts;
using Rainworld.Scripts.Card.RedCat;
using Rainworld.Scripts.Powers;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.Scripts.Card.Liver.Attack;

public class Rainworld_Liver_Spearboom : LiverCardModel

{
    // 基础耗能
    private const int energyCost = 2;

    // 卡牌类型
    private const CardType type = CardType.Attack;

    // 卡牌稀有度
    private const CardRarity rarity = CardRarity.Uncommon;

    // 目标类型（AnyEnemy表示任意敌人）
    private const TargetType targetType = TargetType.AnyEnemy;

    // 是否在卡牌图鉴中显示
    private const bool shouldShowInCardLibrary = true;
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromCard<Rainworld_Liver_Spear>(IsUpgraded) };
    public override CardAssetProfile AssetProfile => new(
        GetLocalizedCardImage(),
        FramePath: Type switch
        {
            CardType.Attack => "res://Resource/Card/Liver/CardAtk1024.png",
            CardType.Skill => "res://Resource/Card/Liver/CardSkill1024.png",
            CardType.Power => "res://Resource/Card/Liver/CardPower1024.png",
            _ => ""
        }
    );

    private string GetLocalizedCardImage()
    {
        var cardName = GetType().Name.ToLowerInvariant()
            .Replace("slugcat_the_spire_ii_card_", "");
        var locale = TranslationServer.GetLocale(); // 如 "zh", "en"
        string localizedPath = $"res://Resource/Card/Liver/{cardName}.png";
        if (locale == "ko")
        {
            localizedPath = $"res://Resource/Card/diff/KR/{cardName}.png";
        }

        return ResourceLoader.Exists(localizedPath) 
            ? localizedPath 
            : $"res://Resource/Card/Liver/{cardName}.png"; // 回退到默认
    }
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[]
        {  RainworldKeywords.Spear, RainworldKeywords.Treasure };


    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(8m, ValueProp.Move)
    ];

    public Rainworld_Liver_Spearboom() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    // 打出时的效果逻辑
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        await DamageCmd
            .Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx(sfx:"event:/Rainworld/sfx/bombspear")
            .Execute(choiceContext);
        await Cmd.Wait(0.5f);
        Vfx.BombBurstVfx.Play(CombatState!.HittableEnemies);
            await DamageCmd
                .Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this, cardPlay)
                .TargetingAllOpponents(CombatState)
                .Execute(choiceContext);
            await CardCmd.Discard(choiceContext, this);



    }
    

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card == this)
        {
            CardModel card = CombatState!.CreateCard<Rainworld_Liver_Spear>(Owner);
            if(IsUpgraded) 
                CardCmd.Upgrade(card);
            await CardCmd.Transform(this, card);

        }

        await base.AfterCardPlayed(choiceContext, cardPlay);
    }

    // 升级后的效果逻辑
    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m); // 升级后增加4点伤害
    }
}