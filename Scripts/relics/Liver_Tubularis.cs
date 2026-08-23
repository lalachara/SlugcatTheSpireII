using System.Collections.Generic;
using System.Threading.Tasks;
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
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using Rainworld.Scripts;
using Rainworld.Scripts.Card.Liver.Attack;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.relics;
[RegisterRelic(typeof(Rainworld_Liver_RelicPool))]
public sealed class Liver_Tubularis : ModRelicTemplate
{
	public override RelicRarity Rarity => RelicRarity.Shop;
	public override RelicAssetProfile AssetProfile => new(
		IconPath: "res://Resource/Relics/xiguan.png",
		IconOutlinePath: "res://Resource/Relics/outline/xiguan.png",
		BigIconPath: "res://Resource/Relics/xiguan.png"
	);

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[]
	{
		HoverTipFactory.ForEnergy(this),
		HoverTipFactory.FromCard<Slimed>()
	};
	protected override IEnumerable<DynamicVar> CanonicalVars => new []{new EnergyVar(1)};


	public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, ICombatState combatState)
	{
		if (player == base.Owner && combatState.RoundNumber == 1)
		{
			Flash();
			List<CardModel> list = new List<CardModel>();
			for (int i = 0; i < 2; i++)
			{
				list.Add(combatState.CreateCard<Slimed>(base.Owner));
			}
			CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardsToCombat(list, PileType.Draw, Owner, CardPilePosition.Random));
			await Cmd.Wait(3f);
		}
	}

	public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
	{
		if (cardPlay.Card is Slimed)
		{
			await PlayerCmd.GainEnergy(1, Owner);
			if (Owner.Character is Slugcat)
			{
				await SlugcatData.addfood(Owner, 1);
			}
		}
	}
}