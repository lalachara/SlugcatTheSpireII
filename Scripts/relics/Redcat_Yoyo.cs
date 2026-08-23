using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Models.Relics;
using Rainworld.Scripts;
using Rainworld.Scripts.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.relics;
[RegisterRelic(typeof(SharedRelicPool))]
public sealed class Redcat_Yoyo : ModRelicTemplate
{
	public override RelicRarity Rarity => RelicRarity.Uncommon;
	public override RelicAssetProfile AssetProfile => new(
		IconPath: "res://Resource/Relics/Yoyo.png",
		IconOutlinePath: "res://Resource/Relics/outline/yoyo.png",
		BigIconPath: "res://Resource/Relics/Yoyo.png"
	);

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromKeyword(RainworldKeywords.Backtohand) };

	private bool _hasPlayedFirstCardThisTurn;

	public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
	{
		if (side == CombatSide.Player)
			_hasPlayedFirstCardThisTurn = false;
		await base.AfterSideTurnStart(side, participants, combatState);
	}

	
	public override CardLocation ModifyCardPlayResultLocation(
		CardModel card,
		bool isAutoPlay,
		ResourceInfo resources,
		CardLocation location)
	{
		if (card.Owner.Creature != Owner.Creature || _hasPlayedFirstCardThisTurn || !(card.Type == CardType.Skill || card.Type == CardType.Attack) || card.Keywords.Contains(CardKeyword.Exhaust))
			return location;


		if (location.pileType == PileType.Discard)
		{
			location.pileType = PileType.Hand;
			location.position = CardPilePosition.Top;
		}
		return location;
	}

	public override async Task AfterModifyingCardPlayResultLocation(
		CardModel card,
		CardLocation location)
	{
		_hasPlayedFirstCardThisTurn = true;
		await Tools.BackToHandAciton(Owner, card);
		if (card.Owner.Creature != Owner.Creature)
			return;
		Flash();
	}
}