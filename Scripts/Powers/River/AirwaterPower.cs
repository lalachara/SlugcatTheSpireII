using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
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
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.ValueProps;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

#nullable enable
[RegisterPower]
public sealed class AirwaterPower : ModPowerTemplate
{
    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"res://Resource/Powers/River/{GetType().Name}128.png",
        BigIconPath: $"res://Resource/Powers/River/{GetType().Name}128.png"
    );

    public override PowerType Type => PowerType.Buff;
    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    public override PowerStackType StackType => PowerStackType.Single;
    

    private decimal lastBlock = 0;

    public override async Task BeforeSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side == CombatSide.Enemy)
        {
            lastBlock = Owner.Block;
        }
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        // 玩家回合开始时，上一回合末（敌人回合结束）记录的格挡会自然消失。
        if (lastBlock <= 0 || Owner != player.Creature)
        {
            return;
        }

        // 若玩家身上存在阻止格挡清除的效果（其 ShouldClearBlock 对持有者自己返回 false），
        // 则自身格挡在回合开始不会自然消失，此时不触发伤害。这类能力包括：壁垒 Barricade、
        // 残影 Blur、潜地 Burrowed、夹固 SturdyClamp 等。传入 Owner（玩家自己）以判定。
        if (Owner.CombatState == null || !Hook.ShouldClearBlock(Owner.CombatState, Owner, out _))
        {
            lastBlock = 0;
            return;
        }

        // 自然消失的格挡 = 上一回合末记录的格挡量 lastBlock。
        // 不能写成 lastBlock - Owner.Block：格挡在回合开始先被清空，若随后又获得新格挡，
        // Owner.Block 会被抬升，导致差值被低估（伤害减少）。这里直接用消失前的值，与新获格挡无关。
        var lostBlock = lastBlock;
        Flash();
        await CreatureCmd.Damage(new BlockingPlayerChoiceContext(), CombatState.HittableEnemies,
            lostBlock, ValueProp.Unpowered, Owner);
        lastBlock = 0;
    }
}