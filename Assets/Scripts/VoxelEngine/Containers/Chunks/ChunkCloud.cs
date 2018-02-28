using System;
using System.Collections.Generic;
using Assets.Scripts.VoxelEngine.Materials;
using Assets.Scripts.VoxelEngine.Renderers;
using UnityEngine;

namespace Assets.Scripts.VoxelEngine.Containers.Chunks
{
    public class ChunkCloud : MonoBehaviour
    {
        private MaterialCollection _materialCollection;
        private Grid3D<Chunk> _chunks;
        private Grid3D<MeshBuilder> _chunksMeshes;

        public void Init(MaterialCollection materialCollection)
        {
            _materialCollection = materialCollection;
            _chunks = new Grid3D<Chunk>();
            _chunksMeshes = new Grid3D<MeshBuilder>();
        }

        public void SetVoxel(VoxelMaterial material, Vector3Int pos)
        {
            SetVoxel(_materialCollection.GetId(material), pos);
        }
        public void SetVoxel(string material, Vector3Int pos)
        {
            SetVoxel(_materialCollection.GetId(material), pos);
        }
        public void SetVoxel(ushort material, Vector3Int pos)
        {
            var cx = pos.x / ChunkDataSettings.XSize - (pos.x < 0 ? 1 : 0);
            var cy = pos.y / ChunkDataSettings.YSize - (pos.y < 0 ? 1 : 0);
            var cz = pos.z / ChunkDataSettings.ZSize - (pos.z < 0 ? 1 : 0);
            if (_chunks[cx, cy, cz] == null)
            {
                _chunks[cx, cy, cz] = new Chunk();
                var go = new GameObject($"Chunk [{cx}, {cy}, {cz}]");
                go.transform.parent = transform;
                go.transform.localPosition = new Vector3(cx * ChunkDataSettings.XSize, cy * ChunkDataSettings.YSize, cz*ChunkDataSettings.ZSize);
                _chunksMeshes[cx, cy, cz] = go.AddComponent<MeshBuilder>();
                _chunksMeshes[cx, cy, cz].Init();
            }
            var p = new Vector3Int(Mod(pos.x, ChunkDataSettings.XSize), Mod(pos.y, ChunkDataSettings.YSize), Mod(pos.z, ChunkDataSettings.ZSize));
            var neighbours = _chunks[cx, cy, cz].SetVoxelData(p, material, _materialCollection);
            _chunksMeshes[cx, cy, cz].BuildMesh(_materialCollection, GetNeighbours(cx, cy, cz), _chunks[cx, cy, cz]);
            foreach (var neighbour in neighbours)
            {
                var nPos = GetNeighbourPos(pos, neighbour);
                if (_chunksMeshes[nPos.x, nPos.y, nPos.z] != null)
                {
                    _chunksMeshes[nPos.x, nPos.y, nPos.z].BuildMesh(_materialCollection, GetNeighbours(nPos.x, nPos.y, nPos.z), _chunks[nPos.x, nPos.y, nPos.z]);
                }
            }
        }

        private static int Mod(int num, ushort mod)
        {
            return (num % mod + mod) % mod;
        }

        private Dictionary<ChunkSide, Chunk> GetNeighbours(int cx, int cy, int cz)
        {
            return new Dictionary<ChunkSide, Chunk>
            {
                {ChunkSide.Px, _chunks[cx + 1, cy, cz] },
                {ChunkSide.Nx, _chunks[cx - 1, cy, cz] },
                {ChunkSide.Py, _chunks[cx, cy + 1, cz] },
                {ChunkSide.Ny, _chunks[cx, cy - 1, cz] },
                {ChunkSide.Pz, _chunks[cx, cy, cz + 1] },
                {ChunkSide.Nz, _chunks[cx, cy, cz - 1] }
            };
        }

        private Vector3Int GetNeighbourPos(Vector3Int pos, ChunkSide side)
        {
            switch (side)
            {
                case ChunkSide.Px:
                    return pos + new Vector3Int(1, 0, 0);
                case ChunkSide.Nx:
                    return pos - new Vector3Int(1, 0, 0);
                case ChunkSide.Py:
                    return pos + new Vector3Int(0, 1, 0);
                case ChunkSide.Ny:
                    return pos - new Vector3Int(0, 1, 0);
                case ChunkSide.Pz:
                    return pos + new Vector3Int(0, 0, 1);
                case ChunkSide.Nz:
                    return pos - new Vector3Int(0, 0, 1);
                default:
                    throw new ArgumentOutOfRangeException(nameof(side), side, null);
            }
        }
    }
}
