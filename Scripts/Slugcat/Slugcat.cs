using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using BaseLib;
using BaseLib.Abstracts;
using Rainworld.Resource.Card;
using Godot;
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

namespace Rainworld.Scripts;
public abstract class Slugcat : PlaceholderCharacterModel
{

	//此处数据仅用于处理存档读档，不要操作。
	public int workLevel=2,maxWorkLevel = 4,food=0,maxFood=7,sleepfood=4;
	// [SavedProperty]
	// public int WorkLevel
	// {
	// 	get
	// 	{
	// 		return workLevel;
	// 	}
	// 	set
	// 	{
	// 		AssertMutable();
	// 		workLevel = value;
	// 		
	// 	}
	// }
	// [SavedProperty]
	// public int MaxWorkLevel
	// {
	// 	get
	// 	{
	// 		return maxWorkLevel;
	// 	}
	// 	set
	// 	{
	// 		AssertMutable();
	// 		maxWorkLevel = value;
	// 	}
	// }
	// [SavedProperty]
	// public int Food
	// {
	// 	get
	// 	{
	// 		return food;
	// 	}
	// 	set
	// 	{
	// 		AssertMutable();
	// 		food = value;
	// 	}
	// }
	
	
	//public override string CustomVisualPath => "res://Godot/CharacterSelectBgs/LiverSelectBg.tscn";
	// 卡牌拖尾路径。
	// public override string CustomTrailPath => "res://scenes/vfx/card_trail_ironclad.tscn";
	// 人物头像路径。
	//public override string CustomIconTexturePath => "res://Resource/SlugCat/Head.png";
	// 人物头像2号。
	 //public override string CustomIconPath => "res://Godot/Character/Liver/Head.tscn";
	// 能量表盘tscn路径。要自定义见下。
	public override string CustomEnergyCounterPath => "res://Godot/EnergyLable/EnergyCounter.tscn";
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
	//public override string CustomCharacterSelectBg => "res://Godot/CharacterSelectBgs/LiverSelectBg.tscn";
	// 人物选择图标。
	//public override string CustomCharacterSelectIconPath => "res://Resource/SlugCat/Liver_Select_Button.png";
	// 人物选择图标-锁定状态。
	//public override string CustomCharacterSelectLockedIconPath => "res://Resource/SlugCat/Liver_Select_Button.png";
	
	public override CardPoolModel CardPool => ModelDb.CardPool<Rainworld_Liver_CardPool>();
	public override PotionPoolModel PotionPool => ModelDb.PotionPool<Rainworld_Liver_PotionPool>();
	public override RelicPoolModel RelicPool => ModelDb.RelicPool<Rainworld_Liver_RelicPool>();
	
	//音效不能删
	public override string CharacterTransitionSfx => "event:/sfx/ui/wipe_ironclad";
	
	public override CharacterGender Gender => CharacterGender.Neutral;

	protected override CharacterModel? UnlocksAfterRunAs => null;

	public override Color NameColor => StsColors.red;

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

	public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
	{
		MainFile.Logger.Info($"Character执行turnstart");
		CombatUiPatch.callTurnStart();		
	}
	public override async Task AfterCombatVictory(CombatRoom room)
	{
		MainFile.Logger.Info($"Character执行victory");
		CombatUiPatch.callVictory();		
	}
	

	public override async Task BeforeCombatStart()
	{
		MainFile.Logger.Info($"Character执行BeforeCombatStart");
		
		await base.BeforeCombatStart();
	}
}
