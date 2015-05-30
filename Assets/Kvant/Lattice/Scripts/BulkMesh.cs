//
// Bulk mesh handler
//
// Builds a triangular lattice. It has two submeshes in order to store two
// types of vertex orders.
//
// 1st submesh: A-B-C
// 2nd submesh: C-B-D
//
//   B   D
//   .---.---.
//  / \ / \ /
// .---.---.
// A   C
//
// Vertex format:
// position     = not in use
// texcoord0.xy = uv for position texture
// texcoord1.xy = uv for normal texture
//
using UnityEngine;

namespace Kvant
{
    public partial class Lattice
    {
        [System.Serializable]
        class BulkMesh
        {
            #region Properties

            Mesh _mesh;
            public Mesh mesh { get { return _mesh; } }

            #endregion

            #region Public Methods

            public BulkMesh(int columns, int rowsPerSegment, int totalRows)
            {
                _mesh = BuildMesh(columns, rowsPerSegment, totalRows);
            }

            public void Rebuild(int columns, int rowsPerSegment, int totalRows)
            {
                Release();
                _mesh = BuildMesh(columns, rowsPerSegment, totalRows);
            }

            public void Release()
            {
                if (_mesh != null) {
                    Object.DestroyImmediate(_mesh);
                    _mesh = null;
                }
            }

            #endregion

            #region Private Methods

            Mesh BuildMesh(int columns, int rows, int totalRows)
            {
                var Nx = columns + 1;
                var Ny = rows + 1;

                var Sx = 0.5f / Nx;
                var Sy = 1.0f / (totalRows + 1);

                var Oy = 0.0f;

                // Texcoord Array for UV1 and UV2.
                var TA1 = new Vector2[(Nx - 1) * (Ny - 1) * 6];
                var TA2 = new Vector2[(Nx - 1) * (Ny - 1) * 6];
                var iTA = 0;

                // 1st submesh (A-B-C triangles).
                for (var Iy = 0; Iy < Ny - 1; Iy++)
                {
                    for (var Ix = 0; Ix < Nx - 1; Ix++, iTA += 3)
                    {
                        var Ix2 = Ix * 2 + (Iy & 1);
                        // UVs for position.
                        TA1[iTA + 0] = new Vector2(Sx * (Ix2 + 0), Oy + Sy * (Iy + 0));
                        TA1[iTA + 1] = new Vector2(Sx * (Ix2 + 1), Oy + Sy * (Iy + 1));
                        TA1[iTA + 2] = new Vector2(Sx * (Ix2 + 2), Oy + Sy * (Iy + 0));
                        // UVs for normal vector.
                        TA2[iTA] = TA2[iTA + 1] = TA2[iTA + 2] = TA1[iTA];
                    }
                }

                // 2nd submesh (A-C-D triangls).
                for (var Iy = 0; Iy < Ny - 1; Iy++)
                {
                    for (var Ix = 0; Ix < Nx - 1; Ix++, iTA += 3)
                    {
                        var Ix2 = Ix * 2 + 2 - (Iy & 1);
                        // UVs for position.
                        TA1[iTA + 0] = new Vector2(Sx * (Ix2 + 0), Oy + Sy * (Iy + 0));
                        TA1[iTA + 1] = new Vector2(Sx * (Ix2 - 1), Oy + Sy * (Iy + 1));
                        TA1[iTA + 2] = new Vector2(Sx * (Ix2 + 1), Oy + Sy * (Iy + 1));
                        // UVs for normal vector.
                        TA2[iTA] = TA2[iTA + 1] = TA2[iTA + 2] = TA1[iTA];
                    }
                }

                // Index arrays for the 1st and 2nd submesh (surfaces).
                var IA1 = new int[(Nx - 1) * (Ny - 1) * 3];
                var IA2 = new int[(Nx - 1) * (Ny - 1) * 3];
                for (var iIA = 0; iIA < IA1.Length; iIA++)
                {
                    IA1[iIA] = iIA;
                    IA2[iIA] = iIA + IA1.Length;
                }

                // Index array for the 3rd submesh (lines).
                var IA3 = new int[(Nx - 1) * (Ny - 1) * 6];
                var iIA3a = 0;
                var iIA3b = 0;
                for (var Iy = 0; Iy < Ny - 1; Iy++)
                {
                    for (var Ix = 0; Ix < Nx - 1; Ix++, iIA3a += 6, iIA3b += 3)
                    {
                        IA3[iIA3a + 0] = iIA3b;
                        IA3[iIA3a + 1] = iIA3b + 1;
                        IA3[iIA3a + 2] = iIA3b;
                        IA3[iIA3a + 3] = iIA3b + 2;
                        IA3[iIA3a + 4] = iIA3b + 1;
                        IA3[iIA3a + 5] = iIA3b + 2;
                    }
                }

                // Construct a mesh.
                var mesh = new Mesh();
                mesh.subMeshCount = 3;
                mesh.vertices = new Vector3[TA1.Length];
                mesh.uv = TA1;
                mesh.uv2 = TA2;
                mesh.SetIndices(IA1, MeshTopology.Triangles, 0);
                mesh.SetIndices(IA2, MeshTopology.Triangles, 1);
                mesh.SetIndices(IA3, MeshTopology.Lines, 2);
                mesh.Optimize();

                // Avoid being culled.
                mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 100);

                // This only for temporary use. Don't save.
                mesh.hideFlags = HideFlags.DontSave;

                return mesh;
            }

            #endregion
        }
    }
}
