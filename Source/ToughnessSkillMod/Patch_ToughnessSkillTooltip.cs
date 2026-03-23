using HarmonyLib;
using RimWorld;
using Verse;

namespace ToughnessSkillMod;

[HarmonyPatch(typeof(SkillUI), "GetSkillDescription")]
public static class Patch_ToughnessSkillTooltip
{
    private static ThoughnessSkillSettings Settings =>
LoadedModManager.GetMod<ToughnessSkillMod>()?.GetSettings<ThoughnessSkillSettings>() ?? ToughnessSkillMod.settings;

    [HarmonyPostfix]
	public static void Postfix(SkillRecord sk, ref string __result)
	{
		if (!(sk.def.defName != "Toughness") && sk.Pawn != null)
		{
			int level = sk.Level;
			float num;
			if (level <= (int)Settings.breakpoint)
			{
				num = 1f + (float)Settings.slopeEarly * (float)((int)Settings.breakpoint - level);
			}
			else
			{
				float num2 = (float)Settings.maxReduction / (20f - (float)(int)Settings.breakpoint);
				num = 1f - num2 * (float)(level - (int)Settings.breakpoint);
			}
			float num3 = num;
			__result = __result + "\n\n<b>Current Effects:</b>\n" + $"• Incoming Damage: x{num:0.00}\n" + $"• Bleed Rate: x{num3:0.00}";
		}
	}
}
