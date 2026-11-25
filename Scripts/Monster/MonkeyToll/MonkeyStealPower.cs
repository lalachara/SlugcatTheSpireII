using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.Scripts.Monster.MonkeyToll;

/// <summary>拾荒者偷什么（对应塔1 StealPower 的 TYPE_XXX）。</summary>
public enum MonkeyTollKind
{
	Gold,
	Potion,
	Card,
	Relic,
}

/// <summary>
/// 拾荒者的自动拾取能力（对应塔1 powers/monsters/StealPower）：
/// 攻击命中玩家时无条件偷东西（被完全格挡也照偷）；
/// 拾荒者死亡时把赃物折算成奖励交给 <see cref="MonkeyTollLoot" />（被偷的东西还给被偷的人），
/// 并按种类发放击杀补给（金币固定 20 / 随机药水 / 三选一卡牌奖励 / 随机遗物）。
/// </summary>
public abstract class MonkeyStealPower : ModPowerTemplate
{
	/// <summary>能力图标文件名：Resource/Powers/monster 下四种拾荒者各一张。</summary>
	protected abstract string IconFileName { get; }

	public override PowerAssetProfile AssetProfile => new(
		IconPath: $"res://Resource/Powers/monster/{IconFileName}",
		BigIconPath: $"res://Resource/Powers/monster/{IconFileName}"
	);

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

	/// <summary>这只拾荒者偷什么（由子类决定，4 个子类各注册一种）。</summary>
	protected abstract MonkeyTollKind Kind { get; }

	/// <summary>「基础描述 + 已偷取」的合成模板（powers 表里的公共键，见各语言 powers.json）。</summary>
	private const string ComposedLocKey = "SLUGCAT_THE_SPIRE_II_MONKEY_STEAL.composed";

	/// <summary>「已偷取：」前缀（自带换行）。</summary>
	private const string StolenPrefixLocKey = "SLUGCAT_THE_SPIRE_II_MONKEY_STEAL.stolenPrefix";

	/// <summary>金币专用的「已偷取 N 金币」模板（自带前缀，省一次拼接）。</summary>
	private const string StolenGoldLocKey = "SLUGCAT_THE_SPIRE_II_MONKEY_STEAL.stolenGold";


	/// <summary>
	/// 悬停描述：本地化里的 <c>description</c> 原文，偷到东西后再追加一行「已偷取：xxx」。
	/// 本地化缺合成模板时退回原描述，保证不会因为打包漏资源而变成空描述。
	/// </summary>
	public override LocString Description
	{
		get
		{
			string baseKey = $"{Id.Entry}.description";
			var composed = new LocString("powers", ComposedLocKey);
			if (!composed.Exists())
				return new LocString("powers", baseKey);

			composed.Add("Base", new LocString("powers", baseKey).GetRawText());

			// 只显示「本地玩家」被偷的东西：多人模式下别人的赃物不显示在自己的悬停里
			string stolen = string.Empty;
			if (LocalContext.NetId is { } netId && State.StolenByPlayer.TryGetValue(netId, out string? line))
				stolen = line;
			composed.Add("Stolen", stolen);
			return composed;
		}
	}

	/// <summary>记录「已偷取：{物品名}」（药水 / 卡牌 / 遗物共用同一套前缀）。</summary>
	private void SetStolenItem(Player player, string itemName)
	{
		var prefix = new LocString("powers", StolenPrefixLocKey);
		if (!prefix.Exists())
			return;
		State.StolenByPlayer[player.NetId] = prefix.GetFormattedText() + itemName;
	}

	/// <summary>记录「已偷取 N 金币」（金币模板自带「已偷取：」和颜色标签）。</summary>
	private void SetStolenGold(Player player, int amount)
	{
		var line = new LocString("powers", StolenGoldLocKey);
		if (!line.Exists())
			return;
		line.Add("Amount", amount);
		State.StolenByPlayer[player.NetId] = line.GetFormattedText();
	}

	/// <summary>偷卡稀有度优先级（塔1 StealAction.CARD_ORDER：RARE → UNCOMMON → COMMON → BASIC → SPECIAL → CURSE）。</summary>
	private static readonly CardRarity[] StealOrder =
	[
		CardRarity.Rare,
		CardRarity.Uncommon,
		CardRarity.Common,
		CardRarity.Basic,
		CardRarity.Event,
		CardRarity.Ancient,
		CardRarity.Quest,
		CardRarity.Curse,
	];

	/// <summary>
	/// 每场战斗的可变状态。必须放在 InternalData 里：PowerModel 的 mutable 克隆是
	/// MemberwiseClone（浅拷贝），普通字段里的字典会与 canonical 实例共享，
	/// 导致上一场偷到的东西 / 赃物袋残留到下一场（客户端悬停显示旧东西、赃物重复发放）。
	/// InitInternalData 每个 mutable 实例各建一份，战斗结束随实例一起丢弃。
	/// </summary>
	private sealed class Data
	{
		/// <summary>每个玩家各自被偷的东西（NetId → 展示文本）。</summary>
		public readonly Dictionary<ulong, string> StolenByPlayer = [];

		/// <summary>赃物袋：按被偷玩家分开记账，死亡时各还各的。</summary>
		public readonly Dictionary<Player, StolenPouch> Pouches = [];
	}

	protected override object InitInternalData() => new Data();

	private Data State => GetInternalData<Data>();

	/// <summary>击杀偷金拾荒者的额外金币奖励（不含被它偷走的金币）。</summary>
	private const int KillGoldReward = 20;

	/// <summary>赃物袋：按被偷玩家分开记账，死亡时各还各的。</summary>
	private sealed class StolenPouch
	{
		public decimal Gold;
		public readonly List<PotionModel> Potions = [];
		public readonly List<CardModel> Cards = [];
		public readonly List<RelicModel> Relics = [];
	}

	

	private StolenPouch GetPouch(Player player)
	{
		if (!State.Pouches.TryGetValue(player, out StolenPouch? pouch))
		{
			pouch = new StolenPouch();
			State.Pouches[player] = pouch;
		}
		return pouch;
	}

	// ———— 偷窃（塔1 StealPower.onAttack / StealAction）————
	public override async Task AfterDamageGiven(
		PlayerChoiceContext choiceContext,
		Creature? dealer,
		DamageResult result,
		ValueProp props,
		Creature target,
		CardModel? cardSource)
	{
		if (dealer != Owner || !target.IsPlayer || !props.IsPoweredAttack())
			return;
		Player? player = target.Player;
		if (player == null)
			return;

		switch (Kind)
		{
			case MonkeyTollKind.Gold:
				await StealGold(player);
				break;
			case MonkeyTollKind.Potion:
				await StealPotion(player);
				break;
			case MonkeyTollKind.Card:
				await StealCard(player);
				break;
			case MonkeyTollKind.Relic:
				await StealRelic(player);
				break;
		}
	}

	/// <summary>偷金：偷走 min(能力数值, 当前金币)（塔1 StealAction 的金币分支）。</summary>
	private async Task StealGold(Player player)
	{
		int amount = Math.Min((int)Amount, player.Gold);
		if (amount <= 0)
			return;
		GetPouch(player).Gold += amount;
		await PlayerCmd.LoseGold(amount, player, GoldLossType.Stolen);
		SetStolenGold(player, amount);
		Flash();
	}

	/// <summary>偷药：随机拿走一瓶（塔1 StealAction 的 getRandomPotion + removePotion）。</summary>
	private async Task StealPotion(Player player)
	{
		List<PotionModel> potions = player.Potions.ToList();
		if (potions.Count == 0)
			return;
		PotionModel potion = player.RunState.Rng.CombatCardGeneration.NextItem(potions) ?? potions[0];
		await PotionCmd.Discard(potion);
		GetPouch(player).Potions.Add(potion);
		SetStolenItem(player, potion.Title.GetFormattedText());
		Flash();
	}

	/// <summary>偷卡：按稀有度优先级从牌组里挑一张永久移除，战斗中的同名副本一并移除。</summary>
	private async Task StealCard(Player player)
	{
		CardModel? card = PickCardToSteal(player);
		if (card == null)
			return;

		// 从牌组永久移除（塔1 masterDeck.removeCard）
		await CardPileCmd.RemoveFromDeck(card, showPreview: true);
		// 手牌 / 抽牌堆 / 弃牌堆里的同一张卡（战斗副本）也一起消失
		CardModel? copy = CardPile.GetCards(player, PileType.Hand, PileType.Draw, PileType.Discard)
			.FirstOrDefault(c => c.DeckVersion == card);
		if (copy != null)
			await CardPileCmd.RemoveFromCombat(copy);

		GetPouch(player).Cards.Add(card);
		// CardModel.Title 已经是格式化好的名字，带升级后缀（猫猫拳+）
		SetStolenItem(player, card.Title);
		Flash();
	}

	/// <summary>偷遗物：只从普通/罕见/稀有池里随机拿一件（塔1 StealAction 的遗物分支）。</summary>
	private async Task StealRelic(Player player)
	{
		List<RelicModel> pool = player.Relics
			.Where(r => r.Rarity is RelicRarity.Common or RelicRarity.Uncommon or RelicRarity.Rare)
			.ToList();
		if (pool.Count == 0)
			return;
		RelicModel relic = player.RunState.Rng.CombatCardGeneration.NextItem(pool) ?? pool[0];
		await RelicCmd.Remove(relic);
		GetPouch(player).Relics.Add(relic);
		SetStolenItem(player, relic.Title.GetFormattedText());
		Flash();
	}

	/// <summary>按稀有度优先级挑一张牌，档内随机（塔1 StealAction.CARD_ORDER）。</summary>
	private CardModel? PickCardToSteal(Player player)
	{
		foreach (CardRarity rarity in StealOrder)
		{
			List<CardModel> candidates = [];
			foreach (CardModel card in player.Deck.Cards)
			{
				if (card.Rarity == rarity)
					candidates.Add(card);
			}
			if (candidates.Count > 0)
				return player.RunState.Rng.CombatCardGeneration.NextItem(candidates) ?? candidates[0];
		}
		return null;
	}

	// ———— 返还与击杀补给（塔1 StealPower.onDeath + addKillReward）————
	public override Task BeforeDeath(Creature target)
	{
		if (Owner != target)
			return Task.CompletedTask;
		if (CombatState?.RunState is not { } runState)
			return Task.CompletedTask;

		// 1）被偷的东西还给被偷的人
		foreach ((Player player, StolenPouch pouch) in State.Pouches)
		{
			if (pouch.Gold > 0)
				MonkeyTollLoot.Add(player, new GoldReward((int)pouch.Gold, player, wasGoldStolenBack: true));

			foreach (PotionModel potion in pouch.Potions)
				MonkeyTollLoot.Add(player, new PotionReward(potion, player));

			foreach (CardModel card in pouch.Cards)
			{
				// 与 SwipePower 一致：先把卡恢复到 RunState，再以「特殊卡奖励」的形式归还
				runState.AddCard(card, player);
				var special = new SpecialCardReward(card, player);
				special.SetCustomDescriptionEncounterSource(ModelDb.GetId(typeof(MonkeyTollEncounter)));
				MonkeyTollLoot.Add(player, special);
			}

			foreach (RelicModel relic in pouch.Relics)
				MonkeyTollLoot.Add(player, new RelicReward(relic, player));
		}
		State.Pouches.Clear();
		State.StolenByPlayer.Clear();

		// 2）击杀补给：拾荒者被打死时给所有玩家的战利品
		foreach (Player player in runState.Players)
			AddKillReward(player, runState);

		return Task.CompletedTask;
	}

	private void AddKillReward(Player player, IRunState runState)
	{
		switch (Kind)
		{
			case MonkeyTollKind.Gold:
						{
				// 击杀偷金拾荒者的「额外奖励」：固定 20 金币。被它偷走的金币属于返还部分，
				// 由赃物袋另外还给被偷的人，所以「偷后打死」拿到的是 20 + 被偷的金币。
				MonkeyTollLoot.Add(player, new GoldReward(KillGoldReward, player));
				break;
			}
			case MonkeyTollKind.Potion:
				MonkeyTollLoot.Add(player, new PotionReward(player));
				break;
			case MonkeyTollKind.Card:
			{
				// 事件房间不能直接用 ForRoom(RoomType.Event)，借用普通战斗的卡牌奖励
				CardReward reward = new(CardCreationOptions.ForRoom(player, RoomType.Monster), 3, player, null);
				reward.Populate();
				MonkeyTollLoot.Add(player, reward);
				break;
			}
			case MonkeyTollKind.Relic:
				MonkeyTollLoot.Add(player, new RelicReward(player).SetRng(runState.Rng.TreasureRoomRelics));
				break;
		}
	}
}

// ———— 四种拾荒者各注册一个偷窃能力（Type 即状态，规避自定义字段的序列化问题）————

/// <summary>拾荒者（金）：攻击时偷金币，死亡时归还。</summary>
[RegisterPower]
public sealed class MonkeyStealGoldPower : MonkeyStealPower
{
	protected override MonkeyTollKind Kind => MonkeyTollKind.Gold;

	protected override string IconFileName => "monkeygold.png";
}

/// <summary>拾荒者（药）：攻击时偷一瓶药水，死亡时归还。</summary>
[RegisterPower]
public sealed class MonkeyStealPotionPower : MonkeyStealPower
{
	protected override MonkeyTollKind Kind => MonkeyTollKind.Potion;

	protected override string IconFileName => "monkeypotion.png";
}

/// <summary>拾荒者（卡）：攻击时按稀有度偷一张牌，死亡时归还。</summary>
[RegisterPower]
public sealed class MonkeyStealCardPower : MonkeyStealPower
{
	protected override MonkeyTollKind Kind => MonkeyTollKind.Card;

	protected override string IconFileName => "monkeycard.png";
}

/// <summary>拾荒者（遗物）：攻击时偷一件遗物，死亡时归还。</summary>
[RegisterPower]
public sealed class MonkeyStealRelicPower : MonkeyStealPower
{
	protected override MonkeyTollKind Kind => MonkeyTollKind.Relic;

	protected override string IconFileName => "monkeyrelic.png";
}
