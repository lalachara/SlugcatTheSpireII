using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Rainworld.Scripts;
using Rainworld.Scripts.Powers;
using Rainworld.Scripts.Card.CardVars;

namespace Rainworld.Scripts.Card.Liver.Attack;

public class Rainworld_Liver_Spearfire:LiverCardModelAtk

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
    
    public override IEnumerable<CardKeyword> CanonicalKeywords => new []{RainworldKeywords.Spear,RainworldKeywords.Chuang};

    //public override IEnumerable<CardKeyword> CanonicalKeywords => new []{CardKeyword.Exhaust};
    private const string _increaseKey = "Increase";

    private const int _baseDamage = 16;

    private int _currentDamage = 16;

    private int _increasedDamage;
    
    [SavedProperty]
    public int CurrentDamage
    {
        get
        {
            return _currentDamage;
        }
        set
        {
            AssertMutable();
            _currentDamage = value;
            base.DynamicVars.Damage.BaseValue = _currentDamage;
        }
    }

    [SavedProperty]
    public int IncreasedDamage
    {
        get
        {
            return _increasedDamage;
        }
        set
        {
            AssertMutable();
            _increasedDamage = value;
        }
    }
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(CurrentDamage, ValueProp.Move),
        new DynamicVar("Chuang",5m),
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
        bool shouldTriggerFatal = cardPlay.Target.Powers.All((PowerModel p) => p.ShouldOwnerDeathTriggerFatal());
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        AttackCommand attackCommand = await DamageCmd
            .Attack(base.DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
        await PowerCmd.Apply<ChuangPower>(choiceContext,cardPlay.Target, DynamicVars["Chuang"].BaseValue, base.Owner.Creature, this);

        if (shouldTriggerFatal && attackCommand.Results.Any((DamageResult r) => r.WasTargetKilled))
        {
            int intValue = base.DynamicVars["Increase"].IntValue;
            BuffFromPlay(intValue);
            (base.DeckVersion as Rainworld_Liver_Spearfire)?.BuffFromPlay(intValue);
        }
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