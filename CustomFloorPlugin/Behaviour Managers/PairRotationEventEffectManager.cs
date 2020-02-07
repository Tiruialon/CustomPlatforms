using BS_Utils.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;
using Harmony;
using System.Reflection;


namespace CustomFloorPlugin {
    public class PairRotationEventEffectManager:MonoBehaviour {
        List<PairRotationEventEffect> effectDescriptors;
        List<LightPairRotationEventEffect> lightRotationEffects;

        internal void RegisterForEvents() {
            BSEvents.menuSceneLoaded += HandleSceneChange;
            BSEvents.gameSceneLoaded += HandleSceneChange;
            HandleSceneChange();
        }

        private void OnDisable() {
            BSEvents.menuSceneLoaded -= HandleSceneChange;
            BSEvents.gameSceneLoaded -= HandleSceneChange;
            HandleSceneChange();
        }

        /// <summary>
        /// Resets the rotation of every LightPairRotationEventEffect to its original rotation, so it's ready for the next level.
        /// </summary>
        private void HandleSceneChange() {
            // Since LightPairRotationEventEffect.RotationData is a private internal member, we need to get its type dynamically.
            Type RotationData = Type.GetType("LightPairRotationEventEffect+RotationData,MainAssembly");

            // The reflection method to get the rotation data must have its generic method created dynamically, so as to use the dynamic type.
            MethodInfo GetPrivateFieldM = typeof(ReflectionUtil).GetMethod("GetPrivateField", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(object), typeof(string) }, null);
            GetPrivateFieldM = GetPrivateFieldM.MakeGenericMethod(RotationData);

            foreach (LightPairRotationEventEffect rotEffect in lightRotationEffects) {
                Vector3 rotationVector = ReflectionUtil.GetPrivateField<Vector3>(rotEffect, "_rotationVector");
                float startRotation = ReflectionUtil.GetPrivateField<float>(rotEffect, "_startRotation");

                // Reset the rotation of the objects.
                var rotationDataL = GetPrivateFieldM.Invoke(null, new object[]{ rotEffect, "_rotationDataL" });
                if (rotationDataL != null) {
                    Transform transformL = rotationDataL.GetField<Transform>("transform");

                    // Restore the start rotation of the object.
                    transformL.localRotation = rotationDataL.GetField<Quaternion>("startRotation");

                    // Undo the rotation applied by default, so the object is back to its initial rotation.
                    transformL.Rotate(rotationVector, -startRotation, Space.Self);
                }

                var rotationDataR = GetPrivateFieldM.Invoke(null, new object[] { rotEffect, "_rotationDataR" });
                if (rotationDataR != null) {
                    Transform transformR = rotationDataR.GetField<Transform>("transform");

                    // Restore the start rotation of the object.
                    transformR.localRotation = rotationDataR.GetField<Quaternion>("startRotation");

                    // Undo the rotation applied by default, so the object is back to its initial rotation.
                    transformR.Rotate(rotationVector, startRotation, Space.Self);
                }

                rotEffect.enabled = true;
            }
        }

        public void CreateEffects(GameObject go) {
            if(lightRotationEffects == null) lightRotationEffects = new List<LightPairRotationEventEffect>();
            if(effectDescriptors == null) effectDescriptors = new List<PairRotationEventEffect>();

            PairRotationEventEffect[] localDescriptors = go.GetComponentsInChildren<PairRotationEventEffect>(true);

            if(localDescriptors == null) return;

            // Get the BeatmapObjectCallbackController floating in the level.
            BeatmapObjectCallbackController beatmapObjectCallbackController = ((BeatmapObjectCallbackController[])GameObject.FindObjectsOfType(typeof(BeatmapObjectCallbackController)))[0];

            foreach (PairRotationEventEffect effectDescriptor in localDescriptors) {
                LightPairRotationEventEffect rotEvent = effectDescriptor.gameObject.AddComponent<LightPairRotationEventEffect>();
                PlatformManager.SpawnedComponents.Add(rotEvent);

                // Convert the effect descriptor into the behaviour from the base game.
                ReflectionUtil.SetPrivateField(rotEvent, "_eventL", (BeatmapEventType)effectDescriptor.eventTypeL);
                ReflectionUtil.SetPrivateField(rotEvent, "_eventR", (BeatmapEventType)effectDescriptor.eventTypeR);
                ReflectionUtil.SetPrivateField(rotEvent, "_rotationVector", effectDescriptor.rotationVector);
                ReflectionUtil.SetPrivateField(rotEvent, "_overrideRandomValues", false);
                ReflectionUtil.SetPrivateField(rotEvent, "_startRotation", effectDescriptor.startRotation);
                ReflectionUtil.SetPrivateField(rotEvent, "_transformL", effectDescriptor.transformL);
                ReflectionUtil.SetPrivateField(rotEvent, "_transformR", effectDescriptor.transformR);
                ReflectionUtil.SetPrivateField(rotEvent, "_beatmapObjectCallbackController", beatmapObjectCallbackController);
                lightRotationEffects.Add(rotEvent);
                effectDescriptors.Add(effectDescriptor);
            }
        }
    }
}

namespace CustomFloorPlugin.HarmonyPatches {
    /// <summary>
    /// Bug in Beat Games code: The startRotation of every RotationData is set to transform.rotation, when it should be transform.localRotation.
    /// To fix this bug, the entire function must be re-implemented in this harmony patch with the fix included.
    /// </summary>
    [HarmonyPatch(typeof(LightPairRotationEventEffect), "Start")]
    class LightPairRotationEventEffectStartPatch {
        static bool Prefix(LightPairRotationEventEffect __instance) {
            ReflectionUtil.GetPrivateField<BeatmapObjectCallbackController>(__instance, "_beatmapObjectCallbackController").beatmapEventDidTriggerEvent += __instance.HandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger;

            Vector3 rotationVector = ReflectionUtil.GetPrivateField<Vector3>(__instance, "_rotationVector");
            float startRotation = ReflectionUtil.GetPrivateField<float>(__instance, "_startRotation");
            ReflectionUtil.GetPrivateField<Transform>(__instance, "_transformL").transform.Rotate(rotationVector, startRotation, Space.Self);
            ReflectionUtil.GetPrivateField<Transform>(__instance, "_transformR").transform.Rotate(rotationVector, -startRotation, Space.Self);

            // Since LightPairRotationEventEffect.RotationData is a private internal member, we need to get its type dynamically.
            Type RotationData = Type.GetType("LightPairRotationEventEffect+RotationData,MainAssembly");

            var rotationDataL = Activator.CreateInstance(RotationData);
            rotationDataL.SetField("enabled", false);
            rotationDataL.SetField("rotationSpeed", 0f);
            rotationDataL.SetField("startRotation", ReflectionUtil.GetPrivateField<Transform>(__instance, "_transformL").localRotation); // This is the fix.
            rotationDataL.SetField("transform", ReflectionUtil.GetPrivateField<Transform>(__instance, "_transformL"));
            ReflectionUtil.SetPrivateField(__instance, "_rotationDataL", rotationDataL);

            var rotationDataR = Activator.CreateInstance(RotationData);
            rotationDataR.SetField("enabled", false);
            rotationDataR.SetField("rotationSpeed", 0f);
            rotationDataR.SetField("startRotation", ReflectionUtil.GetPrivateField<Transform>(__instance, "_transformR").localRotation); // This is the fix.
            rotationDataR.SetField("transform", ReflectionUtil.GetPrivateField<Transform>(__instance, "_transformR"));
            ReflectionUtil.SetPrivateField(__instance, "_rotationDataR", rotationDataR);

            __instance.enabled = false;

            return false;
        }
    }
}
