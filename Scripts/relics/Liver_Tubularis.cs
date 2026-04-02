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
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using Rainworld.Scripts;
using Rainworld.Scripts.Card.Liver.Attack;

namespace Rainworld.relics;
[Pool(typeof(Rainworld_Liver_RelicPool))]
public sealed class Liver_Tubularis : CustomRelicModel
{
	public override RelicRarity Rarity => RelicRarity.Shop;
	// 小图标
	public override string PackedIconPath =>  $"res://Resource/Relics/xiguan.png";
	// 轮廓图标
	protected override string PackedIconOutlinePath =>  $"res://Resource/Relics/outline/xiguan.png";
	// 大图标
	protected override string BigIconPath => $"res://Resource/Relics/xiguan.png";

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new[]
	{
		HoverTipFactory.ForEnergy(this),
		HoverTipFactory.FromCard<Slimed>()
	};
	protected override IEnumerable<DynamicVar> CanonicalVars => new []{new EnergyVar(1)};


	public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, CombatState combatState)
	{
		if (player == base.Owner && combatState.RoundNumber == 1)
		{
			Flash();
			List<CardModel> list = new List<CardModel>();
			for (int i = 0; i < 2; i++)
			{
				list.Add(combatState.CreateCard<Slimed>(base.Owner));
			}
			CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardsToCombat(list, PileType.Draw, addedByPlayer: true, CardPilePosition.Random));
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
				SlugcatField.playerdata?.addfood(1);
			}
		}
	}
}
