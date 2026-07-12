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
using Rainworld.Scripts.Potions;
using STS2RitsuLib.Data.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Godot;
using STS2RitsuLib.Utils;

namespace Rainworld.Scripts;

[RegisterCharacter]
public sealed class Rainworld_Liver : Slugcat<Rainworld_Liver_CardPool, Rainworld_Liver_RelicPool, Rainworld_Liver_PotionPool>,Slugcat
{
	public int workLevel { get; set; } = 4;
	public int maxWorkLevel { get; set; } = 4;
	public int food { get; set; } = 0;
	public int maxFood { get; set; } = 7;
	public int sleepfood { get; set; } = 4;	
	public int enemyfood { get; set; } = 3;
	public int jyFood { get; set; } = 5;
	public int bossfood { get; set; } = 7;
	public int roomfood { get; set; } = 2;
	
	public override CharacterAssetProfile AssetProfile => CharacterAssetProfiles.Merge(
		CharacterAssetProfiles.Ironclad(),
		new(
			Scenes: new(
				VisualsPath: "res://Godot/Character/Liver/Liver.tscn",
				EnergyCounterPath: "res://Godot/EnergyLable/EnergyCounter.tscn",
				MerchantAnimPath: "res://Godot/Character/Liver/shop.tscn",
				RestSiteAnimPath: "res://Godot/Character/Liver/Liver_rest_site.tscn"
				
			),
			Ui: new(
				IconTexturePath: "res://Godot/Character/Liver/layer4.png",
				IconPath: "res://Godot/Character/Liver/Head.tscn",
				CharacterSelectBgPath: "res://Godot/CharacterSelectBgs/LiverSelectBg.tscn",
				CharacterSelectIconPath: "res://Resource/SlugCat/Liver_Select_Button.png",
				CharacterSelectLockedIconPath: "res://Resource/SlugCat/Liver_Select_Button.png",
				IconOutlineTexturePath: "res://Godot/Character/Liver/layer4.png"
			),
			Audio: new(
				CharacterTransitionSfx: "event:/sfx/ui/wipe_ironclad",
				CharacterSelectSfx: "event:/Rainworld/sfx/Meow"

			),
			Multiplayer: new(
				// 多人模式-手指。
				 ArmPointingTexturePath:"res://Godot/Character/Liver/mult_hand_1.png",
				// 多人模式剪刀石头布-石头。
				 ArmRockTexturePath:"res://Godot/Character/Liver/mult_hand_0.png",
				// 多人模式剪刀石头布-布。
				 ArmPaperTexturePath:"res://Godot/Character/Liver/mult_hand_3.png",
				// 多人模式剪刀石头布-剪刀。
				 ArmScissorsTexturePath:"res://Godot/Character/Liver/mult_hand_2.png"
			)
			
			
		));

	public const string energyColorName = "ironclad";

	public override CharacterGender Gender => CharacterGender.Neutral;

	public override Color NameColor => new(0f, 0f, 0f);

	public override int StartingHp => 9;

	public override int StartingGold => 99;

	protected override IEnumerable<StartingDeckEntry> StartingDeckEntries => [
		new(typeof(Rainworld_Liver_Strike), 4),
		new(typeof(Rainworld_Liver_Spear), 1),
		new(typeof(Rainworld_Liver_Defend), 4),
		new(typeof(Rainworld_Liver_Rock), 1)
	];

	protected override IEnumerable<Type> StartingRelicTypes => [typeof(Liver_Fruit1)];

	public override float AttackAnimDelay => 0.15f;
	public override float CastAnimDelay => 0.25f;
	public override Color EnergyLabelOutlineColor => new(0.1f, 0.1f, 1f);
	public override Color DialogueColor => new Color("2a2a2a");
	public override Color MapDrawingColor => new Color("FFFFFF");
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