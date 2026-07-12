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
public sealed class Liver_Sleeppotion : ModRelicTemplate
{
	public override RelicRarity Rarity => RelicRarity.Ancient;
	public override RelicAssetProfile AssetProfile => new(
		IconPath: "res://Resource/Relics/SleepPotions.png",
		IconOutlinePath: "res://Resource/Relics/outline/SleepPotions.png",
		BigIconPath: "res://Resource/Relics/SleepPotions.png"
	);
	protected override IEnumerable<DynamicVar> CanonicalVars => new []{new EnergyVar(2)};

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[]
	{
		HoverTipFactory.ForEnergy(this),
	};
}