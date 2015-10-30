//
// Lattice - fractal-deformed lattice renderer
//
using UnityEngine;
using UnityEngine.Rendering;

namespace Kvant
{
    [ExecuteInEditMode]
    [AddComponentMenu("Kvant/Lattice")]
    public partial class Lattice : MonoBehaviour
    {
        #region Basic Properties

        [SerializeField]
        int _columns = 100;

        public int columns {
            get { return _columns; }
        }

        [SerializeField]
        int _rows = 100;

        public int rows {
            get { return _totalRows; }
        }

        [SerializeField]
        Vector2 _extent = Vector2.one * 50;

        public Vector2 extent {
            get { return _extent; }
            set { _extent = value; }
        }

        [SerializeField]
        Vector2 _offset = Vector2.zero;

        public Vector2 offset {
            get { return _offset; }
            set { _offset = value; }
        }

        #endregion

        #region Noise Parameters

        [SerializeField, Range(0, 1)]
        float _noiseFrequency = 0.3f;

        public float noiseFrequency {
            get { return _noiseFrequency; }
            set { _noiseFrequency = value; }
        }

        [SerializeField, Range(1, 5)]
        int _noiseDepth = 3;

        public int noiseDepth {
            get { return _noiseDepth; }
            set { _noiseDepth = value; }
        }

        [SerializeField]
        float _noiseClampMin = -1.0f;

        public float noiseClampMin {
            get { return _noiseClampMin; }
            set { _noiseClampMin = value; }
        }

        [SerializeField]
        float _noiseClampMax = 1.0f;

        public float noiseClampMax {
            get { return _noiseClampMax; }
            set { _noiseClampMax = value; }
        }

        [SerializeField]
        float _noiseElevation = 1.0f;

        public float noiseElevation {
            get { return _noiseElevation; }
            set { _noiseElevation = value; }
        }

        [SerializeField, Range(0, 1)]
        float _noiseWarp = 0.0f;

        public float noiseWarp {
            get { return _noiseWarp; }
            set { _noiseWarp = value; }
        }

        [SerializeField]
        Vector2 _noiseOffset;

        public Vector2 noiseOffset {
            get { return _noiseOffset; }
            set { _noiseOffset = value; }
        }

        #endregion

        #region Render Settings

        [SerializeField]
        Material _material;
        bool _owningMaterial; // whether owning the material

        public Material sharedMaterial {
            get { return _material; }
            set { _material = value; }
        }

        public Material material {
            get {
                if (!_owningMaterial) {
                    _material = Instantiate<Material>(_material);
                    _owningMaterial = true;
                }
                return _material;
            }
            set {
                if (_owningMaterial) Destroy(_material, 0.1f);
                _material = value;
                _owningMaterial = false;
            }
        }

        [SerializeField]
        ShadowCastingMode _castShadows;

        public ShadowCastingMode shadowCastingMode {
            get { return _castShadows; }
            set { _castShadows = value; }
        }

        [SerializeField]
        bool _receiveShadows = false;

        public bool receiveShadows {
            get { return _receiveShadows; }
            set { _receiveShadows = value; }
        }

        [SerializeField, ColorUsage(true, true, 0, 8, 0.125f, 3)]
        Color _lineColor = new Color(0, 0, 0, 0.4f);

        public Color lineColor {
            get { return _lineColor; }
            set { _lineColor = value; }
        }

        #endregion

        #region Editor Properties

        [SerializeField]
        bool _debug;

        #endregion

        #region Built-in Resources

        [SerializeField] Shader _kernelShader;
        [SerializeField] Shader _lineShader;
        [SerializeField] Shader _debugShader;

        #endregion

        #region Private Variables And Properties

        int _rowsPerSegment;
        int _totalRows;

        RenderTexture _positionBuffer;
        RenderTexture _normalBuffer1;
        RenderTexture _normalBuffer2;

        BulkMesh _bulkMesh;

        Material _kernelMaterial;
        Material _lineMaterial;
        Material _debugMaterial;

        bool _needsReset = true;

        float XOffset {
            get { return Mathf.Repeat(_offset.x, _extent.x / (_columns + 1)); }
        }

        float YOffset {
            get { return Mathf.Repeat(_offset.y, _extent.y / (_totalRows + 1) * 2); }
        }

        float UOffset {
            get { return XOffset - _offset.x; }
        }

        float VOffset {
            get { return YOffset - _offset.y; }
        }

        void UpdateColumnAndRowCounts()
        {
            // Sanitize the numbers.
            _columns = Mathf.Clamp(_columns, 4, 4096);
            _rows = Mathf.Clamp(_rows, 4, 4096);

            // Total number of vertices.
            var total_vc = (_columns + 1) * (_rows + 1) * 6;

            // Number of segments.
            var segments = total_vc / 65000 + 1;

            _rowsPerSegment = segments > 1 ? (_rows / segments) / 2 * 2 : _rows;
            _totalRows = _rowsPerSegment * segments;
        }

        #endregion

        #region Resource Management

        public void NotifyConfigChange()
        {
            _needsReset = true;
        }

        Material CreateMaterial(Shader shader)
        {
            var material = new Material(shader);
            material.hideFlags = HideFlags.DontSave;
            return material;
        }

        RenderTexture CreateBuffer()
        {
            var width = (_columns + 1) * 2;
            var height = _totalRows + 1;
            var buffer = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBFloat);
            buffer.hideFlags = HideFlags.DontSave;
            buffer.filterMode = FilterMode.Point;
            buffer.wrapMode = TextureWrapMode.Repeat;
            return buffer;
        }

        void UpdateKernelShader()
        {
            var m = _kernelMaterial;

            m.SetVector("_Extent", _extent);
            m.SetVector("_Offset", new Vector2(UOffset, VOffset) + _noiseOffset);
            m.SetFloat("_Frequency", _noiseFrequency);
            m.SetVector("_Amplitude", new Vector3(_noiseWarp, 1, _noiseWarp) * _noiseElevation);
            m.SetVector("_ClampRange", new Vector2(_noiseClampMin, _noiseClampMax) * 1.415f);

            if (_noiseWarp > 0.0f)
                m.EnableKeyword("ENABLE_WARP");
            else
                m.DisableKeyword("ENABLE_WARP");

            for (var i = 1; i <= 5; i++)
                if (i == _noiseDepth)
                    m.EnableKeyword("DEPTH" + i);
                else
                    m.DisableKeyword("DEPTH" + i);
        }

        void ResetResources()
        {
            UpdateColumnAndRowCounts();

            if (_bulkMesh == null)
                _bulkMesh = new BulkMesh(_columns, _rowsPerSegment, _totalRows);
            else
                _bulkMesh.Rebuild(_columns, _rowsPerSegment, _totalRows);

            if (_positionBuffer) DestroyImmediate(_positionBuffer);
            if (_normalBuffer1)  DestroyImmediate(_normalBuffer1);
            if (_normalBuffer2)  DestroyImmediate(_normalBuffer2);

            _positionBuffer = CreateBuffer();
            _normalBuffer1  = CreateBuffer();
            _normalBuffer2  = CreateBuffer();

            if (!_kernelMaterial) _kernelMaterial = CreateMaterial(_kernelShader);
            if (!_lineMaterial)   _lineMaterial   = CreateMaterial(_lineShader);
            if (!_debugMaterial)  _debugMaterial  = CreateMaterial(_debugShader);

            _lineMaterial.SetTexture("_PositionBuffer", _positionBuffer);

            _needsReset = false;
        }

        #endregion

        #region MonoBehaviour Functions

        void Reset()
        {
            _needsReset = true;
        }

        void OnDestroy()
        {
            if (_bulkMesh != null) _bulkMesh.Release();
            if (_positionBuffer) DestroyImmediate(_positionBuffer);
            if (_normalBuffer1)  DestroyImmediate(_normalBuffer1);
            if (_normalBuffer2)  DestroyImmediate(_normalBuffer2);
            if (_kernelMaterial) DestroyImmediate(_kernelMaterial);
            if (_lineMaterial)   DestroyImmediate(_lineMaterial);
            if (_debugMaterial)  DestroyImmediate(_debugMaterial);
        }

        void LateUpdate()
        {
            if (_needsReset) ResetResources();

            // Call the kernels.
            UpdateKernelShader();
            Graphics.Blit(null, _positionBuffer, _kernelMaterial, 0);
            Graphics.Blit(_positionBuffer, _normalBuffer1, _kernelMaterial, 1);
            Graphics.Blit(_positionBuffer, _normalBuffer2, _kernelMaterial, 2);

            // Update the line material.
            _lineMaterial.SetColor("_Color", _lineColor);

            // Make a material property block for the following drawcalls.
            var props1 = new MaterialPropertyBlock();
            var props2 = new MaterialPropertyBlock();

            props1.SetTexture("_PositionBuffer", _positionBuffer);
            props2.SetTexture("_PositionBuffer", _positionBuffer);

            props1.SetTexture("_NormalBuffer", _normalBuffer1);
            props2.SetTexture("_NormalBuffer", _normalBuffer2);

            var mapOffs = new Vector3(UOffset, 0, VOffset);
            props1.SetVector("_MapOffset", mapOffs);
            props2.SetVector("_MapOffset", mapOffs);

            props1.SetFloat("_UseBuffer", 1);
            props2.SetFloat("_UseBuffer", 1);

            // Temporary variables.
            var mesh = _bulkMesh.mesh;
            var position = transform.position;
            var rotation = transform.rotation;
            var uv = new Vector2(0.5f / _positionBuffer.width, 0);

            position += transform.right * XOffset;
            position += transform.forward * YOffset;

            // Draw mesh segments.
            for (var i = 0; i < _totalRows; i += _rowsPerSegment)
            {
                uv.y = (0.5f + i) / _positionBuffer.height;

                props1.SetVector("_BufferOffset", uv);
                props2.SetVector("_BufferOffset", uv);

                if (_material)
                {
                    // 1st half
                    Graphics.DrawMesh(
                        mesh, position, rotation,
                        _material, 0, null, 0, props1,
                        _castShadows, _receiveShadows);

                    // 2nd half
                    Graphics.DrawMesh(
                        mesh, position, rotation,
                        _material, 0, null, 1, props2,
                        _castShadows, _receiveShadows);
                }

                // lines
                if (_lineColor.a > 0.0f)
                    Graphics.DrawMesh(
                        mesh, position, rotation,
                        _lineMaterial, 0, null, 2,
                        props1, false, false);
            }
        }

        void OnGUI()
        {
            if (_debug && Event.current.type.Equals(EventType.Repaint))
            {
                if (_debugMaterial && _positionBuffer && _normalBuffer1 && _normalBuffer2)
                {
                    var rect = new Rect(0, 0, (_columns + 1) * 2, _totalRows + 1);
                    Graphics.DrawTexture(rect, _positionBuffer, _debugMaterial);

                    rect.y += rect.height;
                    Graphics.DrawTexture(rect, _normalBuffer1, _debugMaterial);

                    rect.y += rect.height;
                    Graphics.DrawTexture(rect, _normalBuffer2, _debugMaterial);
                }
            }
        }

        #endregion
    }
}
