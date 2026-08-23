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
using Rainworld.Scripts.Card.River.Attack;
using Rainworld.Scripts.Card.River.Skills;
using Rainworld.Scripts.Potions;
using STS2RitsuLib.Data.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Godot;
using STS2RitsuLib.Utils;

namespace Rainworld.Scripts;

[RegisterCharacter]
public sealed class Rainworld_River : Slugcat<Rainworld_River_CardPool, Rainworld_River_RelicPool, Rainworld_River_PotionPool>,Slugcat
{
	public int workLevel { get; set; } = 4;
	public int maxWorkLevel { get; set; } = 4;
	public int food { get; set; } = 0;
	public int maxFood { get; set; } = 6;
	public int sleepfood { get; set; } = 5;
	
	public int enemyfood { get; set; } = 3;
	public int jyFood { get; set; } = 5;
	public int bossfood { get; set; } = 6;
	public int roomfood { get; set; } = 2;
	
	public override int StartingHp => 8;
	public override int StartingGold => 99;
	public override CharacterAssetProfile AssetProfile => CharacterAssetProfiles.Merge(
		CharacterAssetProfiles.Ironclad(),
		new(
			Scenes: new(
				VisualsPath: "res://Godot/Character/River/River.tscn",
				EnergyCounterPath: "res://Godot/EnergyLable/EnergyCounter.tscn",
				MerchantAnimPath: "res://Godot/Character/River/shop.tscn",
				RestSiteAnimPath: "res://Godot/Character/River/River_rest_site.tscn"
			),
			Ui: new(
				IconTexturePath: "res://Godot/Character/River/River_Head.png",
				IconPath: "res://Godot/Character/River/Head.tscn",
				CharacterSelectBgPath: "res://Godot/CharacterSelectBgs/RiverSelectBg.tscn",
				CharacterSelectIconPath: "res://Resource/SlugCat/River_Select_Button.png",
				CharacterSelectLockedIconPath: "res://Resource/SlugCat/River_Select_Button.png",
				IconOutlineTexturePath: "res://Godot/Character/River/River_Head.png"
				
			),
			Audio: new(
				CharacterTransitionSfx: "event:/sfx/ui/wipe_ironclad",
				CharacterSelectSfx: "event:/Rainworld/sfx/MeowRiver",
				 AttackSfx: "event:/Rainworld/sfx/strike"

			),
			Multiplayer: new(
				// 多人模式-手指。
				ArmPointingTexturePath:"res://Godot/Character/River/mult_hand_1.png",
				// 多人模式剪刀石头布-石头。
				ArmRockTexturePath:"res://Godot/Character/River/mult_hand_0.png",
				// 多人模式剪刀石头布-布。
				ArmPaperTexturePath:"res://Godot/Character/River/mult_hand_3.png",
				// 多人模式剪刀石头布-剪刀。
				ArmScissorsTexturePath:"res://Godot/Character/River/mult_hand_2.png"
			),
			VanillaRelicVisualOverrides: [
				new (CharacterOwnedVanillaRelicModelId.YummyCookie, new("res://Resource/Relics/rivercookie.png")) // 美味饼干覆盖
			]

		));
	
	public override Color NameColor => new(0f, 0f, 0f);

	protected override IEnumerable<StartingDeckEntry> StartingDeckEntries => [
		new(typeof(Rainworld_River_Strike), 4),
		new(typeof(Rainworld_River_Spear), 1),
		new(typeof(Rainworld_River_Defend), 4),
		new(typeof(Rainworld_River_Inertia), 1)
	];

	protected override IEnumerable<Type> StartingRelicTypes => [typeof(River_Waterball1)];

	public override float AttackAnimDelay => 0.15f;
	public override float CastAnimDelay => 0.25f;
	public override Color EnergyLabelOutlineColor => new("1a4750");
	public override Color DialogueColor => new Color("3f8999");
	public override Color MapDrawingColor => new Color(145f/255f, 204f/255f, 240f/255f, 1f);
	public override Color RemoteTargetingLineColor => new Color(145f/255f, 204f/255f, 240f/255f, 1f);
	public override Color RemoteTargetingLineOutline => new Color(145f/255f, 204f/255f, 240f/255f, 1f);
	protected override NCreatureVisuals? TryCreateCreatureVisuals() => RitsuGodotNodeFactories.CreateFromScenePath<NCreatureVisuals>(AssetProfile.Scenes!.VisualsPath!);

	public override List<string> GetArchitectAttackVfx() => [
		"vfx/vfx_attack_blunt",
		"vfx/vfx_heavy_blunt",
		"vfx/vfx_attack_slash",
		"vfx/vfx_bloody_impact",
		"vfx/vfx_rock_shatter"
	];
}