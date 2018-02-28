using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.VoxelEngine.Containers.Chunks
{
    public class ChunkDataSettings
    {
        public const ushort XSize = 16;
        public const ushort YSize = 16;
        public const ushort ZSize = 16;

        public static ushort GetSizeBySide(ChunkSide side)
        {
            return (int) side < 2 ? XSize : ((int) side < 4 ? YSize : ZSize);
        }
    }

    public unsafe struct ChunkData
    {
        public fixed ushort VoxelData[ChunkDataSettings.XSize * ChunkDataSettings.YSize * ChunkDataSettings.ZSize];
        
        #region Data Access
        public ushort GetVoxelData(Vector3Int pos)
        {
            #if UNITY_EDITOR
            if (ChunkDataSettings.XSize <= pos.x || ChunkDataSettings.YSize <= pos.y || ChunkDataSettings.ZSize <= pos.z || pos.x < 0 || pos.y < 0 || pos.z < 0)
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
            if (ChunkDataSettings.XSize <= pos.x || ChunkDataSettings.YSize <= pos.y || ChunkDataSettings.ZSize <= pos.z || pos.x < 0 || pos.y < 0 || pos.z < 0)
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
}
