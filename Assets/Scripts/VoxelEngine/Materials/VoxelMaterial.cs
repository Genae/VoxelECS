using UnityEngine;

namespace Assets.Scripts.VoxelEngine.Materials
{
    [CreateAssetMenu]
    public class VoxelMaterial : ScriptableObject
    {
        public Material Material;
        public Color Color;
        public bool Transparent;
    }
}