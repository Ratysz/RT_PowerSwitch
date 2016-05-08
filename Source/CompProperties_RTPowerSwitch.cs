using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Verse;
using RimWorld;

namespace RT_PowerSwitch
{
	public class CompProperties_RTPowerSwitch : CompProperties
	{
		public CompProperties_RTPowerSwitch()
		{
			compClass = typeof(CompRTPowerSwitch);
		}
	}
}
