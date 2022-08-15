using BepInEx;
using HarmonyLib;
using MijuTools;
using SpaceCraft;
using System.Reflection;
using UnityEngine;

namespace TPCEasyDeconstructStorages
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class TPCEasyDeconstructStorages : BaseUnityPlugin
    {

        public const string pluginGuid = "crazymetal.theplanetcrafter.TPCEasyDeconstructStorages";
        public const string pluginName = "TPCEasyDeconstructStorages";
        public const string pluginVersion = "1.0.1";
        public void Awake()
        {
            Logger.LogInfo(pluginName + " " + pluginVersion + " " + "starting...");
            Harmony harmony = new Harmony(pluginGuid);

            MethodInfo originalUpdate = AccessTools.Method(typeof(ActionDeconstructible), "Deconstruct");
            MethodInfo patchedUpdate = AccessTools.Method(typeof(TPCEasyDeconstructStorages), "Deconstruct_Patched");
            harmony.Patch(originalUpdate, prefix: new HarmonyMethod(patchedUpdate));
        }

        public static void Deconstruct_Patched(ActionDeconstructible __instance, bool ___isDestroying, bool ___canDeconstructIfContainerNotEmpty, Inventory ___playerInventory)
        {
            if (___isDestroying)
            {
                return;
            }
            if (!___canDeconstructIfContainerNotEmpty)
            {
                InventoryAssociated component = __instance.gameObjectRoot.GetComponent<InventoryAssociated>();
                Inventory destructingInventory = component.GetInventory();
                InformationsDisplayer informationsDisplayer = Managers.GetManager<DisplayersHandler>().GetInformationsDisplayer();
                float lifeTime = 2.5f;
                if (component != null && destructingInventory.GetInsideWorldObjects().Count > 0)
                {
                    for (int i = destructingInventory.GetInsideWorldObjects().Count - 1; i>=0;i--){
                        WorldObject element = destructingInventory.GetInsideWorldObjects()[i];
                        if (___playerInventory.GetSize() < ___playerInventory.GetInsideWorldObjects().Count)
                        {
                            destructingInventory.RemoveItem(element, false);
                            ___playerInventory.AddItem(element);
                            informationsDisplayer.AddInformation(lifeTime, Readable.GetGroupName(element.GetGroup()), DataConfig.UiInformationsType.InInventory, element.GetGroup().GetImage());
                        }
                        else {
                            destructingInventory.RemoveItem(element, false);
                            Vector3 position = __instance.gameObjectRoot.transform.position + new Vector3(0, 1, 0);
                            WorldObjectsHandler.DropOnFloor(element, position, 1f);
                            informationsDisplayer.AddInformation(lifeTime, Readable.GetGroupName(element.GetGroup()), DataConfig.UiInformationsType.DropOnFloor, element.GetGroup().GetImage());
                        }           
                     }
                }
            }
        }

    }
}

