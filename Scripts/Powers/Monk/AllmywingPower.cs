using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using Rainworld.Resource.Card;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.Scripts.Powers;

/// <summary>
/// 戮力同心：当你打出其它蛞蝓猫颜色的牌时，令随机敌人失去若干生命。
/// </summary>
[RegisterPower]
public sealed class AllmywingPower : ModPowerTemplate
{
    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://Resource/Powers/Monk/lltx128.png",
        BigIconPath: "res://Resource/Powers/Monk/lltx128.png"
    );

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var card = cardPlay.Card;
        if (card.Owner != Owner.Player)
            return;
        if (!IsOtherSlugcatColor(card))
            return;

        var enemies = CombatState.HittableEnemies;
        if (enemies.Count <= 0)
            return;

        var target = enemies[Owner.Player.RunState.Rng.CombatTargets.NextInt(0, enemies.Count)];
        Flash();
        await CreatureCmd.Damage(choiceContext, target, Amount, ValueProp.Unblockable | ValueProp.Unpowered, Owner);
    }

    /// <summary>
    /// 是否为"其它蛞蝓猫颜色的牌"：颜色属于蛞蝓猫颜色卡池、且不是当前玩家自己颜色的牌。
    /// 彩色（彩虹）牌被视为包含所有蛞蝓猫颜色，与七彩石、云翳把彩虹牌当作"额外一种颜色"的口径一致。
    /// </summary>
    private bool IsOtherSlugcatColor(CardModel card)
    {
        if (card.Keywords.Contains(RainworldKeywords.Colorful))
            return true;

        var pool = card.VisualCardPool;
        // 与打牌者自己的颜色比较（card.Owner 就是打出这张牌的玩家）
        if (pool == null || pool == card.Owner.Character.CardPool)
            return false;

        return pool is Rainworld_Liver_CardPool or Rainworld_River_CardPool
            or Rainworld_Redcat_CardPool or Rainworld_Monk_CardPool;
    }
}
