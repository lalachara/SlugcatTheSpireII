using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Potions;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace Rainworld.Scripts.Powers;

#nullable enable
[RegisterPower]
public sealed class FakedeathPower : ModPowerTemplate
{
    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://Resource/Powers/RedCat/two.png",
        BigIconPath: "res://Resource/Powers/RedCat/two.png"
    );

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    private const int MaxCardsPerTurn = 5;

    public override decimal ModifyHpLostAfterOsty(
        Creature target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        // 假死期间不掉血：伙伴替主人挡下的伤害同样归零
        if (!FriendGuardPower.IsOwnerOrGuardingPet(Owner, target))
            return amount;
        return 0;

    }
    
    public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        NCreature? creatureNode = NCombatRoom.Instance?.GetCreatureNode(Owner.Player?.Creature!);
      
        creatureNode?.SetAnimationTrigger("FakeDeath");
        if(Owner.Player.Character is Slugcat slugcat)
            slugcat.SuppressHurtAnim = true;
        
    }

    // 0.113：AbstractModel.AfterDeath 新增了 CardModel? source 参数（致命伤害来源卡牌）
    public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, CardModel? source, bool wasRemovalPrevented, float deathAnimLength)
    {
        if (wasRemovalPrevented  || !creature.IsPlayer || creature == Owner)
            return;

        var otherPlayersAlive = CombatState.Players.Any(p => p != Owner.Player && p.Creature.IsAlive && (!p.Creature.HasPower<FakedeathPower>()));
        if (!otherPlayersAlive)
            await CreatureCmd.Kill(Owner);
    }

    public override async Task AfterAutoPrePlayPhaseEnteredLate(PlayerChoiceContext choiceContext, Player player)
    {
        if (player == Owner.Player )
        {
            var hand = PileType.Hand.GetPile(Owner.Player).Cards.ToList();
            int cardsPlayed = 0;
            foreach (var card in hand)
            {
                if (cardsPlayed >= MaxCardsPerTurn)
                    break;
                if (card.CanPlay()&&card.Type == CardType.Attack)
                {
                    var target = GetTarget(card, CombatState);
                    await card.SpendResources();
                    await CardCmd.AutoPlay(choiceContext, card, target, skipXCapture: false);
                    cardsPlayed++;
                }
            }
            PlayerCmd.EndTurn(Owner.Player,false);
        }
    }
    private Creature? GetTarget(CardModel card, ICombatState combatState)
    {
        Rng combatTargets = this.Owner.Player.RunState.Rng.CombatTargets;
        Creature target;
        switch (card.TargetType)
        {
            case TargetType.AnyEnemy:
                target = combatState.HittableEnemies.FirstOrDefault<Creature>();
                break;
            case TargetType.AnyPlayer:
                target = this.Owner.Player.Creature;
                break;
            case TargetType.AnyAlly:
                target = combatTargets.NextItem<Creature>(combatState.Allies.Where<Creature>((Func<Creature, bool>) (c => c != null && c.IsAlive && c.IsPlayer && c != this.Owner.Player.Creature)));
                break;
            default:
                target = (Creature) null;
                break;
        }
        return target;
    }
}