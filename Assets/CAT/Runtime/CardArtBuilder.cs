using UnityEngine;
using CAT.Data;

namespace CAT.Runtime
{
    [ExecuteAlways]
    public class CardArtBuilder : MonoBehaviour
    {
        [Header("Pre-Allocated Empty Transform Slots")]
        [SerializeField] private Transform[] _backgroundSlots;
        [SerializeField] private Transform[] _subjectSlots;
        [SerializeField] private Transform _frameSlot;
        [SerializeField] private Transform _maskSlot;
        [SerializeField] private Transform[] _foregroundSlots;
        [SerializeField] private Transform _nameplateSlot;

        [Header("Materials")]
        public Material MaskMaterial;
        public Material ArtMaterial;

        [Header("Editor Testing")]
        public CardArtProfile TestProfile;
        public bool LiveUpdate = true;

        // Dynamically generated ID to prevent cards from masking each other
        private int _uniqueStencilID;

        // Cache materials to prevent Editor memory leaks
        private Material _maskMat;
        private Material _bgMat;
        private Material _frameMat;
        private Material _subjectMat;
        private Material _fgMat;
        private Material _nameplateMat;
        private MaterialPropertyBlock _propBlock;

        private void OnEnable()
        {
            _uniqueStencilID = GetInstanceID();
            _propBlock = new MaterialPropertyBlock();
        }

        private void OnDisable()
        {
            if (_maskMat != null) DestroyImmediate(_maskMat);
            if (_bgMat != null) DestroyImmediate(_bgMat);
            if (_frameMat != null) DestroyImmediate(_frameMat);
            if (_subjectMat != null) DestroyImmediate(_subjectMat);
            if (_fgMat != null) DestroyImmediate(_fgMat);
            if (_nameplateMat != null) DestroyImmediate(_nameplateMat);
        }

        private void Update()
        {
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
            if (profile == null || MaskMaterial == null || ArtMaterial == null) return;

            if (_propBlock == null) _propBlock = new MaterialPropertyBlock();

            float depthMult = profile.GlobalDepthMultiplier;
            float zStep = profile.BaseZStep;

            // Initialize or update cached materials
            if (_maskMat == null) _maskMat = new Material(MaskMaterial);
            _maskMat.SetInt("_StencilID", _uniqueStencilID);

            if (_bgMat == null) _bgMat = new Material(ArtMaterial);
            _bgMat.SetInt("_StencilID", _uniqueStencilID);
            _bgMat.SetInt("_StencilComp", 3); // Equal
            _bgMat.renderQueue = 3000;

            if (_frameMat == null) _frameMat = new Material(ArtMaterial);
            _frameMat.SetInt("_StencilID", _uniqueStencilID);
            _frameMat.SetInt("_StencilComp", 8); // Always
            _frameMat.renderQueue = 3010;

            if (_subjectMat == null) _subjectMat = new Material(ArtMaterial);
            _subjectMat.SetInt("_StencilID", _uniqueStencilID);
            _subjectMat.SetInt("_StencilComp", 8); // Always
            _subjectMat.renderQueue = 3020;

            if (_fgMat == null) _fgMat = new Material(ArtMaterial);
            _fgMat.SetInt("_StencilID", _uniqueStencilID);
            _fgMat.SetInt("_StencilComp", 8); // Always
            _fgMat.renderQueue = 3030;

            if (_nameplateMat == null) _nameplateMat = new Material(ArtMaterial);
            _nameplateMat.SetInt("_StencilID", _uniqueStencilID);
            _nameplateMat.SetInt("_StencilComp", 8); // Always
            _nameplateMat.renderQueue = 3100;

            // Build Mask
            if (profile.MaskLayer != null && _maskSlot != null)
            {
                ApplyMeshLayer(_maskSlot, profile.MaskLayer, 0, Vector2.zero, 1f, 1, null, _maskMat);
            }

            // Build Masked Backgrounds
            for (int i = 0; i < _backgroundSlots.Length; i++)
            {
                if (profile.BackgroundLayers != null && i < profile.BackgroundLayers.Length)
                {
                    CardLayer layer = profile.BackgroundLayers[i];
                    float finalZ = ((i + 1) * zStep * depthMult) + layer.ZOffset;
                    ApplyMeshLayer(_backgroundSlots[i], layer.Artwork, finalZ, layer.Offset, layer.Scale, layer.MeshResolution, layer.VertexDepths, _bgMat);
                }
            }

            // Frame
            if (profile.FrameLayer != null && _frameSlot != null)
            {
                float frameZ = profile.FrameLayer.ZOffset;
                ApplyMeshLayer(_frameSlot, profile.FrameLayer.Artwork, frameZ, profile.FrameLayer.Offset, profile.FrameLayer.Scale, profile.FrameLayer.MeshResolution, profile.FrameLayer.VertexDepths, _frameMat);
            }

            // Subjects
            for (int i = 0; i < _subjectSlots.Length; i++)
            {
                if (profile.SubjectLayers != null && i < profile.SubjectLayers.Length)
                {
                    CardLayer layer = profile.SubjectLayers[i];
                    float finalZ = (-0.1f * (i + 1) * zStep * depthMult) + layer.ZOffset;
                    ApplyMeshLayer(_subjectSlots[i], layer.Artwork, finalZ, layer.Offset, layer.Scale, layer.MeshResolution, layer.VertexDepths, _subjectMat);
                }
            }

            // Foregrounds
            for (int i = 0; i < _foregroundSlots.Length; i++)
            {
                if (profile.ForegroundLayers != null && i < profile.ForegroundLayers.Length)
                {
                    CardLayer layer = profile.ForegroundLayers[i];
                    float finalZ = (-(i + 1) * zStep * depthMult) + layer.ZOffset;
                    ApplyMeshLayer(_foregroundSlots[i], layer.Artwork, finalZ, layer.Offset, layer.Scale, layer.MeshResolution, layer.VertexDepths, _fgMat);
                }
            }

            // Nameplate
            if (profile.NameplateLayer != null && _nameplateSlot != null)
            {
                int totalFg = profile.ForegroundLayers?.Length ?? 0;
                float nameplateFinalZ = (-(totalFg + 2) * zStep * depthMult) + profile.NameplateLayer.ZOffset;
                ApplyMeshLayer(_nameplateSlot, profile.NameplateLayer.Artwork, nameplateFinalZ, profile.NameplateLayer.Offset, profile.NameplateLayer.Scale, profile.NameplateLayer.MeshResolution, profile.NameplateLayer.VertexDepths, _nameplateMat);
            }
        }

        private void ApplyMeshLayer(Transform slot, Sprite artwork, float targetZ, Vector2 offset, float scale, int resolution, float[] vertexDepths, Material mat)
        {
            if (slot == null || artwork == null) return;

            MeshFilter filter = slot.GetComponent<MeshFilter>();
            if (filter == null) filter = slot.gameObject.AddComponent<MeshFilter>();

            MeshRenderer renderer = slot.GetComponent<MeshRenderer>();
            if (renderer == null) renderer = slot.gameObject.AddComponent<MeshRenderer>();

            MeshCollider collider = slot.GetComponent<MeshCollider>();
            if (collider == null) collider = slot.gameObject.AddComponent<MeshCollider>();

            // Retrieve existing mesh to prevent memory leaks, or create one if it doesn't exist
            Mesh mesh = filter.sharedMesh;
            if (mesh == null || mesh.name != "Generated_Card_Grid")
            {
                mesh = new Mesh();
                mesh.name = "Generated_Card_Grid";
            }
            else
            {
                mesh.Clear();
            }

            GenerateGridMesh(mesh, artwork, resolution, vertexDepths);
            filter.sharedMesh = mesh;

            renderer.sharedMaterial = mat;
            renderer.GetPropertyBlock(_propBlock);
            _propBlock.SetTexture("_MainTex", artwork.texture);
            renderer.SetPropertyBlock(_propBlock);

            collider.sharedMesh = mesh;

            slot.localPosition = new Vector3(offset.x, offset.y, targetZ);
            slot.localScale = new Vector3(scale, scale, 1f);
        }

        private void GenerateGridMesh(Mesh mesh, Sprite sprite, int resolution, float[] vertexDepths)
        {
            float width = sprite.rect.width / sprite.pixelsPerUnit;
            float height = sprite.rect.height / sprite.pixelsPerUnit;

            int numVerts = (resolution + 1) * (resolution + 1);
            Vector3[] vertices = new Vector3[numVerts];
            Vector2[] uvs = new Vector2[numVerts];
            Color[] colors = new Color[numVerts];
            int[] triangles = new int[resolution * resolution * 6];

            float uvMinX = sprite.rect.xMin / sprite.texture.width;
            float uvMaxX = sprite.rect.xMax / sprite.texture.width;
            float uvMinY = sprite.rect.yMin / sprite.texture.height;
            float uvMaxY = sprite.rect.yMax / sprite.texture.height;

            int vertIndex = 0;
            int triIndex = 0;

            bool hasValidDepths = vertexDepths != null && vertexDepths.Length == numVerts;

            for (int y = 0; y <= resolution; y++)
            {
                for (int x = 0; x <= resolution; x++)
                {
                    float xPos = ((float)x / resolution - 0.5f) * width;
                    float yPos = ((float)y / resolution - 0.5f) * height;
                    float zPos = hasValidDepths ? vertexDepths[vertIndex] : 0f;

                    vertices[vertIndex] = new Vector3(xPos, yPos, zPos);

                    // Interpolate using the corrected bounding box
                    float uvX = Mathf.Lerp(uvMinX, uvMaxX, (float)x / resolution);
                    float uvY = Mathf.Lerp(uvMinY, uvMaxY, (float)y / resolution);
                    uvs[vertIndex] = new Vector2(uvX, uvY);

                    colors[vertIndex] = Color.white;

                    if (x < resolution && y < resolution)
                    {
                        triangles[triIndex + 0] = vertIndex;
                        triangles[triIndex + 1] = vertIndex + resolution + 1;
                        triangles[triIndex + 2] = vertIndex + 1;
                        triangles[triIndex + 3] = vertIndex + 1;
                        triangles[triIndex + 4] = vertIndex + resolution + 1;
                        triangles[triIndex + 5] = vertIndex + resolution + 2;
                        triIndex += 6;
                    }
                    vertIndex++;
                }
            }

            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.colors = colors;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
        }
    }
}