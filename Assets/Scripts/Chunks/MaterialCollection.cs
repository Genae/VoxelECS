namespace Assets.Scripts.Chunks
{
    public class MaterialCollectionSettings
    {
        public const int AtlasSize = 8;
    }

    public class MaterialCollection
    {
        public LoadedVoxelMaterial[] Materials;

        public MaterialCollection()
        {
            Materials = new LoadedVoxelMaterial[2];
            Materials[0] = new LoadedVoxelMaterial(new VoxelMaterial())
            {
                AtlasPosition = 0,
                Id = 0,
                Transparent = true,
            };
            Materials[1] = new LoadedVoxelMaterial(new VoxelMaterial())
            {
                AtlasPosition = 0,
                Id = 1,
                Transparent = false,
            };
        }

        public LoadedVoxelMaterial GetById(ushort voxelData)
        {
            return Materials[voxelData];
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