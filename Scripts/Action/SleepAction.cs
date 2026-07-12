using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Runs;
using Rainworld.Scripts;
using Rainworld.Scripts.Powers;
using STS2RitsuLib.Combat.SecondaryResources;

namespace SlugcatTheSpireII.Scripts;

public class SleepAction : GameAction
{
    private readonly Player _player;

    public override ulong OwnerId => _player.NetId;
    public override GameActionType ActionType => GameActionType.Combat;

    public SleepAction(Player player)
    {
        _player = player;
    }

    protected override async Task ExecuteAction()
    {
        var player = _player;
        
        //await SecondaryResourceCmd.Set(player, SlugcatResources.SleepFlagID, 1);
        await SlugcatData.addfood(player, -SlugcatData.GetSleepFood(player));
        GD.Print($"{player}执行sleep，减少{SlugcatData.GetSleepFood(player)}个食物");
        NCreature creatureNode = NCombatRoom.Instance?.GetCreatureNode(player.Creature);
        creatureNode?.SetAnimationTrigger("sleep");
        await PowerCmd.Apply<SleepPower>(new ThrowingPlayerChoiceContext(), player.Creature,1,player.Creature,null);
        PlayerCmd.EndTurn(this._player, false);

        Log.Info($"{nameof(SleepAction)} executed,尝试休眠");
    }

    public override INetAction ToNetAction()
    {
        return new NetSleepAction();
    }

    public override string ToString()
    {
        return $"SleepAction for player {_player.NetId}";
    }
}

public struct NetSleepAction : INetAction, IPacketSerializable
{
    public GameAction ToGameAction(Player player)
    {
        return new SleepAction(player);
    }

    public void Serialize(PacketWriter writer)
    {
    }

    public void Deserialize(PacketReader reader)
    {
    }

    public override string ToString()
    {
        return nameof(NetSleepAction);
    }
}