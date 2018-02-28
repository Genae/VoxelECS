using UnityEngine;

namespace Assets.Scripts.VoxelEngine.Containers
{
    public interface IVoxelContainer
    {
        Vector3Int GetSize();
        ushort GetVoxelData(Vector3Int pos);
    }
}