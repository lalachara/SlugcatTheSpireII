using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Runs.History;
using Rainworld.Scripts;
using Rainworld.Scripts.Card.Liver.Attack;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Relics.Visibility;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Utils;

namespace Rainworld.relics;

[RegisterRelic(typeof(SharedRelicPool))]
public sealed class Worklevelrepair : ModRelicTemplate,IModRelicVisibility
{
    public bool IsRelicVisible => false;
    public static readonly SavedAttachedState<Worklevelrepair, int> WorkLevel = new("WorkLevel", _ => 0);
    public static readonly SavedAttachedState<Worklevelrepair, int> MaxWorkLevel = new("MaxWorkLevel", _ => 0);
    public static readonly SavedAttachedState<Worklevelrepair, int> SleepFood = new("SleepFood", _ => 0);
    public static readonly SavedAttachedState<Worklevelrepair, int> Food = new("Food", _ => 0);
    public static readonly SavedAttachedState<Worklevelrepair, int> MaxFood = new("MaxFood", _ => 0);
    public static readonly SavedAttachedState<Worklevelrepair, bool> Chaosheng = new("Chaosheng", _ => false);

    

    public override RelicRarity Rarity => RelicRarity.Ancient;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromKeyword(RainworldKeywords.Worklevel) };

    public override async Task AfterObtained()
    {
        if (Owner.Character is Slugcat slugcat)
        {
            WorkLevel[this] = slugcat.workLevel;
            MaxWorkLevel[this] = slugcat.maxWorkLevel;
            SleepFood[this] = slugcat.sleepfood;
            Food[this] = slugcat.food;
            MaxFood[this] = slugcat.maxFood;
            Chaosheng[this] = false;
        }
        
        SlugcatData.Initialize(Owner);
        Initialize();


    }

    
    

    public void Initialize()
    {
        Player player = Owner;
        SlugcatResources.Data.Modify(player, d =>
        {
            d.WorkLevel = WorkLevel[this];
            d.MaxWorkLevel = MaxWorkLevel[this];
            d.Food = Food[this];
            d.MaxFood = MaxFood[this];
            d.SleepFood = SleepFood[this];
            d.IsInitialized = true;
            d.HaveChaoshengt = Chaosheng[this];
        });
        GD.Print($"Worklevelrepair Initialize {WorkLevel[this]} {MaxWorkLevel[this]} {SleepFood[this]} {Food[this]} {MaxFood[this]} {Chaosheng[this]}");

    }
    public void LoadData()
    {
        WorkLevel[this] = SlugcatData.GetWorkLevel(Owner);
        MaxWorkLevel[this] = SlugcatData.GetMaxWorkLevel(Owner);
        SleepFood[this] = SlugcatData.GetSleepFood(Owner);
        Food[this] = SlugcatData.GetFood(Owner);
        MaxFood[this] = SlugcatData.GetMaxFood(Owner);
        Chaosheng[this] = SlugcatData.GetChaosheng(Owner);
        
    }
    
    

}