// using System.Linq;
// using MegaCrit.Sts2.Core.Commands;
// using MegaCrit.Sts2.Core.Entities.Cards;
// using MegaCrit.Sts2.Core.Factories;
// using MegaCrit.Sts2.Core.GameActions.Multiplayer;
// using MegaCrit.Sts2.Core.Models;
// using MegaCrit.Sts2.Core.Models.Cards;
// using MinionLib.Commands;
// using MinionLib.Minion;
// using Rainworld.Scripts.Monster.Minions;
//
// namespace Rainworld.Scripts.Card.Monk;
//
// public class Rainworld_Monk_Order : MonkCardModel
// {
//     private const int energyCost = 0;
//     private const CardType type = CardType.Skill;
//     private const CardRarity rarity = CardRarity.Basic;
//     private const TargetType targetType = TargetType.Self;
//     private const bool shouldShowInCardLibrary = true;
//
//     public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };
//
//     public Rainworld_Monk_Order() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
//     {
//     }
//
//     protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
//     {
//         await MinionCmd.AddMinion<MyMinion>(choiceContext, Owner, new MinionSummonOptions(
//             MaxHp: 8m,
//             PrimaryStatAmount: 2m,
//             Source: this,
//             Position: MinionPosition.Front));
//     }
//
//     protected override void OnUpgrade()
//     {
//         EnergyCost.UpgradeBy(-1);
//     }
// }