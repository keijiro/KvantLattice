//
// Lattice - fractal-deformed lattice renderer
//
using UnityEngine;
using UnityEngine.Rendering;

namespace Kvant
{
    [ExecuteInEditMode, AddComponentMenu("Kvant/Lattice")]
    public partial class Lattice : MonoBehaviour
    {
        #region Parameters Exposed To Editor

        [SerializeField] int _columns = 100;
        [SerializeField] int _rows = 100;
        [SerializeField] Vector2 _size = Vector2.one * 10;

        [SerializeField] Vector2 _noiseOffset = Vector2.zero;
        [SerializeField] int _noiseFrequency = 2;
        [SerializeField] float _noiseElevation = 0.5f;
        [SerializeField] float _noiseWarp = 0.1f;

        [ColorUsage(true, true, 0, 8, 0.125f, 3)]
        [SerializeField] Color _surfaceColor = Color.white;

        [ColorUsage(true, true, 0, 8, 0.125f, 3)]
        [SerializeField] Color _lineColor = new Color(0, 0, 0, 0.4f);

        [SerializeField] bool _debug;

        #endregion

        #region Public Properties

        public int columns { get { return _columns; } }

        public int rows {
            // Returns the actual number of rows.
            get {
                var rps = rowsPerSegment;
                return (_rows + rps - 1) / rps * rps;
            }
        }

        public Vector2 size {
            get { return _size; }
            set { _size = value; }
        }

        public Vector2 noiseOffset {
            get { return _noiseOffset; }
            set { _noiseOffset = value; }
        }

        public int noiseFrequency {
            get { return _noiseFrequency; }
            set { _noiseFrequency = value; }
        }

        public float noiseElevation {
            get { return _noiseElevation; }
            set { _noiseElevation = value; }
        }

        public float noiseWarp {
            get { return _noiseWarp; }
            set { _noiseWarp = value; }
        }

        public Color surfaceColor {
            get { return _surfaceColor; }
            set { _surfaceColor = value; }
        }

        public Color lineColor {
            get { return _lineColor; }
            set { _lineColor = value; }
        }

        #endregion

        #region Shaders And Materials

        [SerializeField] Shader _kernelShader;
        [SerializeField] Shader _surfaceShader;
        [SerializeField] Shader _lineShader;
        [SerializeField] Shader _debugShader;

        Material _kernelMaterial;
        Material _surfaceMaterial1;
        Material _surfaceMaterial2;
        Material _lineMaterial;
        Material _debugMaterial;

        #endregion

        #region Private Variables And Objects

        RenderTexture _positionBuffer;
        RenderTexture _normalBuffer1;
        RenderTexture _normalBuffer2;
        BulkMesh _bulkMesh;
        bool _needsReset = true;

        #endregion

        #region Private Properties

        int rowsPerSegment {
            get {
                // Estimate the total count of vertices.
                var total_vc = (_columns + 1) * (_rows + 1) * 6;
                // Number of segments.
                var segments = total_vc / 65000 + 1;
                // Rows per segment.
                return _rows / segments;
            }
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
            var height = rows + 1;
            var buffer = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBFloat);
            buffer.hideFlags = HideFlags.DontSave;
            buffer.filterMode = FilterMode.Point;
            buffer.wrapMode = TextureWrapMode.Repeat;
            return buffer;
        }

        void UpdateKernelShader()
        {
            var m = _kernelMaterial;
            var nparams = new Vector4(1, 1, _noiseOffset.x, _noiseOffset.y) * _noiseFrequency;
            m.SetVector("_SizeParams", _size);
            m.SetVector("_NoiseParams", nparams);
            m.SetVector("_NoisePeriod", new Vector3(100000, 100000));
            m.SetVector("_Displace", new Vector3(_noiseElevation, _noiseWarp, _noiseWarp));
        }

        void UpdateDisplayShader()
        {
            _surfaceMaterial1.SetColor("_Color", _surfaceColor);
            _surfaceMaterial2.SetColor("_Color", _surfaceColor);
            _lineMaterial.SetColor("_Color", _lineColor);
        }

        void ResetResources()
        {
            // Sanitize the parameters.
            _columns = Mathf.Clamp(_columns, 4, 1024);
            _rows = Mathf.Clamp(_rows, 4, 1024);

            // Mesh object.
            if (_bulkMesh == null)
                _bulkMesh = new BulkMesh(_columns, rowsPerSegment, rows);
            else
                _bulkMesh.Rebuild(_columns, rowsPerSegment, rows);

            // Displacement buffers.
            if (_positionBuffer) DestroyImmediate(_positionBuffer);
            if (_normalBuffer1)  DestroyImmediate(_normalBuffer1);
            if (_normalBuffer2)  DestroyImmediate(_normalBuffer2);

            _positionBuffer = CreateBuffer();
            _normalBuffer1  = CreateBuffer();
            _normalBuffer2  = CreateBuffer();

            // Shader materials.
            if (!_kernelMaterial)   _kernelMaterial   = CreateMaterial(_kernelShader);
            if (!_surfaceMaterial1) _surfaceMaterial1 = CreateMaterial(_surfaceShader);
            if (!_surfaceMaterial2) _surfaceMaterial2 = CreateMaterial(_surfaceShader);
            if (!_lineMaterial)     _lineMaterial     = CreateMaterial(_lineShader);
            if (!_debugMaterial)    _debugMaterial    = CreateMaterial(_debugShader);

            // Set buffer references.
            _surfaceMaterial1.SetTexture("_PositionTex", _positionBuffer);
            _surfaceMaterial2.SetTexture("_PositionTex", _positionBuffer);
            _lineMaterial    .SetTexture("_PositionTex", _positionBuffer);
            _surfaceMaterial1.SetTexture("_NormalTex",   _normalBuffer1);
            _surfaceMaterial2.SetTexture("_NormalTex",   _normalBuffer2);

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
            if (_positionBuffer)   DestroyImmediate(_positionBuffer);
            if (_normalBuffer1)    DestroyImmediate(_normalBuffer1);
            if (_normalBuffer2)    DestroyImmediate(_normalBuffer2);
            if (_kernelMaterial)   DestroyImmediate(_kernelMaterial);
            if (_surfaceMaterial1) DestroyImmediate(_surfaceMaterial1);
            if (_surfaceMaterial2) DestroyImmediate(_surfaceMaterial2);
            if (_lineMaterial)     DestroyImmediate(_lineMaterial);
            if (_debugMaterial)    DestroyImmediate(_debugMaterial);
        }

        void Update()
        {
            if (_needsReset) ResetResources();

            UpdateKernelShader();

            // Execute the kernel shaders.
            Graphics.Blit(null, _positionBuffer, _kernelMaterial, 0);
            Graphics.Blit(_positionBuffer, _normalBuffer1, _kernelMaterial, 1);
            Graphics.Blit(_positionBuffer, _normalBuffer2, _kernelMaterial, 2);

            // Draw the bulk mesh.
            UpdateDisplayShader();

            var p = transform.position;
            var r = transform.rotation;

            var uv = new Vector2(0.5f / _positionBuffer.width, 0);
            var offs = new MaterialPropertyBlock();
            var mesh = _bulkMesh.mesh;
            var rps = rowsPerSegment;
            var rowCount = rows;

            for (var i = 0; i < rowCount; i += rps)
            {
                uv.y = (0.5f + i) / _positionBuffer.height;

                offs.AddVector("_UVOffset", uv);

                Graphics.DrawMesh(mesh, p, r, _surfaceMaterial1, 0, null, 0, offs);
                Graphics.DrawMesh(mesh, p, r, _surfaceMaterial2, 0, null, 1, offs);

                if (_lineColor.a > 0.0f)
                    Graphics.DrawMesh(mesh, p, r, _lineMaterial, 0, null, 2, offs);
            }
        }

        void OnGUI()
        {
            if (_debug && Event.current.type.Equals(EventType.Repaint) && _debugMaterial)
            {
                var w = 64;
                var r1 = new Rect(0 * w, 0, w, w);
                var r2 = new Rect(1 * w, 0, w, w);
                var r3 = new Rect(2 * w, 0, w, w);
                if (_positionBuffer) Graphics.DrawTexture(r1, _positionBuffer, _debugMaterial);
                if (_normalBuffer1)  Graphics.DrawTexture(r2, _normalBuffer1,  _debugMaterial);
                if (_normalBuffer2)  Graphics.DrawTexture(r3, _normalBuffer2,  _debugMaterial);
            }
        }

        #endregion
    }
}
