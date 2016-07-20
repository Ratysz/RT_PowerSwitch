using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Verse;
using RimWorld;

namespace RT_PowerSwitch
{
	[StaticConstructorOnStartup]
	public static class Resources
	{
		public static Texture2D emergencyPowerButtonTexture = ContentFinder<Texture2D>.Get("RT_UI/EmergencyPower", true);
	}
}
