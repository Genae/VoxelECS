using System.Collections.Generic;
using System.Diagnostics;
using Assets.Scripts.Chunks;
using NUnit.Framework;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Assets.Tests.Editor.Chunks
{
    public class ChunkTests
    {
        [Test]
        public void ChunkCanReadAndWriteVoxels()
        {
            var chunk = new Chunk();
            var pos = new Vector3Int(10, 13, 2);
            chunk.SetVoxelData(pos, 1);
            var data = chunk.GetVoxelData(pos);
            Assert.AreEqual(1, data);
        }

        [Test]
        public void ChunkCanReadAndWriteVoxelsAtBorder()
        {
            var chunk = new Chunk();
            var pos = new Vector3Int(0, 13, 2);
            chunk.SetVoxelData(pos, 1);
            var data = chunk.GetVoxelData(pos);
            Assert.AreEqual(1, data);
        }

        [Test]
        public void ChunkChangedSidesAreCorrect()
        {
            var chunk = new Chunk();

            var changed = chunk.SetVoxelData(new Vector3Int(0,0,0), 1);
            CollectionAssert.AreEquivalent(changed, new List<ChunkSide>{ChunkSide.Nx, ChunkSide.Nz, ChunkSide.Ny});

            changed = chunk.SetVoxelData(new Vector3Int(ChunkDataSettings.XSize - 1, ChunkDataSettings.YSize - 1, ChunkDataSettings.ZSize - 1), 1);
            CollectionAssert.AreEquivalent(changed, new List<ChunkSide> { ChunkSide.Px, ChunkSide.Py, ChunkSide.Pz });

            changed = chunk.SetVoxelData(new Vector3Int(ChunkDataSettings.XSize - 1, ChunkDataSettings.YSize - 1, ChunkDataSettings.ZSize - 1), 1);
            CollectionAssert.AreEquivalent(changed, new List<ChunkSide>());

            changed = chunk.SetVoxelData(new Vector3Int(ChunkDataSettings.XSize - 1, ChunkDataSettings.YSize - 2, ChunkDataSettings.ZSize - 2), 1);
            CollectionAssert.AreEquivalent(changed, new List<ChunkSide> { ChunkSide.Px });

            changed = chunk.SetVoxelData(new Vector3Int(ChunkDataSettings.XSize - 1, ChunkDataSettings.YSize - 2, ChunkDataSettings.ZSize - 2), 0);
            CollectionAssert.AreEquivalent(changed, new List<ChunkSide> { ChunkSide.Px });

            changed = chunk.SetVoxelData(new Vector3Int(ChunkDataSettings.XSize - 1, ChunkDataSettings.YSize - 2, ChunkDataSettings.ZSize - 2), 0);
            CollectionAssert.AreEquivalent(changed, new List<ChunkSide>());

            changed = chunk.SetVoxelData(new Vector3Int(ChunkDataSettings.XSize - 1, ChunkDataSettings.YSize - 2, ChunkDataSettings.ZSize - 2), 1);
            CollectionAssert.AreEquivalent(changed, new List<ChunkSide> { ChunkSide.Px });
            changed = chunk.SetVoxelData(new Vector3Int(ChunkDataSettings.XSize - 2, ChunkDataSettings.YSize - 1, ChunkDataSettings.ZSize - 2), 1);
            CollectionAssert.AreEquivalent(changed, new List<ChunkSide> { ChunkSide.Py });
            changed = chunk.SetVoxelData(new Vector3Int(ChunkDataSettings.XSize - 2, ChunkDataSettings.YSize - 2, ChunkDataSettings.ZSize - 1), 1);
            CollectionAssert.AreEquivalent(changed, new List<ChunkSide> { ChunkSide.Pz });
            changed = chunk.SetVoxelData(new Vector3Int(0, 1, 1), 1);
            CollectionAssert.AreEquivalent(changed, new List<ChunkSide> { ChunkSide.Nx });
            changed = chunk.SetVoxelData(new Vector3Int(1, 0, 1), 1);
            CollectionAssert.AreEquivalent(changed, new List<ChunkSide> { ChunkSide.Ny });
            changed = chunk.SetVoxelData(new Vector3Int(1, 1, 0), 1);
            CollectionAssert.AreEquivalent(changed, new List<ChunkSide> { ChunkSide.Nz });
        }

        [Test]
        public void ChunkBordersSolidFlagsAreSetCorrectly()
        {
            var chunk = new Chunk();

            //NX
            for (var z = 0; z < ChunkDataSettings.ZSize; z++)
            {
                for (var y = 0; y < ChunkDataSettings.YSize; y++)
                {
                    Assert.True(!chunk.GetBorderSolid(ChunkSide.Nx));
                    chunk.SetVoxelData(new Vector3Int(0,y,z), 1);
                }
            }
            Assert.True(chunk.GetBorderSolid(ChunkSide.Nx));
            Assert.True(chunk.GetBorderSolid(ChunkSide.Px.OppositeSite()));

            chunk.SetVoxelData(new Vector3Int(0, 0, 0), 0);
            Assert.True(!chunk.GetBorderSolid(ChunkSide.Nx));

            //PX
            chunk = new Chunk();
            for (var z = 0; z < ChunkDataSettings.ZSize; z++)
            {
                for (var y = 0; y < ChunkDataSettings.YSize; y++)
                {
                    Assert.True(!chunk.GetBorderSolid(ChunkSide.Px));
                    chunk.SetVoxelData(new Vector3Int(ChunkDataSettings.XSize - 1, y, z), 1);
                }
            }
            Assert.True(chunk.GetBorderSolid(ChunkSide.Px));
            Assert.True(chunk.GetBorderSolid(ChunkSide.Nx.OppositeSite()));

            chunk.SetVoxelData(new Vector3Int(ChunkDataSettings.XSize - 1, ChunkDataSettings.YSize - 1, ChunkDataSettings.ZSize - 1), 0);
            Assert.True(!chunk.GetBorderSolid(ChunkSide.Px));

            //NZ
            chunk = new Chunk();
            for (var x = 0; x < ChunkDataSettings.XSize; x++)
            {
                for (var y = 0; y < ChunkDataSettings.YSize; y++)
                {
                    Assert.True(!chunk.GetBorderSolid(ChunkSide.Nz));
                    chunk.SetVoxelData(new Vector3Int(x, y, 0), 1);
                }
            }
            Assert.True(chunk.GetBorderSolid(ChunkSide.Nz));
            Assert.True(chunk.GetBorderSolid(ChunkSide.Pz.OppositeSite()));

            chunk.SetVoxelData(new Vector3Int(0, 0, 0), 0);
            Assert.True(!chunk.GetBorderSolid(ChunkSide.Nz));

            //PZ
            chunk = new Chunk();
            for (var x = 0; x < ChunkDataSettings.XSize; x++)
            {
                for (var y = 0; y < ChunkDataSettings.YSize; y++)
                {
                    Assert.True(!chunk.GetBorderSolid(ChunkSide.Pz));
                    chunk.SetVoxelData(new Vector3Int(x, y, ChunkDataSettings.ZSize - 1), 1);
                }
            }
            Assert.True(chunk.GetBorderSolid(ChunkSide.Pz));
            Assert.True(chunk.GetBorderSolid(ChunkSide.Nz.OppositeSite()));

            chunk.SetVoxelData(new Vector3Int(ChunkDataSettings.XSize - 1, ChunkDataSettings.YSize - 1, ChunkDataSettings.ZSize - 1), 0);
            Assert.True(!chunk.GetBorderSolid(ChunkSide.Pz));

            //NY
            chunk = new Chunk();
            for (var x = 0; x < ChunkDataSettings.XSize; x++)
            {
                for (var z = 0; z < ChunkDataSettings.ZSize; z++)
                {
                    Assert.True(!chunk.GetBorderSolid(ChunkSide.Ny));
                    chunk.SetVoxelData(new Vector3Int(x, 0, z), 1);
                }
            }
            Assert.True(chunk.GetBorderSolid(ChunkSide.Ny));
            Assert.True(chunk.GetBorderSolid(ChunkSide.Py.OppositeSite()));

            chunk.SetVoxelData(new Vector3Int(0, 0, 0), 0);
            Assert.True(!chunk.GetBorderSolid(ChunkSide.Ny));

            //PY
            chunk = new Chunk();
            for (var x = 0; x < ChunkDataSettings.XSize; x++)
            {
                for (var z = 0; z < ChunkDataSettings.ZSize; z++)
                {
                    Assert.True(!chunk.GetBorderSolid(ChunkSide.Py));
                    chunk.SetVoxelData(new Vector3Int(x, ChunkDataSettings.YSize - 1, z), 1);
                }
            }
            Assert.True(chunk.GetBorderSolid(ChunkSide.Py));
            Assert.True(chunk.GetBorderSolid(ChunkSide.Ny.OppositeSite()));

            chunk.SetVoxelData(new Vector3Int(ChunkDataSettings.XSize - 1, ChunkDataSettings.YSize - 1, ChunkDataSettings.ZSize - 1), 0);
            Assert.True(!chunk.GetBorderSolid(ChunkSide.Py));
        }

        [Test(Description = "Performance Test")]
        public void CheckReadAndWritePerformance()
        {
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
                chunks[index++ % chunkcount].SetVoxelData(pos, (ushort) (++val % 1337));
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
