using BepInEx;
using HarmonyLib;
using HarmonyLib.Tools;
using MijuTools;
using SpaceCraft;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;

namespace ThePlanetCrafterEquipmentExtend
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class EquipmentExtender : BaseUnityPlugin
    {
        public const string pluginGuid = "crazymetal.theplanetcrafter.EquipmentExtend";
        public const string pluginName = "EquipmentExtend";
        public const string pluginVersion = "0.0.1";

        public void Awake()
        {
            Logger.LogInfo(pluginName + " " + pluginVersion + " " + "starting...");
            Harmony harmony = new Harmony(pluginGuid);
        
            MethodInfo originalHasSameEquipableTypeItemInEquipment = AccessTools.Method(typeof(PlayerEquipment), "HasSameEquipableTypeItemInEquipment");
            MethodInfo patchHasSameEquipableTypeItemInEquipment = AccessTools.Method(typeof(EquipmentExtender), "HasSameEquipableTypeItemInEquipment_Patch");
            harmony.Patch(originalHasSameEquipableTypeItemInEquipment, new HarmonyMethod(patchHasSameEquipableTypeItemInEquipment));

            MethodInfo originalTrueRefreshContent = AccessTools.Method(typeof(InventoryDisplayer), "TrueRefreshContent");
            MethodInfo patchTrueRefreshContent = AccessTools.Method(typeof(EquipmentExtender), "TrueRefreshContent_Patch");
            harmony.Patch(originalTrueRefreshContent,postfix:new HarmonyMethod(patchTrueRefreshContent));
            
            MethodInfo originalUpdateGaugesDependingOnEquipment = AccessTools.Method(typeof(PlayerGaugesHandler), "UpdateGaugesDependingOnEquipment");
            MethodInfo patchUpdateGaugesDependingOnEquipment = AccessTools.Method(typeof(EquipmentExtender), "UpdateGaugesDependingOnEquipment_Patch");
            harmony.Patch(originalUpdateGaugesDependingOnEquipment, postfix:new HarmonyMethod(patchUpdateGaugesDependingOnEquipment));


        }
        public static void TrueRefreshContent_Patch(InventoryDisplayer __instance, UnityEngine.UI.GridLayoutGroup ___grid, Inventory ___inventory, ref Vector2 ___originalSizeDelta)
        {
            if (__instance == null)
            {
                return;
            }

            if (___originalSizeDelta == Vector2.zero)
            {
                ___originalSizeDelta = __instance.GetComponent<RectTransform>().sizeDelta;
            }

            if (___grid.cellSize != Vector2.zero && ___originalSizeDelta != Vector2.zero)
            {           

                if (___inventory.GetSize() <= 35) {__instance.GetComponent<RectTransform>().sizeDelta = new Vector2(___originalSizeDelta.x, ___originalSizeDelta.y); 
                } else {
                    __instance.GetComponent<RectTransform>().sizeDelta = new Vector2(___originalSizeDelta.x+275f, ___originalSizeDelta.y+100f);
                }
               
                if (___inventory.GetSize() > 35)
                {
                    float containerWidth = __instance.GetComponent<RectTransform>().sizeDelta.x;
                    float containerheidht = __instance.GetComponent<RectTransform>().sizeDelta.y;
                 //   Console.WriteLine("containerWidth:" + containerWidth + ", containerheidht" + containerheidht);
                    for (int i = 100; i > 35; i--)
                    {
                        int rows = (int)Math.Floor(containerWidth / (float)i);
                        int columns = (int)Math.Floor(containerheidht / (float)i);
                  //      Console.WriteLine("rows:" + containerWidth + ", columns" + containerheidht);
                        int size = rows * columns;
                 //      Console.WriteLine("Size:" + size + ", inventorySize" + ___inventory.GetSize());
                        if ((rows * columns) >= ___inventory.GetSize())
                        {
                            ___grid.cellSize = new Vector2(i, i);
                  //          Console.WriteLine("CellSize:" + i);
                            break;
                        }
                    }
                }
                __instance.SetIconsPositionRelativeToGrid();
            }

        }
        public static bool HasSameEquipableTypeItemInEquipment_Patch(GroupItem groupItemClicked, PlayerEquipment __instance)
        {
            using (List<WorldObject>.Enumerator enumerator = __instance.GetInventory().GetInsideWorldObjects().GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (((GroupItem)enumerator.Current.GetGroup()).GetEquipableType() == groupItemClicked.GetEquipableType() && ((GroupItem)enumerator.Current.GetGroup()).GetEquipableType() != DataConfig.EquipableType.BackpackIncrease && ((GroupItem)enumerator.Current.GetGroup()).GetEquipableType() != DataConfig.EquipableType.OxygenTank)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static void UpdateGaugesDependingOnEquipment_Patch(Inventory equipmentInventory ,PlayerGaugeOxygen ___gOxygen) {
            {
                if (___gOxygen == null) return;
                float increase = 0;
                ___gOxygen.AddToInitialValue(increase);
                foreach (WorldObject insideWorldObject in equipmentInventory.GetInsideWorldObjects())
                {
                    GroupItem groupItem = (GroupItem)insideWorldObject.GetGroup();

                    if (groupItem.GetEquipableType() == DataConfig.EquipableType.OxygenTank)
                    {
                        increase += groupItem.GetGroupValue();
                        
                    }
                }
                ___gOxygen.AddToInitialValue(increase);
            }
        }
    }
}
