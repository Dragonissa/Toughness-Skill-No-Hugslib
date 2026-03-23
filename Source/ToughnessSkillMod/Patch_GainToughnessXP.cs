using System;
using HarmonyLib;
using Verse;

namespace ToughnessSkillMod;

[HarmonyPatch(typeof(DamageWorker_AddInjury))]
[HarmonyPatch("ApplyDamageToPart")]
[HarmonyPatch(new Type[]
{
	typeof(DamageInfo),
	typeof(Pawn),
	typeof(DamageWorker.DamageResult)
})]
public static class Patch_GainToughnessXP
{
	[HarmonyPostfix]
	public static void Postfix(DamageInfo dinfo, Pawn pawn, DamageWorker.DamageResult result)
	{
        if (pawn?.skills != null && !(result.totalDamageDealt <= 0f))
		{
			pawn.TryGetComp<CompToughnessCache>()?.NotifyDamageApplied(dinfo, result.totalDamageDealt, pawn);
		}
	}
}
