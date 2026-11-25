using MegaCrit.Sts2.Core.Commands;
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

    /// <summary>
    /// 斩杀后的永久伤害加成。
    /// <para>必须按「增量」往上加，不能用 <see cref="_baseDamage" /> 重算整个伤害：
    /// 重算会把升级（<see cref="OnUpgrade" /> 的 +6）和附魔的伤害加成一起冲掉，
    /// 使本机内存里的这张牌变成「16 + 永久加成」；而其他玩家在下一场战斗开始的同步里，
    /// 会用 SavedProperties（CurrentDamage）+ CurrentUpgradeLevel 重新建出同一张牌，
    /// 得到「16 + 永久加成 + 6」——两端同一张牌伤害不同，打出后 checksum 对不上，
    /// 多人模式就会报 StateDivergence（不同步）。</para>
    /// </summary>
    private void BuffFromPlay(int extraDamage)
    {
        // 先用「当前」字段把 DynamicVars 建出来：否则首次访问会用已经 +extraDamage 的 CurrentDamage 重建 DamageVar，
        // 下面再 += 就会多加一份
        _ = DynamicVars.Damage;

        IncreasedDamage += extraDamage;                 // 存档字段：基础伤害 + 已累积的永久加成
        _currentDamage += extraDamage;                  // 直接改字段，保持「16 + 加成」（属性 setter 会把 DynamicVars 重置掉）
        DynamicVars.Damage.BaseValue += extraDamage;    // 在当前伤害上叠加，保留升级 / 附魔加成
    }

    /// <summary>降级后把伤害重置回「基础 + 永久加成」（升级加成由 <see cref="OnUpgrade" /> 负责重新加）。</summary>
    private void UpdateDamage()
    {
        CurrentDamage = _baseDamage + IncreasedDamage;
    }
}