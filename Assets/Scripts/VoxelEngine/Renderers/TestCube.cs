using Assets.Scripts.VoxelEngine.Containers.Chunks;
using Assets.Scripts.VoxelEngine.Materials;
using UnityEngine;

namespace Assets.Scripts.VoxelEngine.Renderers
{
    public class TestCube : MonoBehaviour
    {
        private MaterialCollection _collection;
        public VoxelMaterial OpaqueMaterial;
        public VoxelMaterial TransparentMaterial;

        // Use this for initialization
        void Start ()
        {
            _collection = new MaterialCollection();
            var go = new GameObject("map");
            var cloud = new ChunkCloud(_collection, go.transform);
            for (var x = -3; x < 3; x++)
            {
                for (var y = -3; y < 3; y++)
                {
                    for (var z = -3; z < 3; z++)
                    {
                        cloud.SetVoxel(TransparentMaterial, new Vector3Int(x, y, z));
                    }
                }
            }
            for (var x = -1; x < 1; x++)
            {
                for (var y = -1; y < 1; y++)
                {
                    for (var z = -1; z < 1; z++)
                    {
                        cloud.SetVoxel(OpaqueMaterial, new Vector3Int(x, y, z));
                    }
                }
            }
        }
	
        // Update is called once per frame
        void Update () {
		
        }
    }
}
