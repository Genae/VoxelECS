using System;
using UnityEngine;

namespace Assets.Scripts.Chunks
{
    public class Chunk
    {
        private ChunkMetaData _metaData;
        private ChunkData _data;

        public Chunk()
        {
            _data = new ChunkData();
            _metaData = new ChunkMetaData();
        }

        public ushort GetVoxelData(Vector3Int pos)
        {
            return _data.GetVoxelData(pos);
        }

        public void SetVoxelData(Vector3Int pos, ushort data)
        {
            _data.SetVoxelData(pos, data);

            if (pos.x == ChunkDataSettings.XSize-1)
                RecalculateBorder(ChunkSide.Px);
            if (pos.x == 0)
                RecalculateBorder(ChunkSide.Nx);

            if (pos.y == ChunkDataSettings.YSize - 1)
                RecalculateBorder(ChunkSide.Py);
            if (pos.y == 0)
                RecalculateBorder(ChunkSide.Ny);

            if (pos.z == ChunkDataSettings.ZSize - 1)
                RecalculateBorder(ChunkSide.Pz);
            if (pos.z == 0)
                RecalculateBorder(ChunkSide.Nz);
        }

        private unsafe void RecalculateBorder(ChunkSide side)
        {
            switch (side)
            {
                case ChunkSide.Px:
                    fixed(bool* ptr = _metaData.ChunkBorders.Px)
                        _metaData.ChunkBorders.SetBorderSolid(side, _data.CalculateBorder(side, ptr));
                    break;
                case ChunkSide.Nx:
                    fixed (bool* ptr = _metaData.ChunkBorders.Nx)
                        _metaData.ChunkBorders.SetBorderSolid(side, _data.CalculateBorder(side, ptr));
                    break;
                case ChunkSide.Py:
                    fixed (bool* ptr = _metaData.ChunkBorders.Py)
                        _metaData.ChunkBorders.SetBorderSolid(side, _data.CalculateBorder(side, ptr));
                    break;
                case ChunkSide.Ny:
                    fixed (bool* ptr = _metaData.ChunkBorders.Ny)
                        _metaData.ChunkBorders.SetBorderSolid(side, _data.CalculateBorder(side, ptr));
                    break;
                case ChunkSide.Pz:
                    fixed (bool* ptr = _metaData.ChunkBorders.Pz)
                        _metaData.ChunkBorders.SetBorderSolid(side, _data.CalculateBorder(side, ptr));
                    break;
                case ChunkSide.Nz:
                    fixed (bool* ptr = _metaData.ChunkBorders.Nz)
                        _metaData.ChunkBorders.SetBorderSolid(side, _data.CalculateBorder(side, ptr));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(side), side, null);
            }
        }

        public void SetVoxelData(ushort[,,] data)
        {
            _data.SetVoxelData(data);
        }
    }
}