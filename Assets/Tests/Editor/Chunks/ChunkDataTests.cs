using System;
using Assets.Scripts.Chunks;
using NUnit.Framework;
using UnityEngine;

namespace Assets.Tests.Editor.Chunks
{
    public class ChunkDataTests {

        [Test]
        public void CanReadAndWriteVoxelData() {
            var chunkData = new ChunkData(new Vector3Int(16, 16, 16));
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
            var chunkData = new ChunkData(new Vector3Int(16, 16, 16));
            var pos = new Vector3Int(10, 6, 8);
            Assert.AreEqual(0, chunkData.GetVoxelData(pos));
            chunkData.SetVoxelData(pos, 1);
            Assert.AreEqual(1, chunkData.GetVoxelData(pos));
            var data = chunkData.SerializeVoxelData();
            Debug.Log(BitConverter.ToString(data));
            var chunkData2 = new ChunkData(new Vector3Int(16, 16, 16));
            chunkData2.DeserializeVoxelData(data);
            Assert.AreEqual(1, chunkData2.GetVoxelData(pos));
        }
    }
}
