using BS_Utils.Utilities;
using System.Collections.Generic;
using UnityEngine;
using Harmony;
using System.Reflection.Emit;


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

        private void HandleSceneChange() {
            foreach(LightRotationEventEffect rotEffect in lightRotationEffects) {
                rotEffect.transform.localRotation = ReflectionUtil.GetPrivateField<Quaternion>(rotEffect, "_startRotation");
                rotEffect.enabled = true;
            }
        }

        public void CreateEffects(GameObject go) {
            if(lightRotationEffects == null) lightRotationEffects = new List<LightRotationEventEffect>();
            if(effectDescriptors == null) effectDescriptors = new List<RotationEventEffect>();

            RotationEventEffect[] localDescriptors = go.GetComponentsInChildren<RotationEventEffect>(true);

            if(localDescriptors == null) return;

            BeatmapObjectCallbackController[] beatmapObjectCallbackControllers = (BeatmapObjectCallbackController[]) GameObject.FindObjectsOfType(typeof(BeatmapObjectCallbackController));

            foreach(RotationEventEffect effectDescriptor in localDescriptors) {
                LightRotationEventEffect rotEvent = effectDescriptor.gameObject.AddComponent<LightRotationEventEffect>();
                PlatformManager.SpawnedComponents.Add(rotEvent);

                ReflectionUtil.SetPrivateField(rotEvent, "_event", (BeatmapEventType)effectDescriptor.eventType);
                ReflectionUtil.SetPrivateField(rotEvent, "_rotationVector", effectDescriptor.rotationVector);
                ReflectionUtil.SetPrivateField(rotEvent, "_beatmapObjectCallbackController", beatmapObjectCallbackControllers[0]);
                ReflectionUtil.SetPrivateField(rotEvent, "_transform", rotEvent.transform);
                ReflectionUtil.SetPrivateField(rotEvent, "_startRotation", rotEvent.transform.localRotation);
                lightRotationEffects.Add(rotEvent);
                effectDescriptors.Add(effectDescriptor);
            }
        }
    }
}

namespace CustomFloorPlugin.HarmonyPatches {
    [HarmonyPatch(typeof(LightRotationEventEffect), "Start")]
    class LightRotationEventEffectStartPatch {
        static bool Prefix(LightRotationEventEffect __instance) {
            ReflectionUtil.GetPrivateField<BeatmapObjectCallbackController>(__instance, "_beatmapObjectCallbackController").beatmapEventDidTriggerEvent += __instance.HandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger;
            ReflectionUtil.SetPrivateField(__instance, "_transform", __instance.transform);
            ReflectionUtil.SetPrivateField(__instance, "_startRotation", __instance.transform.localRotation); // thank you Beat Games for using the global rotation
            __instance.enabled = false;
            return false;
        }
    }
}
