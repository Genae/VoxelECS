using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.VoxelEngine.Materials;
using UnityEngine;

namespace Assets.Scripts.VoxelEngine.Containers.Chunks
{
    public class Chunk : IVoxelContainer
    {
        public ChunkMetaData MetaData;
        private ChunkData _data;

        public Chunk()
        {
            _data = new ChunkData();
            MetaData = new ChunkMetaData();
        }

        public Vector3Int GetSize()
        {
            return new Vector3Int(ChunkDataSettings.XSize, ChunkDataSettings.YSize, ChunkDataSettings.ZSize);
        }

        public ushort GetVoxelData(Vector3Int pos)
        {
            return _data.GetVoxelData(pos);
        }

        public List<ChunkSide> SetVoxelData(Vector3Int pos, ushort data, MaterialCollection materialCollection)
        {
            var list = new List<ChunkSide>();
            var oldVal = GetVoxelData(pos);
            _data.SetVoxelData(pos, data);
            var changed = (data == 0 ^ oldVal == 0) || (materialCollection.GetById(oldVal).Transparent ^ materialCollection.GetById(data).Transparent);

            if (changed)
            {
                if (pos.x == ChunkDataSettings.XSize - 1)
                    list.Add(ChunkSide.Px);
                if (pos.x == 0)
                    list.Add(ChunkSide.Nx);

                if (pos.y == ChunkDataSettings.YSize - 1)
                    list.Add(ChunkSide.Py);
                if (pos.y == 0)
                    list.Add(ChunkSide.Ny);

                if (pos.z == ChunkDataSettings.ZSize - 1)
                    list.Add(ChunkSide.Pz);
                if (pos.z == 0)
                    list.Add(ChunkSide.Nz);
            }
            return list;
        }

        public List<ChunkSide> SetVoxelData(ushort[,,] data)
        {
            _data.SetVoxelData(data);

            return Enum.GetValues(typeof(ChunkSide)).Cast<ChunkSide>().ToList();
        }
        

        
    }
}