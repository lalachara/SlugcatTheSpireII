// using MegaCrit.Sts2.Core.Commands;
// using MegaCrit.Sts2.Core.Entities.Players;
// using MegaCrit.Sts2.Core.GameActions.Multiplayer;
// using MegaCrit.Sts2.Core.Models.Powers;
// using MinionLib.Minion;
// using MinionLib.Powers;
// using SlugcatTheSpireII.RitsuAdapters;
//
// namespace Rainworld.Scripts.Monster.Minions;
//
// public sealed class MyMinion : ModMinionTemplate
// {
//     public override int MinInitialHp => 6;
//     public override int MaxInitialHp => 6;
//     public override string? CustomVisualsPath =>
//         "res://Godot/Monster/mushroom.tscn";
//
//     public override async Task OnSummon(
//         PlayerChoiceContext choiceContext,
//         Player owner,
//         MinionSummonOptions options)
//     {
//         var self = owner.PlayerCombatState?.Pets.FirstOrDefault(p => ReferenceEquals(p.Monster, this));
//         if (self == null) return;
//
//         if (options.MaxHp is decimal maxHp)
//             await CreatureCmd.SetMaxAndCurrentHp(self, maxHp);
//
//         if (options.PrimaryStatAmount is decimal strength && strength > 0m)
//             await PowerCmd.Apply<StrengthPower>(choiceContext, self, strength, owner.Creature, options.Source);
//
//         await PowerCmd.Apply<MinionGuardianPower>(choiceContext, self, 1m, owner.Creature, options.Source);
//     }
// }