using HarmonyLib;
using Kingmaker.UI;
using Kingmaker.UI.MVVM;

using System;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityExplorer;

using UnityModManagerNet;

namespace UnityExplorerLoader
{
    [HarmonyPatch]
    class UnityExplorerLoader
    {
        static bool loaded;

        [HarmonyPatch(typeof(RootUIContext), nameof(RootUIContext.InitializeUiScene))]
        [HarmonyPostfix]
        static void InitializeUiScene_Postfix() => LoadUnityExplorer();

        static void LoadUnityExplorer()
        {
            if (loaded)
                return;

            Main.Logger.Log("Load UE");

            try
            {
                ExplorerStandalone.CreateInstance(delegate (string msg, LogType logType)
                {
                    switch (logType)
                    {
                        case LogType.Error:
                            Main.Logger.Error(msg);
                            break;
                        case LogType.Assert:
                            Main.Logger.Critical(msg);
                            break;
                        case LogType.Warning:
                            Main.Logger.Warning(msg);
                            break;
                        case LogType.Log:
                            Main.Logger.Log(msg);
                            break;
                        case LogType.Exception:
                            Main.Logger.Error(msg);
                            break;
                    }
                });

                loaded = true;
            }
            catch (Exception e)
            {
                Main.Logger.LogException(e);
            }
        }
    }

    static class Main
    {
        internal static Harmony HarmonyInstance;
        internal static UnityModManager.ModEntry.ModLogger Logger;

        internal static ExplorerStandalone UnitExplorer;

        internal static bool Load(UnityModManager.ModEntry modEntry)
        {
            Logger = modEntry.Logger;
            modEntry.OnUnload = OnUnload;
            modEntry.OnGUI = OnGUI;
            HarmonyInstance = new Harmony(modEntry.Info.Id);
            HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
            return true;
        }

        static void OnGUI(UnityModManager.ModEntry modEntry) { }

        static bool OnUnload(UnityModManager.ModEntry modEntry)
        {
            HarmonyInstance.UnpatchAll(modEntry.Info.Id);
            return true;
        }
    }

    [HarmonyPatch]
    static class KeyboardAccessPatch
    {
        [HarmonyPatch(typeof(KeyboardAccess), nameof(KeyboardAccess.IsInputFieldSelected))]
        [HarmonyPostfix]
        static void CheckFieldType(ref bool __result)
        {
            if (__result == true) return;

            __result = EventSystem.current != null 
                && EventSystem.current.currentSelectedGameObject != null
                && EventSystem.current.currentSelectedGameObject.gameObject.GetComponent<InputField>() != null;
        }
    }
}