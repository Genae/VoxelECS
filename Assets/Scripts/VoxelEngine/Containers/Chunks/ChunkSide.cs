namespace Assets.Scripts.VoxelEngine.Containers.Chunks
{
    public enum ChunkSide
    {
        Px,
        Nx,
        Py,
        Ny,
        Pz,
        Nz
    }

    public static class ChunkSideMethods
    {
        public static ChunkSide OppositeSite(this ChunkSide side)
        {
            var si = (int)side;
            si = si % 2 == 0 ? si + 1 : si - 1;
            return (ChunkSide)si;
        }
    }
}