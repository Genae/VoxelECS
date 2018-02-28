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
            var cloud = go.AddComponent<ChunkCloud>();
            cloud.Init(_collection);
            for (var x = -1; x < 4; x++)
            {
                for (var y = -1; y < 4; y++)
                {
                    for (var z = -1; z < 4; z++)
                    {
                        cloud.SetVoxel(TransparentMaterial, new Vector3Int(x, y, z));
                    }
                }
            }
        }
	
        // Update is called once per frame
        void Update () {
		
        }
    }
}
