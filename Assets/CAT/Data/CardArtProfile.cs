using System;
using UnityEngine;

namespace CAT.Data
{
    [Serializable]
    public class CardLayer
    {
        public Sprite Artwork;
        [Range(-5f, 5f)] public float ZOffset;
        public Vector2 Offset = Vector2.zero;
        public float Scale = 1f;
    }

    [CreateAssetMenu(fileName = "New Card Art", menuName = "Card Art Tool (CAT)/Card Art Profile")]
    public class CardArtProfile : ScriptableObject
    {
        [Header("Global Settings")]
        [Range(0.01f, 5f)] public float BaseZStep = 0.5f;
        [Range(0.1f, 5f)] public float GlobalDepthMultiplier = 1f;

        [Header("Layers")]
        public CardLayer[] BackgroundLayers;
        public CardLayer[] SubjectLayers;
        public CardLayer FrameLayer;
        public Sprite MaskLayer;
        public CardLayer[] ForegroundLayers;
        public CardLayer NameplateLayer;
    }
}