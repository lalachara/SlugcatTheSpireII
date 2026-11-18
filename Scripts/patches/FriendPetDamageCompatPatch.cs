using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Patching.Models;

namespace Rainworld.Scripts.patches;

/// <summary>
/// 伙伴（本 mod 的蜥蜴/猫崽/拾荒者、以及原版 Osty 之外的任何宠物）作为伤害来源时，修正原版的能力。
/// <para>原版 <see cref="PersonalHivePower" /> 的 <c>AfterDamageReceived</c> 里写的是：</para>
/// <code>
/// if (dealer.Monster is Osty) dealer = dealer.PetOwner.Creature;   // 只认 Osty 这一种宠物
/// CardModel card = base.CombatState.CreateCard&lt;Dazed&gt;(dealer.Player);   // dealer.Player 为 null 时炸
/// </code>
/// <para>宠物怪物本身没有 Player，只有 <c>PetOwner</c>；原版只特殊处理了 Osty，所以本 mod 的伙伴
/// 打带有「个人蜂巢」的敌人时，<c>dealer.Player</c> 为 null → 造出来的 Dazed 没有主人 →
/// <c>CardPileCmd.AddGeneratedCardsToCombat</c> 里 <c>owner.Creature.CombatState</c> 空引用，
/// 整个回合循环直接崩掉（表现为「结束回合时卡住」）。</para>
/// <para>这里把这条规则推广到<b>所有宠物</b>：宠物当 dealer 时换成它的主人；
/// 实在找不到主人（既没有 Player 也没有 PetOwner）时直接跳过这条能力，绝不让它把战斗打断。</para>
/// </summary>
public sealed class FriendPetDamageCompatPatch : IPatchMethod
{
	public static string PatchId => "rainworld_friend_pet_damage_compat";
	public static string Description => "伙伴造成伤害时修正 PersonalHivePower 对宠物 dealer 的空引用";
	public static bool IsCritical => false;

	public static ModPatchTarget[] GetTargets() =>
	[
		new(typeof(PersonalHivePower), nameof(PersonalHivePower.AfterDamageReceived), new[]
		{
			typeof(MegaCrit.Sts2.Core.GameActions.Multiplayer.PlayerChoiceContext),
			typeof(Creature),
			typeof(DamageResult),
			typeof(MegaCrit.Sts2.Core.ValueProps.ValueProp),
			typeof(Creature),
			typeof(CardModel)
		})
	];

	public static bool Prefix(ref Creature? dealer, ref Task __result)
	{
		// 原版在这条能力里只会把 Osty 还原成主人，其它宠物一律漏掉
		if (dealer == null)
			return true;

		if (dealer.Monster is Osty)
			return true;   // Osty 由原版自己处理

		// 宠物（PetOwner 非空但自身没有 Player）→ 换回主人，等价于原版对 Osty 的处理
		if (dealer.Player == null && dealer.PetOwner?.Creature is { } ownerCreature)
		{
			dealer = ownerCreature;
			return true;
		}

		// 既没有 Player 也没有 PetOwner：原版会在 CreateCard(dealer.Player) 处空引用，直接跳过
		if (dealer.Player == null)
		{
			__result = Task.CompletedTask;
			return false;
		}

		return true;
	}
}
