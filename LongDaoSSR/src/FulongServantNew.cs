using GameData.Domains.Mod;
using TaiwuModdingLib.Core.Plugin;
using GameData.Utilities;
using HarmonyLib;
using GameData.Common;
using System;
using GameData.Domains.Character;
using GameData.Domains.Item;
//using NLog;
//using Logger = NLog.Logger;

namespace LongDaoSSR.src
{

    [PluginConfig("LongDaoSSR", "Naced", "1.0.0")]
    public class FulongServantSSR : TaiwuRemakePlugin
    {

        Harmony harmony;

        // private static Logger Logger = LogManager.GetCurrentClassLogger();

        public override void Dispose()
        {
            //      Logger.Info("Dispose  LongDaoSSR ");
            if (harmony != null)
            {
                harmony.UnpatchSelf();
            }

        }

        public override void Initialize()
        {
            harmony = Harmony.CreateAndPatchAll(typeof(FulongServantSSR));
            AdaptableLog.Info("Initialize  LongDaoSSR");
            //      Logger.Info("Initialize  LongDaoSSR ");
        }

        [HarmonyPrefix, HarmonyPatch(typeof(Character), "AddFeature")]
        public static bool postAddFeature(Character __instance, DataContext context, short featureId, bool removeMutexFeature)
        {
            AdaptableLog.Info("instance.GetId " + Convert.ToString(__instance.GetId()));
            //   AdaptableLog.Info("DataContext " + context.ToString());
            AdaptableLog.Info("featureId " + Convert.ToString(featureId));
            __instance.AddFeature(context, featureId, removeMutexFeature);
            return true;
        }

        /*
        [HarmonyPostfix, HarmonyPatch(typeof(Character), "AddFeature")]
        public static void postAddFeature(Character __instance, short featureId, bool removeMutexFeature)
        {
            AdaptableLog.Info("instance.GetId " + Convert.ToString(__instance.GetId()));
            //   AdaptableLog.Info("DataContext " + context.ToString());
            AdaptableLog.Info("featureId " + Convert.ToString(featureId));
        }
        */


        /*
        [HarmonyPrefix, HarmonyPatch(typeof(SkillBook), "SetCurrDurability")]
        public static bool postAddFeature(ref SkillBook __instance,ref short currDurability)
        {
            currDurability = __instance.GetMaxDurability();
            return true;
            //    AdaptableLog.Info("instance.GetId " + Convert.ToString( __instance.GetId()));
            //AdaptableLog.Info("DataContext " + context.ToString());
            //   AdaptableLog.Info("featureId " + Convert.ToString(featureId));
        }
        */
    }
}
