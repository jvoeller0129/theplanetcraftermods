using BepInEx;
using SpaceCraft;
using System;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Unity.Netcode;

namespace ThePlanetCrafterEquipmentExtend
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class EquipmentExtender : BaseUnityPlugin
    {
        public const string pluginGuid = "crazymetal.theplanetcrafter.EquipmentExtend";
        public const string pluginName = "EquipmentExtend";
        public const string pluginVersion = "0.0.2";

        public void Awake()
        {
            Logger.LogInfo(pluginName + " " + pluginVersion + " " + "starting...");
            var harmony = new Harmony(pluginGuid);
        
            MethodInfo originalHasSameEquipableTypeItemInEquipment = AccessTools.Method(typeof(PlayerEquipment), "HasSameEquipableTypeItemInEquipment");
            MethodInfo patchHasSameEquipableTypeItemInEquipment = AccessTools.Method(typeof(EquipmentExtender), "HasSameEquipableTypeItemInEquipment_Patch");
            harmony.Patch(originalHasSameEquipableTypeItemInEquipment, new HarmonyMethod(patchHasSameEquipableTypeItemInEquipment));

            MethodInfo originalUpdateGaugesDependingOnEquipment = AccessTools.Method(typeof(PlayerGaugesHandler), "UpdateGaugesDependingOnEquipment");
            MethodInfo patchUpdateGaugesDependingOnEquipment = AccessTools.Method(typeof(EquipmentExtender), "UpdateGaugesDependingOnEquipment_Patch");
            harmony.Patch(originalUpdateGaugesDependingOnEquipment, finalizer: new HarmonyMethod(patchUpdateGaugesDependingOnEquipment));

            MethodInfo originalTrueRefreshContent = AccessTools.Method(typeof(InventoryDisplayer), "TrueRefreshContent");
            MethodInfo patchTrueRefreshContent = AccessTools.Method(typeof(EquipmentExtender), "TrueRefreshContent_Patch");
            harmony.Patch(originalTrueRefreshContent,finalizer:new HarmonyMethod(patchTrueRefreshContent));

        }
        
        public static void TrueRefreshContent_Patch(InventoryDisplayer __instance, int ____selectionIndex, UnityEngine.UI.GridLayoutGroup ____grid, GroupSelector ___groupSelector,
            Inventory ____inventoryInteracting, Inventory ____inventory, ref Vector2 ____originalSizeDelta, LogisticManager ____logisticManager)
        {
          

            if (____originalSizeDelta == Vector2.zero)
            {
                ____originalSizeDelta = __instance.GetComponent<RectTransform>().sizeDelta;
            }

            if (____grid.cellSize != Vector2.zero && ____originalSizeDelta != Vector2.zero)
            {           

                if (____inventory.GetSize() <= 35) {__instance.GetComponent<RectTransform>().sizeDelta = new Vector2(____originalSizeDelta.x, ____originalSizeDelta.y); 
                } else {
                    __instance.GetComponent<RectTransform>().sizeDelta = new Vector2(____originalSizeDelta.x+275f, ____originalSizeDelta.y+100f);
                }
               
                if (____inventory.GetSize() > 35)
                {
                    float containerWidth = __instance.GetComponent<RectTransform>().sizeDelta.x;
                    float containerheidht = __instance.GetComponent<RectTransform>().sizeDelta.y;
                 //   Console.WriteLine("containerWidth:" + containerWidth + ", containerheidht" + containerheidht);
                    for (int i = 100; i > 10; i--)
                    {
                        int rows = (int)Math.Floor(containerWidth / (float)i);
                        int columns = (int)Math.Floor(containerheidht / (float)i);
                     //Console.WriteLine("rows:" + containerWidth + ", columns" + containerheidht);
                        int size = rows * columns;
                     //Console.WriteLine("Size:" + size + ", inventorySize" + ___inventory.GetSize());
                        if ((rows * columns) >= ____inventory.GetSize())
                        {
                            ____grid.cellSize = new Vector2(i, i);
                            ____grid.spacing = new Vector2(0.5f, 0.5f);
                     //Console.WriteLine("CellSize:" + i);
                            break;
                        }
                    }
                }
                __instance.SetIconsPositionRelativeToGrid();
                ____selectionIndex = -1;
            }

        }
       
        public static bool HasSameEquipableTypeItemInEquipment_Patch(GroupItem groupItemClicked, PlayerEquipment __instance)
        {
            using (List<WorldObject>.Enumerator enumerator = (List<WorldObject>.Enumerator)__instance.GetInventory().GetInsideWorldObjects().GetEnumerator()) 
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
       
        public static void UpdateGaugesDependingOnEquipment_Patch(PlayerGaugesHandler __instance,Inventory equipmentInventory,float ____initialValue , NetworkVariable<float> ____oxygenValue, NetworkVariable<float> ____oxygenMaxValue)
        {
            
             float oxygen = ____initialValue;
             foreach (WorldObject insideWorldObject in equipmentInventory.GetInsideWorldObjects())
                {
                    GroupItem groupItem = (GroupItem)insideWorldObject.GetGroup();

                    if (groupItem.GetEquipableType() == DataConfig.EquipableType.OxygenTank)
                    {
                        oxygen += groupItem.GetGroupValue();
                        
                    }
                }
                if (____oxygenMaxValue.Value > oxygen)
                {
                    ____oxygenValue.Value = oxygen;
                }

                ____oxygenMaxValue.Value = oxygen;
            
        }
    }
}
