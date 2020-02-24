using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RT_PowerSwitch
{
	public class CompRTPowerSwitch : ThingComp
	{
		private CompProperties_RTPowerSwitch properties
		{
			get
			{
				return (CompProperties_RTPowerSwitch)props;
			}
		}

		private static bool emergencyPowerResearchCompleted_cache = false;
		private static bool emergencyPowerResearchCompleted_dirty = true;

		private static bool EmergencyPowerResearchCompleted
		{
			get
			{
				if (emergencyPowerResearchCompleted_dirty)
				{
					emergencyPowerResearchCompleted_cache =
						DefDatabase<ResearchProjectDef>.GetNamed("ResearchProject_RTEmergencyPower").IsFinished;
				}
				return emergencyPowerResearchCompleted_cache;
			}
		}

		private static System.Reflection.FieldInfo wantsSwitchOnField = typeof(CompFlickable).GetField(
			"wantSwitchOn",
			System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

		private CompFlickable compFlickable;

		private List<IntVec3> cells = new List<IntVec3>();
		private int cellIndex = 0;
		private int tickStagger;
		private static int lastTickStagger;

		public bool emergencyPowerEnabled = false;

		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			base.PostSpawnSetup(respawningAfterLoad);

			lastTickStagger++;
			tickStagger = lastTickStagger;

			compFlickable = parent.TryGetComp<CompFlickable>();
			if (compFlickable == null)
			{
				Log.Error("CompRTPowerSwitch could not get parent's CompFlickable!");
			}

			cellIndex = tickStagger;
			cells = GenAdj.CellsAdjacentCardinal(parent).ToList();
			while (cellIndex > cells.Count)
			{
				cellIndex -= cells.Count;
			}

			emergencyPowerResearchCompleted_dirty = true;
		}

		public override void CompTick()
		{
			PowerSwitchTick(5);
		}

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			if (EmergencyPowerResearchCompleted)
			{
				Command_Toggle command = new Command_Toggle
				{
					isActive = () => emergencyPowerEnabled,
					toggleAction = () =>
					{
						emergencyPowerEnabled = !emergencyPowerEnabled;
					},
					icon = Resources.emergencyPowerButtonTexture,
					defaultLabel = "CompRTPowerSwitch_EmergencyPowerToggle".Translate()
				};
				if (emergencyPowerEnabled)
				{
					command.defaultDesc = "CompRTPowerSwitch_EmergencyPowerOn".Translate();
				}
				else
				{
					command.defaultDesc = "CompRTPowerSwitch_EmergencyPowerOff".Translate();
				}
				yield return command;
			}
		}

		public override void PostExposeData()
		{
			Scribe_Values.Look(ref emergencyPowerEnabled, "emergencyPowerEnabled", false);
		}

		private void PowerSwitchTick(int tickAmount)
		{
			if ((Find.TickManager.TicksGame + tickStagger) % tickAmount == 0)
			{
				if (EmergencyPowerResearchCompleted && emergencyPowerEnabled)
				{
					if (cellIndex >= cells.Count)
					{
						cellIndex = 0;
					}
					ProcessCell(cells[cellIndex]);
					cellIndex++;
				}
			}
		}

		public void ProcessCell(IntVec3 cell)
		{
			PowerNet powerNet = parent.Map.powerNetGrid.TransmittedPowerNetAt(cell);
			if (powerNet != null)
			{
				float maxEnergy = 0.0f;
				float storedEnergy = 0.0f;
				float currentRate = powerNet.CurrentEnergyGainRate();
				foreach (CompPowerBattery compPowerBattery in powerNet.batteryComps)
				{
					maxEnergy += compPowerBattery.Props.storedEnergyMax;
					storedEnergy += compPowerBattery.StoredEnergy;
				}

				if (compFlickable.SwitchIsOn)
				{
					if (storedEnergy >= maxEnergy)
					{
						wantsSwitchOnField.SetValue(compFlickable, false);
						compFlickable.SwitchIsOn = false;
						FlickUtility.UpdateFlickDesignation(parent);
					}
				}
				else
				{
					if (currentRate <= 0
					&& storedEnergy <= -currentRate * GenDate.TicksPerHour / 6.0f)
					{
						wantsSwitchOnField.SetValue(compFlickable, true);
						compFlickable.SwitchIsOn = true;
						FlickUtility.UpdateFlickDesignation(parent);
					}
				}
			}
		}
	}
}