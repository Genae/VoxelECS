using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.VoxelEngine.Containers;
using Assets.Scripts.VoxelEngine.Containers.Chunks;
using Assets.Scripts.VoxelEngine.Materials;
using UnityEngine;

namespace Assets.Scripts.VoxelEngine.Renderers
{
    public class MeshData
    {
        public Vector3[] Vertices;
        public Dictionary<int, int[]> Triangles;
        public Vector3[] Normals;
        public Vector2[] Uvs;
        public ushort[][][,] Planes;

        public MeshData(Vector3[] verticies, Dictionary<int, int[]> triangles, Vector3[] normals, Vector2[] uvs, ushort[][][,] planes)
        {
            Vertices = verticies;
            Triangles = triangles;
            Normals = normals;
            Uvs = uvs;
            Planes = planes;
        }
    }
    public class GreedyMeshing
    {
        public static MeshData CreateMesh(IVoxelContainer container, Dictionary<ChunkSide, Chunk> neighbours, MaterialCollection materialCollection, int? slice, bool topSlice, out List<Vector3> upVoxels)
        {
            //create planes
            if (neighbours == null)
            {
                neighbours = new Dictionary<ChunkSide, Chunk>
                {
                    {ChunkSide.Nx, null},
                    {ChunkSide.Px, null},
                    {ChunkSide.Nz, null},
                    {ChunkSide.Pz, null},
                    {ChunkSide.Ny, null},
                    {ChunkSide.Py, null},
                };
            }
            var size = container.GetSize();
            var planes = InitializePlanes(container, neighbours, materialCollection, slice, topSlice, out upVoxels);

            //Planes to Rects
            var rects = new List<Rect>[6][];
            for (var side = 0; side < 6; side++)
            {
                rects[side] = new List<Rect>[side < 2 ? size.x : (side < 4 ? size.z : size.y)];
                for (var depth = 0; depth < rects[side].Length; depth++)
                {
                    rects[side][depth] = CreateRectsForPlane(planes[side][depth]);
                }
            }

            //Rects to Mesh
            var verticesL = new List<Vector3>();
            var trianglesL = new Dictionary<int, List<int>>();
            var normalsL = new List<Vector3>();
            var uvsL = new List<Vector2>();
            for (var side = 0; side < 6; side++)
            {
                for (var depth = 0; depth < rects[side].Length; depth++)
                {
                    AddRectsToMesh(side, depth, rects[side][depth], materialCollection, ref verticesL, ref trianglesL, ref normalsL, ref uvsL);
                }
            }

            var meshData = new MeshData(verticesL.ToArray(), trianglesL.ToDictionary(v => v.Key, v => v.Value.ToArray()), normalsL.ToArray(), uvsL.ToArray(), planes);
            return meshData;
        }

        private static ushort[][][,] InitializePlanes(IVoxelContainer container, Dictionary<ChunkSide, Chunk> neigbours, MaterialCollection materialCollection, int? slice, bool topSlice, out List<Vector3> upVoxels)
        {
            //initialize Plane Arrays
            var size = container.GetSize();
            upVoxels = new List<Vector3>();
            var planes = new ushort[6][][,];
            for (var side = 0; side < 6; side++)
            {
                planes[side] = new ushort[side < 2 ? size.x : (side < 4 ? size.z : size.y)][,];
                for (var depth = 0; depth < planes[side].Length; depth++)
                {
                    planes[side][depth] = new ushort[side < 2 ? size.y : size.x, side == 2 || side == 3 ? size.y : size.z];
                }
            }
            for (var x = 0; x < size.x; x++)
            {
                for (var y = 0; y < size.y; y++)
                {
                    for (var z = 0; z < size.z; z++)
                    {
                        if (slice != null && (slice.Value <= y && !topSlice || slice.Value > y && topSlice))
                            continue;
                        var id = container.GetVoxelData(new Vector3Int(x, y, z));
                        var material = materialCollection.GetById(id);
                        if (id != 0)
                        {
                            if (x == size.x - 1 && IsTransparent(0, y, z, neigbours[ChunkSide.Px], materialCollection, material) || x != size.x - 1 && IsTransparent(x + 1, y, z, container, materialCollection, material)) //px
                            {
                                planes[0][x][y, z] = id;
                            }
                            if (x == 0 && IsTransparent(size.x - 1, y, z, neigbours[ChunkSide.Nx], materialCollection, material) || x != 0 && IsTransparent(x - 1, y, z, container, materialCollection, material)) //nx
                            {
                                planes[1][x][y, z] = id;
                            }
                            if (z == size.z - 1 && IsTransparent(x, y, 0, neigbours[ChunkSide.Pz], materialCollection, material) || z != size.z - 1 && IsTransparent(x, y, z + 1, container, materialCollection, material)) //pz
                            {
                                planes[2][z][x, y] = id;
                            }
                            if (z == 0 && IsTransparent(x, y, size.z-1, neigbours[ChunkSide.Nz], materialCollection, material) || z != 0 && IsTransparent(x, y, z - 1, container, materialCollection, material)) //nz
                            {
                                planes[3][z][x, y] = id;
                            }
                            if (y == size.y - 1 && IsTransparent(x, 0, z, neigbours[ChunkSide.Py], materialCollection, material, slice.HasValue ? slice - size.y : null) || y != size.y - 1 && IsTransparent(x, y + 1, z, container, materialCollection, material, slice)) //py
                            {
                                if (y < size.y - 1 && container.GetVoxelData(new Vector3Int(x, y + 1, z)) == 0)
                                    upVoxels.Add(new Vector3(x, y + 1, z));
                                planes[4][y][x, z] = id;
                            }
                            if (y == 0 && IsTransparent(x, size.y - 1, z, neigbours[ChunkSide.Ny], materialCollection, material) || y != 0 && IsTransparent(x, y - 1, z, container, materialCollection, material)) //ny
                            {
                                planes[5][y][x, z] = id;
                            }
                        }
                        else
                        {
                            if (y == 0 && (neigbours[ChunkSide.Ny] == null || neigbours[ChunkSide.Ny].GetVoxelData(new Vector3Int(x, size.y - 1, z)) == 0))
                            {
                                upVoxels.Add(new Vector3(x, y, z));
                            }
                        }
                    }
                }
            }
            return planes;
        }

        private static bool IsTransparent(int x, int y, int z, IVoxelContainer container, MaterialCollection matCol, LoadedVoxelMaterial mat, int? slice = null)
        {
            if (container == null)
                return true;
            if (slice != null && slice <= y)
                return true;
            var id = container.GetVoxelData(new Vector3Int(x, y, z));
            if (id == mat.Id)
                return false;
            return id == 0 || matCol.GetById(id).Transparent;
        }

        public static List<Rect> CreateRectsForPlane(ushort[,] plane)
        {
            var rects = new List<Rect>();
            var visited = new bool[plane.GetLength(0), plane.GetLength(1)];
            Rect curRectangle = null;
            ushort curType = 0;


            var l1 = plane.GetLength(1);
            var l0 = plane.GetLength(0);
            for (var j = 0; j < l1; j++)
            {
                for (var i = 0; i < l0; i++)
                {
                    var vox = plane[i, j];
                    if (vox != curType || visited[i, j]) //End Rect because of current voxel
                    {
                        if (curRectangle != null)
                        {
                            ExpandVertically(curRectangle, curType, plane);
                            rects.Add(curRectangle);
                            SetVisited(curRectangle, visited);
                            curRectangle = null;
                        }
                        if (visited[i, j])
                            continue;
                    }
                    if (vox != 0) //Create new Rect if there is no
                    {
                        if (curRectangle == null)
                        {
                            curRectangle = new Rect(i, j, vox);
                            curType = vox;
                        }
                        else
                        {
                            curRectangle.Width++;
                        }
                    }
                    if (i == l1 - 1) // End because of Border
                    {
                        if (curRectangle != null)
                        {
                            ExpandVertically(curRectangle, curType, plane);
                            rects.Add(curRectangle);
                            SetVisited(curRectangle, visited);
                            curRectangle = null;
                        }
                    }
                }
            }
            return rects;
        }

        
        public static void SetVisited(Rect curRectangle, bool[,] visited)
        {
            for (var i = curRectangle.X; i < curRectangle.X + curRectangle.Width; i++)
            {
                for (var j = curRectangle.Y; j < curRectangle.Y + curRectangle.Height; j++)
                {
                    visited[i, j] = true;
                }
            }
        }

        internal static void ExpandVertically(Rect curRectangle, ushort curType, ushort[,] plane)
        {
            while (true)
            {
                if (curRectangle.Y + curRectangle.Height == plane.GetLength(1))
                    return; //reached bottom

                //check next line
                for (var i = curRectangle.X; i < curRectangle.X + curRectangle.Width; i++)
                {
                    if (curType != plane[i, curRectangle.Y + curRectangle.Height])
                        return; // found wrong type
                }
                curRectangle.Height++;
            }
        }

        private static void AddRectsToMesh(int side, int depth, List<Rect> rects, MaterialCollection materialCollection, ref List<Vector3> vertices, ref Dictionary<int, List<int>> triangles, ref List<Vector3> normals, ref List<Vector2> uvs)
        {
            foreach (var rect in rects)
            {
                var offset = vertices.Count;
                Vector3 vertA = Vector3.zero, vertB = Vector3.zero, vertC = Vector3.zero, vertD = Vector3.zero, norm = Vector3.zero;
                switch (side)
                {
                    case 0:
                    case 1:
                        vertA = new Vector3(depth + 0.5f - side, rect.X - 0.5f + rect.Width, rect.Y - 0.5f);
                        vertB = new Vector3(depth + 0.5f - side, rect.X - 0.5f + rect.Width, rect.Y - 0.5f + rect.Height);
                        vertC = new Vector3(depth + 0.5f - side, rect.X - 0.5f, rect.Y - 0.5f);
                        vertD = new Vector3(depth + 0.5f - side, rect.X - 0.5f, rect.Y - 0.5f + rect.Height);
                        norm = new Vector3(side % 2 != 0 ? -1 : 1, 0, 0);
                        break;
                    case 2:
                    case 3:
                        vertA = new Vector3(rect.X - 0.5f + rect.Width, rect.Y - 0.5f, depth + 0.5f - side % 2);
                        vertB = new Vector3(rect.X - 0.5f + rect.Width, rect.Y - 0.5f + rect.Height, depth + 0.5f - side % 2);
                        vertC = new Vector3(rect.X - 0.5f, rect.Y - 0.5f, depth + 0.5f - side % 2);
                        vertD = new Vector3(rect.X - 0.5f, rect.Y - 0.5f + rect.Height, depth + 0.5f - side % 2);
                        norm = new Vector3(0, 0, side % 2 != 0 ? -1 : 1);
                        break;
                    case 4:
                    case 5:
                        vertA = new Vector3(rect.X - 0.5f, depth + 0.5f - side % 2, rect.Y - 0.5f + rect.Height);
                        vertB = new Vector3(rect.X - 0.5f + rect.Width, depth + 0.5f - side % 2, rect.Y - 0.5f + rect.Height);
                        vertC = new Vector3(rect.X - 0.5f, depth + 0.5f - side % 2, rect.Y - 0.5f);
                        vertD = new Vector3(rect.X - 0.5f + rect.Width, depth + 0.5f - side % 2, rect.Y - 0.5f);
                        norm = new Vector3(0, side % 2 != 0 ? -1 : 1, 0);
                        break;
                }

                vertices.AddRange(new[]
                {
                    new Vector3(vertA.x, vertA.y, vertA.z),
                    new Vector3(vertB.x, vertB.y, vertB.z),
                    new Vector3(vertC.x, vertC.y, vertC.z),
                    new Vector3(vertD.x, vertD.y, vertD.z)
                });

                normals.AddRange(new[]
                {
                    new Vector3(norm.x, norm.y, norm.z),
                    new Vector3(norm.x, norm.y, norm.z),
                    new Vector3(norm.x, norm.y, norm.z),
                    new Vector3(norm.x, norm.y, norm.z)
                });

                if (!triangles.ContainsKey(rect.Type))
                    triangles[rect.Type] = new List<int>();

                if (side % 2 == 0)
                {
                    triangles[rect.Type].AddRange(new[]
                    {
                        0 + offset, 1 + offset, 3 + offset,
                        0 + offset, 3 + offset, 2 + offset
                    });
                }
                else
                {
                    triangles[rect.Type].AddRange(new[]
                    {
                        1 + offset, 0 + offset, 3 + offset,
                        3 + offset, 0 + offset, 2 + offset
                    });
                }

                // ReSharper disable once PossibleLossOfFraction
                var material = materialCollection.GetById(rect.Type);
                var uvcoord = new Vector2((int)(material.AtlasPosition / MaterialCollectionSettings.AtlasSize) / (float)MaterialCollectionSettings.AtlasSize, material.AtlasPosition % MaterialCollectionSettings.AtlasSize / (float)MaterialCollectionSettings.AtlasSize);
                uvs.AddRange(new[]
                {
                    new Vector2(uvcoord.x + 0.1f/MaterialCollectionSettings.AtlasSize, uvcoord.y+ 0.1f/MaterialCollectionSettings.AtlasSize),
                    new Vector2(uvcoord.x + 0.1f/MaterialCollectionSettings.AtlasSize, uvcoord.y+ 0.1f/MaterialCollectionSettings.AtlasSize),
                    new Vector2(uvcoord.x + 0.1f/MaterialCollectionSettings.AtlasSize, uvcoord.y+ 0.1f/MaterialCollectionSettings.AtlasSize),
                    new Vector2(uvcoord.x + 0.1f/MaterialCollectionSettings.AtlasSize, uvcoord.y+ 0.1f/MaterialCollectionSettings.AtlasSize)
                });
            }
        }

    }

    public class Rect
    {
        public int X, Y;
        public int Width, Height;
        public ushort Type;

        public Rect(int x, int y, ushort type)
        {
            X = x;
            Y = y;
            Width = 1;
            Height = 1;
            Type = type;
        }

        protected bool Equals(Rect other)
        {
            return X == other.X && Y == other.Y && Width == other.Width && Height == other.Height && Equals(Type, other.Type);
        }

        public override string ToString()
        {
            return "Point: (" + X + "/" + Y + "), " + Width + "x" + Height;
        }
    }
}