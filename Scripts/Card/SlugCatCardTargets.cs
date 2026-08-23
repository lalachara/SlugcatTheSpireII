﻿﻿using MegaCrit.Sts2.Core.Entities.Cards;
using Rainworld.Scripts.Powers;
using SlugcatTheSpireII.Scripts.patches.Singleton;
using STS2RitsuLib.Combat.CardTargeting;

namespace Rainworld.Scripts.Card;

public class SlugCatCardTargets
{
    public static TargetType NeverHitMe { get; private set; }
    public static TargetType CorrodeEnemy { get; private set; }
    public static TargetType SlugcatAlly { get; private set; }

    public static void Register()
    {
        NeverHitMe = CustomTargetType.RegisterSingleTargetType(
            Entry.ModId,
            "NEVER_HIT_ME_ENEMY",
            (creature, player) =>
                !WorkLevelSingleton.DamagingPairs.Contains((creature, player))&&!creature.IsPlayer);

        CorrodeEnemy = CustomTargetType.RegisterSingleTargetType(
            Entry.ModId,
            "CORRODE_ENEMY",
            creature => creature.HasPower<CorrodePower>()
        );

        SlugcatAlly = CustomTargetType.RegisterSingleTargetType(
            Entry.ModId,
            "SLUGCAT_ALLY",
            creature => creature.Player?.Character is Slugcat
        );
    }
}