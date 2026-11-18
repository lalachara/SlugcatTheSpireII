using Godot;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Screens;
using Rainworld.Scripts;
using Rainworld.Scripts.Skins;
using SlugcatTheSpireII.Scripts.patches.Singleton;
using STS2RitsuLib.Screens;

namespace Rainworld.Godot.Character;

public partial class NRedCat : NCreatureVisuals
{
	public override void _Ready()
	{
		base._Ready();
		SetProcessInput(true);

		var creature = GetParent() as NCreature;
		var player = creature?.Entity?.Player;
		// 联机时队友的皮肤要等同步数据到达，ApplyWithRetry 会在数据变化后重新套用
		SlugcatSkins.ApplyWithRetry(this, this, player, () => new[] { SlugcatSkins.GetSelectedSkin(player) });
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: true })
		{
			var bounds = GetNodeOrNull<Control>("Bounds");
			if (bounds == null)
			{
				return;
			}

			var mousePos = GetGlobalMousePosition();
			var rect = bounds.GetGlobalRect();

			if (rect.HasPoint(mousePos))
			

// 判断地图是否打开
			{
				
				var creature = GetParent() as NCreature;
				if (creature == null|| creature.Entity.IsDead||creature.Entity.Player?.Character is not Slugcat slugcat||slugcat.SuppressHurtAnim||!RunSingleton.needmeow)
				{
					return;
				}
				if(slugcat.wawawa)
					SfxCmd.Play("event:/Rainworld/sfx/wawawa",0.7f);
				else
					SfxCmd.Play("event:/Rainworld/sfx/MeowRedCat");

				creature.SetAnimationTrigger("Meow");
			}
		}
	}
}
