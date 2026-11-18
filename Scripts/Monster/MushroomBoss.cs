using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Models.Encounters;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.Scripts.Monster;

[RegisterActEncounter(typeof(Glory))]
public class MushroomBoss : ModEncounterTemplate
{
    public override IEnumerable<MonsterModel> AllPossibleMonsters => [ModelDb.Monster<Mushroom>()];

    public override RoomType RoomType => RoomType.Boss;
    protected override bool UseActCombatBackground => true;
    
    public override string CustomBgm => "event:/Rainworld/sfx/mushroom";

    // public override string? CustomBackgroundScenePath =>
    //     "res://Godot/Encouters/testAncient.tscn";

    public override string BossNodePath => "res://Resource/Monster/Map";
    
    public override EncounterAssetProfile AssetProfile => new(
        MapNodeAssetPaths: ["res://Resource/Monster/mushroomicon.png"],
        RunHistoryIconPath: "res://Resource/Monster/mushroomicon.png",
        RunHistoryIconOutlinePath: "res://Resource/Monster/mushroomicon.png"
    );

    /// <summary>恐惧模式的统御：道中普通 / 精英敌人最大生命 +20%（见 MushroomDominion）。</summary>
#if STS2_0_113_0
    public override MegaCrit.Sts2.Core.Models.Dread.DominionModel? GetDominion()
        => new MushroomDominion();
#endif

    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters() => [
        (ModelDb.Monster<Mushroom>().ToMutable(), null)
    ];
}
