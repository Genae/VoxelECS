using System;
using System.Collections.Generic;
using Assets.Scripts.VoxelEngine.Materials;
using Assets.Scripts.VoxelEngine.Renderers;
using UnityEngine;

namespace Assets.Scripts.VoxelEngine.Containers.Chunks
{
    public class ChunkCloud
    {
        private readonly MaterialCollection _materialCollection;
        private readonly Grid3D<Chunk> _chunks;
        private readonly Grid3D<MeshBuilder> _chunksMeshes;
        private readonly Transform _map;
        private int _slice = 100;

        //batch
        private bool _batchMode;
        private List<Vector3Int> _batchedChunks = new List<Vector3Int>();

        public ChunkCloud(MaterialCollection materialCollection, Transform map)
        {
            _materialCollection = materialCollection;
            _map = map;
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
            var cx = (pos.x + (pos.x < 0 ? 1 : 0)) / ChunkDataSettings.XSize - (pos.x < 0 ? 1 : 0);
            var cy = (pos.y + (pos.y < 0 ? 1 : 0)) / ChunkDataSettings.YSize - (pos.y < 0 ? 1 : 0);
            var cz = (pos.z + (pos.z < 0 ? 1 : 0)) / ChunkDataSettings.ZSize - (pos.z < 0 ? 1 : 0);
            if (_chunks[cx, cy, cz] == null)
            {
                _chunks[cx, cy, cz] = new Chunk();
                var go = new GameObject($"Chunk [{cx}, {cy}, {cz}]");
                go.transform.parent = _map;
                go.transform.localPosition = new Vector3(cx * ChunkDataSettings.XSize, cy * ChunkDataSettings.YSize, cz*ChunkDataSettings.ZSize);
                _chunksMeshes[cx, cy, cz] = go.AddComponent<MeshBuilder>();
                _chunksMeshes[cx, cy, cz].Init();
            }
            var p = new Vector3Int(Mod(pos.x, ChunkDataSettings.XSize), Mod(pos.y, ChunkDataSettings.YSize), Mod(pos.z, ChunkDataSettings.ZSize));
            var neighbours = _chunks[cx, cy, cz].SetVoxelData(p, material, _materialCollection);
            if (_batchMode)
            {
                var cp = new Vector3Int(cx, cy, cz);
                if (!_chunksMeshes[cx, cy, cz].NeedsUpdate)
                {
                    _chunksMeshes[cx, cy, cz].NeedsUpdate = true;
                    _batchedChunks.Add(cp);
                }
            }
            else
            {
                var mySlice = _slice - cy * ChunkDataSettings.YSize;
                _chunksMeshes[cx, cy, cz].BuildMesh(_materialCollection, GetNeighbours(cx, cy, cz), _chunks[cx, cy, cz], mySlice, true);
            }
            foreach (var neighbour in neighbours)
            {
                var nPos = GetNeighbourPos(cx, cy, cz, neighbour);
                if (_chunksMeshes[nPos.x, nPos.y, nPos.z] != null)
                {
                    if (_batchMode)
                    {
                        if (!_chunksMeshes[nPos.x, nPos.y, nPos.z].NeedsUpdate)
                        {
                            _chunksMeshes[nPos.x, nPos.y, nPos.z].NeedsUpdate = true;
                            _batchedChunks.Add(nPos);
                        }
                    }
                    else
                    {
                        var mySlice = _slice - nPos.y * ChunkDataSettings.YSize;
                        _chunksMeshes[nPos.x, nPos.y, nPos.z].BuildMesh(_materialCollection, GetNeighbours(nPos.x, nPos.y, nPos.z), _chunks[nPos.x, nPos.y, nPos.z], mySlice, true);
                    }
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

        private Vector3Int GetNeighbourPos(int cx, int cy, int cz, ChunkSide side)
        {
            switch (side)
            {
                case ChunkSide.Px:
                    return new Vector3Int(cx + 1, cy, cz);
                case ChunkSide.Nx:
                    return new Vector3Int(cx - 1, cy, cz);
                case ChunkSide.Py:
                    return new Vector3Int(cx, cy + 1, cz);
                case ChunkSide.Ny:
                    return new Vector3Int(cx, cy - 1, cz);
                case ChunkSide.Pz:
                    return new Vector3Int(cx, cy, cz + 1);
                case ChunkSide.Nz:
                    return new Vector3Int(cx, cy, cz - 1);
                default:
                    throw new ArgumentOutOfRangeException(nameof(side), side, null);
            }
        }

        public void StartBatch()
        {
            _batchMode = true;
        }

        public void FinishBatch()
        {
            _batchMode = false;
            foreach (var batchedChunk in _batchedChunks)
            {
                var mySlice = _slice - batchedChunk.y * ChunkDataSettings.YSize;
                _chunksMeshes[batchedChunk.x, batchedChunk.y, batchedChunk.z].BuildMesh(_materialCollection, GetNeighbours(batchedChunk.x, batchedChunk.y, batchedChunk.z), _chunks[batchedChunk.x, batchedChunk.y, batchedChunk.z], mySlice, true);
            }
            _batchedChunks.Clear();
        }

        public void SetSlice(int slice)
        {
            _slice = slice;
            foreach (var mesh in _chunksMeshes)
            {
                var mySlice = slice - mesh.Key.y * ChunkDataSettings.YSize;
                mesh.Value.BuildMesh(_materialCollection, GetNeighbours(mesh.Key.x, mesh.Key.y, mesh.Key.z), _chunks[mesh.Key.x, mesh.Key.y, mesh.Key.z], mySlice, false);
            }
        }
    }
}
