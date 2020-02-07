using BS_Utils.Utilities;
using System.Collections.Generic;
using UnityEngine;
using Harmony;


namespace CustomFloorPlugin {
    public class RotationEventEffectManager:MonoBehaviour {
        List<RotationEventEffect> effectDescriptors;
        List<LightRotationEventEffect> lightRotationEffects;

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
        /// Resets the rotation of every LightRotationEventEffect to its original rotation, so it's ready for the next level.
        /// </summary>
        private void HandleSceneChange() {
            foreach (LightRotationEventEffect rotEffect in lightRotationEffects) {
                // Restore the start rotation of the object.
                rotEffect.transform.localRotation = ReflectionUtil.GetPrivateField<Quaternion>(rotEffect, "_startRotation");

                // The effect needs to be enabled so its Start is called when the platform loads, and the _startRotation is saved properly.
                rotEffect.enabled = true;
            }
        }

        public void CreateEffects(GameObject go) {
            if(lightRotationEffects == null) lightRotationEffects = new List<LightRotationEventEffect>();
            if(effectDescriptors == null) effectDescriptors = new List<RotationEventEffect>();

            RotationEventEffect[] localDescriptors = go.GetComponentsInChildren<RotationEventEffect>(true);

            if(localDescriptors == null) return;

            // Get the BeatmapObjectCallbackController floating in the level.
            BeatmapObjectCallbackController beatmapObjectCallbackController = ((BeatmapObjectCallbackController[]) GameObject.FindObjectsOfType(typeof(BeatmapObjectCallbackController)))[0];

            foreach(RotationEventEffect effectDescriptor in localDescriptors) {
                LightRotationEventEffect rotEvent = effectDescriptor.gameObject.AddComponent<LightRotationEventEffect>();
                PlatformManager.SpawnedComponents.Add(rotEvent);

                // Convert the effect descriptor into the behaviour from the base game.
                ReflectionUtil.SetPrivateField(rotEvent, "_event", (BeatmapEventType)effectDescriptor.eventType);
                ReflectionUtil.SetPrivateField(rotEvent, "_rotationVector", effectDescriptor.rotationVector);
                ReflectionUtil.SetPrivateField(rotEvent, "_beatmapObjectCallbackController", beatmapObjectCallbackController);
                ReflectionUtil.SetPrivateField(rotEvent, "_transform", rotEvent.transform);
                ReflectionUtil.SetPrivateField(rotEvent, "_startRotation", rotEvent.transform.localRotation);
                lightRotationEffects.Add(rotEvent);
                effectDescriptors.Add(effectDescriptor);
            }
        }
    }
}

namespace CustomFloorPlugin.HarmonyPatches {
    /// <summary>
    /// Bug in Beat Games code: The _startRotation is set to transform.rotation, when it should be transform.localRotation.
    /// To fix this bug, the entire function must be re-implemented in this harmony patch with the fix included.
    /// </summary>
    [HarmonyPatch(typeof(LightRotationEventEffect), "Start")]
    class LightRotationEventEffectStartPatch {
        static bool Prefix(LightRotationEventEffect __instance) {
            ReflectionUtil.GetPrivateField<BeatmapObjectCallbackController>(__instance, "_beatmapObjectCallbackController").beatmapEventDidTriggerEvent += __instance.HandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger;
            ReflectionUtil.SetPrivateField(__instance, "_transform", __instance.transform);
            ReflectionUtil.SetPrivateField(__instance, "_startRotation", __instance.transform.localRotation); // This is the fix.
            __instance.enabled = false;

            return false;
        }
    }
}
