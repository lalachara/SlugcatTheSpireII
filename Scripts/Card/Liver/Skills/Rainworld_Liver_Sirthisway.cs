using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Orbs;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using Rainworld.Scripts.Powers;

namespace Rainworld.Scripts.Card.Liver.Attack;

public class Rainworld_Liver_Sirthisway:LiverCardModel

{
    // 基础耗能
    private const int energyCost = 1;
    // 卡牌类型
    private const CardType type = CardType.Skill;
    // 卡牌稀有度
    private const CardRarity rarity = CardRarity.Uncommon;
    // 目标类型（AnyEnemy表示任意敌人）
    private const TargetType targetType = TargetType.Self;
    // 是否在卡牌图鉴中显示
    private const bool shouldShowInCardLibrary = false;
    
    public override bool GainsBlock => true;
    protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { };



    // 卡牌的基础属性（例如这里是12点伤害）
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        
    ];

    public Rainworld_Liver_Sirthisway() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    // 打出时的效果逻辑
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        RandomEvent(choiceContext,cardPlay);
    }

    // 升级后的效果逻辑
    protected override void OnUpgrade()
    {
        UpgradeStarCostBy(-1);
    }


    private async void RandomEvent(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int random = Owner.RunState.Rng.CombatCardGeneration.NextInt(0, 20);
        switch (random)
        {
            case 0://1业力
                if(Owner.Character is Slugcat)
                    SlugcatField.playerdata.addworklevel(1);
                else
                {
                    rock(choiceContext,IsUpgraded?25:15);
                }
                break;
            case 1://抽牌
                await CardPileCmd.Draw(choiceContext, IsUpgraded?4:2, base.Owner);
                break;
            case 2://制衡
                IEnumerable<CardModel> cards = PileType.Hand.GetPile(base.Owner).Cards;
                int cardsToDraw = cards.Count();
                await CardCmd.DiscardAndDraw(choiceContext, cards, cardsToDraw);
                break;
            case 3://获得能量
                await PlayerCmd.GainEnergy(IsUpgraded?4:2,base.Owner);
                break;
            case 4://格挡
                await CreatureCmd.GainBlock(base.Owner.Creature, new BlockVar(IsUpgraded?16:12,ValueProp.Unpowered), cardPlay);
                break;
            case 5://获得随机牌
                drawcard();  
                break;
            case 6://造成aoe伤害
                rock(choiceContext,IsUpgraded?30:20);
                break;
            case 7://debuff
                applydebuff(choiceContext);
                break;
            case 8://金币
                await PlayerCmd.GainGold(IsUpgraded?20:10, base.Owner);
                break;
            case 9://悔恨和仙人指路
                CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(CombatState.CreateCard<Rainworld_Liver_Sirthisway>(base.Owner) , PileType.Hand, Owner));
                CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(CombatState.CreateCard<Regret>(base.Owner) , PileType.Hand, Owner));
                break;
            case 10://弃置所有手牌并获得奇怪的东西
                discardand(choiceContext, cardPlay);
                break;
            case 11://铸造1召唤1黑暗球1
                await OstyCmd.Summon(choiceContext, base.Owner, 1, this);
                await ForgeCmd.Forge(1, base.Owner, this);
                await OrbCmd.Channel<DarkOrb>(choiceContext, base.Owner);
                break;
            case 12:
                BaseReplayCount+=1;
                discardand(choiceContext, cardPlay);
                drawcard();
                break;
            case 13://这张牌获得重放
                BaseReplayCount+=2;
                drawcard();
                break;
            case 14://巨石
                rock(choiceContext,1);
                break;
            case 15://结束回合并尝试休眠
                if(Owner.Character is Slugcat)
                    if (SlugcatField.playerdata.cansleep())
                    {   
                        SlugcatField.playerdata.sleep();
                        break;
                    }
                PlayerCmd.EndTurn(base.Owner, canBackOut: false);
                break; 
            case 16://对自己造成15伤害
                await CreatureCmd.Damage(choiceContext,Owner.Creature,new DamageVar(15m, ValueProp.Unpowered),Owner.Creature);
                break; 
            case 17://无实体
                break; 
            case 18://小概率事件随机
                bigevnet(choiceContext,cardPlay);
                break;
            case 19://所有手牌+1费获得保留
                foreach (CardModel card in Owner.PlayerCombatState.Hand.Cards)
                {
                    if (card.Type == CardType.Attack && card.Type == CardType.Skill)
                    {
                         card.EnergyCost.SetThisCombat(card.EnergyCost.Canonical+1);
                         card.AddKeyword(CardKeyword.Retain);
                    }
                   
                }
                break;
                
        }
    }

    private async void applydebuff(PlayerChoiceContext choiceContext)
    {
        int random = Owner.RunState.Rng.CombatCardGeneration.NextInt(0, 7);

        switch (random)
        {
            case 0://灾厄
                await PowerCmd.Apply<DoomPower>(choiceContext,Owner.Creature, 3, base.Owner.Creature, this);
                break;
            case 1://创伤
                await PowerCmd.Apply<ChuangPower>(choiceContext,Owner.Creature, 2, base.Owner.Creature, this);
                break;
            case 2://魂缚
                if(Owner.Creature.GetPower<ChainsOfBindingPower>()==null)
                    await PowerCmd.Apply<ChainsOfBindingPower>(choiceContext,Owner.Creature, 1, base.Owner.Creature, this);
                else
                {
                    await PowerCmd.Apply<DoomPower>(choiceContext,Owner.Creature, 3, base.Owner.Creature, this);
                }
                break;
            case 3://柔嫩
                if(Owner.Creature.GetPower<TenderPower>()==null)
                    await PowerCmd.Apply<TenderPower>(choiceContext,Owner.Creature, 1, base.Owner.Creature, this);
                else
                {
                    await PowerCmd.Apply<DoomPower>(choiceContext,Owner.Creature, 3, base.Owner.Creature, this);
                }
                break;
            case 4://烟雾弥漫
                if(Owner.Creature.GetPower<SmoggyPower>()==null)
                    await PowerCmd.Apply<SmoggyPower>(choiceContext,Owner.Creature, 1, base.Owner.Creature, this);
                else
                {
                    await PowerCmd.Apply<DoomPower>(choiceContext,Owner.Creature, 3, base.Owner.Creature, this);
                }
                break;
            case 5://因果
                await PowerCmd.Apply<BecausePower>(choiceContext,Owner.Creature, 1, base.Owner.Creature, this);
                break;
            case 6://大补汤
                await PowerCmd.Apply<WeakPower>(choiceContext,Owner.Creature, 99, base.Owner.Creature, this);
                await PowerCmd.Apply<FrailPower>(choiceContext,Owner.Creature, 99, base.Owner.Creature, this);
                await PowerCmd.Apply<VulnerablePower>(choiceContext,Owner.Creature, 99, base.Owner.Creature, this);

                break;

        }
    }

    private async void drawcard()
    {
        int random = Owner.RunState.Rng.CombatCardGeneration.NextInt(0, 2);

        switch (random)
        {
            case 0://3张黏液
              for (int i = 0; i < 3; i++)
                      {
                          CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(CombatState.CreateCard<Slimed>(base.Owner) , PileType.Hand, Owner));
                      }  
              break;
            case 1://印随机牌
                IEnumerable<CardModel> distinctForCombat = CardFactory.GetDistinctForCombat(base.Owner, from c in base.Owner.Character.CardPool.GetUnlockedCards(base.Owner.UnlockState, base.Owner.RunState.CardMultiplayerConstraint)
        
                    where ((c.Type==CardType.Attack||c.Type==CardType.Skill)&&(c.Rarity==CardRarity.Common||c.Rarity==CardRarity.Uncommon||c.Rarity==CardRarity.Rare))
                    select c, 1, base.Owner.RunState.Rng.CombatCardGeneration);
                await CardPileCmd.AddGeneratedCardsToCombat(distinctForCombat, PileType.Hand, Owner);
                break;
            
        }

        
        
        
    }

    private async void bigevnet(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int random = Owner.RunState.Rng.CombatCardGeneration.NextInt(0, 7);
        switch (random)
        {
            case 0://巨石
                rock(choiceContext,10000);
                break;
            case 1://药水
                await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
                await PotionCmd.TryToProcure(PotionFactory.CreateRandomPotionInCombat(base.Owner, base.Owner.RunState.Rng.CombatPotionGeneration).ToMutable(), base.Owner);
                break;
            case 2://加血上限
                if(Owner.Character is Slugcat)
                    await CreatureCmd.GainMaxHp(Owner.Creature,10001m);
                else
                    await CreatureCmd.GainMaxHp(Owner.Creature,5m);
                break;
            case 3://神化
                foreach (CardModel allCard in base.Owner.PlayerCombatState.AllCards)
                {
                    if (allCard != this && allCard.IsUpgradable)
                    {
                        CardCmd.Upgrade(allCard);
                    }
                }
                break;
            case 4://回满业力
                if(Owner.Character is Slugcat)
                    SlugcatField.playerdata.addworklevel(10);
                break;
            case 5://5种形态
                await PowerCmd.Apply<EchoFormPower>(choiceContext,Owner.Creature, 1, base.Owner.Creature, this);
                await PowerCmd.Apply<VoidFormPower>(choiceContext,Owner.Creature, 1, base.Owner.Creature, this);
                await PowerCmd.Apply<DemonFormPower>(choiceContext,Owner.Creature, 1, base.Owner.Creature, this);
                await PowerCmd.Apply<ReaperFormPower>(choiceContext,Owner.Creature, 1, base.Owner.Creature, this);
                await PowerCmd.Apply<WraithFormPower>(choiceContext,Owner.Creature, 1, base.Owner.Creature, this);
                await PowerCmd.Apply<SerpentFormPower>(choiceContext,Owner.Creature, 1, base.Owner.Creature, this);
                break;
            case 6:
                discardand(choiceContext, cardPlay);
                drawcard();
                applydebuff(choiceContext);
                break;
            
        }
    }
    private async void discardand(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CardCmd.Discard(choiceContext, PileType.Hand.GetPile(base.Owner).Cards);

        int random = Owner.RunState.Rng.CombatCardGeneration.NextInt(0, 5);
        switch (random)
        {
            case 0://重启和重放仙人指路
                CardModel card = CombatState.CreateCard<Rainworld_Liver_Sirthisway>(base.Owner);
                        CardCmd.Upgrade(card);
                        card.BaseReplayCount = 2;
                        CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(card , PileType.Hand, Owner));
                        CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(CombatState.CreateCard<Reboot>(base.Owner) , PileType.Hand, Owner));
                        break;
            case 1://3张蛇咬
                for (int i = 0; i < 3; i++)
                {
                    CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(CombatState.CreateCard<Snakebite>(base.Owner) , PileType.Hand, Owner));
                }
                break;
            case 2://区和呼唤
                if (CombatState?.MultiplayerScalingModel != null)
                {
                    CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(CombatState.CreateCard<Rainworld_Liver_Treasurebag>(base.Owner) , PileType.Hand,Owner));
                }
                CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(CombatState.CreateCard<Infection>(base.Owner) , PileType.Hand, Owner));
                CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(CombatState.CreateCard<Beckon>(base.Owner) , PileType.Hand, Owner));
                CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(CombatState.CreateCard<Infection>(base.Owner) , PileType.Hand, Owner));
                break;
            case 3://宝物猫库
                CardModel arsenal = CombatState.CreateCard<Arsenal>(base.Owner);
                arsenal.AddKeyword(RainworldKeywords.Treasurespear);
                arsenal.AddKeyword(CardKeyword.Retain);
                CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(arsenal , PileType.Hand, Owner));
                break;
            case 4://3随机牌
                IEnumerable<CardModel> distinctForCombat = CardFactory.GetDistinctForCombat(base.Owner, from c in base.Owner.Character.CardPool.GetUnlockedCards(base.Owner.UnlockState, base.Owner.RunState.CardMultiplayerConstraint)
        
                    where ((c.Type==CardType.Attack||c.Type==CardType.Skill)&&(c.Rarity==CardRarity.Common||c.Rarity==CardRarity.Uncommon||c.Rarity==CardRarity.Rare))
                    select c, 3, base.Owner.RunState.Rng.CombatCardGeneration);
                await CardPileCmd.AddGeneratedCardsToCombat(distinctForCombat, PileType.Hand,Owner);
                break;
           

        }

        

    }

    private async void rock(PlayerChoiceContext choiceContext,int damage)
    {
        List<Task> damageTasks = new List<Task>();
        NRollingBoulderVfx vfx = NRollingBoulderVfx.Create(base.CombatState.HittableEnemies, damage);
        vfx.Connect(NRollingBoulderVfx.SignalName.HitCreature, Callable.From(delegate(NCreature c)
        {
            damageTasks.Add(CreatureCmd.Damage(choiceContext, new[]{ c.Entity },  damage, ValueProp.Unpowered, base.Owner.Creature));
        }));
        Callable.From(delegate
        {
            NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(vfx);
            if (!vfx.IsInsideTree())
            {
                throw new InvalidOperationException("VFX is not inside tree after adding it to combat room!");
            }
        }).CallDeferred();
        await vfx.ToSignal(vfx, Node.SignalName.TreeExiting);
        await Task.WhenAll(damageTasks);
        foreach (Creature enemy in base.CombatState.HittableEnemies)
        {
            if(enemy.IsAlive)
                return;
        }
        PlayerCmd.EndTurn(base.Owner, canBackOut: false);

    }
}