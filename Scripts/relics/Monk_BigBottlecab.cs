using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Relics;
using Rainworld.Scripts;
using Rainworld.Scripts.Monster.Minions;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.relics;

/// <summary>
/// 瓶装蜥蜴：Monk 初始遗物。战斗开始时增援一只蓝蜥蜴（对应塔1 Monk:BlueCab）。
/// </summary>
[RegisterRelic(typeof(Rainworld_Monk_RelicPool))]
public sealed class Monk_BigBottlecab : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Ancient;

    /// <summary>本遗物会在战斗中生成宠物（对齐原版 BoundPhylactery/Byrdpip 的声明）。</summary>
    public override bool SpawnsPets => true;
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromKeyword(RainworldKeywords.Bluecab), HoverTipFactory.FromKeyword(RainworldKeywords.Companion), HoverTipFactory.FromKeyword(RainworldKeywords.Companioncab) };


    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://Resource/Relics/cabegg2.png",
        IconOutlinePath: "res://Resource/Relics/outline/cabegg2.png",
        BigIconPath: "res://Resource/Relics/cabegg2.png"
    );

    public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, ICombatState combatState)
    {
        if (player == base.Owner && combatState.RoundNumber == 1)
        {
            Flash();
            await FriendSummonCmd.Summon<FBlueCab>(choiceContext, Owner,
                new FriendSummonOptions(Source: null, Position: FriendPositions.Next(Owner)));
            await FriendSummonCmd.Summon<FBlueCab>(choiceContext, Owner,
                new FriendSummonOptions(Source: null, Position: FriendPositions.Next(Owner)));
        }
    }
}
