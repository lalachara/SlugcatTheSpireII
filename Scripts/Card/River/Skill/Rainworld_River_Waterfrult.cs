using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using Rainworld.Resource.Card;
using Rainworld.Scripts;
using Rainworld.Scripts.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.Scripts.Card.River.Skills;
[RegisterCard(typeof(Rainworld_Future_CardPool), Inherit = true)]
public class Rainworld_River_Waterfruit : ModCardTemplate
{
    
    private const int energyCost = 0;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Token;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    private int _strengthenCount = 0;
    public override int MaxUpgradeLevel { get; } = 3;
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromKeyword(RainworldKeywords.Food)  };

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        new[] { CardKeyword.Retain, RainworldKeywords.Foodcard, CardKeyword.Exhaust };

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new("Food", 0m),
        new EnergyVar(0)
    ];

    public override CardAssetProfile AssetProfile => new(
        IsUpgraded
            ? $"res://Resource/Card/River/rainworld_river_waterfruit2.png"
            : $"res://Resource/Card/River/rainworld_river_waterfruit1.png",
        FramePath: Type switch
        {
            CardType.Attack => "res://Resource/Card/River/CardAtk1024.png",
            CardType.Skill => "res://Resource/Card/River/CardSkill1024.png",
            CardType.Power => "res://Resource/Card/River/CardPower1024.png",
            _ => ""
        }
    );
    public override TargetType TargetType
    {
        get => !this.HasSharedFood ? TargetType.Self : TargetType.AllAllies;
    }
    private bool HasSharedFood
    {
        get => this.IsMutable&& this.Owner!=null  && this.Owner.Creature.HasPower<SharefoodPower>();
    }

    public Rainworld_River_Waterfruit() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override bool IsPlayable => IsUpgraded;

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side == CombatSide.Player && _strengthenCount < MaxUpgradeLevel)
        {
            var handPile = PileType.Hand.GetPile(Owner);
            bool isInHand = handPile.Cards.Contains(this);

            if (isInHand)
            {
                _strengthenCount++;
                CardCmd.Upgrade(this);
            }

        }
    }


    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (IsUpgraded)
        {
            if (HasSharedFood)
            {
                foreach (var player in CombatState!.Players)
                {
                    await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, player);
                    if (player.Character is Slugcat)
                        await SlugcatData.addfood(player, (int)DynamicVars["Food"].BaseValue);
                }
            }
            else
            {
                await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, Owner);
                            if (Owner.Character is Slugcat)
                                await SlugcatData.addfood(Owner, (int)DynamicVars["Food"].BaseValue);
            }
            
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Food"].UpgradeValueBy(1m);
        DynamicVars.Energy.UpgradeValueBy(1m);
        this.RequestVisualReload();
    }
}