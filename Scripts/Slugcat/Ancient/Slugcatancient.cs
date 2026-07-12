﻿using System.Linq;
using Godot;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Runs;
using Rainworld.relics;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Utils;

namespace Rainworld.Scripts.Ancient;
[RegisterSharedAncient]
public class Slugcatancient: ModAncientEventTemplate
{
    // 选项按钮颜色
    public override Color ButtonColor => new("281509", 0.9f);
    // 对话框颜色
    public override Color DialogueColor => new("281509");
    // 自定义场景的路径
    public override EventAssetProfile AssetProfile => new(
        BackgroundScenePath: "res://Godot/Ancient/testAncient.tscn"
    );
    
    

    // 对于图片，只要是godot支持的格式都可以，例如png,jpg,svg等等
    // 自定义地图图标和轮廓的路径
    public override AncientEventPresentationAssetProfile AncientPresentationAssetProfile => new(
        MapIconPath: "res://Godot/Ancient/mapicon.png",
        MapIconOutlinePath: "res://Godot/Ancient/mapiconoutline.png",
        RunHistoryIconPath: "res://Godot/Ancient/runhistory.png",
        RunHistoryIconOutlinePath: "res://Godot/Ancient/runhistoryoutline.png"
    );

    // 判断事件触发者（Owner）是否为 Slugcat 角色
    private bool IsSlugcatOwner => Owner?.Character is Slugcat;

    // 固定池一和二
    private IReadOnlyList<EventOption> Pool1 => [
        CreateModRelicOption<Partner>(),
        CreateModRelicOption<Battery>(),
        CreateModRelicOption<Moonrock>()

    ];
    private IReadOnlyList<EventOption> Pool1Slugcat => [
        CreateModRelicOption<Partner>(),
        CreateModRelicOption<Battery>(),
        CreateModRelicOption<Mushroom>()

    ];
    private IReadOnlyList<EventOption> Pool2 => [
        CreateModRelicOption<Liver_Whiteskin>(),
        CreateModRelicOption<Future>(),
        CreateModRelicOption<Anthem>()

    ];
    private IReadOnlyList<EventOption> Pool2Slugcat => [
        CreateModRelicOption<Liver_Whiteskin>(),
        CreateModRelicOption<Liver_Sleeppotion>(),
        CreateModRelicOption<Future>(),
        CreateModRelicOption<Anthem>()


    ];

    // 带权重池三。权重越大越有机会生成。当然你也可以写自定义的列表生成函数
    private IReadOnlyList<EventOption> Pool3 => [
        CreateModRelicOption<Touchallow>(),


    ];
    private IReadOnlyList<EventOption> Pool3Slugcat => [
        CreateModRelicOption<Forever>(),
        CreateModRelicOption<Talksign>(),
        CreateModRelicOption<Redmushroom>(),
    ];
    // private WeightedList<EventOption> Pool3Slugcat => new()
    // {
    //     CreateModRelicOption<Forever>(),
    //     CreateModRelicOption<Talksign>()
    //
    // };

    // 所有可能的选项
    public override IEnumerable<EventOption> AllPossibleOptions => [.. Pool1, .. Pool2, .. Pool3, .. Pool1Slugcat, .. Pool2Slugcat, .. Pool3Slugcat];

    // 生成选项
    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        if (!IsSlugcatOwner)
            return [ Rng.NextItem(Pool1)!,
                Rng.NextItem(Pool2)!,
                Rng.NextItem(Pool3)!,
            ];
        return
        [
            Rng.NextItem(Pool1Slugcat)!,
            Rng.NextItem(Pool2Slugcat)!,
            Rng.NextItem(Pool3Slugcat)!,
        ];
    }

    public override bool IsAllowed(IRunState runState)
    {
        return runState.Players.Any(p => p.Character is Slugcat);
    }

}