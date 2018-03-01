using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.VoxelEngine.Containers.Chunks;
using Assets.Scripts.VoxelEngine.Materials;
using UnityEngine;
using UnityEngine.Rendering;

namespace Assets.Scripts.VoxelEngine.Renderers
{
    public class MeshBuilder : MonoBehaviour
    {
        public Mesh Mesh;
        private MeshRenderer _meshRenderer;
        private MeshCollider _meshCollider;
        private MeshFilter _meshFilter;

        public void Init()
        {
            _meshRenderer = gameObject.GetComponent<MeshRenderer>() != null ? gameObject.GetComponent<MeshRenderer>() : gameObject.AddComponent<MeshRenderer>();
            _meshCollider = gameObject.GetComponent<MeshCollider>() != null ? gameObject.GetComponent<MeshCollider>() : gameObject.AddComponent<MeshCollider>();
            _meshFilter = gameObject.GetComponent<MeshFilter>() != null ? gameObject.GetComponent<MeshFilter>() : gameObject.AddComponent<MeshFilter>();
        }
        

        public void BuildMesh(MaterialCollection materials, Dictionary<ChunkSide, Chunk> neighbours, Chunk chunk, int slice, bool rebuild)
        {
            _meshRenderer.shadowCastingMode = slice <= 0 ? ShadowCastingMode.ShadowsOnly : ShadowCastingMode.On;
            if (slice <= 0 || slice > ChunkDataSettings.YSize && !rebuild)
            {
                return;
            }

            List<Vector3> upVoxels;
            var meshdata = GreedyMeshing.CreateMesh(chunk, neighbours, materials, slice, out upVoxels);


            if (Mesh == null)
                Mesh = new Mesh();
            Mesh.Clear();
            Mesh.vertices = meshdata.Vertices;
            Mesh.normals = meshdata.Normals;
            Mesh.subMeshCount = meshdata.Triangles.Keys.Count;
            Mesh.uv = meshdata.Uvs;
            Mesh.colors = new Color[meshdata.Vertices.Length];

            var keyArray = meshdata.Triangles.Keys.ToArray();
            var myMats = new Material[meshdata.Triangles.Keys.Count];
            for (var i = 0; i < meshdata.Triangles.Keys.Count; i++)
            {
                Mesh.SetTriangles(meshdata.Triangles[keyArray[i]], i);
                myMats[i] = materials.GetById((ushort)keyArray[i]).Material;
            }
            _meshRenderer.sharedMaterials = myMats;
            _meshFilter.sharedMesh = Mesh;
            _meshCollider.sharedMesh = null;
            _meshCollider.sharedMesh = Mesh;
            //SetHighlightMaterial(_highlightColor);
            gameObject.SetActive(meshdata.Vertices.Length != 0);
        }
    }
}