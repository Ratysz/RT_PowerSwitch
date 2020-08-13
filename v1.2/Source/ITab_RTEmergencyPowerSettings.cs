using RimWorld;
using UnityEngine;
using Verse;

namespace RT_PowerSwitch
{
	internal class ITab_RTEmergencyPowerSettings : ITab
	{
		public ITab_RTEmergencyPowerSettings()
		{
			size = new Vector2(300.0f, 5.0f * Text.LineHeight + 8.0f);
			labelKey = "ITab_RTEmergencyPowerSettings_LabelKey";
		}

		private CompRTPowerSwitch SelectedComp
		{
			get
			{
				Thing thing = Find.Selector.SingleSelectedThing;
				MinifiedThing minifiedThing = thing as MinifiedThing;
				if (minifiedThing != null)
				{
					thing = minifiedThing.InnerThing;
				}
				return thing?.TryGetComp<CompRTPowerSwitch>();
			}
		}

		public override bool IsVisible => SelectedComp != null && SelectedComp.emergencyPowerEnabled;

		protected override void FillTab()
		{
			var comp = SelectedComp;
			var rect = (new Rect(default, size)).ContractedBy(4.0f).LeftPartPixels(size.x - 40.0f);
			var list = new Listing_Standard
			{
				ColumnWidth = rect.width,
				maxOneColumn = true
			};
			list.Begin(rect);
			{
				var rectUnits = list.GetRect(Text.LineHeight);
				Widgets.DrawHighlightIfMouseover(rectUnits);
				TooltipHandler.TipRegion(rectUnits, "ITab_RTEmergencyPowerSettings_UnitsTip".Translate());
				var row = new Listing_Standard(GameFont.Small)
				{
					ColumnWidth = (rectUnits.width - 50.0f) / 4.0f
				};
				row.Begin(rectUnits);
				if (row.RadioButton_NewTemp("%", comp.usePercentages))
				{
					comp.usePercentages = true;
				}
				row.NewColumn();
				row.NewColumn();
				if (row.RadioButton_NewTemp("Wd", !comp.usePercentages))
				{
					comp.usePercentages = false;
				}
				row.End();
			}
			{
				string buffer = comp.kickInPercentage.ToString();
				Rect rectLine = list.GetRect(Text.LineHeight);
				Rect rectLeft = rectLine.LeftHalf().Rounded();
				Rect rectRight = rectLine.RightHalf().Rounded();
				Rect rectUnit = rectRight.RightPartPixels(Text.LineHeight);
				rectRight = rectRight.LeftPartPixels(rectRight.width - Text.LineHeight);
				Widgets.DrawHighlightIfMouseover(rectLine);
				TooltipHandler.TipRegion(rectLine, "ITab_RTEmergencyPowerSettings_KickInTip".Translate());
				TextAnchor anchorBuffer = Text.Anchor;
				Text.Anchor = TextAnchor.MiddleLeft;
				Widgets.Label(rectLeft, "ITab_RTEmergencyPowerSettings_KickInLabel".Translate());
				Text.Anchor = anchorBuffer;
				Widgets.TextFieldNumeric(rectRight, ref comp.kickInPercentage, ref buffer, 0, 1000000000);
				Widgets.Label(rectUnit, comp.usePercentages ? "%" : "Wd");
			}
			{
				string buffer = comp.shutOffPercentage.ToString();
				Rect rectLine = list.GetRect(Text.LineHeight);
				Rect rectLeft = rectLine.LeftHalf().Rounded();
				Rect rectRight = rectLine.RightHalf().Rounded();
				Rect rectUnit = rectRight.RightPartPixels(Text.LineHeight);
				rectRight = rectRight.LeftPartPixels(rectRight.width - Text.LineHeight);
				Widgets.DrawHighlightIfMouseover(rectLine);
				TooltipHandler.TipRegion(rectLine, "ITab_RTEmergencyPowerSettings_ShutOffTip".Translate());
				TextAnchor anchorBuffer = Text.Anchor;
				Text.Anchor = TextAnchor.MiddleLeft;
				Widgets.Label(rectLeft, "ITab_RTEmergencyPowerSettings_ShutOffLabel".Translate());
				Text.Anchor = anchorBuffer;
				Widgets.TextFieldNumeric(rectRight, ref comp.shutOffPercentage, ref buffer, 0, 1000000000);
				Widgets.Label(rectUnit, comp.usePercentages ? "%" : "Wd");
			}
			{
				var rectBoth = list.GetRect(2.0f * Text.LineHeight);
				var rectLabel = rectBoth.TopHalf();
				var rectRadioButtons = rectBoth.BottomHalf();
				Widgets.DrawHighlightIfMouseover(rectBoth);
				TooltipHandler.TipRegion(rectBoth, "ITab_RTEmergencyPowerSettings_DirectionTip".Translate());
				Widgets.Label(rectLabel, "ITab_RTEmergencyPowerSettings_DirectionLabel".Translate());
				var row = new Listing_Standard(GameFont.Small)
				{
					ColumnWidth = (rectRadioButtons.width - 50.0f) / 5.0f
				};
				row.Begin(rectRadioButtons);
				if (row.RadioButton_NewTemp(Direction8Way.North.LabelShort(), comp.directionToMonitor == Direction8Way.North))
				{
					comp.directionToMonitor = Direction8Way.North;
				}
				if (row.RadioButton_NewTemp(Direction8Way.West.LabelShort(), comp.directionToMonitor == Direction8Way.West))
				{
					comp.directionToMonitor = Direction8Way.West;
				}
				if (row.RadioButton_NewTemp(Direction8Way.South.LabelShort(), comp.directionToMonitor == Direction8Way.South))
				{
					comp.directionToMonitor = Direction8Way.South;
				}
				if (row.RadioButton_NewTemp(Direction8Way.East.LabelShort(), comp.directionToMonitor == Direction8Way.East))
				{
					comp.directionToMonitor = Direction8Way.East;
				}
				row.End();
			}
			list.End();
		}
	}
}