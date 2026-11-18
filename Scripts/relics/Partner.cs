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
using Rainworld.Scripts.Card.Colorless;
using Rainworld.Scripts.Card.Liver.Attack;
using STS2RitsuLib;
using STS2RitsuLib.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.relics;
[RegisterRelic(typeof(SharedRelicPool))]
public sealed class Partner : ModRelicTemplate
{
	public override RelicRarity Rarity => RelicRarity.Ancient;
	public override RelicAssetProfile AssetProfile => new(
		IconPath: "res://Resource/Relics/Partner.png",
		IconOutlinePath: "res://Resource/Relics/outline/Partner.png",
		BigIconPath: "res://Resource/Relics/Partner.png"
	);
	protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromCard<Rainworld_Turtal>() };
	protected override IEnumerable<DynamicVar> CanonicalVars =>
	[
		new EnergyVar(1)
	];
	
	public override Decimal ModifyMaxEnergy(Player player, Decimal amount)
	{
		return player != this.Owner ? amount : amount + (Decimal) this.DynamicVars.Energy.IntValue;
	}
	public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, ICombatState combatState)
	{
		if (player == base.Owner && combatState.RoundNumber == 1)
		{
			Flash();
			List<CardModel> list = new List<CardModel>();
			list.Add(combatState.CreateCard<Rainworld_Turtal>(base.Owner));
			CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardsToCombat(list, PileType.Draw, Owner, CardPilePosition.Random));
		}
	}

	
	
	

	
	
}