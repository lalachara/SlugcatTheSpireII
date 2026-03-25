using Rainworld.Resource.Card;
using Godot.Bridge;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Models.Relics;
using Rainworld.relics;
using Rainworld.Scripts.Card.Liver;
using Rainworld.Scripts.Card.Liver.Attack;
using Rainworld.Scripts.Card.Liver.Skills;
using SlugcatTheSpireII.Scripts.patches;

namespace Rainworld.Scripts;

// 必须要加的属性，用于注册Mod。字符串和初始化函数命名一致。
[ModInitializer("Init")]
public class Entry
{
    // 打patch（即修改游戏代码的功能）用
    private static Harmony? _harmony;

    // 初始化函数
    public static void Init()
    {
        // 传入参数随意，只要不和其他人撞车即可
        _harmony = new Harmony("sts2.llc.rainworld");
        _harmony.PatchAll();
        Log.Debug("Mod initialized!");
        ScriptManagerBridge.LookupScriptsInAssembly(typeof(Entry).Assembly);
        InitCard();
        InitRelics();
        //InitPatch();
        
    }
    public static void InitCard()
    {      
        ModHelper.AddModelToPool<Rainworld_Liver_CardPool, Rainworld_Liver_Defend>();
        ModHelper.AddModelToPool<Rainworld_Liver_CardPool, Rainworld_Liver_Spear>();
        ModHelper.AddModelToPool<Rainworld_Liver_CardPool, Rainworld_Liver_Strike>();
        ModHelper.AddModelToPool<Rainworld_Liver_CardPool, Rainworld_Liver_Rock>();
        ModHelper.AddModelToPool<Rainworld_Liver_CardPool, Rainworld_Liver_Backflip>();
        ModHelper.AddModelToPool<Rainworld_Liver_CardPool, Rainworld_Liver_Fruit>();
        ModHelper.AddModelToPool<Rainworld_Liver_CardPool, Rainworld_Liver_Worry>();
        ModHelper.AddModelToPool<Rainworld_Liver_CardPool, Rainworld_Liver_Spearstab>();
        
        
        
        ModHelper.AddModelToPool<Rainworld_Liver_CardPool, Rainworld_Liver_Bomb>();
        ModHelper.AddModelToPool<Rainworld_Liver_CardPool, Rainworld_Liver_Speardouble>();
        ModHelper.AddModelToPool<Rainworld_Liver_CardPool, Rainworld_Liver_Spearelec>();
        ModHelper.AddModelToPool<Rainworld_Liver_CardPool, Rainworld_Liver_Spearskip>();
        ModHelper.AddModelToPool<Rainworld_Liver_CardPool, Rainworld_Liver_Spearthroat>();
        
        ModHelper.AddModelToPool<Rainworld_Liver_CardPool, Rainworld_Liver_Jumphit>();
        ModHelper.AddModelToPool<Rainworld_Liver_CardPool, Rainworld_Liver_Spearstabhead>();
        ModHelper.AddModelToPool<Rainworld_Liver_CardPool, Rainworld_Liver_Raid>();
        ModHelper.AddModelToPool<Rainworld_Liver_CardPool, Rainworld_Liver_Superslide>();
        ModHelper.AddModelToPool<Rainworld_Liver_CardPool, Rainworld_Liver_Chase>();
        
        ModHelper.AddModelToPool<Rainworld_Liver_CardPool, Rainworld_Liver_Bigjump>();
        ModHelper.AddModelToPool<Rainworld_Liver_CardPool, Rainworld_Liver_Pearl>();
        ModHelper.AddModelToPool<Rainworld_Liver_CardPool, Rainworld_Liver_Lovemoney>();
        ModHelper.AddModelToPool<Rainworld_Liver_CardPool, Rainworld_Liver_Hunter>();
        ModHelper.AddModelToPool<Rainworld_Liver_CardPool, Rainworld_Liver_Tearwound>();

        ModHelper.AddModelToPool<Rainworld_Liver_CardPool, Rainworld_Liver_Thinkless>();
        ModHelper.AddModelToPool<Rainworld_Liver_CardPool, Rainworld_Liver_Threeagree>();
        ModHelper.AddModelToPool<Rainworld_Liver_CardPool, Rainworld_Liver_Rolling>();
        ModHelper.AddModelToPool<Rainworld_Liver_CardPool, Rainworld_Liver_Avenger>();
        ModHelper.AddModelToPool<Rainworld_Liver_CardPool, Rainworld_Liver_Honeycomb>();

        ModHelper.AddModelToPool<Rainworld_Liver_CardPool, Rainworld_Liver_Keepeye>();
        ModHelper.AddModelToPool<Rainworld_Liver_CardPool, Rainworld_Liver_Wawawa>();
        ModHelper.AddModelToPool<Rainworld_Liver_CardPool, Rainworld_Liver_Ready>();
        ModHelper.AddModelToPool<Rainworld_Liver_CardPool, Rainworld_Liver_Prepare>();
        
        ModHelper.AddModelToPool<Rainworld_Liver_CardPool, Rainworld_Liver_Mdbaole>();
        ModHelper.AddModelToPool<Rainworld_Liver_CardPool, Rainworld_Liver_Acmedefend>();
        ModHelper.AddModelToPool<Rainworld_Liver_CardPool, Rainworld_Liver_Warmup>();
        ModHelper.AddModelToPool<Rainworld_Liver_CardPool, Rainworld_Liver_Alert>();
        ModHelper.AddModelToPool<Rainworld_Liver_CardPool, Rainworld_Liver_Tread>();
        
        ModHelper.AddModelToPool<Rainworld_Liver_CardPool, Rainworld_Liver_Spearmasternote>();
        ModHelper.AddModelToPool<Rainworld_Liver_CardPool, Rainworld_Liver_Strongercat>();
        ModHelper.AddModelToPool<Rainworld_Liver_CardPool, Rainworld_Liver_Catcome>();
        ModHelper.AddModelToPool<Rainworld_Liver_CardPool, Rainworld_Liver_Workcat>();
        ModHelper.AddModelToPool<Rainworld_Liver_CardPool, Rainworld_Liver_Strangebomb>();
        
        ModHelper.AddModelToPool<Rainworld_Liver_CardPool, Rainworld_Liver_Eatwhat>();
        ModHelper.AddModelToPool<Rainworld_Liver_CardPool, Rainworld_Liver_Foodhit>();
        ModHelper.AddModelToPool<Rainworld_Liver_CardPool, Rainworld_Liver_Twohands>();
        ModHelper.AddModelToPool<Rainworld_Liver_CardPool, Rainworld_Liver_Tube>();
        ModHelper.AddModelToPool<Rainworld_Liver_CardPool, Rainworld_Liver_Twistfate>();
        
        ModHelper.AddModelToPool<Rainworld_Liver_CardPool, Rainworld_Liver_Centblock>();
        ModHelper.AddModelToPool<Rainworld_Liver_CardPool, Rainworld_Liver_Kick>();
        ModHelper.AddModelToPool<Rainworld_Liver_CardPool, Rainworld_Liver_Catbag>();
        ModHelper.AddModelToPool<Rainworld_Liver_CardPool, Rainworld_Liver_Ruminate>();
        ModHelper.AddModelToPool<Rainworld_Liver_CardPool, Rainworld_Liver_Combo>();
        
        ModHelper.AddModelToPool<Rainworld_Liver_CardPool, Rainworld_Liver_Slugfeel>();
        ModHelper.AddModelToPool<Rainworld_Liver_CardPool, Rainworld_Liver_Dissolve>();
        ModHelper.AddModelToPool<Rainworld_Liver_CardPool, Rainworld_Liver_Spearslug>();
        ModHelper.AddModelToPool<Rainworld_Liver_CardPool, Rainworld_Liver_Iteration>();
        ModHelper.AddModelToPool<Rainworld_Liver_CardPool, Rainworld_Liver_Fatworld>();
        
        ModHelper.AddModelToPool<Rainworld_Liver_CardPool, Rainworld_Liver_Neurone>();
        ModHelper.AddModelToPool<Rainworld_Liver_CardPool, Rainworld_Liver_Reuselj>();
        ModHelper.AddModelToPool<Rainworld_Liver_CardPool, Rainworld_Liver_Acidslime>();
        ModHelper.AddModelToPool<Rainworld_Liver_CardPool, Rainworld_Liver_Quickjump>();
        ModHelper.AddModelToPool<Rainworld_Liver_CardPool, Rainworld_Liver_Goldcat>();
        
        ModHelper.AddModelToPool<Rainworld_Liver_CardPool, Rainworld_Liver_Livewill>();
        ModHelper.AddModelToPool<Rainworld_Liver_CardPool, Rainworld_Liver_Kitty>();
        ModHelper.AddModelToPool<Rainworld_Liver_CardPool, Rainworld_Liver_Workdefend>();
        ModHelper.AddModelToPool<Rainworld_Liver_CardPool, Rainworld_Liver_Tailatk>();
        ModHelper.AddModelToPool<Rainworld_Liver_CardPool, Rainworld_Liver_Spearboom>();
        
        ModHelper.AddModelToPool<Rainworld_Liver_CardPool, Rainworld_Liver_Fishing>();
        ModHelper.AddModelToPool<Rainworld_Liver_CardPool, Rainworld_Liver_Spearfire>();
        ModHelper.AddModelToPool<Rainworld_Liver_CardPool, Rainworld_Liver_Metabolize>();
        ModHelper.AddModelToPool<Rainworld_Liver_CardPool, Rainworld_Liver_Usefulslime>();
        ModHelper.AddModelToPool<Rainworld_Liver_CardPool, Rainworld_Liver_Help>();

        ModHelper.AddModelToPool<Rainworld_Liver_CardPool, Rainworld_Liver_Cabhelp>();
        ModHelper.AddModelToPool<Rainworld_Liver_CardPool, Rainworld_Liver_Monkeyhelp>();
        ModHelper.AddModelToPool<Rainworld_Liver_CardPool, Rainworld_Liver_Birdhelp>();
        ModHelper.AddModelToPool<Rainworld_Liver_CardPool, Rainworld_Liver_Oldfram>();
        ModHelper.AddModelToPool<Rainworld_Liver_CardPool, Rainworld_Liver_Execute>();

        ModHelper.AddModelToPool<Rainworld_Liver_CardPool, Rainworld_Liver_Workerror>();
        ModHelper.AddModelToPool<Rainworld_Liver_CardPool, Rainworld_Liver_Roadlight>();
        ModHelper.AddModelToPool<Rainworld_Liver_CardPool, Rainworld_Liver_Bigchuang>();
        ModHelper.AddModelToPool<Rainworld_Liver_CardPool, Rainworld_Liver_Void>();
        ModHelper.AddModelToPool<Rainworld_Liver_CardPool, Rainworld_Liver_Want>();

        
        //联机
        ModHelper.AddModelToPool<Rainworld_Liver_CardPool, Rainworld_Liver_Sweethome>();
        ModHelper.AddModelToPool<Rainworld_Liver_CardPool, Rainworld_Liver_Treasurebag>();
        ModHelper.AddModelToPool<Rainworld_Liver_CardPool, Rainworld_Liver_Bag>();











        






        












        
    }

    public static void InitRelics()
    {
        ModHelper.AddModelToPool<Rainworld_Liver_RelicPool, Liver_Fruit1>();
    }

    public static void InitPatch()
    {
        ArchaicToothReflectionPatch.AddMyCards();
    }
}