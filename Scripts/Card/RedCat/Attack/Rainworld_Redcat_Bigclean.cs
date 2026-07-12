using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Rainworld.Scripts.Card.RedCat;

public class Rainworld_Redcat_Bigclean : RedCatCardModel
{
    private const int energyCost = 2;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.RandomEnemy;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(5m, ValueProp.Move)
    ];
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

    public Rainworld_Redcat_Bigclean() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 选择任意张手牌消耗
        var handCards = PileType.Hand.GetPile(Owner).Cards.ToList();
        if (handCards.Count > 0)
        {
            var prefs = new CardSelectorPrefs(SelectionScreenPrompt, 0, handCards.Count);
            var selected = (await CardSelectCmd.FromHand(choiceContext, Owner, prefs, c => c != this, this)).ToList();
            if (selected.Count > 0)
                foreach (var card in selected)
                    await CardCmd.Exhaust(choiceContext, card);
        }

        // 统计已消耗牌堆总张数
        var exhaustCount = PileType.Exhaust.GetPile(Owner).Cards.Count;
        if (exhaustCount == 0) return;

        var enemies = CombatState.HittableEnemies.ToList();
        if (enemies.Count == 0) return;

        for (int i = 0; i < exhaustCount; i++)
        {
            var target = enemies[Owner.RunState.Rng.CombatTargets.NextInt(enemies.Count)];
            await DamageCmd
                .Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this,cardPlay)
                .TargetingRandomOpponents(CombatState)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(1m);
    }
}