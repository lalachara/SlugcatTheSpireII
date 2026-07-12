using Godot;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes.Combat;
using Rainworld.Scripts;

namespace Rainworld.Godot.Character;

public partial class NRedCat : NCreatureVisuals
{
    public override void _Ready()
    {
        base._Ready();
        SetProcessInput(true);
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
            {
                var creature = GetParent() as NCreature;
                if (creature == null||creature.Entity.Player?.Character is not Slugcat slugcat||slugcat.SuppressHurtAnim)
                {
                    return;
                }
                if(slugcat.wawawa)
                    SfxCmd.Play("event:/Rainworld/sfx/wawawa");
                else
                    SfxCmd.Play("event:/Rainworld/sfx/MeowRedCat");

                creature.SetAnimationTrigger("Meow");
            }
        }
    }
}