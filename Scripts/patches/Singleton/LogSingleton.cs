﻿﻿﻿﻿﻿using Godot;
   using MegaCrit.Sts2.Core.Combat;
   using MegaCrit.Sts2.Core.Commands;
   using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using STS2RitsuLib.Interop.AutoRegistration;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using Rainworld.Scripts;
using Rainworld.Scripts.Powers;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models;

namespace SlugcatTheSpireII.Scripts.patches.Singleton;

[RegisterSingleton]
public class LogSingleton: HookedSingletonModel
{
    public LogSingleton() : base(HookType.Run)
    {
    }
    


  
}