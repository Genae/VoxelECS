using UnityEngine;

namespace Assets.Scripts.Chunks
{
    [CreateAssetMenu]
    public class VoxelMaterial : ScriptableObject
    {
        public Material Material;
        public Color Color;
        public bool Transparent;
    }
}