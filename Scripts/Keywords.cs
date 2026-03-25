using BaseLib.Patches.Content;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;

namespace Rainworld.Scripts;

public static class  RainworldKeywords
{
    
        // 自定义枚举的名字。最终会变成{前缀}-{枚举值大写}的形式，例如TEST-UNIQUE
        //前缀关键词
        [CustomEnum("Spear")]
        [KeywordProperties(AutoKeywordPosition.Before)]
        public static CardKeyword Spear;
        
        [CustomEnum("Treasure")]
        [KeywordProperties(AutoKeywordPosition.Before)]
        public static CardKeyword Treasure;
        [CustomEnum("Treasurespear")]
        [KeywordProperties(AutoKeywordPosition.Before)]
        public static CardKeyword Treasurespear;

        
        //以下只在提示词出现
        [CustomEnum("Nimble")]
        public static CardKeyword Nimble;
        
        [CustomEnum("Chuang")]
        public static CardKeyword Chuang;
        
        [CustomEnum("Rock")]
        public static CardKeyword Rock;
        
        [CustomEnum("Huntsign")]
        public static CardKeyword Huntsign;
        
        [CustomEnum("Food")]
        public static CardKeyword Food;
        
        [CustomEnum("Worklevel")]
        public static CardKeyword Worklevel;
        
        [CustomEnum("Sharp")]
        public static CardKeyword Sharp;
        
        [CustomEnum("Tube")]
        public static CardKeyword Tube;

        [CustomEnum("Because")]
        public static CardKeyword Because;
        
        [CustomEnum("Livewill")]
        public static CardKeyword Livewill;
        [CustomEnum("Workerror")]
        public static CardKeyword Workerror;
        [CustomEnum("Worklock")]
        public static CardKeyword Worklock;
        [CustomEnum("Pull")]
        public static CardKeyword Pull;

}