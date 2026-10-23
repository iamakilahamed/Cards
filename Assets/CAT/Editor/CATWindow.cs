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

        // Sculpting Module
        private bool _isSculptMode = false;
        private float _brushRadius = 1.5f;
        private float _brushStrength = 0.5f;
        private LayerType _sculptTargetType = LayerType.Subject;
        private int _sculptTargetIndex = 0;
        private bool _isSculptDragging = false;

        // Clipboard
        private static float _copiedDepthMult = 1f;
        private static Vector2 _copiedSubjectOffset = Vector2.zero;
        private static float _copiedSubjectScale = 1f;
        private static bool _hasCopiedTemplate = false;

        [MenuItem("Tools/Card Art Tool (CAT)")]
        public static void OpenWindow()
        {
            CATWindow window = GetWindow<CATWindow>("Card Art Tool (CAT)");
            window.minSize = new Vector2(400, 600);
            window.Show();
        }

        private void OnEnable()
        {
            wantsMouseMove = true;
            InitializePreview();
        }

        private void OnDisable()
        {
            if (_previewTarget != null) DestroyImmediate(_previewTarget);
            if (_previewUtility != null) _previewUtility.Cleanup();
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
                CardArtBuilder builder = _previewTarget.AddComponent<CardArtBuilder>();
                SetupVirtualBuilder(builder);
            }

            _previewTarget.hideFlags = HideFlags.HideAndDontSave;
            _previewUtility.AddSingleGO(_previewTarget);
            _previewBuilder = _previewTarget.GetComponentInChildren<CardArtBuilder>();

            _previewUtility.camera.nearClipPlane = 1f;
            _previewUtility.camera.clearFlags = CameraClearFlags.SolidColor;
            _previewUtility.camera.backgroundColor = new Color(0.15f, 0.15f, 0.15f, 1f);
            _previewUtility.camera.transparencySortMode = TransparencySortMode.CustomAxis;
            _previewUtility.camera.transparencySortAxis = new Vector3(0, 0, 1);
        }

        private void SetupVirtualBuilder(CardArtBuilder builder)
        {
            SerializedObject so = new SerializedObject(builder);

            Transform CreateSlot(string n)
            {
                GameObject go = new GameObject(n);
                go.transform.SetParent(builder.transform);
                return go.transform;
            }

            so.FindProperty("_frameSlot").objectReferenceValue = CreateSlot("Frame");
            so.FindProperty("_maskSlot").objectReferenceValue = CreateSlot("Mask");
            so.FindProperty("_nameplateSlot").objectReferenceValue = CreateSlot("Nameplate");

            void SetupArray(string propName, string prefix, int count)
            {
                SerializedProperty prop = so.FindProperty(propName);
                prop.arraySize = count;
                for (int i = 0; i < count; i++)
                {
                    prop.GetArrayElementAtIndex(i).objectReferenceValue = CreateSlot($"{prefix}{i + 1}");
                }
            }

            SetupArray("_backgroundSlots", "BackgroundLayer", 3);
            SetupArray("_subjectSlots", "SubjectLayer", 3);
            SetupArray("_foregroundSlots", "ForegroundLayer", 3);

            // Auto-assign custom materials to the virtual prefab
            string[] maskGuids = AssetDatabase.FindAssets("M_CAT_Mask t:Material");
            if (maskGuids.Length > 0) so.FindProperty("MaskMaterial").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(maskGuids[0]));
            string[] artGuids = AssetDatabase.FindAssets("M_CAT_Art t:Material");
            if (artGuids.Length > 0) so.FindProperty("ArtMaterial").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(artGuids[0]));
            so.ApplyModifiedProperties();
        }

        private void OnGUI()
        {
            if (_previewUtility == null) InitializePreview();

            DrawHeader();

            if (_activeProfile == null)
            {
                EditorGUILayout.HelpBox("Please assign or create a CardArtProfile to begin editing.", MessageType.Info);
                return;
            }

            if (_serializedProfile == null) SetProfile(_activeProfile);
            _serializedProfile.Update();

            DrawInteractivePreview();
            DrawSculptingModule();

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
                    CATScenePreview.ApplySoloFilterToBuilder(_previewBuilder);
                }
            }
        }

        private void DrawInteractivePreview()
        {
            float dynamicHeight = Mathf.Max(200f, position.height * 0.45f);
            Rect previewRect = GUILayoutUtility.GetRect(0f, dynamicHeight, GUILayout.ExpandWidth(true));
            if (previewRect.width <= 0 || previewRect.height <= 0) return;

            Event evt = Event.current;
            bool isAlt = evt.modifiers == EventModifiers.Alt;
            int controlID = GUIUtility.GetControlID("PreviewViewport".GetHashCode(), FocusType.Passive);

            if (previewRect.Contains(evt.mousePosition))
            {
                if (evt.type == EventType.MouseMove && _isSculptMode)
                {
                    Repaint();
                }
                else if (evt.type == EventType.MouseDown)
                {
                    GUIUtility.hotControl = controlID;
                    if (evt.button == 0 && !isAlt && _isSculptMode)
                    {
                        _isSculptDragging = true;
                        HandleSculptInput(previewRect, evt);
                        evt.Use();
                    }
                }
                else if (evt.type == EventType.MouseDrag)
                {
                    if (evt.button == 0 && !isAlt && _isSculptMode && _isSculptDragging)
                    {
                        HandleSculptInput(previewRect, evt);
                        evt.Use();
                    }
                    else if ((evt.button == 0 && isAlt) || (evt.button == 0 && !_isSculptMode))
                    {
                        _dragRotation.x -= evt.delta.y * 0.5f;
                        _dragRotation.y -= evt.delta.x * 0.5f;
                        evt.Use();
                        Repaint();
                    }
                    else if (evt.button == 2)
                    {
                        float panSpeed = _cameraDistance * 0.002f;
                        _cameraPan.x -= evt.delta.x * panSpeed;
                        _cameraPan.y += evt.delta.y * panSpeed;
                        evt.Use();
                        Repaint();
                    }
                }
                else if (evt.type == EventType.ScrollWheel)
                {
                    _cameraDistance += evt.delta.y * 0.5f;
                    _cameraDistance = Mathf.Clamp(_cameraDistance, 5f, 150f);
                    evt.Use();
                    Repaint();
                }
            }

            if (evt.type == EventType.MouseUp && GUIUtility.hotControl == controlID)
            {
                GUIUtility.hotControl = 0;
                if (_isSculptDragging)
                {
                    _isSculptDragging = false;
                    EditorUtility.SetDirty(_activeProfile);
                    _previewBuilder.BuildArt(_activeProfile);
                }
                evt.Use();
            }

            if (_previewTarget != null)
            {
                _previewTarget.transform.rotation = Quaternion.Euler(_dragRotation.x, _dragRotation.y, 0f);
            }

            _previewUtility.camera.transform.position = new Vector3(_cameraPan.x, _cameraPan.y, -_cameraDistance);
            _previewUtility.camera.farClipPlane = _cameraDistance + 50f;

            _previewUtility.BeginPreview(previewRect, GUIStyle.none);
            _previewUtility.Render();
            Texture previewTexture = _previewUtility.EndPreview();

            GUI.DrawTexture(previewRect, previewTexture);

            if (_isSculptMode && previewRect.Contains(evt.mousePosition) && evt.type == EventType.Repaint)
            {
                Transform targetSlot = GetTargetSlotTransform();
                if (targetSlot != null)
                {
                    Vector2 localMouse = evt.mousePosition - previewRect.position;
                    localMouse.y = previewRect.height - localMouse.y;
                    Ray ray = _previewUtility.camera.ScreenPointToRay(new Vector3(localMouse.x, localMouse.y, 0));
                    Plane layerPlane = new Plane(targetSlot.forward, targetSlot.position);

                    if (layerPlane.Raycast(ray, out float enter))
                    {
                        Vector3 worldHit = ray.GetPoint(enter);
                        float dynamicRadius = GetDynamicBrushRadius(worldHit);

                        int segments = 36;
                        Vector3[] guiPoints = new Vector3[segments + 1];

                        for (int i = 0; i <= segments; i++)
                        {
                            float angle = i * Mathf.PI * 2f / segments;
                            Vector3 offset = (targetSlot.right * Mathf.Cos(angle) + targetSlot.up * Mathf.Sin(angle)) * dynamicRadius;
                            Vector3 point3D = worldHit + offset;

                            Vector3 screenPt = _previewUtility.camera.WorldToScreenPoint(point3D);
                            guiPoints[i] = new Vector3(previewRect.x + screenPt.x, previewRect.y + previewRect.height - screenPt.y, 0);
                        }

                        Handles.color = evt.shift ? new Color(1f, 0.2f, 0.2f, 0.8f) : new Color(0.2f, 0.8f, 1f, 0.8f);
                        Handles.DrawPolyLine(guiPoints);
                    }
                }
            }

            GUI.Label(new Rect(previewRect.x + 5, previewRect.y + 5, 300, 20), _isSculptMode ? "SCULPT MODE ACTIVE (Alt+Click to Rotate)" : "Live 3D Viewport", EditorStyles.boldLabel);

            if (GUI.Button(new Rect(previewRect.x + previewRect.width - 85, previewRect.y + 5, 80, 20), "Recenter"))
            {
                _dragRotation = Vector2.zero;
                _cameraPan = Vector2.zero;
                _cameraDistance = DEFAULT_CAMERA_DISTANCE;
                Repaint();
            }
        }

        private void HandleSculptInput(Rect previewRect, Event evt)
        {
            CardLayer targetLayer = GetSelectedSculptLayer();
            if (targetLayer == null || targetLayer.Artwork == null) return;

            Transform targetSlot = GetTargetSlotTransform();
            if (targetSlot == null) return;

            MeshFilter filter = targetSlot.GetComponent<MeshFilter>();
            if (filter == null || filter.sharedMesh == null) return;

            Vector2 localMouse = evt.mousePosition - previewRect.position;
            localMouse.y = previewRect.height - localMouse.y;
            Ray ray = _previewUtility.camera.ScreenPointToRay(new Vector3(localMouse.x, localMouse.y, 0));

            Plane layerPlane = new Plane(targetSlot.forward, targetSlot.position);
            if (layerPlane.Raycast(ray, out float enter))
            {
                Vector3 worldHit = ray.GetPoint(enter);
                float dynamicRadius = GetDynamicBrushRadius(worldHit);
                SculptVertices(targetLayer, targetSlot, filter.sharedMesh, worldHit, evt.shift, dynamicRadius);
            }
        }

        private void SculptVertices(CardLayer layer, Transform slot, Mesh mesh, Vector3 worldHit, bool isPushing, float dynamicRadius)
        {
            Vector3[] vertices = mesh.vertices;
            if (layer.VertexDepths == null || layer.VertexDepths.Length != vertices.Length) layer.VertexDepths = new float[vertices.Length];

            float dirMult = isPushing ? -1f : 1f;
            float str = _brushStrength * 0.05f;
            bool modified = false;

            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 worldVert = slot.TransformPoint(vertices[i]);
                float dist = Vector3.Distance(worldVert, worldHit);

                if (dist < _brushRadius)
                {
                    float falloff = Mathf.SmoothStep(0f, 1f, 1f - (dist / dynamicRadius));
                    layer.VertexDepths[i] += str * dirMult * falloff;
                    vertices[i].z = layer.VertexDepths[i];
                    modified = true;
                }
            }

            if (modified)
            {
                mesh.vertices = vertices;
                mesh.RecalculateNormals();
                Repaint();
            }
        }

        private void DrawSculptingModule()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("2.5D Sculpting Module", EditorStyles.boldLabel);
            GUI.backgroundColor = _isSculptMode ? Color.green : Color.white;
            if (GUILayout.Button(_isSculptMode ? "Sculpt Mode: ON" : "Sculpt Mode: OFF", GUILayout.Height(25))) _isSculptMode = !_isSculptMode;
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

            if (_isSculptMode)
            {
                EditorGUILayout.HelpBox("Left-Click & Drag to Pull. Shift + Left-Click to Push. Alt+Click to Rotate Camera.", MessageType.Info);
                _sculptTargetType = (LayerType)EditorGUILayout.EnumPopup("Target Layer Type", _sculptTargetType);

                int maxIndex = 0;
                if (_activeProfile != null)
                {
                    maxIndex = _sculptTargetType switch
                    {
                        LayerType.Background => (_activeProfile.BackgroundLayers?.Length ?? 1) - 1,
                        LayerType.Subject => (_activeProfile.SubjectLayers?.Length ?? 1) - 1,
                        LayerType.Foreground => (_activeProfile.ForegroundLayers?.Length ?? 1) - 1,
                        _ => 0
                    };
                }
                _sculptTargetIndex = EditorGUILayout.IntSlider("Target Layer Index", _sculptTargetIndex, 0, Mathf.Max(0, maxIndex));

                EditorGUI.BeginChangeCheck();
                _brushRadius = EditorGUILayout.Slider("Brush Radius", _brushRadius, 0.1f, 10f);
                _brushStrength = EditorGUILayout.Slider("Brush Strength", _brushStrength, 0.1f, 5f);
                if (EditorGUI.EndChangeCheck() && _isSculptMode) Repaint();
            }
            EditorGUILayout.EndVertical();
        }

        private CardLayer GetSelectedSculptLayer()
        {
            if (_activeProfile == null) return null;
            return _sculptTargetType switch
            {
                LayerType.Background => (_activeProfile.BackgroundLayers.Length > _sculptTargetIndex) ? _activeProfile.BackgroundLayers[_sculptTargetIndex] : null,
                LayerType.Subject => (_activeProfile.SubjectLayers.Length > _sculptTargetIndex) ? _activeProfile.SubjectLayers[_sculptTargetIndex] : null,
                LayerType.Foreground => (_activeProfile.ForegroundLayers.Length > _sculptTargetIndex) ? _activeProfile.ForegroundLayers[_sculptTargetIndex] : null,
                LayerType.Frame => _activeProfile.FrameLayer,
                LayerType.Nameplate => _activeProfile.NameplateLayer,
                _ => null
            };
        }

        private Transform GetTargetSlotTransform()
        {
            if (_previewTarget == null) return null;
            string n = _sculptTargetType switch
            {
                LayerType.Background => $"BackgroundLayer{_sculptTargetIndex + 1}",
                LayerType.Subject => $"SubjectLayer{_sculptTargetIndex + 1}",
                LayerType.Foreground => $"ForegroundLayer{_sculptTargetIndex + 1}",
                LayerType.Frame => "Frame",
                LayerType.Nameplate => "Nameplate",
                _ => ""
            };

            Transform target = _previewTarget.transform.Find("VisualRoot/" + n);
            if (target == null) target = _previewTarget.transform.Find(n);

            return target;
        }

        private float GetDynamicBrushRadius(Vector3 worldHit)
        {
            float dist = Vector3.Distance(_previewUtility.camera.transform.position, worldHit);
            return _brushRadius * (dist * 0.015f);
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
                if (_previewBuilder != null) _previewBuilder.BuildArt(_activeProfile);
            }
            else _serializedProfile = null;
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
            EditorGUILayout.LabelField("Frame Layer", EditorStyles.boldLabel);
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