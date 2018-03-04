using Assets.Scripts.VoxelEngine.Containers.Chunks;
using Assets.Scripts.VoxelEngine.Materials;
using NUnit.Framework;
using UnityEngine;

namespace Assets.Testing.VoxelEngine
{
    public class RenderTesting
    {
        [Test]
        public void TestRenderIsVisibleFromAllSides()
        {
            var collection = new MaterialCollection();
            var go = new GameObject("map");
            var cloud = new ChunkCloud(collection, go.transform);
            cloud.StartBatch();
            var start = new Vector3Int(-30, -30, -30);
            var end = new Vector3Int(30, 30, 30);
            for (var x = start.x; x < end.x; x++)
            {
                for (var y = start.y; y < end.y; y++)
                {
                    for (var z = start.z; z < end.z; z++)
                    {
                        cloud.SetVoxel("glass", new Vector3Int(x, y, z));
                    }
                }
            }
            cloud.FinishBatch();
            CheckBlockRendered(start, end);
        }

        private void CheckBlockRendered(Vector3Int start, Vector3Int end)
        {
            //Check borders are rendered
            for (var x = start.x; x < end.x; x++)
            {
                for (var y = start.y; y < end.y; y++)
                {
                    for (var z = start.z; z < end.z; z++)
                    {
                        if (x == start.x)
                        { 
                            Assert.True(Physics.Raycast(new Vector3(x - 1, y, z), new Vector3(1, 0, 0)));
                        }
                        if (y == start.y)
                        {
                            Assert.True(Physics.Raycast(new Vector3(x, y - 1, z), new Vector3(0, 1, 0)));
                        }
                        if (z == start.z)
                        {
                            Assert.True(Physics.Raycast(new Vector3(x, y, z - 1), new Vector3(0, 0, 1)));
                        }
                        if (x == end.x)
                        {
                            Assert.True(Physics.Raycast(new Vector3(x + 1, y, z), new Vector3(-1, 0, 0)));
                        }
                        if (y == end.y)
                        {
                            Assert.True(Physics.Raycast(new Vector3(x, y + 1, z), new Vector3(0, -1, 0)));
                        }
                        if (z == end.z)
                        {
                            Assert.True(Physics.Raycast(new Vector3(x, y, z + 1), new Vector3(0, 0, -1)));
                        }
                    }
                }
            }
        }

        [Test]
        public void TestRenderIsInvisibleInside()
        {
            var collection = new MaterialCollection();
            var go = new GameObject("map");
            var cloud = new ChunkCloud(collection, go.transform);

            var start = new Vector3Int(-1, -1, -1);
            var end = new Vector3Int(1, 1, 1);
            for (var x = start.x; x < end.x; x++)
            {
                for (var y = start.y; y < end.y; y++)
                {
                    for (var z = start.z; z < end.z; z++)
                    {
                        cloud.SetVoxel("glass", new Vector3Int(x, y, z));
                    }
                }
            }
            CheckBlockEmpty(start, end);
        }

        private void CheckBlockEmpty(Vector3Int start, Vector3Int end)
        {
            for (var x = start.x; x < end.x; x++)
            {
                for (var y = start.y; y < end.y; y++)
                {
                    for (var z = start.z; z < end.z; z++)
                    {
                        Assert.False(Physics.Raycast(new Vector3(x, y, z), new Vector3(1, 0, 0)));
                        Assert.False(Physics.Raycast(new Vector3(x, y, z), new Vector3(-1, 0, 0)));
                        Assert.False(Physics.Raycast(new Vector3(x, y, z), new Vector3(0, 1, 0)));
                        Assert.False(Physics.Raycast(new Vector3(x, y, z), new Vector3(0, -1, 0)));
                        Assert.False(Physics.Raycast(new Vector3(x, y, z), new Vector3(0, 0, 1)));
                        Assert.False(Physics.Raycast(new Vector3(x, y, z), new Vector3(0, 0, -1)));
                    }
                }
            }
        }
    }
}
