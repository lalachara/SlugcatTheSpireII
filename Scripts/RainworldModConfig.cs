using BaseLib.Config;
using Godot;
using MegaCrit.Sts2.Core.Modding;

namespace Rainworld.Scripts;

public class RainworldModConfig : ModConfig
{
    public static bool 测试中暂无实际功能 { get; set; } = true;
    public static bool 如有bug可反馈b站拉拉猹 { get; set; } = false;
    public static bool 感谢体验 { get; set; } = true;
    public override void SetupConfigUI(Control optionContainer)
    {
        throw new NotImplementedException();
    }
}