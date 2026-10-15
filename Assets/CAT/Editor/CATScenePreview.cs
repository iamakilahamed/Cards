using UnityEngine;
using UnityEditor;
using CAT.Data;
using CAT.Runtime;

namespace CAT.Editor
{
    public static class CATScenePreview
    {
        public static LayerType? ActiveSoloType { get; private set; } = null;
        public static int ActiveSoloIndex { get; private set; } = -1;

        public static void RefreshScenePreview(CardArtProfile profile)
        {
            CardArtBuilder builder = Object.FindFirstObjectByType<CardArtBuilder>();
            if (builder != null && profile != null)
            {
                builder.BuildArt(profile);
                ApplySoloFilterToBuilder(builder);
                SceneView.RepaintAll();
            }
        }

        public static void ToggleSolo(LayerType type, int index)
        {
            if (ActiveSoloType == type && ActiveSoloIndex == index)
            {
                ResetSoloMode();
            }
            else
            {
                ActiveSoloType = type;
                ActiveSoloIndex = index;

                CardArtBuilder builder = Object.FindFirstObjectByType<CardArtBuilder>();
                if (builder != null)
                {
                    ApplySoloFilterToBuilder(builder);
                }
            }
            SceneView.RepaintAll();
        }

        public static bool IsLayerSoloed(LayerType type, int index)
        {
            return ActiveSoloType == type && ActiveSoloIndex == index;
        }

        public static void ResetSoloMode()
        {
            ActiveSoloType = null;
            ActiveSoloIndex = -1;

            CardArtBuilder builder = Object.FindFirstObjectByType<CardArtBuilder>();
            if (builder != null)
            {
                SpriteRenderer[] renderers = builder.GetComponentsInChildren<SpriteRenderer>(true);
                foreach (var r in renderers)
                {
                    r.enabled = true;
                }
            }
            SceneView.RepaintAll();
        }

        public static void ApplySoloFilterToBuilder(CardArtBuilder builder)
        {
            if (!ActiveSoloType.HasValue)
            {
                SpriteRenderer[] allRenderers = builder.GetComponentsInChildren<SpriteRenderer>(true);
                foreach (var r in allRenderers) r.enabled = true;
                return;
            }

            SpriteRenderer[] renderers = builder.GetComponentsInChildren<SpriteRenderer>(true);

            foreach (var r in renderers)
            {
                r.enabled = false;
            }

            string targetName = GetTargetGameObjectName(ActiveSoloType.Value, ActiveSoloIndex);
            foreach (var r in renderers)
            {
                if (r.gameObject.name.Equals(targetName, System.StringComparison.OrdinalIgnoreCase))
                {
                    r.enabled = true;
                    break;
                }
            }
        }

        private static string GetTargetGameObjectName(LayerType type, int index)
        {
            return type switch
            {
                LayerType.Background => $"BackgroundLayer{index + 1}",
                LayerType.Subject => "Subject",
                LayerType.Frame => "Frame",
                LayerType.Foreground => $"ForegroundLayer{index + 1}",
                LayerType.Nameplate => "Nameplate",
                _ => ""
            };
        }
    }
}