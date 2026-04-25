using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.Core.ValueProps;
using Rainworld.Scripts;
using Rainworld.Scripts.Card.CardVars;
using Rainworld.Scripts.Powers;

namespace Rainworld.Scripts.Card.Liver.Attack;

public class Rainworld_Liver_Lovemoney:LiverCardModelAtk

{
    // 基础耗能
    private const int energyCost = 1;
    // 卡牌类型
    private const CardType type = CardType.Attack;
    // 卡牌稀有度
    private const CardRarity rarity = CardRarity.Uncommon;
    // 目标类型（AnyEnemy表示任意敌人）
    private const TargetType targetType = TargetType.AnyEnemy;
    // 是否在卡牌图鉴中显示
    private const bool shouldShowInCardLibrary = true;


    private static decimal CalculateDamageMultiplier(CardModel card, Creature? creature)
    {
        // 安全检查：避免空引用异常
        if (card is not Rainworld_Liver_Lovemoney loveMoneyCard)
            return 0m;
        
        var owner = loveMoneyCard.Owner;
        if (owner == null)
            return 0m;

        // 原有的乘法器计算逻辑
        return owner.Gold / 40m + owner.PlayerCombatState.AllCards.Count(
            c => c.Keywords.Contains(RainworldKeywords.Treasure)
        );
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CalculationBaseVar(0m),
        new ExtraDamageVar(2m),
        new GoldVar(10),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier(CalculateDamageMultiplier)

    ];

    public Rainworld_Liver_Lovemoney() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    // 打出时的效果逻辑
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        bool shouldTriggerFatal = cardPlay.Target.Powers.All((PowerModel p) => p.ShouldOwnerDeathTriggerFatal());
        Vector2? monsterPos = null;
        if (TestMode.IsOff)
        {
            monsterPos = NCombatRoom.Instance.GetCreatureNode(cardPlay.Target)?.VfxSpawnPosition;
        }
        AttackCommand attackCommand = await DamageCmd.Attack(base.DynamicVars.CalculatedDamage).FromCard(this).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_blunt", null, "blunt_attack.mp3")
            .Execute(choiceContext);
        if (shouldTriggerFatal && attackCommand.Results.Any((DamageResult r) => r.WasTargetKilled))
        {
            if (monsterPos.HasValue)
            {
                //VfxCmd.PlayVfx(monsterPos.Value, "vfx/vfx_coin_explosion_regular",null);
            }
            await PlayerCmd.GainGold(base.DynamicVars["Gold"].IntValue, base.Owner);
        }

    }

    // 升级后的效果逻辑
    protected override void OnUpgrade()
    {
        DynamicVars.Gold.UpgradeValueBy(10); // 升级后增加4点伤害
    }
}