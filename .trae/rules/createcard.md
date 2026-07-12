卡牌使用连击的时候用
await DamageCmd.Attack(xxx)
.WithHitCount(7)
写法，不要用for循环

卡牌升级后减耗使用EnergyCost.UpgradeBy(-1);

卡牌、遗物的Owner是player类，能力的Owner是creature类，不要混用，能力里判断是否由角色打出需要用Owner.Player==card.Owner

获取多人游戏人数用CombatState.Players.Count;

await PowerCmd.Apply<xxxpower>(choiceContext, xxCreature, amount, xxCreature, this/null);

卡牌命名格式Rainworld_Future_Quitmagic，只在每段首字母大写，卡牌名称不要用驼峰命名，即使卡牌名是多个单次组合。

引雷请使用Tools.Yinlei(cardPlay, choiceContext);
回手请使用Tools.BackToHand(Player player, CardModel card)
描述中如果有关键字，用protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
new[] {HoverTipFactory.FromPower<StrengthPower>()};实现
如果卡牌本身不提供格挡，不要使用    public override bool GainsBlock => true;
    