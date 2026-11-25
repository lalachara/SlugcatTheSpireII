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
    
    /// <summary>
    /// Boss 战 BGM。原版会在进入本遭遇时用它替换本幕音乐（所以必须保留 CustomBgm，否则听到的是本幕音乐）。
    /// 风险是它不是 event:/music/... 的音乐事件，离开战斗时原版不一定能停掉；
    /// 由 CombatUiPatch.StopMushroomBossBgm 在战斗结束时兜底停止并恢复幕音乐。
    /// </summary>
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
#if STS2_0_113_0 || STS2_0_114_0
    /// <summary>兜底单例：只在 ModelDb 里查不到时才会用到（见下）。</summary>
    private static MegaCrit.Sts2.Core.Models.Dread.DominionModel? _dominionFallback;

    public override MegaCrit.Sts2.Core.Models.Dread.DominionModel? GetDominion()
    {
        // 统御是 ModelDb 里的规范单例：AbstractModel 的构造函数会把 ID 注册进 ModelDb，
        // 每次 new 都会 DuplicateModelException，所以必须从 ModelDb 取。
        try
        {
            return ModelDb.Dominion<MushroomDominion>();
        }
        catch (System.Exception)
        {
            // 理论上不会走到：模型在启动时就已被注册。万一查不到，也只创建一次并缓存。
            return _dominionFallback ??= new MushroomDominion();
        }
    }
#endif

    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters() => [
        (ModelDb.Monster<Mushroom>().ToMutable(), null)
    ];
}
