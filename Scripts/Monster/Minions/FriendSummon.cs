using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Rainworld.Scripts.Monster.Minions;

/// <summary>
/// 伙伴站位槽：伙伴按召唤顺序固定占用五个位置（右上方 → 左上方 → 正上方 → 右侧 → 左侧），
/// 第 6 只起循环复用。具体坐标见 <see cref="FriendLayout" />。
/// 原生实现，替代原先 MinionLib 的 <c>MinionPosition</c>，槽位顺序与数值完全保持一致。
/// </summary>
public enum FriendPosition
{
    Front,
    Back,
    FrontUpper,
    BackUpper,
    Upper
}

/// <summary>
/// 召唤伙伴时的参数（替代原先 MinionLib 的 <c>MinionSummonOptions</c>，字段含义与默认值不变）：
/// <para><c>MaxHp</c>：需要覆盖初始生命上限时传入（不传则用怪物自身的 MinInitialHp）。</para>
/// <para><c>PrimaryStatAmount</c>：主人附加给伙伴的力量层数。</para>
/// <para><c>Source</c>：召唤来源（卡牌 / 遗物），用于能力来源显示与悬停提示。</para>
/// <para><c>Position</c>：预定的站位槽，由 <see cref="FriendPositions.Next" /> 计算。</para>
/// </summary>
public readonly record struct FriendSummonOptions(
    decimal? MaxHp = null,
    decimal? PrimaryStatAmount = null,
    decimal? SecondaryStatAmount = null,
    decimal? TertiaryStatAmount = null,
    CardModel? Source = null,
    FriendPosition Position = FriendPosition.Front);

/// <summary>
/// 伙伴召唤指令：完全走原版宠物链路（与官方 Byrdpip / PaelsLegion / Wuppy 的召唤写法一致）。
/// <para>1. <c>PlayerCmd.AddPet&lt;T&gt;</c>：<c>CombatState.CreateCreature</c> 造出生物，
/// 登记进 <c>PlayerCombatState.Pets</c>，再由 <c>CreatureCmd.Add</c> 进场并在战斗场景里生成节点。</para>
/// <para>2. 给模型打上站位槽，跑伙伴自己的 <see cref="FriendPetBase.OnSummon" />（血量 / 力量 / 行动驱动）。</para>
/// <para>3. 按本 mod 的五站位重新摆位：原版只会把宠物贴着主人排成一排，
/// 具体坐标由 <see cref="FriendLayout" /> 接管。</para>
/// </summary>
public static class FriendSummonCmd
{
    /// <summary>召唤一只伙伴（T 必须是本 mod 的伙伴模板）。</summary>
    public static async Task<Creature> Summon<T>(PlayerChoiceContext choiceContext, Player owner,
        FriendSummonOptions options = default) where T : FriendPetBase
    {
        ArgumentNullException.ThrowIfNull(owner);

        Creature pet = await PlayerCmd.AddPet<T>(owner);

        if (pet.Monster is FriendPetBase friend)
        {
            friend.Position = options.Position;

            // 召唤初始化：血量上限、力量、行动驱动能力、专属能力与初始意图
            await friend.OnSummon(choiceContext, owner, options);
        }

        // 进场后立刻按五站位摆好（原版 AddCreature 只会把宠物贴在主人脚下排成一排）
        await FriendLayout.RearrangeAsync();

        return pet;
    }
}
