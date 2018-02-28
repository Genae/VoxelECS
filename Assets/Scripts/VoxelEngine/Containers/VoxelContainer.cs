using UnityEngine;

namespace Assets.Scripts.VoxelEngine.Containers
{
    public class VoxelContainer : IVoxelContainer
    {
        private readonly Vector3Int _size;
        private readonly ushort[,,] _data;

        public VoxelContainer(Vector3Int size)
        {
            _size = size;
            _data = new ushort[size.x,size.y,size.z];
        }

        public Vector3Int GetSize()
        {
            return _size;
        }

        public ushort GetVoxelData(Vector3Int pos)
        {
            return _data[pos.x, pos.y, pos.z];
        }
        public void SetVoxelData(Vector3Int pos, ushort data)
        {
            _data[pos.x, pos.y, pos.z] = data;
        }
    }
}
