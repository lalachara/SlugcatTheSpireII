using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rooms;
using Rainworld.Scripts;
using Rainworld.Scripts.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.relics;
[RegisterRelic(typeof(Rainworld_Redcat_RelicPool))]
public sealed class Redcat_Book2 : ModRelicTemplate
{
	private const int HuntsignAmount = 2;

	public override RelicRarity Rarity => RelicRarity.Starter;
	public override RelicAssetProfile AssetProfile => new(
		IconPath: "res://Resource/Relics/Menu2.png",
		IconOutlinePath: "res://Resource/Relics/outline/Menu.png",
		BigIconPath: "res://Resource/Relics/Menu2.png"
	);

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromPower<HuntsignPower>() };

	public override int DisplayAmount => _killCount;

	private int _killCount;

	public override async Task AfterObtained()
	{
		if (Owner.Character is not Slugcat)
			return;
		SlugcatData.Initialize(Owner);
		Log.Info($"{Owner.Creature.Name} 初始化业力，{SlugcatData.GetWorkLevel(Owner)}");
	}

	public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, ICombatState combatState)
	{
		if (player == base.Owner && combatState.RoundNumber == 1)
		{
			foreach (var enemy in combatState.HittableEnemies)
				await PowerCmd.Apply<HuntsignPower>(choiceContext, enemy, HuntsignAmount, Owner.Creature, null);
		}
	}

	public override async Task AfterCombatVictory(CombatRoom room)
	{
		if (Owner.Character is Slugcat && room.RoomType == RoomType.Elite)
		{
			await SlugcatData.addworklevel(Owner, 1);
			Flash();

		}

	}
}