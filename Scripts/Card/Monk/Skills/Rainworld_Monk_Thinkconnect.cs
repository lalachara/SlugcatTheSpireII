// using MegaCrit.Sts2.Core.Commands;
// using MegaCrit.Sts2.Core.Entities.Cards;
// using MegaCrit.Sts2.Core.Entities.Players;
// using MegaCrit.Sts2.Core.GameActions.Multiplayer;
//
// namespace Rainworld.Scripts.Card.Monk;
//
// /// <summary>
// /// 思维链：选择1名盟友。本回合内双方轮流打出牌时，由后打出的一方抽1张牌；
// /// 同一方连续打出牌只触发一次，必须等另一方也打出牌后才能再次触发。
// /// </summary>
// public class Rainworld_Monk_Thinkconnect : MonkCardModel
// {
//     private const int energyCost = 2;
//     private const CardType type = CardType.Skill;
//     private const CardRarity rarity = CardRarity.Uncommon;
//     private const TargetType targetType = TargetType.AnyAlly;
//     private const bool shouldShowInCardLibrary = true;
//
//     /// <summary>生效的轮次编号，RoundNumber 变化后自动失效。</summary>
//     private int _activeRound = -1;
//
//     /// <summary>选中的盟友。</summary>
//     private Player? _partner;
//
//     /// <summary>上一次通过本效果触发抽牌的一方，用于阻止同一方连续触发。</summary>
//     private Player? _lastTrigger;
//
//     public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;
//
//     public Rainworld_Monk_Thinkconnect() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
//     {
//     }
//
//     protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
//     {
//         ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
//
//         _partner = cardPlay.Target.Player;
//         _lastTrigger = null;
//         _activeRound = CombatState?.RoundNumber ?? -1;
//         return Task.CompletedTask;
//     }
//
//     public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
//     {
//         // 只在本回合、且不是这张牌自己打出时生效。
//         if (cardPlay.Card == this || _partner == null || CombatState == null
//             || CombatState.RoundNumber != _activeRound)
//         {
//             await base.AfterCardPlayed(choiceContext, cardPlay);
//             return;
//         }
//
//         var actor = cardPlay.Card.Owner;
//         if ((actor != Owner && actor != _partner) || actor == _lastTrigger)
//         {
//             await base.AfterCardPlayed(choiceContext, cardPlay);
//             return;
//         }
//
//         var receiver = actor == Owner ? _partner : Owner;
//         if (!receiver.Creature.IsAlive)
//         {
//             await base.AfterCardPlayed(choiceContext, cardPlay);
//             return;
//         }
//
//         _lastTrigger = actor;
//         await CardPileCmd.Draw(choiceContext, 1, receiver);
//         await base.AfterCardPlayed(choiceContext, cardPlay);
//     }
// }
