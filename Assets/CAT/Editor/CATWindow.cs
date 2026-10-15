using UnityEngine;
using UnityEditor;
using CAT.Data;
using CAT.Runtime;

namespace CAT.Editor
{
    public class CATWindow : EditorWindow
    {
        private CardArtProfile _activeProfile;
        private SerializedObject _serializedProfile;
        private Vector2 _scrollPosition;

        // Preview Render Properties
        private PreviewRenderUtility _previewUtility;
        private GameObject _previewTarget;
        private CardArtBuilder _previewBuilder;

        // Camera Controls
        private Vector2 _dragRotation = Vector2.zero;
        private Vector2 _cameraPan = Vector2.zero;
        private const float DEFAULT_CAMERA_DISTANCE = 75f;
        private float _cameraDistance = DEFAULT_CAMERA_DISTANCE;

        // Clipboard for Template Copy/Paste
        private static float _copiedDepthMult = 1f;
        private static Vector2 _copiedSubjectOffset = Vector2.zero;
        private static float _copiedSubjectScale = 1f;
        private static bool _hasCopiedTemplate = false;

        [MenuItem("Tools/Card Art Tool (CAT)/Card Art Dashboard")]
        public static void OpenWindow()
        {
            CATWindow window = GetWindow<CATWindow>("CAT Dashboard");
            window.minSize = new Vector2(400, 600);
            window.Show();
        }

        private void OnEnable()
        {
            InitializePreview();
        }

        private void OnDisable()
        {
            // Clean up our temporary materials to guarantee zero memory leaks
            if (_previewTarget != null)
            {
                SpriteRenderer[] renderers = _previewTarget.GetComponentsInChildren<SpriteRenderer>(true);
                foreach (var renderer in renderers)
                {
                    if (renderer.sharedMaterial != null)
                    {
                        DestroyImmediate(renderer.sharedMaterial);
                    }
                }
                DestroyImmediate(_previewTarget);
            }

            if (_previewUtility != null)
                _previewUtility.Cleanup();
        }

        private void InitializePreview()
        {
            if (_previewUtility != null) return;

            _previewUtility = new PreviewRenderUtility();

            string[] guids = AssetDatabase.FindAssets("Prefab_Card t:Prefab");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                _previewTarget = Instantiate(prefab);
            }
            else
            {
                _previewTarget = new GameObject("Virtual_CardPrefab");
                _previewTarget.AddComponent<CardArtBuilder>();
            }

            _previewTarget.hideFlags = HideFlags.HideAndDontSave;
            _previewUtility.AddSingleGO(_previewTarget);
            _previewBuilder = _previewTarget.GetComponentInChildren<CardArtBuilder>();

            SpriteRenderer[] renderers = _previewTarget.GetComponentsInChildren<SpriteRenderer>(true);
            foreach (var renderer in renderers)
            {
                if (renderer.sharedMaterial != null)
                {
                    // Create an isolated instance of the material just for this preview clone
                    renderer.sharedMaterial = new Material(renderer.sharedMaterial);
                }
            }

            _previewUtility.camera.nearClipPlane = 1f;
            _previewUtility.camera.clearFlags = CameraClearFlags.SolidColor;
            _previewUtility.camera.backgroundColor = new Color(0.15f, 0.15f, 0.15f, 1f);

            _previewUtility.camera.transparencySortMode = TransparencySortMode.CustomAxis;
            _previewUtility.camera.transparencySortAxis = new Vector3(0, 0, 1);
        }

        private void OnGUI()
        {
            // Safety check for Domain Reloads
            if (_previewUtility == null) InitializePreview();

            DrawHeader();

            if (_activeProfile == null)
            {
                EditorGUILayout.HelpBox("Please assign or create a CardArtProfile to begin editing.", MessageType.Info);
                return;
            }

            // Rebuild the serialized object if Unity wiped it during compilation
            if (_serializedProfile == null)
            {
                SetProfile(_activeProfile);
            }

            _serializedProfile.Update();

            DrawInteractivePreview();

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            DrawGlobalControls();
            EditorGUILayout.Space(10);
            DrawLayerSections();

            EditorGUILayout.EndScrollView();

            if (_serializedProfile.hasModifiedProperties)
            {
                _serializedProfile.ApplyModifiedProperties();
                if (_previewBuilder != null)
                {
                    _previewBuilder.BuildArt(_activeProfile);
                }
            }
        }

        private void DrawInteractivePreview()
        {
            float dynamicHeight = Mathf.Max(200f, position.height * 0.5f);
            Rect previewRect = GUILayoutUtility.GetRect(0f, dynamicHeight, GUILayout.ExpandWidth(true));

            if (previewRect.width <= 0 || previewRect.height <= 0) return;

            // SAFETY SYNC: Force the virtual preview prefab to aggressively update and match the profile
            // This prevents the Live Preview from getting "stuck" when you edit assets externally
            if (_previewBuilder != null && _activeProfile != null)
            {
                _previewBuilder.BuildArt(_activeProfile);

                CATScenePreview.ApplySoloFilterToBuilder(_previewBuilder);
            }

            Event evt = Event.current;
            if (previewRect.Contains(evt.mousePosition))
            {
                if (evt.type == EventType.MouseDrag)
                {
                    if (evt.button == 0) // Left click: Rotate
                    {
                        _dragRotation.x -= evt.delta.y * 0.5f;
                        _dragRotation.y -= evt.delta.x * 0.5f;
                        evt.Use();
                        Repaint();
                    }
                    else if (evt.button == 2) // Middle click: Pan
                    {
                        // Adjust pan sensitivity based on zoom level for consistent feel
                        float panSpeed = _cameraDistance * 0.002f;
                        _cameraPan.x -= evt.delta.x * panSpeed;
                        _cameraPan.y += evt.delta.y * panSpeed;
                        evt.Use();
                        Repaint();
                    }
                }
                else if (evt.type == EventType.ScrollWheel) // Scroll: Zoom
                {
                    _cameraDistance += evt.delta.y * 0.5f;
                    _cameraDistance = Mathf.Clamp(_cameraDistance, 5f, 150f);
                    evt.Use();
                    Repaint();
                }
            }

            // Apply target rotation
            if (_previewTarget != null)
            {
                _previewTarget.transform.rotation = Quaternion.Euler(_dragRotation.x, _dragRotation.y, 0f);
            }

            // Apply dynamic camera position and clip planes
            _previewUtility.camera.transform.position = new Vector3(_cameraPan.x, _cameraPan.y, -_cameraDistance);
            _previewUtility.camera.farClipPlane = _cameraDistance + 50f;

            _previewUtility.BeginPreview(previewRect, GUIStyle.none);
            _previewUtility.Render();
            Texture previewTexture = _previewUtility.EndPreview();

            GUI.DrawTexture(previewRect, previewTexture);

            // Overlays
            GUI.Label(new Rect(previewRect.x + 5, previewRect.y + 5, 300, 20), "Live 3D Viewport (L-Click Rotate, M-Click Pan, Scroll Zoom)", EditorStyles.boldLabel);

            if (GUI.Button(new Rect(previewRect.x + previewRect.width - 85, previewRect.y + 5, 80, 20), "Recenter"))
            {
                _dragRotation = Vector2.zero;
                _cameraPan = Vector2.zero;
                _cameraDistance = DEFAULT_CAMERA_DISTANCE;
                Repaint();
            }
        }

        private void DrawHeader()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUI.BeginChangeCheck();
            CardArtProfile newProfile = (CardArtProfile)EditorGUILayout.ObjectField("Active Profile", _activeProfile, typeof(CardArtProfile), false);
            if (EditorGUI.EndChangeCheck())
            {
                SetProfile(newProfile);
            }
            EditorGUILayout.EndVertical();
        }

        private void SetProfile(CardArtProfile profile)
        {
            _activeProfile = profile;
            if (_activeProfile != null)
            {
                _serializedProfile = new SerializedObject(_activeProfile);
                if (_previewBuilder != null)
                {
                    _previewBuilder.BuildArt(_activeProfile);
                }
            }
            else
            {
                _serializedProfile = null;
            }
        }

        private void DrawGlobalControls()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Global Parallax & Template Controls", EditorStyles.boldLabel);

            SerializedProperty baseZStep = _serializedProfile.FindProperty("BaseZStep");
            EditorGUILayout.PropertyField(baseZStep);

            SerializedProperty globalDepth = _serializedProfile.FindProperty("GlobalDepthMultiplier");
            EditorGUILayout.PropertyField(globalDepth);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Copy Layout Template"))
            {
                _copiedDepthMult = globalDepth.floatValue;
                _copiedSubjectOffset = _serializedProfile.FindProperty("SubjectOffset").vector2Value;
                _copiedSubjectScale = _serializedProfile.FindProperty("SubjectScale").floatValue;
                _hasCopiedTemplate = true;
                Debug.Log("[CAT] Layout template copied to clipboard.");
            }

            EditorGUI.BeginDisabledGroup(!_hasCopiedTemplate);
            if (GUILayout.Button("Paste Layout Template"))
            {
                globalDepth.floatValue = _copiedDepthMult;
                _serializedProfile.FindProperty("SubjectOffset").vector2Value = _copiedSubjectOffset;
                _serializedProfile.FindProperty("SubjectScale").floatValue = _copiedSubjectScale;
                Debug.Log("[CAT] Layout template applied.");
            }
            EditorGUI.EndDisabledGroup();

            if (GUILayout.Button("Reset Solo Mode"))
            {
                CATScenePreview.ResetSoloMode();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void DrawLayerSections()
        {
            CATLayerDrawer.DrawArraySection("Foreground Layers", _serializedProfile.FindProperty("ForegroundLayers"), LayerType.Foreground);

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Frame & Subject", EditorStyles.boldLabel);
            CATLayerDrawer.DrawSingleLayer("Frame Layer (Anchor)", _serializedProfile.FindProperty("FrameLayer"));
            EditorGUILayout.PropertyField(_serializedProfile.FindProperty("MaskLayer"), new GUIContent("Mask Layer (Window Cutout)"));
            CATLayerDrawer.DrawSingleLayer("Nameplate Layer", _serializedProfile.FindProperty("NameplateLayer"));
            EditorGUILayout.EndVertical();

            CATLayerDrawer.DrawArraySection("Subject Layers", _serializedProfile.FindProperty("SubjectLayers"), LayerType.Subject);
            CATLayerDrawer.DrawArraySection("Background Layers", _serializedProfile.FindProperty("BackgroundLayers"), LayerType.Background);
        }
    }

    public enum LayerType
    {
        Background,
        Subject,
        Frame,
        Foreground,
        Nameplate
    }
}