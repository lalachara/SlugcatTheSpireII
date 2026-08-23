using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using Rainworld.Scripts.Card.Liver.Attack;
using Rainworld.Scripts.Card.River.Skills;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.Scripts.Powers;

using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using System.Collections.Generic;
using System.Threading.Tasks;

#nullable enable
[RegisterPower]
public sealed class BigjumpMonkPower : ModPowerTemplate
{
  public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;
  public override PowerAssetProfile AssetProfile => new(
    IconPath: "res://Resource/Powers/bigjump.png",
    BigIconPath: "res://Resource/Powers/bigjump.png"
  );
  public override PowerType Type => PowerType.Buff;
  public override PowerStackType StackType => PowerStackType.Single;
  
  protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    new[] { HoverTipFactory.FromCard<Rainworld_Liver_Fruit>(), HoverTipFactory.FromCard<Rainworld_Redcat_Smallcent>(), HoverTipFactory.FromCard<Rainworld_River_Waterfruit>() };

  public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, ICombatState combatState)
  {
    if (player == base.Owner.Player)
    {
      var random = base.Owner.Player.RunState.Rng.CombatCardGeneration.NextInt(0, 3);
      switch (random)
      {
        case 0:
          await CardPileCmd.AddGeneratedCardToCombat(
            base.CombatState.CreateCard<Rainworld_Liver_Fruit>(base.Owner.Player), PileType.Hand, base.Owner.Player);
          break;
        case 1:
          await CardPileCmd.AddGeneratedCardToCombat(
            base.CombatState.CreateCard<Rainworld_Redcat_Smallcent>(base.Owner.Player), PileType.Hand, base.Owner.Player);
          break;
        case 2:
          await CardPileCmd.AddGeneratedCardToCombat(
            base.CombatState.CreateCard<Rainworld_River_Waterfruit>(base.Owner.Player), PileType.Hand, base.Owner.Player);
          break;

      }
      await PowerCmd.Remove(this);
    }
  }
}