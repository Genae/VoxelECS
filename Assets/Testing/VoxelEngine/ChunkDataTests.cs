using System;
using System.Diagnostics;
using System.Linq;
using Assets.Scripts.VoxelEngine.Containers.Chunks;
using NUnit.Framework;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

namespace Assets.Testing.VoxelEngine
{
    public class ChunkDataTests {

        [Test]
        public void CanReadAndWriteVoxelData()
        {
            var chunkData = new ChunkData();
            var pos = new Vector3Int(10, 6, 8);
            Assert.AreEqual(0, chunkData.GetVoxelData(pos));
            chunkData.SetVoxelData(pos, 1);
            Assert.AreEqual(1, chunkData.GetVoxelData(pos));
            
            var bulkData = new ushort[16, 16, 16];
            bulkData[1, 2, 3] = 2;
            chunkData.SetVoxelData(bulkData);
            Assert.AreEqual(0, chunkData.GetVoxelData(pos));
            Assert.AreEqual(2, chunkData.GetVoxelData(new Vector3Int(1, 2, 3)));
            
        }

        [Test]
        public void CanSerializeAndDeserializeData()
        {
            var chunkData = new ChunkData();
            var pos = new Vector3Int(10, 6, 8);
            Assert.AreEqual(0, chunkData.GetVoxelData(pos));
            chunkData.SetVoxelData(pos, 1);
            Assert.AreEqual(1, chunkData.GetVoxelData(pos));
            var data = chunkData.SerializeVoxelData();
            Debug.Log(BitConverter.ToString(data));
            var chunkData2 = new ChunkData();
            chunkData2.DeserializeVoxelData(data);
            Assert.AreEqual(1, chunkData2.GetVoxelData(pos));
        }

        [Test]
        public void OutOfBoundsAccessThrowsError()
        {
            var chunkData = new ChunkData();
            Assert.Throws<IndexOutOfRangeException>(() => chunkData.GetVoxelData(new Vector3Int(16, 6, 8)));
            Assert.Throws<IndexOutOfRangeException>(() => chunkData.GetVoxelData(new Vector3Int(10, 18, 8)));
            Assert.Throws<IndexOutOfRangeException>(() => chunkData.GetVoxelData(new Vector3Int(10, 6, 20)));
            Assert.Throws<IndexOutOfRangeException>(() => chunkData.SetVoxelData(new Vector3Int(16, 6, 8), 1));
            Assert.Throws<IndexOutOfRangeException>(() => chunkData.SetVoxelData(new Vector3Int(10, 18, 8), 1));
            Assert.Throws<IndexOutOfRangeException>(() => chunkData.SetVoxelData(new Vector3Int(10, 6, 20), 1));
            Assert.Throws<IndexOutOfRangeException>(() => chunkData.SetVoxelData(new ushort[16, 16, 17]));
        }

        [Test(Description = "Performance Test")]
        public void CheckReadWriteAndSerializePerformance()
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
            var chunks = new ChunkData[chunkcount];
            for (var i = 0; i < chunkcount; i++)
            {
                chunks[i] = new ChunkData();
            }

            Debug.Log("Writing");
            var val = 0;
            var index = 0;
            var watch = new Stopwatch();
            watch.Start();
            foreach (var pos in randPos)
            {
                chunks[index++%chunkcount].SetVoxelData(pos, (ushort)(++val%1337));
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
                data[index] = chunks[index++%chunkcount].GetVoxelData(pos);
            }
            var timeR = watch.ElapsedMilliseconds;
            Debug.Log(timeR);

            Debug.Log("Serializing");
            var bytes = new byte[chunkcount][];
            watch.Reset();
            watch.Start();
            for (var i = 0; i < chunkcount; i++)
            {
                bytes[i] = chunks[i].SerializeVoxelData();
            }
            var timeS = watch.ElapsedMilliseconds;
            Debug.Log(timeS);


            Debug.Log("Deserializing");
            watch.Reset();
            watch.Start();
            for (var i = 0; i < chunkcount; i++)
            {
                chunks[i] = new ChunkData();
                chunks[i].DeserializeVoxelData(bytes[i]);
            }
            var timeD = watch.ElapsedMilliseconds;
            Debug.Log(timeD);

            Debug.Log("Serialized Data Size in bytes: " + bytes.SelectMany(b => b).ToArray().Length);

            Assert.LessOrEqual(timeW, 400);
            Assert.LessOrEqual(timeR, 400);
            Assert.LessOrEqual(timeS, 1000);
            Assert.LessOrEqual(timeD, 1000);
        }
    }
}
