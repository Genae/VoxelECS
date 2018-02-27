using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Chunks
{
    public class MeshData
    {
        public Vector3[] Vertices;
        public Dictionary<int, int[]> Triangles;
        public Vector3[] Normals;
        public Vector2[] Uvs;

        public MeshData(Vector3[] verticies, Dictionary<int, int[]> triangles, Vector3[] normals, Vector2[] uvs)
        {
            Vertices = verticies;
            Triangles = triangles;
            Normals = normals;
            Uvs = uvs;
        }
    }
    public class GreedyMeshing
    {
        public static unsafe MeshData CreateMesh(MaterialCollection materialCollection, Dictionary<ChunkSide, Chunk> neighbours, Chunk chunk, Vector3Int size, out List<Vector3> upVoxels)
        {
            //create planes
            LoadedVoxelMaterial[][][,] planes;
            bool*[] neighbourBorders = new bool*[6];
            fixed (bool* px = neighbours[ChunkSide.Nx].MetaData.ChunkBorders.Px)
            {
                neighbourBorders[0] = px;
                fixed (bool* nx = neighbours[ChunkSide.Px].MetaData.ChunkBorders.Nx)
                {
                    neighbourBorders[1] = nx;
                    fixed (bool* pz = neighbours[ChunkSide.Nz].MetaData.ChunkBorders.Pz)
                    {
                        neighbourBorders[2] = pz;
                        fixed (bool* nz = neighbours[ChunkSide.Pz].MetaData.ChunkBorders.Nz)
                        {
                            neighbourBorders[3] = nz;
                            fixed (bool* py = neighbours[ChunkSide.Ny].MetaData.ChunkBorders.Py)
                            {
                                neighbourBorders[4] = py;
                                fixed (bool* ny = neighbours[ChunkSide.Py].MetaData.ChunkBorders.Ny)
                                {
                                    neighbourBorders[5] = ny;
                                    planes = InitializePlanes(chunk, neighbourBorders, size, materialCollection, out upVoxels);
                                }
                            }
                        }

                    }
                }
            }

            //Planes to Rects
            var rects = new Rect[6][][];
            for (var side = 0; side < 6; side++)
            {
                rects[side] = new Rect[side < 2 ? size.x : (side < 4 ? size.z : size.y)][];
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
                    AddRectsToMesh(side, depth, rects[side][depth], ref verticesL, ref trianglesL, ref normalsL, ref uvsL);
                }
            }

            var meshData = new MeshData(verticesL.ToArray(), trianglesL.ToDictionary(v => v.Key, v => v.Value.ToArray()), normalsL.ToArray(), uvsL.ToArray());
            return meshData;
        }

        private static unsafe LoadedVoxelMaterial[][][,] InitializePlanes(Chunk chunk, bool*[] borders, Vector3Int size, MaterialCollection materialCollection, out List<Vector3> upVoxels)
        {
            //initialize Plane Arrays
            upVoxels = new List<Vector3>();
            var planes = new LoadedVoxelMaterial[6][][,];
            for (var side = 0; side < 6; side++)
            {
                planes[side] = new LoadedVoxelMaterial[side < 2 ? size.x : (side < 4 ? size.z : size.y)][,];
                for (var depth = 0; depth < planes[side].Length; depth++)
                {
                    planes[side][depth] = new LoadedVoxelMaterial[side < 2 ? size.y : size.x, side == 2 || side == 3 ? size.y : size.z];
                }
            }
            for (var x = 0; x < size.x; x++)
            {
                for (var y = 0; y < size.y; y++)
                {
                    for (var z = 0; z < size.z; z++)
                    {
                        var id = chunk.GetVoxelData(new Vector3Int(x, y, z));
                        var material = materialCollection.GetById(id);
                        if (id != 0)
                        {
                            if (x == size.x - 1 && !borders[0][z + y * size.z] || x != size.x - 1 && IsTransparent(x + 1, y, z, chunk, materialCollection)) //px
                            {
                                planes[0][x][z, y] = material;
                            }
                            if (x == 0 && !borders[1][z + y * size.z] || x != 0 && IsTransparent(x - 1, y, z, chunk, materialCollection)) //nx
                            {
                                planes[1][x][z, y] = material;
                            }
                            if (z == size.z - 1 && !borders[2][x + y * size.x] || z != size.z - 1 && IsTransparent(x, y, z + 1, chunk, materialCollection)) //pz
                            {
                                planes[2][z][x, y] = material;
                            }
                            if (z == 0 && !borders[3][x + y * size.x] || z != 0 && IsTransparent(x, y, z - 1, chunk, materialCollection)) //nz
                            {
                                planes[3][z][x, y] = material;
                            }
                            if (y == size.y - 1 && !borders[4][x + z * size.x] || y != size.y - 1 && IsTransparent(x, y + 1, z, chunk, materialCollection)) //py
                            {
                                if (y < size.y - 1)
                                    upVoxels.Add(new Vector3(x, y + 1, z));
                                planes[4][y][x, z] = material;
                            }
                            if (y == 0 && !borders[5][x + z * size.x] || y != 0 && IsTransparent(x, y - 1, z, chunk, materialCollection)) //ny
                            {
                                planes[5][y][x, z] = material;
                            }
                        }
                        else
                        {
                            if (y == 0 && borders[5][x + z * size.x])
                            {
                                upVoxels.Add(new Vector3(x, y, z));
                            }
                        }
                    }
                }
            }
            return planes;
        }

        private static bool IsTransparent(int x, int y, int z, Chunk chunk, MaterialCollection mat)
        {
            var id = chunk.GetVoxelData(new Vector3Int(x, y, z));
            return id == 0 || mat.GetById(id).Transparent;
        }

        public static Rect[] CreateRectsForPlane(LoadedVoxelMaterial[,] plane)
        {
            var rects = new List<Rect>();
            var visited = new bool[plane.GetLength(0), plane.GetLength(1)];
            Rect curRectangle = null;
            LoadedVoxelMaterial curType = null;


            for (var j = 0; j < plane.GetLength(1); j++)
            {
                for (var i = 0; i < plane.GetLength(0); i++)
                {
                    var vox = plane[i, j];
                    if (!Equals(vox, curType) || visited[i, j]) //End Rect because of current voxel
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
                    if (vox != null) //Create new Rect if there is no
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
                    if (i == plane.GetLength(1) - 1) // End because of Border
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
            return rects.ToArray();
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

        internal static void ExpandVertically(Rect curRectangle, LoadedVoxelMaterial curType, LoadedVoxelMaterial[,] plane)
        {
            while (true)
            {
                if (curRectangle.Y + curRectangle.Height == plane.GetLength(1))
                    return; //reached bottom

                //check next line
                for (var i = curRectangle.X; i < curRectangle.X + curRectangle.Width; i++)
                {
                    if (!Equals(plane[i, curRectangle.Y + curRectangle.Height], curType))
                        return; // found wrong type
                }
                curRectangle.Height++;
            }
        }

        private static void AddRectsToMesh(int side, int depth, Rect[] rects, ref List<Vector3> vertices, ref Dictionary<int, List<int>> triangles, ref List<Vector3> normals, ref List<Vector2> uvs)
        {
            foreach (var rect in rects)
            {
                var offset = vertices.Count;
                Vector3 vertA = Vector3.zero, vertB = Vector3.zero, vertC = Vector3.zero, vertD = Vector3.zero, norm = Vector3.zero;
                switch (side)
                {
                    case 0:
                    case 1:
                        vertA = new Vector3(depth - 0.5f + side - 0, rect.X - 0.5f + rect.Width, rect.Y - 0.5f);
                        vertB = new Vector3(depth - 0.5f + side - 0, rect.X - 0.5f + rect.Width, rect.Y - 0.5f + rect.Height);
                        vertC = new Vector3(depth - 0.5f + side - 0, rect.X - 0.5f, rect.Y - 0.5f);
                        vertD = new Vector3(depth - 0.5f + side - 0, rect.X - 0.5f, rect.Y - 0.5f + rect.Height);
                        norm = new Vector3(side % 2 != 0 ? 1 : -1, 0, 0);
                        break;
                    case 2:
                    case 3:
                        vertA = new Vector3(rect.X - 0.5f + rect.Width, depth - 0.5f + side - 2, rect.Y - 0.5f);
                        vertB = new Vector3(rect.X - 0.5f + rect.Width, depth - 0.5f + side - 2, rect.Y - 0.5f + rect.Height);
                        vertC = new Vector3(rect.X - 0.5f, depth - 0.5f + side - 2, rect.Y - 0.5f);
                        vertD = new Vector3(rect.X - 0.5f, depth - 0.5f + side - 2, rect.Y - 0.5f + rect.Height);
                        norm = new Vector3(0, side % 2 != 0 ? 1 : -1, 0);
                        break;
                    case 4:
                    case 5:
                        vertA = new Vector3(rect.X - 0.5f + rect.Width, rect.Y - 0.5f, depth - 0.5f + side - 4);
                        vertB = new Vector3(rect.X - 0.5f + rect.Width, rect.Y - 0.5f + rect.Height, depth - 0.5f + side - 4);
                        vertC = new Vector3(rect.X - 0.5f, rect.Y - 0.5f, depth - 0.5f + side - 4);
                        vertD = new Vector3(rect.X - 0.5f, rect.Y - 0.5f + rect.Height, depth - 0.5f + side - 4);
                        norm = new Vector3(0, 0, side % 2 != 0 ? 1 : -1);
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

                if (!triangles.ContainsKey(rect.Type.Id))
                    triangles[rect.Type.Id] = new List<int>();

                if (side == 5 || side == 2 || side == 1)
                {
                    triangles[rect.Type.Id].AddRange(new[]
                    {
                        0 + offset, 1 + offset, 3 + offset,
                        0 + offset, 3 + offset, 2 + offset
                    });
                }
                else
                {
                    triangles[rect.Type.Id].AddRange(new[]
                    {
                        1 + offset, 0 + offset, 3 + offset,
                        3 + offset, 0 + offset, 2 + offset
                    });
                }

                // ReSharper disable once PossibleLossOfFraction
                var uvcoord = new Vector2(rect.Type.AtlasPosition / MaterialCollectionSettings.AtlasSize / (float)MaterialCollectionSettings.AtlasSize, rect.Type.AtlasPosition % MaterialCollectionSettings.AtlasSize / (float)MaterialCollectionSettings.AtlasSize);
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
        public LoadedVoxelMaterial Type;

        public Rect(int x, int y, LoadedVoxelMaterial type)
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