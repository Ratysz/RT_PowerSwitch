using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Verse;
using RimWorld;

namespace RT_PowerSwitch
{
	public static class ResearchModsSpecial
	{
		public static void InjectPowerSwitch()
		{
			ThingDef thingDef = DefDatabase<ThingDef>.GetNamed("PowerSwitch");
			thingDef.comps.Add(new CompProperties_RTPowerSwitch());
			foreach (Building building in Find.ListerBuildings.AllBuildingsColonistOfClass<Building_PowerSwitch>())
			{
				if (building.TryGetComp<CompRTPowerSwitch>() == null)
				{
					List<CompProperties> componentsBackup = new List<CompProperties>();
					foreach (CompProperties compProperties in building.def.comps)
					{
						componentsBackup.Add(compProperties);
					}
					building.def.comps.Clear();
					building.def.comps.Add(new CompProperties_RTPowerSwitch());
					building.InitializeComps();
					building.TryGetComp<CompRTPowerSwitch>().PostSpawnSetup();
					building.def.comps.Clear();
					foreach (CompProperties compProperties in componentsBackup)
					{
						building.def.comps.Add(compProperties);
					}
				}
			}
		}
	}
}
