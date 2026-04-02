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
public sealed class Liver_Fireblood : CustomRelicModel
{
	public override RelicRarity Rarity => RelicRarity.Rare;
	// 小图标
	public override string PackedIconPath =>  $"res://Resource/Relics/juice.png";
	// 轮廓图标
	protected override string PackedIconOutlinePath =>  $"res://Resource/Relics/outline/juice.png";
	// 大图标
	protected override string BigIconPath => $"res://Resource/Relics/juice.png";

	
}
