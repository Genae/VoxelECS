using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Chunks
{
    public struct ChunkData
    {
        public ushort[] VoxelData;
        public Vector3Int Size;

        #region Constructors
        public ChunkData(Vector3Int size)
        {
            Size = size;
            VoxelData = new ushort[size.x * size.y * size.z];
        }

        public ChunkData(byte[] data)
        {
            VoxelData = null;
            Size = default(Vector3Int);
            DeserializeVoxelData(data);
        }
        #endregion

        #region Data Access
        public ushort GetVoxelData(Vector3Int pos)
        {
            #if UNITY_EDITOR
            if (Size.x <= pos.x || Size.y <= pos.y || Size.z <= pos.z)
            {
                throw new IndexOutOfRangeException($"ChunkData is of Size {Size.x}/{Size.y}/{Size.z} but you try to access {pos.x}/{pos.y}/{pos.z}");
            }
            #endif
            return VoxelData[pos.x + pos.z * Size.x + pos.y * Size.x * Size.z];
        }

        public void SetVoxelData(Vector3Int pos, ushort data)
        {
            #if UNITY_EDITOR
            if (Size.x <= pos.x || Size.y <= pos.y || Size.z <= pos.z)
            {
                throw new IndexOutOfRangeException($"ChunkData is of Size {Size.x}/{Size.y}/{Size.z} but you try to access {pos.x}/{pos.y}/{pos.z}");
            }
            #endif
            VoxelData[pos.x + pos.z * Size.x + pos.y * Size.x * Size.z] = data;
        }

        public void SetVoxelData(ushort[,,] data)
        {
            #if UNITY_EDITOR
            if (Size.x != data.GetLength(0) || Size.y != data.GetLength(1) || Size.z != data.GetLength(2))
            {
                throw new IndexOutOfRangeException($"Expected data of Size {Size.x}/{Size.y}/{Size.z} but got data of size {data.GetLength(0)}/{data.GetLength(1)}/{data.GetLength(2)}");
            }
            #endif
            for (var y = 0; y < Size.y; y++)
            {
                for (var z = 0; z < Size.z; z++)
                {
                    for (var x = 0; x < Size.x; x++)
                    {
                        VoxelData[x + z * Size.x + y * Size.x * Size.z] = data[x, y, z];
                    }
                }
            }
        }
        #endregion

        #region Serialization
        public byte[] SerializeVoxelData()
        {
            if(Size == default(Vector3Int) || VoxelData == null)
                return new byte[0];
            var current = VoxelData[0];
            ushort counter = 0;
            var shorts = new List<ushort> {(ushort) Size.x, (ushort) Size.y, (ushort) Size.z};
            foreach (var vox in VoxelData)
            {
                if (current.Equals(vox))
                {
                    counter++;
                }
                else
                {
                    shorts.Add(counter);
                    shorts.Add(current);
                    current = vox;
                    counter = 1;
                }
            }
            shorts.Add(counter);
            shorts.Add(current);
            var byteArray = new byte[shorts.Count * 2];
            Buffer.BlockCopy(shorts.ToArray(), 0, byteArray, 0, byteArray.Length);
            return byteArray;
        }

        public void DeserializeVoxelData(byte[] data)
        {
            var shorts = new ushort[data.Length / 2];
            Buffer.BlockCopy(data, 0, shorts, 0, data.Length);
            Size = new Vector3Int(shorts[0], shorts[1], shorts[2]);
            VoxelData = new ushort[Size.x * Size.y * Size.z];
            var pos = 0;
            for (var i = 3; i < shorts.Length; i+=2)
            {
                for (var j = 0; j < shorts[i]; j++)
                {
                    VoxelData[pos++] = shorts[i + 1];
                }
            }
        }
        #endregion
    }
}
