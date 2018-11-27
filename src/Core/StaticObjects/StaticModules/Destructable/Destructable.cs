﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KerbalKonstructs.Core;

namespace KerbalKonstructs
{
    class Destructable
    {
        private static bool isInitialized = false;
        private static VFXSequencer demolitionPrefab, secondaryPrefab;

        private static void Initialize()
        {
            foreach (DestructibleBuilding building in StaticDatabase.GetModelByName("KSC_VehicleAssemblyBuilding_level_3").prefab.GetComponentsInChildren<DestructibleBuilding>(true))
            {
                if (building.name == "mainBuilding")
                {
                    demolitionPrefab = building.DemolitionFXPrefab;

                }

            }
            foreach (DestructibleBuilding building in Resources.FindObjectsOfTypeAll<DestructibleBuilding>())
            {
                if (building.name == "CornerLab")
                {
                    foreach (var bla in building.CollapsibleObjects)
                    {
                        if (bla.SecondaryFXPrefab != null)
                        {
                            secondaryPrefab = bla.SecondaryFXPrefab;
                        }
                    }
                }
            }



            isInitialized = true;
        }



        internal static void MakeDestructable(StaticInstance instance)
        {

            if (!isInitialized)
            {
                Initialize();
            }

            instance.destructible = instance.gameObject.AddComponent<DestructibleBuilding>();
            List<DestructibleBuilding.CollapsibleObject> allCollapsibles = new List<DestructibleBuilding.CollapsibleObject>();
            instance.destructible.enabled = false;

            CreateCollapsables(instance,instance.mesh.transform,allCollapsibles);


            instance.destructible.CollapsibleObjects = allCollapsibles.ToArray();
            instance.destructible.CollapseReputationHit = 0;
            instance.destructible.FacilityDamageFraction = 100;
            instance.destructible.id = instance.UUID;
            instance.destructible.preCompiledId = true;
            instance.destructible.DemolitionFXPrefab = demolitionPrefab;
            instance.destructible.FxTarget = instance.gameObject.transform;

            instance.destructible.enabled = true;
        }


        private static void CreateCollapsables(StaticInstance instance, Transform target, List<DestructibleBuilding.CollapsibleObject> allCollapsibles)
        {

            Bounds staticBounds = target.gameObject.GetAllRendererBounds();

            //Log.Normal("Bounds: " + staticBounds.size.ToString());

            float min = Math.Min(staticBounds.size.x, staticBounds.size.z);
            float max = Math.Max(staticBounds.size.x, staticBounds.size.z);

            float scale = min / 70;
            float times = max / min; 

            int counter = 0;
            while (times > 0.3f)
            {
                float extrascale = Math.Min(times, 1);
                GameObject replacementObject = GameObject.Instantiate(StaticDatabase.GetModelByName("KSC_LaunchPad_level_2_wreck_1").prefab);
                replacementObject.name = "wreck_" + counter; 
                replacementObject.transform.position = target.position;
                replacementObject.transform.rotation = target.rotation;
                replacementObject.transform.parent = instance.transform;
                Vector3 localScale = replacementObject.transform.localScale;
                replacementObject.transform.localScale = new Vector3(localScale.x * scale * extrascale, 0.75f * Math.Min(1f, scale * extrascale), localScale.z * scale * extrascale);
                replacementObject.SetActive(false);
                float offset = max / 2 - ((counter - 0.5f) + ((1 + extrascale) / 2)) * (70 * scale);
                Vector3 pos;
                if (staticBounds.size.x == max)
                {
                    pos = new Vector3(offset, 0, 0);
                }
                else
                {
                    pos = new Vector3(0, 0, offset);
                }

                replacementObject.transform.localPosition += pos;

                DestructibleBuilding.CollapsibleObject collapsible = new DestructibleBuilding.CollapsibleObject();

                collapsible.collapseBehaviour = DestructibleBuilding.CollapsibleObject.Behaviour.Collapse;
                collapsible.collapseDuration = 5f;
                collapsible.collapseObject = target.gameObject;
                collapsible.repairDuration = 0;
                collapsible.replaceDelay = 0.8f;
                collapsible.replacementObject = replacementObject;

                collapsible.collapseTiltMax = new Vector3(5,0,5);
                collapsible.collapseOffset = new Vector3(0,-staticBounds.size.y/4,0);

                collapsible.SecondaryFXPrefab = secondaryPrefab;
                collapsible.Init();

                //       Log.Normal("Added collapsible: " + instance.model.name + " : " + pos.ToString()  + " : " + counter);

                collapsible.sharedWith = instance.destructible;
                allCollapsibles.Add(collapsible);

                times -= 1f;
                counter++;
            }

        }


    }
}
