using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using Rainworld.Scripts;
using Rainworld.Scripts.Card.Liver.Attack;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.Scripts.Powers;


using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.ValueProps;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

#nullable enable
[RegisterPower]
public sealed class AcidslimePower : ModPowerTemplate
{
  public override PowerAssetProfile AssetProfile => new(
    IconPath: "res://Resource/Powers/AcidslimePower128.png",
    BigIconPath: "res://Resource/Powers/AcidslimePower128.png"
  );
  
  public override PowerType Type => PowerType.Buff;

  public override PowerStackType StackType => PowerStackType.Counter;

 // protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] { (HoverTipFactory.FromPower<DoomPower>()) }; 

 public override async Task AfterCardExhausted(PlayerChoiceContext choiceContext, CardModel card, bool _)
 {
     if (card.Owner.Creature == base.Owner&&card is Slimed)
     {
         await CreatureCmd.Damage(new BlockingPlayerChoiceContext(), base.CombatState.HittableEnemies, base.Amount, ValueProp.Unpowered, base.Owner);
     }
 }
 public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult _, ValueProp props, Creature? dealer, CardModel? __)
 {
     if (target == base.Owner && (props & ValueProp.Unpowered) == 0)
     {
         Slimed slimed = CombatState.CreateCard<Slimed>(Owner.Player);
         slimed.AddKeyword(CardKeyword.Ethereal);
         CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(slimed , PileType.Hand, Owner.Player));

     }
 }
 
}