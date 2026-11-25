using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Rainworld.Scripts.Powers;

namespace Rainworld.Scripts.Card.RedCat;

public class Rainworld_Redcat_Eatblood : RedCatCardModel
{
    private const int energyCost = 2;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromKeyword(RainworldKeywords.Workreport),HoverTipFactory.FromPower<MushblockPower>() };

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];

    private const int _baseDamage = 6;
    private const int _baseSummon = 4;

    private int _currentDamage = 6;
    private int _increasedDamage;
    private int _currentSummon = 4;
    private int _increasedSummon;

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

    [SavedProperty]
    public int CurrentSummon
    {
        get => _currentSummon;
        set
        {
            AssertMutable();
            _currentSummon = value;
            DynamicVars["Summon"].BaseValue = _currentSummon;
        }
    }

    [SavedProperty]
    public int IncreasedSummon
    {
        get => _increasedSummon;
        set
        {
            AssertMutable();
            _increasedSummon = value;
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(CurrentDamage, ValueProp.Move),
        new("Summon", CurrentSummon),
        new("Misc", IncreasedDamage)
    ];

    public Rainworld_Redcat_Eatblood() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    public void AddMisc(int amount)
    {
        // 先用「当前」字段把 DynamicVars 建出来：否则首次访问会用已经 +amount 的 CurrentDamage 重建 DamageVar，
        // 下面再 += amount 就会多加一份（数值与本机/重建路径对不上，反过来又是不同步源）
        _ = DynamicVars.Damage;

        IncreasedDamage += amount;
        IncreasedSummon += amount;

        // 伤害必须按「增量」叠加，不能用固定基础值重算：
        // 重算会把升级（OnUpgrade 的 +2）和附魔加成一起冲掉，使本机内存里的这张牌变成「6 + 加成」；
        // 而其他玩家在下一场战斗开始时，会用 SavedProperties（CurrentDamage）+ CurrentUpgradeLevel
        // 重新建出同一张牌，得到「6 + 加成 + 2」——两端数值不同，打出后 checksum 对不上，
        // 多人模式就会报 StateDivergence（不同步）。
        _currentDamage += amount;
        DynamicVars.Damage.BaseValue += amount;

        // 菌壁（Summon）没有升级 / 附魔加成，继续按基础值重算即可
        CurrentSummon = _baseSummon + IncreasedSummon;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        await DamageCmd
            .Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this,cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
        
        await PowerCmd.Apply<MushblockPower>(choiceContext, Owner.Creature, DynamicVars["Summon"].BaseValue, Owner.Creature, this);

    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
    }
}