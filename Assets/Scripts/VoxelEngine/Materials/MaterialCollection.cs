using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.VoxelEngine.Materials
{
    public class MaterialCollectionSettings
    {
        public const int AtlasSize = 8;
    }

    public class MaterialCollection
    {
        public List<LoadedVoxelMaterial> VoxelMaterials;
        public Dictionary<string, LoadedVoxelMaterial> VoxelMaterialIndex;
        public Dictionary<Material, Atlas> Atlases;

        public MaterialCollection()
        {
            Atlases = new Dictionary<Material, Atlas>();
            VoxelMaterials = new List<LoadedVoxelMaterial>
            {
                new LoadedVoxelMaterial(ScriptableObject.CreateInstance<VoxelMaterial>())
                {
                    AtlasPosition = 0,
                    Id = 0,
                    Transparent = true
                }
            };
            VoxelMaterialIndex = new Dictionary<string, LoadedVoxelMaterial> {{"air", VoxelMaterials.First()}};
            LoadAllMaterials();
        }

        public void LoadAllMaterials()
        {
            foreach (var mat in Resources.LoadAll<VoxelMaterial>(""))
            {
                LoadVoxelMaterial(mat);
            }
        }

        public void LoadVoxelMaterial(VoxelMaterial voxelMaterial)
        {
            if (VoxelMaterialIndex.ContainsKey(voxelMaterial.name.ToLower()))
                return;
            var loadedVoxelMaterial = new LoadedVoxelMaterial(voxelMaterial);
            if (!Atlases.ContainsKey(loadedVoxelMaterial.Material))
            {
                Atlases[loadedVoxelMaterial.Material] = new Atlas(loadedVoxelMaterial.Material);
            }
            if (Atlases[loadedVoxelMaterial.Material].AddVoxelMaterial(loadedVoxelMaterial))
            {
                loadedVoxelMaterial.Id = (ushort)VoxelMaterials.Count;
                VoxelMaterials.Add(loadedVoxelMaterial);
                VoxelMaterialIndex[voxelMaterial.name.ToLower()] = loadedVoxelMaterial;
            }
        }

        public LoadedVoxelMaterial GetById(ushort voxelData)
        {
            return VoxelMaterials[voxelData];
        }

        public ushort GetId(VoxelMaterial voxelMaterial)
        {
            return GetId(voxelMaterial.name);
        }

        public ushort GetId(string name)
        {
            if (!VoxelMaterialIndex.ContainsKey(name.ToLower()))
                return 0;
            return VoxelMaterialIndex[name.ToLower()].Id;
        }
    }

    public class Atlas
    {
        public Material Material;
        public ushort Count;
        public Color[] Colors = new Color[MaterialCollectionSettings.AtlasSize * MaterialCollectionSettings.AtlasSize];

        public Atlas(Material material)
        {
            Material = material;
        }

        public bool AddVoxelMaterial(LoadedVoxelMaterial loadedVoxelMaterial)
        {
            if (Colors.Contains(loadedVoxelMaterial.Color))
                return false;
            loadedVoxelMaterial.AtlasPosition = Count;
            Colors[Count++] = loadedVoxelMaterial.Color;

            var texture = (Texture2D)loadedVoxelMaterial.Material.mainTexture;
            if(texture == null)
                texture = new Texture2D(MaterialCollectionSettings.AtlasSize, MaterialCollectionSettings.AtlasSize, TextureFormat.ARGB32, false);
            texture.SetPixel(loadedVoxelMaterial.AtlasPosition / MaterialCollectionSettings.AtlasSize, loadedVoxelMaterial.AtlasPosition % MaterialCollectionSettings.AtlasSize, loadedVoxelMaterial.Color);
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Point;
            texture.Apply();
            loadedVoxelMaterial.Material.mainTexture = texture;
            return true;
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

        public Color Color
        {
            get { return _voxelMaterial.Color; }
            set { _voxelMaterial.Color = value; }
        }
        public Material Material
        {
            get { return _voxelMaterial.Material; }
            set { _voxelMaterial.Material = value; }
        }

        public LoadedVoxelMaterial(VoxelMaterial material)
        {
            _voxelMaterial = material;
        }
    }
}