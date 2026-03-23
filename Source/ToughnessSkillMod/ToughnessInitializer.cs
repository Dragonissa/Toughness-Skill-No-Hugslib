using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace ToughnessSkillMod;

public class ToughnessInitializer : WorldComponent
{
	private bool initialized;

    private static ThoughnessSkillSettings Settings =>
LoadedModManager.GetMod<ToughnessSkillMod>()?.GetSettings<ThoughnessSkillSettings>() ?? ToughnessSkillMod.settings;

    public ToughnessInitializer(World world)
		: base(world)
	{
	}

	public override void FinalizeInit(bool fromLoad)
	{
		DoInit();
	}

	private void DoInit()
	{

        if (initialized)
		{
			return;
		}
		initialized = true;
		foreach (Pawn item in PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive.Where((Pawn p) => p.Faction == Faction.OfPlayer && p.RaceProps.Humanlike))
		{
			SkillRecord skillRecord = item.skills?.GetSkill(DefDatabase<SkillDef>.GetNamed("Toughness", errorOnFail: false));
			if (skillRecord != null && skillRecord.Level <= 0)
			{
				int baseToughness = Rand.RangeInclusive(2, 6);
				baseToughness = (skillRecord.Level = Patch_AdjustStartingToughness.AdjustToughnessByBackstory(item, baseToughness));
				skillRecord.xpSinceLastLevel = 0f;

                if (Prefs.DevMode && Settings.debugMode)
				{
					Log.Message($"[Toughness] Init {item.LabelShortCap} → Toughness {baseToughness}");
				}
			}
		}
	}

	public override void ExposeData()
	{
		Scribe_Values.Look(ref initialized, "initialized", defaultValue: false);
	}
}
