using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Verse;
using RimWorld;

namespace RT_PowerSwitch.Source
{
	[StaticConstructorOnStartup]
	internal static class Injector
	{
		static Injector()
		{
			LongEventHandler.QueueLongEvent(Inject, "LibraryStartup", false, null);
		}

		public static void Inject()
		{
			ThingDef thingDef = DefDatabase<ThingDef>.GetNamed("PowerSwitch");
			if (!thingDef.HasComp(typeof(CompRTPowerSwitch)))
			{
				thingDef.comps.Add(new CompProperties_RTPowerSwitch());
				Log.Message("RT Power Switch: emergency power component injected.");
			}
		}
	}
}
