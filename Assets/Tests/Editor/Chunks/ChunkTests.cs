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
