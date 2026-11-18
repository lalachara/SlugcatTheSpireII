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
using Rainworld.Scripts.Card.RedCat;
using Rainworld.Scripts.Potions;
using STS2RitsuLib.Data.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Godot;
using STS2RitsuLib.Utils;

namespace Rainworld.Scripts;

[RegisterCharacter]
public sealed class Rainworld_Redcat : Slugcat<Rainworld_Redcat_CardPool, Rainworld_Redcat_RelicPool, Rainworld_Redcat_PotionPool>,Slugcat
{
	public override int StartingHp => 11;
	public override int StartingGold => 99;
	public int workLevel { get; set; } = 4;
	public int maxWorkLevel { get; set; } = 4;
	public int food { get; set; } = 0;
	public int maxFood { get; set; } = 9;
	public int sleepfood { get; set; } = 6;
	
	public int enemyfood { get; set; } = 5;
	public int jyFood { get; set; } = 7;
	public int bossfood { get; set; } = 9;
	public int roomfood { get; set; } = 0;
	public override CharacterAssetProfile AssetProfile => CharacterAssetProfiles.Merge(
		CharacterAssetProfiles.Ironclad(),
		new(
			Scenes: new(
				VisualsPath: "res://Godot/Character/RedCat/RedCat.tscn",
				EnergyCounterPath: "res://Godot/EnergyLable/EnergyCounter.tscn",
				MerchantAnimPath: "res://Godot/Character/RedCat/shop.tscn",
				RestSiteAnimPath: "res://Godot/Character/RedCat/RedCat_rest_site.tscn"
			),
			Ui: new(
				IconTexturePath: "res://Godot/Character/RedCat/RedCat_Head.png",
				IconPath: "res://Godot/Character/RedCat/Head.tscn",
				CharacterSelectBgPath: "res://Godot/CharacterSelectBgs/RedCatSelectBg.tscn",
				CharacterSelectIconPath: "res://Resource/SlugCat/RedCat_Select_Button.png",
				CharacterSelectLockedIconPath: "res://Resource/SlugCat/RedCat_Select_Button.png",
				IconOutlineTexturePath: "res://Godot/Character/RedCat/RedCat_Head.png",
				MapMarkerPath: "res://Godot/Character/RedCat/RedCat_Head.png"
				
				
			),
			Audio: new(
				CharacterTransitionSfx: "event:/Rainworld/sfx/MeowRedCat",
				CharacterSelectSfx: "event:/Rainworld/sfx/MeowRedCat"
				
			),
			Multiplayer: new(
				// 多人模式-手指。
				ArmPointingTexturePath:"res://Godot/Character/RedCat/mult_hand_1.png",
				// 多人模式剪刀石头布-石头。
				ArmRockTexturePath:"res://Godot/Character/RedCat/mult_hand_0.png",
				// 多人模式剪刀石头布-布。
				ArmPaperTexturePath:"res://Godot/Character/RedCat/mult_hand_3.png",
				// 多人模式剪刀石头布-剪刀。
				ArmScissorsTexturePath:"res://Godot/Character/RedCat/mult_hand_2.png"
			),
			VanillaRelicVisualOverrides: [
				new (CharacterOwnedVanillaRelicModelId.YummyCookie, new(IconPath: "res://Resource/Relics/redcatcookie.png", IconOutlinePath: "res://Resource/Relics/redcatcookie.png", BigIconPath: "res://Resource/Relics/redcatcookie.png")) // 美味饼干覆盖
			]
			

		));

	public const string energyColorName = "ironclad";

	public override CharacterGender Gender => CharacterGender.Neutral;

	public override Color NameColor => new(0f, 0f, 0f);



	protected override IEnumerable<StartingDeckEntry> StartingDeckEntries => [
		new(typeof(Rainworld_Redcat_Strike), 4),
		new(typeof(Rainworld_Redcat_Spear), 1),
		new(typeof(Rainworld_Redcat_Defend), 4),
		new(typeof(Rainworld_Redcat_Better), 1),
		new(typeof(Rainworld_Redcat_Originalcorrupt), 1)

		
	];
	

	protected override IEnumerable<Type> StartingRelicTypes => [typeof(Redcat_Book1)];

	public override float AttackAnimDelay => 0.15f;
	public override float CastAnimDelay => 0.25f;
	public override Color EnergyLabelOutlineColor => new("09212b");

	public override Color DialogueColor => new Color("a7293b");
	//地图绘制颜色
	public override Color MapDrawingColor => new Color(234f/255f, 13F/255f, 5F/255f);

	/// <summary>恐惧模式（Dread）角色选择界面里名字的颜色：与地图画笔颜色同色（原版默认是黄色 ffd500c0）。</summary>
	public override Color DreadNameColor => new Color("EA0D05C0");

	/// <summary>恐惧模式下名字背景的对比度：浅色（黄/白/浅蓝）调高一点才看得清。</summary>
	public override float DreadBgContrast => 1.1f;
	
	public override Color RemoteTargetingLineColor =>  new Color(234f/255f, 13F/255f, 5F/255f);
	public override Color RemoteTargetingLineOutline =>  new Color(234f/255f, 13F/255f, 5F/255f);
	

	protected override NCreatureVisuals? TryCreateCreatureVisuals() => RitsuGodotNodeFactories.CreateFromScenePath<NCreatureVisuals>(AssetProfile.Scenes!.VisualsPath!);

	public override List<string> GetArchitectAttackVfx() => [
		"vfx/vfx_attack_blunt",
		"vfx/vfx_heavy_blunt",
		"vfx/vfx_attack_slash",
		"vfx/vfx_bloody_impact",
		"vfx/vfx_rock_shatter"
	];
}