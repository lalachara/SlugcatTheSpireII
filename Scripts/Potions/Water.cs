using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models.PotionPools;
using Rainworld.Scripts.Card;
using Rainworld.Scripts.Powers;

namespace Rainworld.Scripts.Potions;

using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;


// 注册药水。如果要写自定义池看添加人物的开头
[RegisterPotion(typeof(Rainworld_River_PotionPool))]
public class Water : ModPotionTemplate
{
    // 稀有度
    public override PotionRarity Rarity => PotionRarity.Uncommon;

    // 使用方式，CombatOnly表示只能在战斗中使用。
    public override PotionUsage Usage => PotionUsage.CombatOnly;
    
    

    // 目标类型
    public override TargetType TargetType => TargetType.AllAllies;

    // 定义动态变量
    // 这里显示预览卡牌灵魂。或者你也可以添加提示关键词
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromPower<CorrodePower>(),HoverTipFactory.FromPower<AdroitPower>(),HoverTipFactory.FromPower<AdroitcapPower>()];

    // 药水图片。不一定非得是png，只要最终能被Godot当成Texture读取即可。
    public override PotionAssetProfile AssetProfile => new(
        ImagePath: "res://Resource/other/potions/waterpotions.png",
        OutlinePath: "res://Resource/other/potions/waterpotions.png"
    );

    
    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        await PowerCmd.Apply<CorrodePower>(choiceContext,target!,10,Owner.Creature,null);
        if (target!.IsPlayer)
        {
            await PowerCmd.Apply<AdroitPower>(choiceContext,target!,3,Owner.Creature,null);
        }
    }
}