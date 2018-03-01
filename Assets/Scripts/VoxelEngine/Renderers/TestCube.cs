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
        private ChunkCloud _cloud;
        private int _oldSlice;
        public int Slice = 10;

        // Use this for initialization
        void Start ()
        {
            _oldSlice = Slice;
            _collection = new MaterialCollection();
            var go = new GameObject("map");
            _cloud = new ChunkCloud(_collection, go.transform);
            _cloud.StartBatch();
            for (var x = -3; x < 3; x++)
            {
                for (var y = -3; y < 3; y++)
                {
                    for (var z = -3; z < 3; z++)
                    {
                        _cloud.SetVoxel(TransparentMaterial, new Vector3Int(x, y, z));
                    }
                }
            }
            for (var x = -1; x < 1; x++)
            {
                for (var y = -1; y < 1; y++)
                {
                    for (var z = -1; z < 1; z++)
                    {
                        _cloud.SetVoxel(OpaqueMaterial, new Vector3Int(x, y, z));
                    }
                }
            }
            _cloud.FinishBatch();
        }
	
        // Update is called once per frame
        void Update () {
            if (_oldSlice != Slice)
            {
                _cloud.SetSlice(Slice);
                _oldSlice = Slice;
            }
        }
    }
}
