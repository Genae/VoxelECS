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
                RecalculateBorder(ChunkSide.Px, pos, data);
            if (pos.x == 0)
                RecalculateBorder(ChunkSide.Nx, pos, data);

            if (pos.y == ChunkDataSettings.YSize - 1)
                RecalculateBorder(ChunkSide.Py, pos, data);
            if (pos.y == 0)
                RecalculateBorder(ChunkSide.Ny, pos, data);

            if (pos.z == ChunkDataSettings.ZSize - 1)
                RecalculateBorder(ChunkSide.Pz, pos, data);
            if (pos.z == 0)
                RecalculateBorder(ChunkSide.Nz, pos, data);
        }

        public void SetVoxelData(ushort[,,] data)
        {
            _data.SetVoxelData(data);
            RecalculateAllBorders();
        }

        private void RecalculateAllBorders()
        {
            foreach (ChunkSide value in Enum.GetValues(typeof(ChunkSide)))
            {
                RecalculateBorder(value);
            }
        }

        private unsafe void RecalculateBorder(ChunkSide side, Vector3Int? pos = null, ushort? data = null)
        {
            switch (side)
            {
                case ChunkSide.Px:
                    fixed(bool* ptr = _metaData.ChunkBorders.Px)
                        _metaData.ChunkBorders.SetBorderSolid(side, _data.CalculateBorder(side, ptr, pos, data));
                    break;
                case ChunkSide.Nx:
                    fixed (bool* ptr = _metaData.ChunkBorders.Nx)
                        _metaData.ChunkBorders.SetBorderSolid(side, _data.CalculateBorder(side, ptr, pos, data));
                    break;
                case ChunkSide.Py:
                    fixed (bool* ptr = _metaData.ChunkBorders.Py)
                        _metaData.ChunkBorders.SetBorderSolid(side, _data.CalculateBorder(side, ptr, pos, data));
                    break;
                case ChunkSide.Ny:
                    fixed (bool* ptr = _metaData.ChunkBorders.Ny)
                        _metaData.ChunkBorders.SetBorderSolid(side, _data.CalculateBorder(side, ptr, pos, data));
                    break;
                case ChunkSide.Pz:
                    fixed (bool* ptr = _metaData.ChunkBorders.Pz)
                        _metaData.ChunkBorders.SetBorderSolid(side, _data.CalculateBorder(side, ptr, pos, data));
                    break;
                case ChunkSide.Nz:
                    fixed (bool* ptr = _metaData.ChunkBorders.Nz)
                        _metaData.ChunkBorders.SetBorderSolid(side, _data.CalculateBorder(side, ptr, pos, data));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(side), side, null);
            }
        }

    }
}