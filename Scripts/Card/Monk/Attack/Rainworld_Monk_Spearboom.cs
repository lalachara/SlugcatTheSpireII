using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using Rainworld.Scripts.Card.Liver.Attack;

namespace Rainworld.Scripts.Card.Monk;

public class Rainworld_Monk_Spearboom : MonkCardModel
{
    private const int energyCost = 2;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromCard<Rainworld_Liver_Spear>(IsUpgraded) };
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { RainworldKeywords.Spear };

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(8m, ValueProp.Move)
    ];

    public Rainworld_Monk_Spearboom() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx(sfx: "event:/Rainworld/sfx/bombspear")
            .Execute(choiceContext);
        await Cmd.Wait(0.5f);
        Vfx.BombBurstVfx.Play(CombatState!.HittableEnemies);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .TargetingAllOpponents(CombatState)
            .Execute(choiceContext);
        await CardCmd.Discard(choiceContext, this);
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card == this)
        {
            CardModel card = CombatState!.CreateCard<Rainworld_Monk_Spear>(Owner);
            if (IsUpgraded)
                CardCmd.Upgrade(card);
            await CardCmd.Transform(this, card);
        }
        await base.AfterCardPlayed(choiceContext, cardPlay);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
    }
}