using Assets.Scripts.Chunks;
using NUnit.Framework;
using UnityEngine;

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
    }
}
