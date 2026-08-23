using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using Rainworld.Scripts;
using Rainworld.Scripts.Card.Liver.Attack;
using Rainworld.Scripts.Powers;
using STS2RitsuLib;
using STS2RitsuLib.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.relics;
[RegisterRelic(typeof(Rainworld_River_RelicPool))]
public sealed class River_Waterball2 : ModRelicTemplate
{
	public override RelicRarity Rarity => RelicRarity.Starter;
	
	public override RelicAssetProfile AssetProfile => new(
		IconPath: "res://Resource/Relics/waterball2.png",
		IconOutlinePath: "res://Resource/Relics/outline/waterball2.png",
		BigIconPath: "res://Resource/Relics/waterball2.png"
	);
	protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromPower<AdroitPower>(), HoverTipFactory.FromPower<AdroitcapPower>() };

	public override bool ShowCounter => true;
	public override int DisplayAmount => _consumeCounter % 10;

	private int _consumeCounter;
	
	
	public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, ICombatState combatState)
	{
		if (player == base.Owner && combatState.RoundNumber == 1)
		{
			 await PowerCmd.Apply<AdroitcapPower>(choiceContext, Owner.Creature, 3, Owner.Creature, null);
		}
		
	}
	
	public override async Task AfterCombatVictory(CombatRoom room)
	{
		_consumeCounter = 0;
		InvokeDisplayAmountChanged();

		await base.AfterCombatVictory(room);
	}

	public void OnAdroitConsumed(int amount)
	{
		_consumeCounter += amount;
		while (_consumeCounter >= 10)
		{
			_consumeCounter -= 10;
			TaskHelper.RunSafely(ApplyAdroitCap());
		}
		this.InvokeDisplayAmountChanged();

	}

	private async Task ApplyAdroitCap()
	{
		await PowerCmd.Apply<AdroitcapPower>(new ThrowingPlayerChoiceContext(), Owner.Creature, 1, Owner.Creature, null);
	}
	
}