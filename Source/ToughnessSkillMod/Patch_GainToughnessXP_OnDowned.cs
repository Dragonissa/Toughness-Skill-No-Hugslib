using HarmonyLib;
using Verse;

namespace ToughnessSkillMod;

[HarmonyPatch(typeof(Pawn_HealthTracker))]
[HarmonyPatch("MakeDowned")]
public static class Patch_GainToughnessXP_OnDowned
{
	[HarmonyPostfix]
	public static void Postfix(Pawn_HealthTracker __instance, DamageInfo? dinfo, Hediff hediff)
	{
		Pawn value = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
		value?.TryGetComp<CompToughnessCache>()?.NotifyDowned(dinfo, hediff, value);
	}
}
