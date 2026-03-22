using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using BaseLib.Abstracts;
using demo.Resource.Card;
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
using Rainworld.relics;
using Rainworld.Scripts;
using Rainworld.Scripts.Card.Liver.Attack;
using Rainworld.Scripts.Potions;

namespace Rainworld.Scripts;

public sealed class Rainworld_Liver : Slugcat
{
	public int workLevel,maxWorkLevel = 4,food,maxFood=7,sleepfood=4;

	public override string CustomVisualPath => "res://Godot/Character/Liver.tscn";
	// 卡牌拖尾路径。
	// public override string CustomTrailPath => "res://scenes/vfx/card_trail_ironclad.tscn";
	// 人物头像路径。
	public override string CustomIconTexturePath => "res://Godot/Character/Liver/layer4.png";
	// 人物头像2号。
	public override string CustomIconPath => "res://Godot/Character/Liver/Head.tscn";
	
	// 篝火休息动画。
	// public override string CustomRestSiteAnimPath => "res://scenes/rest_site/characters/ironclad_rest_site.tscn";
	// 商店人物动画。
	// public override string CustomMerchantAnimPath => "res://scenes/merchant/characters/ironclad_merchant.tscn";
	// 多人模式-手指。
	// public override string CustomArmPointingTexturePath => null;
	// 多人模式剪刀石头布-石头。
	// public override string CustomArmRockTexturePath => null;
	// 多人模式剪刀石头布-布。
	// public override string CustomArmPaperTexturePath => null;
	// 多人模式剪刀石头布-剪刀。
	// public override string CustomArmScissorsTexturePath => null;

	// 人物选择背景。
	public override string CustomCharacterSelectBg => "res://Godot/CharacterSelectBgs/LiverSelectBg.tscn";
	// 人物选择图标。
	public override string CustomCharacterSelectIconPath => "res://Resource/SlugCat/Liver_Select_Button.png";
	// 人物选择图标-锁定状态。
	public override string CustomCharacterSelectLockedIconPath => "res://Resource/SlugCat/Liver_Select_Button.png";
	
	public override CardPoolModel CardPool => ModelDb.CardPool<Rainworld_Liver_CardPool>();
	public override PotionPoolModel PotionPool => ModelDb.PotionPool<Rainworld_Liver_PotionPool>();
	public override RelicPoolModel RelicPool => ModelDb.RelicPool<Rainworld_Liver_RelicPool>();
	
	

	public const string energyColorName = "ironclad";

	public override CharacterGender Gender => CharacterGender.Neutral;

	protected override CharacterModel? UnlocksAfterRunAs => null;

	public override Color NameColor => new(0f, 0f, 0f);

	public override int StartingHp => 9;

	public override int StartingGold => 99;

	//初始卡牌
	public override IEnumerable<CardModel> StartingDeck => new CardModel[10]
	{
		ModelDb.Card<Rainworld_Liver_Strike>(),
		ModelDb.Card<Rainworld_Liver_Strike>(),
		ModelDb.Card<Rainworld_Liver_Strike>(),
		ModelDb.Card<Rainworld_Liver_Strike>(),
		ModelDb.Card<Rainworld_Liver_Spear>(),
		ModelDb.Card<Rainworld_Liver_Defend>(),
		ModelDb.Card<Rainworld_Liver_Defend>(),
		ModelDb.Card<Rainworld_Liver_Defend>(),
		ModelDb.Card<Rainworld_Liver_Defend>(),
		ModelDb.Card<Rainworld_Liver_Rock>()
	};

	public override IReadOnlyList<RelicModel> StartingRelics => new []{(ModelDb.Relic<Liver_Fruit1>())};

	public override float AttackAnimDelay => 0.15f;
	public override float CastAnimDelay => 0.25f;
	public override Color EnergyLabelOutlineColor => new(0.1f, 0.1f, 1f);
	public override Color DialogueColor => new Color("000000");
	public override Color MapDrawingColor => new Color("000000");
	public override Color RemoteTargetingLineColor => new Color("000000");
	public override Color RemoteTargetingLineOutline => new Color("000000");

	public override List<string> GetArchitectAttackVfx() => [
		"vfx/vfx_attack_blunt",
		"vfx/vfx_heavy_blunt",
		"vfx/vfx_attack_slash",
		"vfx/vfx_bloody_impact",
		"vfx/vfx_rock_shatter"
	];
}
