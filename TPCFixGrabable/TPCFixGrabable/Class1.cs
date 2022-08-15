using BepInEx;
using HarmonyLib;
using SpaceCraft;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TPCFixGrabable
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class TPCFixGrabable : BaseUnityPlugin
    {

        public const string pluginGuid = "crazymetal.theplanetcrafter.TPCFixGrabable";
        public const string pluginName = "TPCFixGrabable";
        public const string pluginVersion = "0.0.1";
        public void Awake()
        {
            Logger.LogInfo(pluginName + " " + pluginVersion + " " + "starting...");
            Harmony harmony = new Harmony(pluginGuid);

            MethodInfo originalUpdate= AccessTools.Method(typeof(MachineGrower), "Update");
            MethodInfo patchedUpdate = AccessTools.Method(typeof(TPCFixGrabable), "Update_Patched");
            harmony.Patch(originalUpdate, postfix: new HarmonyMethod(patchedUpdate));
        }

        public static void Update_Patched(MachineGrower __instance, GameObject ___instantiatedGameObject, bool ___hasEnergy, WorldObject ___worldObjectGrower, Inventory ___inventory) {
            if (___hasEnergy && ___worldObjectGrower.GetGrowth() == 100f && ___instantiatedGameObject == null) {
                if (___inventory.GetSize() == 1) {
                    WorldObject inventoryObject = ___inventory.GetInsideWorldObjects()[0];
                    ___inventory.RemoveItem(inventoryObject, false);
                    ___inventory.AddItem(inventoryObject);
                    Console.WriteLine("Fixed instance of grower.");
                }            
            }
        }
    }
}
