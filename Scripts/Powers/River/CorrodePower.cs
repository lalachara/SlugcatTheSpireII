using Godot;
 using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Combat.HealthBars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.Scripts.Powers;


using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.ValueProps;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

#nullable enable
[RegisterPower]
public sealed class CorrodePower : ModPowerTemplate,IHealthBarForecastSource
{
    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"res://Resource/Powers/River/{GetType().Name}128.png",
        BigIconPath: $"res://Resource/Powers/River/{GetType().Name}128.png"
    );

    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task BeforeSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side != Owner.Side)
            return;

        if (Amount >= 10 || CombatState.Players.Any(p => p.Creature.HasPower<WaterworldPower>()))
        {
            Flash();
            var damage = (int)Amount;

            // 锈蚀：每层使受到的水蚀伤害 +1%（锈蚀长矛一次给 50 层 = +50%）
            if (Owner.GetPower<RustspearPower>() is { } rust)
                damage += (int)Math.Floor(damage * rust.Amount / 100m);

            if (Owner.HasPower<WaterworldPower>())
                damage /= 2;

            await CreatureCmd.Damage(choiceContext, Owner,
                new DamageVar(damage, ValueProp.Unpowered), Owner);
        }
    }

    public async Task CorrodeDamage(Creature dealer,PlayerChoiceContext choiceContext,bool fromSeadeath = false )
    
    {
        if (Amount >= 10 || CombatState.Players.Any(p => p.Creature.HasPower<WaterworldPower>()))
        {
            Flash();
            var damage = (int)Amount;

            // 锈蚀：每层使受到的水蚀伤害 +1%（锈蚀长矛一次给 50 层 = +50%）
            if (Owner.GetPower<RustspearPower>() is { } rust)
                damage += (int)Math.Floor(damage * rust.Amount / 100m);

            if (Owner.HasPower<WaterworldPower>())
                damage /= 2;

            var dmgResults = await CreatureCmd.Damage(choiceContext, Owner,
                new DamageVar(damage, ValueProp.Unpowered), Owner);
           
            if (fromSeadeath)
                await CreatureCmd.GainBlock(dealer, dmgResults.Sum(r => r.UnblockedDamage), ValueProp.Unpowered, null);
        }
        
    }



    public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer,
        DamageResult result, ValueProp props, Creature target, CardModel? cardSource)
    {
        if (dealer == null || dealer != Owner || target == null || target == Owner
            || (props & ValueProp.Unpowered) != 0 || result.UnblockedDamage <= 0)
            return;

        Flash();
    
        await PowerCmd.Apply<CorrodePower>(choiceContext, target, 1, Owner, null);

        if (dealer.HasPower<SeasoundPower>())
        {
            // 液态猫：自身水蚀层数固定不低于 1（不随层数变化），多余的这层才转给目标
            if (Amount > 1)
                await PowerCmd.Decrement(this);
        }
        else
        {
            await PowerCmd.Decrement(this);
        }
    }
    /// <summary>
    /// 任意来源给某个单位施加「水蚀」时（amount &gt; 0），如果施加者身上有「液态猫」，
    /// 就把这次施加的层数累计进液态猫（每 10 层给 1 层伶俐上限）。
    /// 旧实现只在「水蚀持有者造成伤害顺带施加 1 层」这条路径里调用，卡牌/能力直接
    /// PowerCmd.Apply&lt;CorrodePower&gt; 给敌人的水蚀完全不计数，所以看起来没生效。
    /// </summary>
    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount,
        Creature? applier, CardModel? cardSource)
    {
        if (power != this || amount <= 0m || applier == null || applier == Owner)
            return;

        if (applier.GetPower<SeasoundPower>() is not { } seasound)
            return;

        await seasound.OnCorrodeApplied(choiceContext, (int)amount);
    }

    public IEnumerable<HealthBarForecastSegment> GetHealthBarForecastSegments(HealthBarForecastContext context)
    {
        return HealthBarForecasts.Single(
            ((CorrodePower)(context.Creature.GetPower<CorrodePower>())).getCorrodeAmount(), // 展示的数量（例如如果你的能力有2倍效果可以乘2）
            new Color(0.2f, 0.1f, 0.9f), // 颜色
            HealthBarForecastGrowthDirection.FromLeft // 从左边开始延伸还是右边开始
            // 0, // 顺序，越大越远离血条边缘，默认0
            // PreloadManager.Cache.GetMaterial("res://xxx.tres") // 如果需要自定义材质
        );
    }

    public override async Task AfterRemoved(Creature oldOwner)
    {
        if(Owner.HasPower<SeasoundPower>())
            await PowerCmd.Apply<CorrodePower>(new ThrowingPlayerChoiceContext(), Owner, 1, Owner, null);
        await base.AfterRemoved(oldOwner);
    }

    public int getCorrodeAmount()
    {
        if(Owner.HasPower<WaterworldPower>())
            return Amount/2;
        if(Amount>=10)
            return Amount;
        return 0;
    }
}