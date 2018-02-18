using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Chunks
{
    public class ChunkDataSettings
    {
        public const ushort XSize = 16;
        public const ushort YSize = 16;
        public const ushort ZSize = 16;
    }

    public unsafe struct ChunkData
    {
        public fixed ushort VoxelData[ChunkDataSettings.XSize * ChunkDataSettings.YSize * ChunkDataSettings.ZSize];
        
        #region Data Access
        public ushort GetVoxelData(Vector3Int pos)
        {
            #if UNITY_EDITOR
            if (ChunkDataSettings.XSize <= pos.x || ChunkDataSettings.YSize <= pos.y || ChunkDataSettings.ZSize <= pos.z)
            {
                throw new IndexOutOfRangeException($"ChunkData is of Size {ChunkDataSettings.XSize}/{ChunkDataSettings.YSize}/{ChunkDataSettings.ZSize} but you try to access {pos.x}/{pos.y}/{pos.z}");
            }
            #endif
            fixed (ushort* prt = VoxelData)
            {
                var index = pos.x + pos.z * ChunkDataSettings.XSize + pos.y * ChunkDataSettings.XSize * ChunkDataSettings.ZSize;
                return *(prt + index);
            }
        }

        public void SetVoxelData(Vector3Int pos, ushort data)
        {
            #if UNITY_EDITOR
            if (ChunkDataSettings.XSize <= pos.x || ChunkDataSettings.YSize <= pos.y || ChunkDataSettings.ZSize <= pos.z)
            {
                throw new IndexOutOfRangeException($"ChunkData is of Size {ChunkDataSettings.XSize}/{ChunkDataSettings.YSize}/{ChunkDataSettings.ZSize} but you try to access {pos.x}/{pos.y}/{pos.z}");
            }
            #endif
            fixed (ushort* prt = VoxelData)
            {
                var index = pos.x + pos.z * ChunkDataSettings.XSize + pos.y * ChunkDataSettings.XSize * ChunkDataSettings.ZSize;
                *(prt + index) = data;
            }
        }

        public void SetVoxelData(ushort[,,] data)
        {
            #if UNITY_EDITOR
            if (ChunkDataSettings.XSize != data.GetLength(0) || ChunkDataSettings.YSize != data.GetLength(1) || ChunkDataSettings.ZSize != data.GetLength(2))
            {
                throw new IndexOutOfRangeException($"Expected data of Size {ChunkDataSettings.XSize}/{ChunkDataSettings.YSize}/{ChunkDataSettings.ZSize} but got data of size {data.GetLength(0)}/{data.GetLength(1)}/{data.GetLength(2)}");
            }
            #endif
            fixed (ushort* prt = VoxelData)
            {
                var curptr = prt;
                for (var y = 0; y < ChunkDataSettings.YSize; y++)
                {
                    for (var z = 0; z < ChunkDataSettings.ZSize; z++)
                    {
                        for (var x = 0; x < ChunkDataSettings.XSize; x++)
                        {
                            var dat = data[x, y, z];
                            *curptr = dat;
                            curptr += 1;
                        }
                    }
                }
            }
        }
        #endregion

        #region CalculateBorders
        public bool CalculateBorder(ChunkSide side, bool* border)
        {
            switch (side)
            {
                case ChunkSide.Px:
                    return CalculateBorder(ChunkDataSettings.ZSize, ChunkDataSettings.YSize, ChunkDataSettings.XSize, ChunkDataSettings.XSize * ChunkDataSettings.ZSize, ChunkDataSettings.XSize -1, border);
                case ChunkSide.Nx:
                    return CalculateBorder(ChunkDataSettings.ZSize, ChunkDataSettings.YSize, ChunkDataSettings.XSize, ChunkDataSettings.XSize * ChunkDataSettings.ZSize, 0, border);
                case ChunkSide.Py:
                    return CalculateBorder(ChunkDataSettings.XSize, ChunkDataSettings.ZSize, 1, ChunkDataSettings.XSize, (ChunkDataSettings.YSize - 1) * ChunkDataSettings.XSize * ChunkDataSettings.ZSize, border);
                case ChunkSide.Ny:
                    return CalculateBorder(ChunkDataSettings.XSize, ChunkDataSettings.ZSize, 1, ChunkDataSettings.XSize, 0, border);
                case ChunkSide.Pz:
                    return CalculateBorder(ChunkDataSettings.XSize, ChunkDataSettings.YSize, 1, ChunkDataSettings.XSize * ChunkDataSettings.ZSize, (ChunkDataSettings.ZSize - 1) * ChunkDataSettings.XSize, border);
                case ChunkSide.Nz:
                    return CalculateBorder(ChunkDataSettings.XSize, ChunkDataSettings.YSize, 1, ChunkDataSettings.XSize * ChunkDataSettings.ZSize, 0, border);
                default:
                    throw new ArgumentOutOfRangeException(nameof(side), side, null);
            }
        }

        private bool CalculateBorder(ushort rowMax, ushort columnMax, ushort rowMultiplier, int columnMultiplier, ushort offset, bool* border)
        {
            var solid = true;
            var i = 0;
            fixed (ushort* ptr = VoxelData)
            {
                for (var c = 0; c < columnMax; c++)
                {
                    for (var r = 0; r < rowMax; r++)
                    {
                        var s = *(ptr + c * columnMultiplier + r * rowMultiplier + offset) != 0;
                        solid = solid && s;
                        border[i++] = s;
                    }
                }
            }
            return solid;
        }
        #endregion

        #region Serialization
        public byte[] SerializeVoxelData()
        {
            ushort current;
            fixed (ushort* prt = VoxelData)
            {
                current = *prt;
            }
            var i = 0;
            ushort counter = 0;
            var shorts = new List<ushort> { ChunkDataSettings.XSize, ChunkDataSettings.YSize, ChunkDataSettings.ZSize };
            fixed (ushort* ptr = VoxelData)
            {
                var curptr = ptr;
                while (i++ < ChunkDataSettings.XSize * ChunkDataSettings.YSize * ChunkDataSettings.ZSize)
                {
                    var val = *curptr;
                    if (current.Equals(val))
                    {
                        counter++;
                    }
                    else
                    {
                        shorts.Add(counter);
                        shorts.Add(current);
                        current = *curptr;
                        counter = 1;
                    }
                    curptr += 1;
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
            fixed (ushort* ptr = VoxelData)
            {
                var curptr = ptr;
                for (var i = 3; i < shorts.Length; i += 2)
                {
                    for (var j = 0; j < shorts[i]; j++)
                    {
                        *curptr = shorts[i + 1];
                        curptr += 1;
                    }
                }
            }
        }
        #endregion
    }

    /*
    public struct SerializeChunkDataJob : IJob
    {
        [ReadOnly]
        public NativeArray<ChunkData> Chunks;
        [WriteOnly]
        public NativeArray<byte> Bytes;
        [WriteOnly]
        public NativeArray<int> Index;
        public void Execute()
        {
            var index = 0;
            for (var i = 0; i < Chunks.Length; i++)
            {
                var bytes = Chunks[i].SerializeVoxelData();
                foreach (var t in bytes)
                {
                    Bytes[index++] = t;
                }
            }
            Index[0] = index;
        }
    }*/
}
