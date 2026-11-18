using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Rainworld.Scripts.Monster.Minions;
using Rainworld.Scripts.Powers;

namespace Rainworld.Scripts.Card.Monk;

/// <summary>
/// 迷途猫崽：获得协调，增援1个随机猫崽伙伴。可对任意玩家使用，目标不是盟友时默认给自己。消耗。
/// </summary>
public class Rainworld_Monk_Adopt : MonkCardModel
{
    private const int energyCost = 3;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.AnyPlayer;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromPower<TunePower>() , HoverTipFactory.FromKeyword(RainworldKeywords.Companion), HoverTipFactory.FromKeyword(RainworldKeywords.Kitty) };

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new("Special", 1m)
    ];

    public Rainworld_Monk_Adopt() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 塔1 Monk_Adopt：获得协调，然后随机増援1只猫崽（白/红/蓝三选一）
        await PowerCmd.Apply<TunePower>(choiceContext, Owner.Creature,
            DynamicVars["Special"].BaseValue, Owner.Creature, this);

        // 可对任意玩家使用：目标不是盟友（含 null、单人模式、已阵亡）时默认给自己。
        var receiver = cardPlay.Target?.Player;
        if (receiver == null || receiver == Owner || receiver.Creature.IsDead)
            receiver = Owner;

        var position = FriendPositions.Next(receiver);
        switch (Owner.RunState.Rng.CombatCardGeneration.NextInt(0, 4))
        {
            case 0:
                await FriendSummonCmd.Summon<FKittyLiver>(choiceContext, receiver,
                    new FriendSummonOptions(Source: this, Position: position));
                break;
            case 1:
                await FriendSummonCmd.Summon<FKittyRedcat>(choiceContext, receiver,
                    new FriendSummonOptions(Source: this, Position: position));
                break;
            case 2:
                await FriendSummonCmd.Summon<FKittyMonk>(choiceContext, receiver,
                    new FriendSummonOptions(Source: this, Position: position));
                break;
            
            default:
                await FriendSummonCmd.Summon<FKittyRiver>(choiceContext, receiver,
                    new FriendSummonOptions(Source: this, Position: position));
                break;
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Special"].UpgradeValueBy(1m);
    }
}
