using UnityEngine;
using CAT.Data;

namespace CAT.Runtime
{
    [ExecuteAlways]
    public class CardArtBuilder : MonoBehaviour
    {
        [Header("Pre-Allocated Sprite Slots")]
        [SerializeField] private SpriteRenderer[] _backgroundSlots;
        [SerializeField] private SpriteRenderer[] _subjectSlots;
        [SerializeField] private SpriteRenderer _frameSlot;
        [SerializeField] private SpriteMask _maskSlot;
        [SerializeField] private SpriteRenderer[] _foregroundSlots;
        [SerializeField] private SpriteRenderer _nameplateSlot;

        [Header("Editor Testing")]
        public CardArtProfile TestProfile;
        public bool LiveUpdate = true;

        private void Update()
        {
            // Automatically rebuild art in the Scene View if a test profile is assigned
            if (!Application.isPlaying && LiveUpdate && TestProfile != null)
            {
                BuildArt(TestProfile);
            }
        }

        /// <summary>
        /// Injects the profile data into the pre-allocated slots and mathematically spaces them.
        /// </summary>
        public void BuildArt(CardArtProfile profile)
        {
            if (profile == null) return;

            float depthMult = profile.GlobalDepthMultiplier;
            float zStep = profile.BaseZStep;

            // Apply Mask Silhouette
            if (_maskSlot != null)
            {
                _maskSlot.sprite = profile.MaskLayer;
            }

            // The Background Layers
            for (int i = 0; i < _backgroundSlots.Length; i++)
            {
                if (profile.BackgroundLayers != null && i < profile.BackgroundLayers.Length)
                {
                    CardLayer layer = profile.BackgroundLayers[i];
                    float finalZ = ((i + 1) * zStep * depthMult) + layer.ZOffset;
                    ApplySprite(_backgroundSlots[i], layer.Artwork, finalZ, layer.Offset, layer.Scale, -20 + i, SpriteMaskInteraction.VisibleInsideMask);
                }
                else _backgroundSlots[i].gameObject.SetActive(false);
            }

            // The Frame Layer (Anchor: Z = 0)
            float frameZ = profile.FrameLayer.ZOffset;
            ApplySprite(_frameSlot, profile.FrameLayer.Artwork, frameZ, profile.FrameLayer.Offset, profile.FrameLayer.Scale, 0);

            // The Subject Layers
            for (int i = 0; i < _subjectSlots.Length; i++)
            {
                if (profile.SubjectLayers != null && i < profile.SubjectLayers.Length)
                {
                    CardLayer layer = profile.SubjectLayers[i];
                    float finalZ = (-0.1f * (i + 1) * zStep * depthMult) + layer.ZOffset;
                    ApplySprite(_subjectSlots[i], layer.Artwork, finalZ, layer.Offset, layer.Scale, 10 + i);
                }
                else _subjectSlots[i].gameObject.SetActive(false);
            }

            // The Foreground Layers
            for (int i = 0; i < _foregroundSlots.Length; i++)
            {
                if (profile.ForegroundLayers != null && i < profile.ForegroundLayers.Length)
                {
                    CardLayer layer = profile.ForegroundLayers[i];
                    float finalZ = (-(i + 1) * zStep * depthMult) + layer.ZOffset;
                    ApplySprite(_foregroundSlots[i], layer.Artwork, finalZ, layer.Offset, layer.Scale, 20 + i);
                }
                else _foregroundSlots[i].gameObject.SetActive(false);
            }

            // Nameplate
            int totalFg = profile.ForegroundLayers?.Length ?? 0;
            float nameplateFinalZ = (-(totalFg + 2) * zStep * depthMult) + profile.NameplateLayer.ZOffset;
            ApplySprite(_nameplateSlot, profile.NameplateLayer.Artwork, nameplateFinalZ, profile.NameplateLayer.Offset, profile.NameplateLayer.Scale, 30);
        }

        private void ApplySprite(SpriteRenderer slot, Sprite artwork, float targetZ, Vector2 offset, float scale, int sortingOrder, SpriteMaskInteraction maskMode = SpriteMaskInteraction.None)
        {
            if (slot == null) return;

            if (artwork != null)
            {
                slot.sprite = artwork;
                slot.sortingOrder = sortingOrder;
                slot.transform.localPosition = new Vector3(offset.x, offset.y, targetZ);
                slot.transform.localScale = new Vector3(scale, scale, 1f);
                slot.maskInteraction = maskMode;
                slot.gameObject.SetActive(true);
            }
            else slot.gameObject.SetActive(false);
        }
    }
}