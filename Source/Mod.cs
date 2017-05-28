using Verse;
using UnityEngine;

namespace RT_PowerSwitch
{
	[StaticConstructorOnStartup]
	class Mod : Verse.Mod
	{
		public static Texture2D emergencyPowerButtonTexture;

		public Mod(ModContentPack content) : base(content)
		{
			LongEventHandler.QueueLongEvent(BindResources, "LibraryStartup", false, null);
		}

		public static void BindResources()
		{
			emergencyPowerButtonTexture = ContentFinder<Texture2D>.Get("RT_UI/EmergencyPower", true);
		}
	}
}
