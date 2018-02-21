using System;

namespace Assets.Scripts.Chunks
{
    public unsafe struct ChunkBorders
    {
        public fixed bool Px[ChunkDataSettings.ZSize * ChunkDataSettings.YSize];
        public fixed bool Nx[ChunkDataSettings.ZSize * ChunkDataSettings.YSize];
        public fixed bool Py[ChunkDataSettings.XSize * ChunkDataSettings.ZSize];
        public fixed bool Ny[ChunkDataSettings.XSize * ChunkDataSettings.ZSize];
        public fixed bool Pz[ChunkDataSettings.XSize * ChunkDataSettings.YSize];
        public fixed bool Nz[ChunkDataSettings.XSize * ChunkDataSettings.YSize];

        private bool _pxSolid;
        private bool _nxSolid;
        private bool _pySolid;
        private bool _nySolid;
        private bool _pzSolid;
        private bool _nzSolid;
        
        public bool GetOppositeBorderSolid(ChunkSide side)
        {
            var si = (int)side;
            si = si % 2 == 0 ? si + 1 : si - 1;
            return GetBorderSolid((ChunkSide)si);
        }

        public bool GetBorderSolid(ChunkSide side)
        {
            switch (side)
            {
                case ChunkSide.Px:
                    return _pxSolid;
                case ChunkSide.Nx:
                    return _nxSolid;
                case ChunkSide.Py:
                    return _pySolid;
                case ChunkSide.Ny:
                    return _nySolid;
                case ChunkSide.Pz:
                    return _pzSolid;
                case ChunkSide.Nz:
                    return _nzSolid;
                default:
                    throw new ArgumentOutOfRangeException(nameof(side), side, null);
            }
        }

        public void SetBorderSolid(ChunkSide side, bool solid)
        {
            switch (side)
            {
                case ChunkSide.Px:
                    _pxSolid = solid;
                    break;
                case ChunkSide.Nx:
                    _nxSolid = solid;
                    break;
                case ChunkSide.Py:
                    _pySolid = solid;
                    break;
                case ChunkSide.Ny:
                    _nySolid = solid;
                    break;
                case ChunkSide.Pz:
                    _pzSolid = solid;
                    break;
                case ChunkSide.Nz:
                    _nzSolid = solid;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(side), side, null);
            }
        }
    }
    
    public enum ChunkSide
    {
        Px,
        Nx,
        Py,
        Ny,
        Pz,
        Nz
    }
}