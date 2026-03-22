using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using BaseLib.Abstracts;
using demo.Resource.Card;
using Godot;
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
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Runs;
using Rainworld.relics;
using Rainworld.Scripts.Powers;
using Rainworld.Scripts.Card.Liver.Attack;
using Rainworld.Scripts.Potions;

namespace Rainworld.Scripts;
public abstract class Slugcat : PlaceholderCharacterModel
{

	public int workLevel=2,maxWorkLevel = 4,food=0,maxFood=7,sleepfood=4;
	[SavedProperty]
	public int _workLevel
	{
		get
		{
			return workLevel;
		}
		set
		{
			AssertMutable();
			workLevel = value;
			
		}
	}
	[SavedProperty]
	public int _maxWorkLevel
	{
		get
		{
			return maxWorkLevel;
		}
		set
		{
			AssertMutable();
			maxWorkLevel = value;
		}
	}
	[SavedProperty]
	public int _food
	{
		get
		{
			return food;
		}
		set
		{
			AssertMutable();
			food = value;
		}
	}
	
	
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


	public async void addfood(int amount,Creature c)
	{
		//音效
		//bool full = food==maxFood;
		food+=amount;
		int exfood = 0;
		// if(!full)  饱食度增加的特效文本，之后补上
		// 	AbstractDungeon.effectsQueue.add(new TextAboveCreatureEffect(this.hb.cX - this.animX, this.hb.cY, characterStrings.TEXT[2] + Integer.toString(amount), Settings.GREEN_TEXT_COLOR));
		if(food>maxFood)
		{
			exfood = food-maxFood;
			//长腿菌块逻辑
			// int damage = food-maxFood;
			// if(hasRelic(Liver_LongLegMushroom.ID)&&AbstractDungeon.getCurrRoom().phase == AbstractRoom.RoomPhase.COMBAT){
			// 	boolean isdamageself = false;
			// 	for (AbstractMonster mo : AbstractDungeon.getMonsters().monsters) {
			// 		if (!mo.isDeadOrEscaped()&&mo.currentHealth>0) {
			// 			AbstractDungeon.actionManager.addToBottom(new DamageAction(mo,new DamageInfo(this,damage, DamageInfo.DamageType.THORNS)));
			// 			isdamageself = true;
			// 		}
			// 	}
			// 	if(isdamageself){
			// 		AbstractDungeon.actionManager.addToBottom(new DamageAction(this,new DamageInfo(this,damage, DamageInfo.DamageType.THORNS)));
			// 	}
			// }
			
			//胖世界逻辑
			// if(hasPower(FatWorldBuff.POWER_ID)){
			// 	AbstractDungeon.actionManager.addToBottom(new ApplyPowerAction(this,this,new StrengthPower(this,getPower(FatWorldBuff.POWER_ID).amount),getPower(FatWorldBuff.POWER_ID).amount));
			// }
			if(c.HasPower<FatworldPower>())
				await PowerCmd.Apply<StrengthPower>(c, exfood, c, null);
		}
		if(food<0)
			food=0;
		food = Math.Min(food,maxFood);
	}
	public void setworklevel(int level)
	{
		if(level>maxWorkLevel)
			level=maxWorkLevel;
		if(level<0)
			level=0;
		workLevel=level;
		GD.Print($"【调试】设置完成，业力{workLevel}，是否可杀：{canbekill()}");
		
	}

	public void addworklevel(int level)
	{
		int result = workLevel+level;
		GD.Print($"【调试】当前业力：{workLevel}, 设置业力{result}");

		setworklevel(result);
	}
	
	public bool canbekill()
	{
		GD.Print($"【调试】Slugcat 受击！当前业力：{workLevel}, canbekill？{workLevel <= 0}");

		return workLevel <= 0;
	}

	public void reLive()
	{
		GD.Print($"【调试】当前业力：{workLevel}, 触发relive");
		this.addworklevel(-1);
	}
	
	
}
