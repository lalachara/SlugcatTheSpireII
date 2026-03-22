using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Rainworld.Scripts.Card.CardVars;

public class ChuangVar :DynamicVar
{
	public const string defaultName = "Chuang";

	public ChuangVar(int var)
		: base("Chuang", var)
	{
	}

	public ChuangVar(string name, int var)
		: base(name, var)
	{
	}
}
