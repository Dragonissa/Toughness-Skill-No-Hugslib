using System.Linq;
using RimWorld;
using Verse;

namespace ToughnessSkillMod;

public class CompToughnessCache : ThingComp
{
	private int lastLevel = -1;

	public float cachedDamageFactor = 1f;

	public float cachedBleedFactor = 1f;

    private static ThoughnessSkillSettings Settings =>
        LoadedModManager.GetMod<ToughnessSkillMod>()?.GetSettings<ThoughnessSkillSettings>() ?? ToughnessSkillMod.settings;

    public void UpdateCache(Pawn pawn)
	{
		SkillRecord skillRecord = pawn.skills?.GetSkill(DefDatabase<SkillDef>.GetNamed("Toughness", errorOnFail: false));
		if (skillRecord != null && skillRecord.Level != lastLevel)
		{
			lastLevel = skillRecord.Level;
			if (lastLevel <= (int)Settings.breakpoint)
			{
				cachedDamageFactor = 1f + (float)Settings.slopeEarly * (float)((int)Settings.breakpoint - lastLevel);
			}
			else
			{
				float num = (float)Settings.maxReduction / (20f - (float)(int)Settings.breakpoint);
				cachedDamageFactor = 1f - num * (float)(lastLevel - (int)Settings.breakpoint);
			}
			if (lastLevel <= 5)
			{
				cachedBleedFactor = 1f + (float)Settings.slopeEarly * (float)((int)Settings.breakpoint - lastLevel);
				return;
			}
			float num2 = (float)Settings.maxReduction / (20f - (float)(int)Settings.breakpoint);
			cachedBleedFactor = 1f - num2 * (float)(lastLevel - (int)Settings.breakpoint);
		}
	}

	public void NotifyDamageApplied(DamageInfo dinfo, float totalDamageDealt, Pawn pawn)
	{
        if (pawn?.skills == null || totalDamageDealt <= 0f)
		{
			return;
		}
		SkillRecord skill = pawn.skills.GetSkill(DefDatabase<SkillDef>.GetNamed("Toughness", errorOnFail: false));
		if (skill == null || skill.TotallyDisabled)
		{
			return;
		}
		float amount = dinfo.Amount;
		float num = totalDamageDealt * (float)Settings.baseXp;
		float num2 = 1f;
		int num3 = 0;
		float num4 = 0f;
		foreach (Hediff_Injury item in pawn.health?.hediffSet?.hediffs.OfType<Hediff_Injury>() ?? Enumerable.Empty<Hediff_Injury>())
		{
			if (!item.IsTended() && !(item.Severity <= 0.01f))
			{
				num3++;
				num4 += item.Severity;
			}
		}
		if (num3 > 0)
		{
			num2 += 0.05f * (float)num3;
		}
		if (num4 > 5f)
		{
			num2 *= 1.2f;
		}
		if (num4 > 10f)
		{
			num2 *= 1.5f;
		}
		if (dinfo.Def == DamageDefOf.Burn || dinfo.Def == DamageDefOf.Frostbite)
		{
			num2 *= 1.25f;
		}
		else if (dinfo.Def == DamageDefOf.Bullet || dinfo.Def == DamageDefOf.Cut)
		{
			num2 *= 1.1f;
		}
		float num5 = num * num2;
		skill.Learn(num5);
		if (Prefs.DevMode && (bool)Settings.debugMode)
		{
			Log.Message($"[ToughnessSkillMod] {pawn.NameShortColored} took {amount:0.##} raw damage, reduced to {totalDamageDealt:0.##}. XP granted: {num5:0.##} (x{num2:0.##}). Base XP: {Settings.baseXp:0.##}");
		}
	}

	public void NotifyDowned(DamageInfo? dinfo, Hediff hediff, Pawn pawn)
	{
		if (pawn?.skills == null || pawn.Dead)
		{
			return;
		}
		SkillRecord skill = pawn.skills.GetSkill(DefDatabase<SkillDef>.GetNamed("Toughness", errorOnFail: false));
		if (skill == null || skill.TotallyDisabled)
		{
			return;
		}
		bool flag = false;
		if (dinfo.HasValue)
		{
			DamageDef def = dinfo.Value.Def;
			if (def == DamageDefOf.Cut || def == DamageDefOf.Blunt || def == DamageDefOf.Stab || def == DamageDefOf.Bullet || def == DamageDefOf.Bomb)
			{
				flag = true;
			}
		}
		if (!flag && pawn.health?.hediffSet?.hediffs != null)
		{
			foreach (Hediff_Injury item in pawn.health.hediffSet.hediffs.OfType<Hediff_Injury>())
			{
				if (!item.IsTended() && !(item.Severity <= 0.01f))
				{
					flag = true;
					break;
				}
			}
		}
		if (!flag || hediff.def == HediffDefOf.FoodPoisoning || hediff.def == HediffDefOf.WoundInfection || hediff.def == HediffDefOf.Plague)
		{
			return;
		}
		float num = 600f;
		float num2 = 1f;
		int num3 = 0;
		float num4 = 0f;
		if (pawn.health?.hediffSet?.hediffs != null)
		{
			foreach (Hediff_Injury item2 in pawn.health.hediffSet.hediffs.OfType<Hediff_Injury>())
			{
				if (!item2.IsTended() && !(item2.Severity <= 0.01f))
				{
					num3++;
					num4 += item2.Severity;
				}
			}
		}
		if (num3 > 0)
		{
			num2 += 0.05f * (float)num3;
		}
		if (num4 > 5f)
		{
			num2 *= 1.2f;
		}
		if (num4 > 10f)
		{
			num2 *= 1.5f;
		}
		float num5 = num * num2;
		skill.Learn(num5);
		if (Prefs.DevMode && (bool)Settings.debugMode)
		{
			Log.Message($"[Toughness] {pawn.Name} downed in combat! Awarding {num5} XP.");
		}
	}
}
