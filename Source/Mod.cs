using Verse;
using UnityEngine;

namespace RT_PowerSwitch
{
	class Mod : Verse.Mod
	{
		public Mod(ModContentPack content) : base(content)
		{

		}
	}

	[StaticConstructorOnStartup]
	static public class Resources
	{
		public static Texture2D emergencyPowerButtonTexture =
			ContentFinder<Texture2D>.Get("RT_UI/EmergencyPower", true);
	}
}
