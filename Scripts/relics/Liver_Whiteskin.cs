using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using Rainworld.Scripts;
using Rainworld.Scripts.Card.Liver.Attack;

namespace Rainworld.relics;
[Pool(typeof(Rainworld_Liver_RelicPool))]
public sealed class Liver_Whiteskin : CustomRelicModel
{
	public override RelicRarity Rarity => RelicRarity.Rare;
	// 小图标
	public override string PackedIconPath =>  $"res://Resource/Relics/whiteskin.png";
	// 轮廓图标
	protected override string PackedIconOutlinePath =>  $"res://Resource/Relics/outline/whiteskin.png";
	// 大图标
	protected override string BigIconPath => $"res://Resource/Relics/whiteskin.png";

	private bool relicused = false;
	private bool atkused = false;

	public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, CombatState combatState)
	{
		if (player == base.Owner && combatState.RoundNumber == 1)
		{
			relicused = false;
			base.Status = RelicStatus.Active;

		}
		atkused = false;

	}
	
	public override Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
	{
		if (base.Owner != cardPlay.Card.Owner)
		{
			return Task.CompletedTask;
		}
		if (!CombatManager.Instance.IsInProgress)
		{
			return Task.CompletedTask;
		}
		if (cardPlay.Card.Type != CardType.Attack)
		{
			return Task.CompletedTask;
		}
		if(relicused||atkused)
			return Task.CompletedTask;

		atkused = true;
		base.Status = RelicStatus.Normal;
		return Task.CompletedTask;
	}
	
	public override async Task BeforeTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
	{
		if (!atkused&&!relicused)
		{
			relicused = true;
			Flash();
			base.Status = RelicStatus.Disabled;
			await PowerCmd.Apply<IntangiblePower>(Owner.Creature, 1, Owner.Creature, null);
		}
	}


}
