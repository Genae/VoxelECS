using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Chunks
{
    public class MeshBuilder : MonoBehaviour
    {
        public Mesh Mesh;
        private MeshRenderer _meshRenderer;
        private MeshCollider _meshCollider;
        private MeshFilter _meshFilter;

        void Start()
        {
            _meshRenderer = gameObject.GetComponent<MeshRenderer>() ?? gameObject.AddComponent<MeshRenderer>();
            _meshCollider = gameObject.GetComponent<MeshCollider>() ?? gameObject.AddComponent<MeshCollider>();
            _meshFilter = gameObject.GetComponent<MeshFilter>() ?? gameObject.AddComponent<MeshFilter>();
        }
        protected void BuildMesh(MaterialCollection materials, Dictionary<ChunkSide, Chunk> neighbours, Chunk chunk, Vector3Int size)
        {
            List<Vector3> upVoxels;
            var meshdata = GreedyMeshing.CreateMesh(materials, neighbours, chunk, size, out upVoxels);


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
                //myMats[i] = _materials[keyArray[i]];
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