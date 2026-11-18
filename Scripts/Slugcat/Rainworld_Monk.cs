using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Rainworld.Resource.Card;
using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Models.PotionPools;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes.Combat;
using Rainworld.relics;
using Rainworld.Scripts;
using Rainworld.Scripts.Card.Liver.Attack;
using Rainworld.Scripts.Card.Monk;
using Rainworld.Scripts.Potions;
using STS2RitsuLib.Data.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Godot;
using STS2RitsuLib.Utils;

namespace Rainworld.Scripts;

[RegisterCharacter]
public sealed class Rainworld_Monk : Slugcat<Rainworld_Monk_CardPool, Rainworld_Monk_RelicPool, Rainworld_Monk_PotionPool>,Slugcat
{
	public int workLevel { get; set; } = 4;
	public int maxWorkLevel { get; set; } = 4;
	public int food { get; set; } = 0;
	public int maxFood { get; set; } = 5;
	public int sleepfood { get; set; } = 3;	
	public int enemyfood { get; set; } = 2;
	public int jyFood { get; set; } = 3;
	public int bossfood { get; set; } = 5;
	public int roomfood { get; set; } = 2;
	
	public override CharacterAssetProfile AssetProfile => CharacterAssetProfiles.Merge(
		CharacterAssetProfiles.Ironclad(),
		new(
			Scenes: new(
				VisualsPath: "res://Godot/Character/Monk/Monk.tscn",
				EnergyCounterPath: "res://Godot/EnergyLable/EnergyCounter.tscn",
				MerchantAnimPath: "res://Godot/Character/Monk/shop.tscn",
					RestSiteAnimPath: "res://Godot/Character/Monk/Monk_rest_site.tscn"
				
			),
			Ui: new(
				IconTexturePath: "res://Godot/Character/Monk/layer4.png",
				IconPath: "res://Godot/Character/Monk/Head.tscn",
				CharacterSelectBgPath: "res://Godot/CharacterSelectBgs/MonkSelectBg.tscn",
				CharacterSelectIconPath: "res://Resource/SlugCat/Monk_Select_Button.png",
				CharacterSelectLockedIconPath: "res://Resource/SlugCat/Liver_Select_Button.png",
				IconOutlineTexturePath: "res://Godot/Character/Monk/layer4.png"
			),
			Audio: new(
				CharacterTransitionSfx: "event:/sfx/ui/wipe_ironclad",
				CharacterSelectSfx: "event:/Rainworld/sfx/Meow"

			),
			Multiplayer: new(
				// 多人模式-手指。
				 ArmPointingTexturePath:"res://Godot/Character/Monk/mult_hand_1.png",
				// 多人模式剪刀石头布-石头。
				 ArmRockTexturePath:"res://Godot/Character/Monk/mult_hand_0.png",
				// 多人模式剪刀石头布-布。
				 ArmPaperTexturePath:"res://Godot/Character/Monk/mult_hand_3.png",
				// 多人模式剪刀石头布-剪刀。
				 ArmScissorsTexturePath:"res://Godot/Character/Monk/mult_hand_2.png"
			),
			VanillaRelicVisualOverrides: [
				     new (CharacterOwnedVanillaRelicModelId.YummyCookie, new(IconPath: "res://Resource/Relics/monkcookie.png", IconOutlinePath: "res://Resource/Relics/monkcookie.png", BigIconPath: "res://Resource/Relics/monkcookie.png")) // 美味饼干覆盖
				 ]
			
			
		));

	public const string energyColorName = "ironclad";

	public override CharacterGender Gender => CharacterGender.Neutral;

	public override Color NameColor => new(0f, 0f, 0f);

	public override int StartingHp => 7;

	public override int StartingGold => 99;

	protected override IEnumerable<StartingDeckEntry> StartingDeckEntries => [
		new(typeof(Rainworld_Monk_Strike), 4),
		new(typeof(Rainworld_Monk_Spear), 1),
		new(typeof(Rainworld_Monk_Defend), 4),
		new(typeof(Rainworld_Monk_Rock), 1),
		new(typeof(Rainworld_Monk_Interact), 1)

		
	];

	protected override IEnumerable<Type> StartingRelicTypes => [typeof(Monk_Bottlecab)];

	public override float AttackAnimDelay => 0.15f;
	public override float CastAnimDelay => 0.25f;
	public override Color EnergyLabelOutlineColor => new("1a4750");
	public override Color DialogueColor => new Color("2a2a2a");
	public override Color MapDrawingColor => new Color("FFFF73");

	/// <summary>恐惧模式（Dread）角色选择界面里名字的颜色：与地图画笔颜色同色（原版默认是黄色 ffd500c0）。</summary>
	public override Color DreadNameColor => new Color("FFFF73C0");

	/// <summary>恐惧模式下名字背景的对比度：浅色（黄/白/浅蓝）调高一点才看得清。</summary>
	public override float DreadBgContrast => 1.5f;
	public override Color RemoteTargetingLineColor => new Color("FFFFFF");
	public override Color RemoteTargetingLineOutline => new Color("FFFFFF");
	
	protected override NCreatureVisuals? TryCreateCreatureVisuals() => RitsuGodotNodeFactories.CreateFromScenePath<NCreatureVisuals>(AssetProfile.Scenes!.VisualsPath!);

	public override List<string> GetArchitectAttackVfx() => [
		"vfx/vfx_attack_blunt",
		"vfx/vfx_heavy_blunt",
		"vfx/vfx_attack_slash",
		"vfx/vfx_bloody_impact",
		"vfx/vfx_rock_shatter"
	];
}