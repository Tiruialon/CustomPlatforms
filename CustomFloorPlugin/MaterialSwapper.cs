﻿using System.Linq;
using UnityEngine;

namespace CustomFloorPlugin {
    static class MaterialSwapper {
        public static Material dark { get; private set; }
        public static Material glow { get; private set; }
        public static Material opaqueGlow { get; private set; }

        const string darkReplaceMatName = "_dark_replace (Instance)";
        const string glowReplaceMatName = "_transparent_glow_replace (Instance)";
        const string opaqueGlowReplaceMatName = "_glow_replace (Instance)";

        public static void GetMaterials() {
            // This object should be created in the Menu Scene
            // Grab materials from Menu Scene objects
            var materials = Resources.FindObjectsOfTypeAll<Material>();

            dark = new Material(materials.First(x => x.name == "DarkEnvironmentSimple"));
            opaqueGlow = new Material(materials.First(x => x.name == "EnvLightOpaque"));
            glow = new Material(materials.First(x => x.name == "EnvLight"));
        }
        public static void ReplaceAllMaterials() {
            if(dark == null || glow == null || opaqueGlow == null) {
                GetMaterials();
            }
            foreach(Renderer r in Plugin.FindAll<Renderer>()) {
                Material[] materialsCopy = r.materials;
                bool materialsDidChange = false;
                for(int i = 0; i < materialsCopy.Length; i++) {
                    if(materialsCopy[i].name.Equals(darkReplaceMatName)) {
                        materialsCopy[i] = dark;
                        materialsDidChange = true;
                    } else if(materialsCopy[i].name.Equals(glowReplaceMatName)) {
                        materialsCopy[i] = glow;
                        materialsDidChange = true;
                    } else if(materialsCopy[i].name.Equals(opaqueGlowReplaceMatName)) {
                        materialsCopy[i] = opaqueGlow;
                        materialsDidChange = true;
                    }
                }
                if(materialsDidChange) {
                    r.materials = materialsCopy;
                }
            }
        }
    }
}
