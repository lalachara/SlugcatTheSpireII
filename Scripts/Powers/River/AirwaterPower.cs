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

        if (Owner.CombatState == null)
        {
            lastBlock = 0;
            return;
        }

        // 自然消失的格挡 = 上一回合末记录的格挡量 lastBlock。
        // 不能写成 lastBlock - Owner.Block：格挡在回合开始先被清空，若随后又获得新格挡，
        // Owner.Block 会被抬升，导致差值被低估（伤害减少）。这里直接用消失前的值，与新获格挡无关。
        decimal lostBlock;

        // ShouldClearBlock 返回 true：回合开始时格挡会被完整清空，全部自然消失。
        if (Hook.ShouldClearBlock(Owner.CombatState, Owner, out var preventer))
        {
            lostBlock = lastBlock;
        }
        else
        {
            // 存在"阻止清除"的能力（preventer）。大多数（壁垒 Barricade、残影 Blur、潜地 Burrowed）
            // 是完全保留格挡，此时格挡不自然消失，不触发伤害。
            // 但原版外卡钳（坚固钳子 SturdyClamp）是"跨回合保留最多 X 点格挡"，超出部分仍会流失，
            // 这部分要看作自然消失并转成伤害。
            var retainCap = GetRetainCap(preventer);
            if (retainCap <= 0)
            {
                lastBlock = 0;
                return;
            }

            lostBlock = Math.Max(0, lastBlock - Math.Min(lastBlock, retainCap));
            if (lostBlock <= 0)
            {
                lastBlock = 0;
                return;
            }
        }

        Flash();
        await CreatureCmd.Damage(new BlockingPlayerChoiceContext(), CombatState.HittableEnemies,
            lostBlock, ValueProp.Unpowered, Owner);
        lastBlock = 0;
    }

    /// <summary>
    /// 取得"阻止清除格挡"的能力留下的保留上限；若无上限（完全保留）返回 0。
    /// 外卡钳（坚固钳子 SturdyClamp）跨回合保留最多 X 点格挡、超出部分流失，其上限取自私有常量
    /// <c>_maxRetainedBlock</c>。壁垒/残影/潜地等完全保留格挡的能力没有该字段，返回 0（即不流失、不触发伤害）。
    /// </summary>
    private static decimal GetRetainCap(AbstractModel preventer)
    {
        var field = preventer.GetType().GetField(
            "_maxRetainedBlock",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        if (field == null || !field.IsLiteral)
        {
            return 0;
        }
        return Convert.ToDecimal(field.GetRawConstantValue());
    }
}