using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Verse;
using RimWorld;

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

		private CompFlickable compFlickable;

		private List<IntVec3> cells = new List<IntVec3>();
		private int cellIndex = 0;
		private int tickStagger;
		private static int lastTickStagger;

		public bool emergencyPowerEnabled = false;

		public override void PostSpawnSetup()
		{
			base.PostSpawnSetup();

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
		}

		public override void CompTick()
		{
			PowerSwitchTick(5);
		}

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			Command_Toggle command = new Command_Toggle();
			command.isActive = () => emergencyPowerEnabled;
			command.toggleAction = () =>
			{
				emergencyPowerEnabled = !emergencyPowerEnabled;
			};
			command.groupKey = 677619692;
			command.icon = Resources.emergencyPowerButtonTexture;
			command.defaultLabel = "CompRTPowerSwitch_EmergencyPowerToggle".Translate();
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

		public override void PostExposeData()
		{
			Scribe_Values.LookValue(ref emergencyPowerEnabled, "emergencyPowerEnabled", false);
		}

		private void PowerSwitchTick(int tickAmount)
		{
			if ((Find.TickManager.TicksGame + tickStagger) % tickAmount == 0)
			{
				if (emergencyPowerEnabled)
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

				if (!compFlickable.SwitchIsOn
					&& currentRate < 0
					&& storedEnergy < -currentRate * GenDate.TicksPerHour / 6.0f)
				{
					System.Reflection.FieldInfo fieldInfo = typeof(CompFlickable).GetField(
						"wantSwitchOn", // Thanks Haplo!
						System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
					fieldInfo.SetValue(compFlickable, true);
					compFlickable.SwitchIsOn = true;
					FlickUtility.UpdateFlickDesignation(parent);
				}
				else
				{
					if (storedEnergy >= maxEnergy)
					{
						System.Reflection.FieldInfo fieldInfo = typeof(CompFlickable).GetField(
							"wantSwitchOn", // Thanks Haplo!
							System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
						fieldInfo.SetValue(compFlickable, false);
						compFlickable.SwitchIsOn = false;
						FlickUtility.UpdateFlickDesignation(parent);
					}
				}
			}
		}
	}
}
