using BS_Utils.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CustomFloorPlugin {
    class TrackRingsManagerSpawner:MonoBehaviour {
        List<TrackRings> trackRingsDescriptors = new List<TrackRings>();
        public List<TrackLaneRingsManager> trackLaneRingsManagers = new List<TrackLaneRingsManager>();
        List<TrackLaneRingsRotationEffectSpawner> rotationSpawners = new List<TrackLaneRingsRotationEffectSpawner>();
        List<TrackLaneRingsPositionStepEffectSpawner> stepSpawners = new List<TrackLaneRingsPositionStepEffectSpawner>();

        private void Start() {
            // make sure the rings are parented to this transform
            foreach(TrackLaneRingsManager trackLaneRingsManager in trackLaneRingsManagers) {
                TrackLaneRing[] rings = ReflectionUtil.GetPrivateField<TrackLaneRing[]>(trackLaneRingsManager, "_rings");
                foreach(TrackLaneRing ring in rings) {
                    ring.transform.parent = transform;
                    PlatformManager.SpawnedObjects.Add(ring.gameObject);
                }
            }
        }
        public void CreateTrackRings(GameObject go) {
            rotationSpawners = new List<TrackLaneRingsRotationEffectSpawner>();
            stepSpawners = new List<TrackLaneRingsPositionStepEffectSpawner>();
            trackLaneRingsManagers = new List<TrackLaneRingsManager>();
            trackRingsDescriptors = new List<TrackRings>();

            TrackRings[] ringsDescriptors = go.GetComponentsInChildren<TrackRings>();
            foreach(TrackRings trackRingDesc in ringsDescriptors) {
                trackRingsDescriptors.Add(trackRingDesc);

                TrackLaneRingsManager ringsManager =
                    trackRingDesc.gameObject.AddComponent<TrackLaneRingsManager>();
                trackLaneRingsManagers.Add(ringsManager);
                PlatformManager.SpawnedComponents.Add(ringsManager);
                TrackLaneRing ring = trackRingDesc.trackLaneRingPrefab.AddComponent<TrackLaneRing>();
                PlatformManager.SpawnedComponents.Add(ring);

                ReflectionUtil.SetPrivateField(ringsManager, "_trackLaneRingPrefab", ring);
                ReflectionUtil.SetPrivateField(ringsManager, "_ringCount", trackRingDesc.ringCount);
                ReflectionUtil.SetPrivateField(ringsManager, "_ringPositionStep", trackRingDesc.ringPositionStep);
                if(trackRingDesc.useRotationEffect) {
                    TrackLaneRingsRotationEffect rotationEffect =
                        trackRingDesc.gameObject.AddComponent<TrackLaneRingsRotationEffect>();
                    PlatformManager.SpawnedComponents.Add(rotationEffect);

                    ReflectionUtil.SetPrivateField(rotationEffect, "_trackLaneRingsManager", ringsManager);
                    ReflectionUtil.SetPrivateField(rotationEffect, "_startupRotationAngle", trackRingDesc.startupRotationAngle);
                    ReflectionUtil.SetPrivateField(rotationEffect, "_startupRotationStep", trackRingDesc.startupRotationStep);
                    ReflectionUtil.SetPrivateField(rotationEffect, "_startupRotationPropagationSpeed", (int)Math.Round(trackRingDesc.startupRotationPropagationSpeed));
                    ReflectionUtil.SetPrivateField(rotationEffect, "_startupRotationFlexySpeed", trackRingDesc.startupRotationFlexySpeed);

                    TrackLaneRingsRotationEffectSpawner rotationEffectSpawner =
                        trackRingDesc.gameObject.AddComponent<TrackLaneRingsRotationEffectSpawner>();
                    rotationSpawners.Add(rotationEffectSpawner);
                    PlatformManager.SpawnedComponents.Add(rotationEffectSpawner);
                    ReflectionUtil.SetPrivateField(rotationEffectSpawner, "_beatmapObjectCallbackController", Plugin.bocc);
                    ReflectionUtil.SetPrivateField(rotationEffectSpawner, "_beatmapEventType", (BeatmapEventType)trackRingDesc.rotationSongEventType);
                    ReflectionUtil.SetPrivateField(rotationEffectSpawner, "_rotationStep", trackRingDesc.rotationStep);
                    ReflectionUtil.SetPrivateField(rotationEffectSpawner, "_rotationPropagationSpeed", (int)Math.Round(trackRingDesc.rotationPropagationSpeed));
                    ReflectionUtil.SetPrivateField(rotationEffectSpawner, "_rotationFlexySpeed", trackRingDesc.rotationFlexySpeed);
                    ReflectionUtil.SetPrivateField(rotationEffectSpawner, "_trackLaneRingsRotationEffect", rotationEffect);
                }
                if(trackRingDesc.useStepEffect) {
                    TrackLaneRingsPositionStepEffectSpawner stepEffectSpawner =
                        trackRingDesc.gameObject.AddComponent<TrackLaneRingsPositionStepEffectSpawner>();
                    stepSpawners.Add(stepEffectSpawner);
                    PlatformManager.SpawnedComponents.Add(stepEffectSpawner);
                    ReflectionUtil.SetPrivateField(stepEffectSpawner, "_beatmapObjectCallbackController", Plugin.bocc);
                    ReflectionUtil.SetPrivateField(stepEffectSpawner, "_trackLaneRingsManager", ringsManager);
                    ReflectionUtil.SetPrivateField(stepEffectSpawner, "_beatmapEventType", (BeatmapEventType)trackRingDesc.stepSongEventType);
                    ReflectionUtil.SetPrivateField(stepEffectSpawner, "_minPositionStep", trackRingDesc.minPositionStep);
                    ReflectionUtil.SetPrivateField(stepEffectSpawner, "_maxPositionStep", trackRingDesc.maxPositionStep);
                    ReflectionUtil.SetPrivateField(stepEffectSpawner, "_moveSpeed", trackRingDesc.moveSpeed);
                }
            }
        }
    }
}
