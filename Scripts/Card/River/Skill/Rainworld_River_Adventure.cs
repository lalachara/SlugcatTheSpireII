// // Rainworld_River_Adventure.cs
//
// using MegaCrit.Sts2.Core.Commands;
// using MegaCrit.Sts2.Core.Entities.Cards;
// using MegaCrit.Sts2.Core.GameActions.Multiplayer;
// using MegaCrit.Sts2.Core.HoverTips;
// using MegaCrit.Sts2.Core.Localization.DynamicVars;
// using Rainworld.Scripts.Powers;
//
// namespace Rainworld.Scripts.Card.River.Skills;
//
// public class Rainworld_River_Adventure : RiverCardModel
// {
//     private const int energyCost = 1;
//     private const CardType type = CardType.Skill;
//     private const CardRarity rarity = CardRarity.Uncommon;
//     private const TargetType targetType = TargetType.Self;
//     private const bool shouldShowInCardLibrary = true;
//     protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromKeyword(RainworldKeywords.Adroit) };
//
//
//     public Rainworld_River_Adventure() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
//     {
//     }
//
//     protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
//     {
//         await PowerCmd.Apply<AdventurePower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);
//     }
//
//     protected override void OnUpgrade()
//     {
//         EnergyCost.UpgradeBy(-1);
//     }
// }