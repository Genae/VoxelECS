namespace Assets.Scripts.Chunks
{
    public class MaterialCollectionSettings
    {
        public const int AtlasSize = 8;
    }

    public class MaterialCollection
    {
        public LoadedVoxelMaterial[] Materials;

        public LoadedVoxelMaterial GetById(ushort voxelData)
        {
            return new LoadedVoxelMaterial(new VoxelMaterial());
        }
    }

    public class LoadedVoxelMaterial
    {
        private readonly VoxelMaterial _voxelMaterial;
        public ushort Id;
        public ushort AtlasPosition;

        public bool Transparent
        {
            get { return _voxelMaterial.Transparent; }
            set { _voxelMaterial.Transparent = value; }
        }

        public LoadedVoxelMaterial(VoxelMaterial material)
        {
            _voxelMaterial = material;
        }
    }
}