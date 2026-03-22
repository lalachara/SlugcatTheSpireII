using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Rainworld.Scripts.Card.CardVars;

public class FoodVar :DynamicVar
{
	public const string defaultName = "Chuang";

	public FoodVar(int var)
		: base("Food", var)
	{
	}

	public FoodVar(string name, int var)
		: base(name, var)
	{
	}
}
