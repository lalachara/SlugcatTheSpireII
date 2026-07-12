using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Rainworld.Resource.Card;
using Godot;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.PotionPools;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Runs;
using Rainworld.Patches;
using Rainworld.relics;
using Rainworld.Scripts.Powers;
using Rainworld.Scripts.Card.Liver.Attack;
using Rainworld.Scripts.Potions;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Utils;

namespace Rainworld.Scripts;

public interface Slugcat
{
	
	public int workLevel { get; set; } 
	public int maxWorkLevel { get; set; }
	public int food { get; set; }
	public int maxFood { get; set; } 
	public int sleepfood { get; set; }
	
	public int enemyfood { get; set; }
	public int jyFood { get; set; } 
	public int bossfood { get; set; }
	
	public int roomfood { get; set; }
	
	public bool SuppressHurtAnim { get; set; }
	
	public bool wawawa { get; set; }

	
}
public abstract class Slugcat<TCardPool, TRelicPool, TPotionPool> 
	: ModCharacterTemplate<TCardPool, TRelicPool, TPotionPool>
	where TCardPool : TypeListCardPoolModel
	where TRelicPool : TypeListRelicPoolModel
	where TPotionPool : TypeListPotionPoolModel
{
	public override bool RequiresEpochAndTimeline => false;  // 加这行
	
	public override CharacterGender Gender => CharacterGender.Neutral;

	public bool SuppressHurtAnim { get; set; }  // ← 新增，实现接口
	public bool wawawa { get; set; }  // ← 新增，实现接口
	
	public override Color NameColor => StsColors.red;
	
	public override float AttackAnimDelay => 0.15f;
	public override float CastAnimDelay => 0.25f;
	public override Color EnergyLabelOutlineColor => new(0.1f, 0.1f, 1f);
	public override Color DialogueColor => new Color("000000");
	public override Color MapDrawingColor => new Color("000000");
	public override Color RemoteTargetingLineColor => new Color("000000");
	public override Color RemoteTargetingLineOutline => new Color("000000");
	private static readonly Material?
		_poolFrameMaterial = MaterialUtils.CreateUnmodulatedHsvShaderMaterial(); // 如果你是自定义卡框，使用这个
	public override List<string> GetArchitectAttackVfx() => [
		"vfx/vfx_attack_blunt",
		"vfx/vfx_heavy_blunt",
		"vfx/vfx_attack_slash",
		"vfx/vfx_bloody_impact",
		"vfx/vfx_rock_shatter"
	];
	
	protected override CreatureAnimator? SetupCustomCreatureAnimator(MegaSprite controller)
	{
		// 定义各动画状态及是否循环
		AnimState idle = new("idle_loop", isLooping: true);
		AnimState attack = new("attack");
		AnimState cast = new("cast");
		AnimState hurt = new("hurt");
		AnimState die = new("die");
		AnimState relaxed = new("relaxed_loop", isLooping: true);
		AnimState sleep = new("sleep", isLooping: true);  // 新增 sleep，循环播放
		AnimState Corrupt = new("xianggu", isLooping: true);
		AnimState Meow = new("meow");
		AnimState FakeDeath = new("relive_loop", isLooping: true);

		// 设置播放完后自动回到 idle
		 attack.NextState = idle;
		 cast.NextState = idle;
		 hurt.NextState = idle;
		 Meow.NextState = idle;
		// idle.NextState = idle;
		// sleep.NextState = idle;  // sleep 结束后回到 idle
		// Corrupt.NextState = idle;
		// FakeDeath.NextState = die;
		

		// 创建 CreatureAnimator，idle 为默认状态
		CreatureAnimator creatureAnimator = new(idle, controller);

		// 绑定状态名到动画
		creatureAnimator.AddAnyState("Idle", idle);
		creatureAnimator.AddAnyState("Dead", die);
		creatureAnimator.AddAnyState("Hit", hurt,() => !SuppressHurtAnim);
		creatureAnimator.AddAnyState("Attack", attack,() => !SuppressHurtAnim);
		creatureAnimator.AddAnyState("Cast", cast,() => !SuppressHurtAnim);
		creatureAnimator.AddAnyState("Relaxed", relaxed);
		creatureAnimator.AddAnyState("Sleep", sleep);  // 注册 sleep 状态
		creatureAnimator.AddAnyState("Corrupt", Corrupt);
		creatureAnimator.AddAnyState("Meow", Meow,() => !SuppressHurtAnim);
		creatureAnimator.AddAnyState("FakeDeath", FakeDeath);
		return creatureAnimator;
	}

}