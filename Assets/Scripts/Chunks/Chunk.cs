using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Chunks
{
    public class Chunk
    {
        public ChunkMetaData MetaData;
        private ChunkData _data;

        public Chunk()
        {
            _data = new ChunkData();
            MetaData = new ChunkMetaData();
        }

        public ushort GetVoxelData(Vector3Int pos)
        {
            return _data.GetVoxelData(pos);
        }

        public List<ChunkSide> SetVoxelData(Vector3Int pos, ushort data)
        {
            var list = new List<ChunkSide>();
            _data.SetVoxelData(pos, data);

            if (pos.x == ChunkDataSettings.XSize-1)
                if(RecalculateBorder(ChunkSide.Px, pos, data))
                    list.Add(ChunkSide.Px);
            if (pos.x == 0)
                if(RecalculateBorder(ChunkSide.Nx, pos, data))
                    list.Add(ChunkSide.Nx);

            if (pos.y == ChunkDataSettings.YSize - 1)
                if(RecalculateBorder(ChunkSide.Py, pos, data))
                    list.Add(ChunkSide.Py);
            if (pos.y == 0)
                if(RecalculateBorder(ChunkSide.Ny, pos, data))
                    list.Add(ChunkSide.Ny);

            if (pos.z == ChunkDataSettings.ZSize - 1)
                if(RecalculateBorder(ChunkSide.Pz, pos, data))
                    list.Add(ChunkSide.Pz);
            if (pos.z == 0)
                if(RecalculateBorder(ChunkSide.Nz, pos, data))
                    list.Add(ChunkSide.Nz);

            return list;
        }

        public List<ChunkSide> SetVoxelData(ushort[,,] data)
        {
            _data.SetVoxelData(data);
            return RecalculateAllBorders();
        }

        public bool GetBorderSolid(ChunkSide side)
        {
            return MetaData.ChunkBorders.GetBorderSolid(side);
        }
        
        #region BorderCalculation
        private List<ChunkSide> RecalculateAllBorders()
        {
            var list = new List<ChunkSide>();
            foreach (ChunkSide side in Enum.GetValues(typeof(ChunkSide)))
            {
                if (RecalculateBorder(side))
                {
                    list.Add(side);
                }
            }
            return list;
        }

        private unsafe bool RecalculateBorder(ChunkSide side, Vector3Int? pos = null, ushort? data = null)
        {
            bool ret;
            bool solid;
            switch (side)
            {
                case ChunkSide.Px:
                    fixed (bool* ptr = MetaData.ChunkBorders.Px)
                    {
                        ret = _data.CalculateBorder(side, ptr, out solid, pos, data);
                        MetaData.ChunkBorders.SetBorderSolid(side, solid);
                    }
                    break;
                case ChunkSide.Nx:
                    fixed (bool* ptr = MetaData.ChunkBorders.Nx)
                    {
                        ret = _data.CalculateBorder(side, ptr, out solid, pos, data);
                        MetaData.ChunkBorders.SetBorderSolid(side, solid);
                    }
                    break;
                case ChunkSide.Py:
                    fixed (bool* ptr = MetaData.ChunkBorders.Py)
                    {
                        ret = _data.CalculateBorder(side, ptr, out solid, pos, data);
                        MetaData.ChunkBorders.SetBorderSolid(side, solid);
                    }
                    break;
                case ChunkSide.Ny:
                    fixed (bool* ptr = MetaData.ChunkBorders.Ny)
                    {
                        ret = _data.CalculateBorder(side, ptr, out solid, pos, data);
                        MetaData.ChunkBorders.SetBorderSolid(side, solid);
                    }
                    break;
                case ChunkSide.Pz:
                    fixed (bool* ptr = MetaData.ChunkBorders.Pz)
                    {
                        ret = _data.CalculateBorder(side, ptr, out solid, pos, data);
                        MetaData.ChunkBorders.SetBorderSolid(side, solid);
                    }
                    break;
                case ChunkSide.Nz:
                    fixed (bool* ptr = MetaData.ChunkBorders.Nz)
                    {
                        ret = _data.CalculateBorder(side, ptr, out solid, pos, data);
                        MetaData.ChunkBorders.SetBorderSolid(side, solid);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(side), side, null);
            }
            return ret;
        }
        #endregion

    }
}