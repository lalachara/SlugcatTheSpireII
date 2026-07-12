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
    private const CardRarity rarity = CardRarity.Uncommon;
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
        IncreasedDamage += amount;
        IncreasedSummon += amount;
        UpdateValues();
    }

    private void UpdateValues()
    {
        CurrentDamage = _baseDamage + IncreasedDamage;
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