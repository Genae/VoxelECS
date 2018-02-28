using System.Collections.Generic;
using System.Diagnostics;
using Assets.Scripts.VoxelEngine.Containers.Chunks;
using Assets.Scripts.VoxelEngine.Materials;
using NUnit.Framework;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Assets.Testing.VoxelEngine
{
    public class ChunkTests
    {
        [Test]
        public void ChunkCanReadAndWriteVoxels()
        {
            var matcol = new MaterialCollection();
            var chunk = new Chunk();
            var pos = new Vector3Int(10, 13, 2);
            chunk.SetVoxelData(pos, 1, matcol);
            var data = chunk.GetVoxelData(pos);
            Assert.AreEqual(1, data);
        }

        [Test]
        public void ChunkCanReadAndWriteVoxelsAtBorder()
        {
            var matcol = new MaterialCollection();
            var chunk = new Chunk();
            var pos = new Vector3Int(0, 13, 2);
            chunk.SetVoxelData(pos, 1, matcol);
            var data = chunk.GetVoxelData(pos);
            Assert.AreEqual(1, data);
        }

        [Test]
        public void ChunkChangedSidesAreCorrect()
        {
            var matcol = new MaterialCollection();
            var chunk = new Chunk();

            var changed = chunk.SetVoxelData(new Vector3Int(0,0,0), 1, matcol);
            CollectionAssert.AreEquivalent(new List<ChunkSide> { ChunkSide.Nx, ChunkSide.Nz, ChunkSide.Ny }, changed);

            changed = chunk.SetVoxelData(new Vector3Int(ChunkDataSettings.XSize - 1, ChunkDataSettings.YSize - 1, ChunkDataSettings.ZSize - 1), 1, matcol);
            CollectionAssert.AreEquivalent(new List<ChunkSide> { ChunkSide.Px, ChunkSide.Py, ChunkSide.Pz }, changed);

            changed = chunk.SetVoxelData(new Vector3Int(ChunkDataSettings.XSize - 1, ChunkDataSettings.YSize - 1, ChunkDataSettings.ZSize - 1), 1, matcol);
            CollectionAssert.AreEquivalent(changed, new List<ChunkSide>());

            changed = chunk.SetVoxelData(new Vector3Int(ChunkDataSettings.XSize - 1, ChunkDataSettings.YSize - 2, ChunkDataSettings.ZSize - 2), 1, matcol);
            CollectionAssert.AreEquivalent(new List<ChunkSide> { ChunkSide.Px }, changed);

            changed = chunk.SetVoxelData(new Vector3Int(ChunkDataSettings.XSize - 1, ChunkDataSettings.YSize - 2, ChunkDataSettings.ZSize - 2), 0, matcol);
            CollectionAssert.AreEquivalent(new List<ChunkSide> { ChunkSide.Px }, changed);

            changed = chunk.SetVoxelData(new Vector3Int(ChunkDataSettings.XSize - 1, ChunkDataSettings.YSize - 2, ChunkDataSettings.ZSize - 2), 0, matcol);
            CollectionAssert.AreEquivalent(new List<ChunkSide>(), changed);

            changed = chunk.SetVoxelData(new Vector3Int(ChunkDataSettings.XSize - 1, ChunkDataSettings.YSize - 2, ChunkDataSettings.ZSize - 2), 1, matcol);
            CollectionAssert.AreEquivalent(new List<ChunkSide> { ChunkSide.Px }, changed);
            changed = chunk.SetVoxelData(new Vector3Int(ChunkDataSettings.XSize - 2, ChunkDataSettings.YSize - 1, ChunkDataSettings.ZSize - 2), 1, matcol);
            CollectionAssert.AreEquivalent(new List<ChunkSide> { ChunkSide.Py }, changed);
            changed = chunk.SetVoxelData(new Vector3Int(ChunkDataSettings.XSize - 2, ChunkDataSettings.YSize - 2, ChunkDataSettings.ZSize - 1), 1, matcol);
            CollectionAssert.AreEquivalent(new List<ChunkSide> { ChunkSide.Pz }, changed);
            changed = chunk.SetVoxelData(new Vector3Int(0, 1, 1), 1, matcol);
            CollectionAssert.AreEquivalent(new List<ChunkSide> { ChunkSide.Nx }, changed);
            changed = chunk.SetVoxelData(new Vector3Int(1, 0, 1), 1, matcol);
            CollectionAssert.AreEquivalent(new List<ChunkSide> { ChunkSide.Ny }, changed);
            changed = chunk.SetVoxelData(new Vector3Int(1, 1, 0), 1, matcol);
            CollectionAssert.AreEquivalent(new List<ChunkSide> { ChunkSide.Nz }, changed);
        }
        

        [Test(Description = "Performance Test")]
        public void CheckReadAndWritePerformance()
        {
            var matcol = new MaterialCollection();
            var count = 1000000;
            var chunkcount = count / 100;

            Debug.Log("Creating Random Positions");
            var randPos = new Vector3Int[count];
            for (var i = 0; i < count; i++)
            {
                randPos[i] = new Vector3Int(Random.Range(0, 16), Random.Range(0, 16), Random.Range(0, 16));
            }

            Debug.Log("Creating Chunks");
            var chunks = new Chunk[chunkcount];
            for (var i = 0; i < chunkcount; i++)
            {
                chunks[i] = new Chunk();
            }

            Debug.Log("Writing");
            var val = 0;
            var index = 0;
            var watch = new Stopwatch();
            watch.Start();
            foreach (var pos in randPos)
            {
                chunks[index++ % chunkcount].SetVoxelData(pos, (ushort) (++val % 2), matcol);
            }
            var timeW = watch.ElapsedMilliseconds;
            Debug.Log(timeW);
            watch.Reset();

            Debug.Log("Reading");
            index = 0;
            var data = new ushort[count];
            watch.Reset();
            watch.Start();
            foreach (var pos in randPos)
            {
                data[index] = chunks[index++ % chunkcount].GetVoxelData(pos);
            }
            var timeR = watch.ElapsedMilliseconds;
            Debug.Log(timeR);
        }
    }
}
