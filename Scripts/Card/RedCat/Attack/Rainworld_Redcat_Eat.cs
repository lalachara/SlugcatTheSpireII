using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using Rainworld.Scripts;
using Rainworld.Scripts.Powers;

namespace Rainworld.Scripts.Card.RedCat;

public class Rainworld_Redcat_Eat : RedCatCardModel
{
    private const int energyCost = 2;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.AllEnemies;
    private const bool shouldShowInCardLibrary = true;
    // 不可被生成类效果抽到：只在持有红蘑菇力量时才能打出，单独抽到完全无用。
    protected override HashSet<CardTag> CanonicalTags => new() { RainworldCardTags.CantGenerate };

    private bool needaddmaxhp = false;
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromPower<RedmushroomPower>(), HoverTipFactory.FromPower<MushblockPower>()};
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [
            RainworldKeywords.Corrupt
        ];
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(6m, ValueProp.Move)
    ];
    
    protected override bool IsPlayable => Owner.Creature.HasPower<RedmushroomPower>();

    public Rainworld_Redcat_Eat() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        AttackContext attackContext = await AttackCommand.CreateContextAsync(CombatState!, choiceContext, cardPlay);
        needaddmaxhp = false;
        try
        {
            var damageResults = await CreatureCmd.Damage(
                choiceContext,
                CombatState!.HittableEnemies,
                DynamicVars.Damage,
                Owner.Creature,
                this,
                cardPlay);
            attackContext.AddHit(damageResults);

            var playerEffectiveHp = Owner.Creature.CurrentHp;
            if (Owner.Creature.HasPower<MushblockPower>())
                playerEffectiveHp += Owner.Creature.GetPowerAmount<MushblockPower>();

            foreach (var result in damageResults)
            {
                if (result.WasTargetKilled)
                {
                    await CreatureCmd.GainMaxHp(Owner.Creature, 5m);
                }

                else if (result.Receiver.CurrentHp < playerEffectiveHp)
                {
                    await CreatureCmd.Kill(result.Receiver);
                    await CreatureCmd.GainMaxHp(Owner.Creature, 5m); 
                }
            }
            
            
            
        }
        finally
        {
            if (attackContext != null)
                await attackContext.DisposeAsync();
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
}