using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Models.Encounters;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.Scripts.Monster;

[RegisterActEncounter(typeof(Glory))]
public class FirecentElite : ModEncounterTemplate
{
    public override IEnumerable<MonsterModel> AllPossibleMonsters => [ModelDb.Monster<Firecent>()];

    public override RoomType RoomType => RoomType.Elite;
    protected override bool UseActCombatBackground => true;
    
    //public override string CustomBgm => "event:/Rainworld/sfx/mushroom";

    // public override string? CustomBackgroundScenePath =>
    //     "res://Godot/Encouters/testAncient.tscn";

    //public override string BossNodePath => "res://Resource/Monster/Map";
    
    // public override EncounterAssetProfile AssetProfile => new(
    //     MapNodeAssetPaths: ["res://Resource/Monster/mushroomicon.png"],
    //     RunHistoryIconPath: "res://Resource/Monster/mushroomicon.png",
    //     RunHistoryIconOutlinePath: "res://Resource/Monster/mushroomicon.png"
    // );

    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters() => [
        (ModelDb.Monster<Firecent>().ToMutable(), null)
    ];
}
