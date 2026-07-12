using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Rainworld.Scripts;
using Rainworld.Scripts.Card.Liver.Attack;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.relics;

// TODO: 确认遗物池，当前使用 Liver 池
// 添加这个 using（第 11 行附近）
using MegaCrit.Sts2.Core.Models.RelicPools;

// 修改 RegisterRelic（第 21 行）
[RegisterRelic(typeof(SharedRelicPool))]
public sealed class Mushroom : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Ancient;
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] {  HoverTipFactory.FromKeyword(RainworldKeywords.Food)};

    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://Resource/Relics/mushroom.png",
        IconOutlinePath: "res://Resource/Relics/outline/mushroom.png",
        BigIconPath: "res://Resource/Relics/mushroom.png"
    );
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new EnergyVar(1)
    ];

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (Owner.Character is not Slugcat||SlugcatData.GetFood(Owner)!=SlugcatData.GetMaxFood(Owner))
            return;
        await PlayerCmd.GainEnergy(1, Owner);
    }
    
    
}