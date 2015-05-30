//
// Lattice - fractal-deformed lattice
//

using UnityEngine;
using UnityEngine.Rendering;

namespace Kvant
{
    [ExecuteInEditMode, AddComponentMenu("Kvant/Lattice")]
    public partial class Lattice : MonoBehaviour
    {
        #region Parameters Exposed To Editor

        [SerializeField] Vector2 _size = Vector2.one * 5;

        [SerializeField] int _slices = 200;
        [SerializeField] int _stacks = 200;

        [SerializeField] Vector2 _offset = Vector2.zero;

        [SerializeField] int _frequency = 2;
        [SerializeField] float _bump = 0;
        [SerializeField] float _warp = 0;

        [ColorUsage(true, true, 0, 8, 0.125f, 3)]
        [SerializeField] Color _surfaceColor = Color.gray;

        [ColorUsage(true, true, 0, 8, 0.125f, 3)]
        [SerializeField] Color _lineColor = Color.white;

        [SerializeField] bool _debug;

        #endregion

        #region Public Properties

        public Vector2 size {
            get { return _size; }
            set { _size = value; }
        }

        public int slices { get { return _slices; } }
        public int stacks { get { return _stacks; } }

        public Vector2 offset {
            get { return _offset; }
            set { _offset = value; }
        }

        public int frequency {
            get { return _frequency; }
            set { _frequency = value; }
        }

        public float bump {
            get { return _bump; }
            set { _bump = value; }
        }

        public float warp {
            get { return _warp; }
            set { _warp = value; }
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

        #region Resource Management

        public void NotifyConfigChange()
        {
            _needsReset = true;
        }

        void SanitizeParameters()
        {
            _slices = Mathf.Clamp(_slices, 8, 255);
            _stacks = Mathf.Clamp(_stacks, 8, 1023);
        }

        Material CreateMaterial(Shader shader)
        {
            var material = new Material(shader);
            material.hideFlags = HideFlags.DontSave;
            return material;
        }

        RenderTexture CreateBuffer()
        {
            var width = (_slices + 1) * 2;
            var height = _stacks + 1;
            var buffer = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBFloat);
            buffer.hideFlags = HideFlags.DontSave;
            buffer.filterMode = FilterMode.Point;
            buffer.wrapMode = TextureWrapMode.Repeat;
            return buffer;
        }

        void UpdateKernelShader()
        {
            var m = _kernelMaterial;

            /*
            var height = _height * (_stacks + 1) / _stacks;
            var vfreq = _frequency / (Mathf.PI * 2 * _radius);
            var nparams = new Vector4(_frequency, vfreq * height, _twist * _frequency, _offset * vfreq);

            m.SetVector("_SizeParams", new Vector2(_radius, height));
            m.SetVector("_NoiseParams",nparams);
            m.SetVector("_NoisePeriod", new Vector3(1, 100000));
            */
            m.SetVector("_Displace", new Vector3(_bump, _warp, _warp));
        }

        void UpdateDisplayShader()
        {
            _surfaceMaterial1.SetColor("_Color", _surfaceColor);
            _surfaceMaterial2.SetColor("_Color", _surfaceColor);
            _lineMaterial.SetColor("_Color", _lineColor);
        }

        void ResetResources()
        {
            SanitizeParameters();

            // Mesh object.
            if (_bulkMesh == null)
                _bulkMesh = new BulkMesh(_slices, _stacks);
            else
                _bulkMesh.Rebuild(_slices, _stacks);

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

            var uv = new Vector2(0.5f / _positionBuffer.width, 0.5f / _positionBuffer.height);
            var offs = new MaterialPropertyBlock();

            for (var i = 0; i < _bulkMesh.meshes.Length; i++)
            {
                var mesh = _bulkMesh.meshes[0];

                uv.y = 0.5f / _positionBuffer.height;

                if (i == _bulkMesh.meshes.Length - 1)
                {
                    mesh = _bulkMesh.meshes[i];
                }
                else
                {
                    uv.y += 1.0f * i / _bulkMesh.meshes.Length;
                }

                offs.AddVector("_UVOffset", uv);

                Graphics.DrawMesh(mesh, p, r, _surfaceMaterial1, 0, null, 0, offs);
                Graphics.DrawMesh(mesh, p, r, _surfaceMaterial2, 0, null, 1, offs);

                if (_lineColor.a > 0.0f)
                    Graphics.DrawMesh(mesh, p, r, _lineMaterial, 0, null, 2, offs);
            }

/*
            foreach (var mesh in _bulkMesh.meshes)
            {
                offs.AddVector("_UVOffset", uv);
                Graphics.DrawMesh(mesh, p, r, _surfaceMaterial1, 0, null, 0, offs);
                Graphics.DrawMesh(mesh, p, r, _surfaceMaterial2, 0, null, 1, offs);
                if (_lineColor.a > 0.0f)
                    Graphics.DrawMesh(mesh, p, r, _lineMaterial, 0, null, 2, offs);
            }
            */
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
