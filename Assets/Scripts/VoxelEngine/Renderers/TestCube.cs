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

        private int _init = 10;

        // Use this for initialization
        void Start ()
        {
        }
        
        private void Init()
        {
            _oldSlice = Slice;
            _collection = new MaterialCollection();
            var go = new GameObject("map");
            _cloud = new ChunkCloud(_collection, go.transform);
            _cloud.StartBatch();
            for (var x = -300; x < 300; x++)
            {
                for (var y = 0; y < 3; y++)
                {
                    for (var z = -300; z < 300; z++)
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
            if (_init == 0)
            {
                Init();
            }
            _init--;
            if (_oldSlice != Slice && _init < 0)
            {
                _cloud.SetSlice(Slice);
                _oldSlice = Slice;
            }
        }
    }
}
