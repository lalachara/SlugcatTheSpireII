using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.Scripts.Powers;

using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rainworld.Scripts.Card.Liver.Attack;

#nullable enable
[RegisterPower]
public sealed class CoffiePower : ModPowerTemplate
{
    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://Resource/Powers/CoffiePower128.png",
        BigIconPath: "res://Resource/Powers/CoffiePower128.png"
    );

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;


    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        new[] { HoverTipFactory.FromCard<Rainworld_Liver_Fruit2>() };

    public override async Task AfterCardExhausted(PlayerChoiceContext choiceContext, CardModel card, bool _)
    {
        if (card.Owner.Creature == Owner)
        {
            await PowerCmd.ModifyAmount(choiceContext,this,1,Owner,null);
            if (4 < Amount)
            {
                await PowerCmd.ModifyAmount(choiceContext,this,-4,Owner,null);
                var aliveAllies = CombatState.Players
                    .Where(p => p.Creature.IsAlive && p != Owner.Player)
                    .ToList();
                if (aliveAllies.Count > 0)
                {
                    var randomAlly = Owner.Player!.RunState.Rng.Shuffle.NextItem(aliveAllies);
                    var fruit = CombatState.CreateCard<Rainworld_Liver_Fruit2>(randomAlly!);
                    CardCmd.PreviewCardPileAdd(
                        await CardPileCmd.AddGeneratedCardToCombat(fruit, PileType.Hand, randomAlly));
                }
            }
        }
    }
}