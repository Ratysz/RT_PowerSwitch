using RimWorld;
using System.Collections.Generic;
using System.Text;
using Verse;

namespace RT_PowerSwitch
{
	public class CompRTPowerSwitch : ThingComp
	{
		private CompProperties_RTPowerSwitch Properties
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

		private static readonly System.Reflection.FieldInfo wantsSwitchOnField = typeof(CompFlickable).GetField(
			"wantSwitchOn",
			System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

		private CompFlickable compFlickable;

		private int tickStagger;
		private static int lastTickStagger;

		public bool emergencyPowerEnabled = false;
		public float kickInPercentage = 1.0f;
		public float shutOffPercentage = 100.0f;
		public Direction8Way directionToMonitor = Direction8Way.North;
		public bool usePercentages = true;

		private IntVec3 MonitoredCell
		{
			get
			{
				var cell = parent.Position;
				switch (directionToMonitor)
				{
					case Direction8Way.North:
						{
							cell.z += 1;
							break;
						}
					case Direction8Way.South:
						{
							cell.z -= 1;
							break;
						}
					case Direction8Way.West:
						{
							cell.x -= 1;
							break;
						}
					case Direction8Way.East:
						{
							cell.x += 1;
							break;
						}
				}
				return cell;
			}
		}

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

			emergencyPowerResearchCompleted_dirty = true;
		}

		public override string CompInspectStringExtra()
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (emergencyPowerEnabled)
			{
				PowerNet powerNet = parent.Map.powerNetGrid.TransmittedPowerNetAt(MonitoredCell);
				if (powerNet != null)
				{
					float maxEnergy = 0;
					float storedEnergy = 0;
					foreach (CompPowerBattery compPowerBattery in powerNet.batteryComps)
					{
						maxEnergy += compPowerBattery.Props.storedEnergyMax;
						storedEnergy += compPowerBattery.StoredEnergy;
					}
					if (maxEnergy > 0.0f)
					{
						stringBuilder.Append("CompRTPowerSwitch_MonitoredNetwork".Translate(
							storedEnergy.ToString("N0"),
							maxEnergy.ToString(),
							(storedEnergy / maxEnergy).ToStringPercent()));
					}
					else
					{
						stringBuilder.Append("CompRTPowerSwitch_MonitoredNetworkInvalid".Translate());
					}
				}
				else
				{
					stringBuilder.Append("CompRTPowerSwitch_MonitoredNetworkInvalid".Translate());
				}
			}
			return stringBuilder.ToString();
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
			Scribe_Values.Look(ref kickInPercentage, "kickInPercentage", 1.0f);
			Scribe_Values.Look(ref shutOffPercentage, "shutOffPercentage", 100.0f);
			Scribe_Values.Look(ref directionToMonitor, "directionToMonitor", Direction8Way.North);
			Scribe_Values.Look(ref usePercentages, "usePercentages", true);
		}

		private void PowerSwitchTick(int tickAmount)
		{
			if ((Find.TickManager.TicksGame + tickStagger) % tickAmount == 0)
			{
				if (emergencyPowerEnabled)
				{
					ProcessCell(MonitoredCell);
				}
			}
		}

		public void ProcessCell(IntVec3 cell)
		{
			PowerNet powerNet = parent.Map.powerNetGrid.TransmittedPowerNetAt(cell);
			if (powerNet != null)
			{
				float maxEnergy = 0;
				float storedEnergy = 0;
				foreach (CompPowerBattery compPowerBattery in powerNet.batteryComps)
				{
					maxEnergy += compPowerBattery.Props.storedEnergyMax;
					storedEnergy += compPowerBattery.StoredEnergy;
				}
				if (maxEnergy > 0.0f)
				{
					if (shutOffPercentage > kickInPercentage)
					{
						if (compFlickable.SwitchIsOn)
						{
							if (usePercentages)
							{
								if (storedEnergy >= maxEnergy * 0.01f * shutOffPercentage)
								{
									SetFlicked(false);
								}
							}
							else
							{
								if (storedEnergy >= shutOffPercentage)
								{
									SetFlicked(false);
								}
							}
						}
						else
						{
							if (usePercentages)
							{
								if (storedEnergy <= maxEnergy * 0.01f * kickInPercentage)
								{
									SetFlicked(true);
								}
							}
							else
							{
								if (storedEnergy <= kickInPercentage)
								{
									SetFlicked(true);
								}
							}
						}
					}
					else
					{
						if (compFlickable.SwitchIsOn)
						{
							if (usePercentages)
							{
								if (storedEnergy <= maxEnergy * 0.01f * shutOffPercentage)
								{
									SetFlicked(false);
								}
							}
							else
							{
								if (storedEnergy <= shutOffPercentage)
								{
									SetFlicked(false);
								}
							}
						}
						else
						{
							if (usePercentages)
							{
								if (storedEnergy >= maxEnergy * 0.01f * kickInPercentage)
								{
									SetFlicked(true);
								}
							}
							else
							{
								if (storedEnergy >= kickInPercentage)
								{
									SetFlicked(true);
								}
							}
						}
					}
				}
			}
		}

		private void SetFlicked(bool flicked)
		{
			wantsSwitchOnField.SetValue(compFlickable, flicked);
			compFlickable.SwitchIsOn = flicked;
			FlickUtility.UpdateFlickDesignation(parent);
		}
	}
}