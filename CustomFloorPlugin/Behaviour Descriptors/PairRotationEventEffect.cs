using UnityEngine;

namespace CustomFloorPlugin {
    class PairRotationEventEffect:MonoBehaviour {
#pragma warning disable CS0649
        public SongEventType eventTypeL;
        public SongEventType eventTypeR;
        public Vector3 rotationVector;
        public float startRotation;
        public Transform transformL;
        public Transform transformR;
#pragma warning restore CS0649
    }
}
