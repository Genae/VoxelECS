using System.Collections.Generic;
using Assets.Scripts.Chunks;
using UnityEngine;

public class TestCube : MonoBehaviour
{
    private Chunk _chunk;
    private MaterialCollection _collection;

	// Use this for initialization
	void Start ()
	{
        _collection = new MaterialCollection();
	    _chunk = new Chunk();
	    _chunk.SetVoxelData(new Vector3Int(1, 1, 1), 1, _collection);
	    var builder = gameObject.AddComponent<MeshBuilder>();
	    var neigbours = new Dictionary<ChunkSide, Chunk>
	    {
	        {ChunkSide.Nx, null},
	        {ChunkSide.Px, null},
	        {ChunkSide.Nz, null},
	        {ChunkSide.Pz, null},
	        {ChunkSide.Ny, null},
	        {ChunkSide.Py, null},
	    };
	    builder.Init();
        builder.BuildMesh(_collection, neigbours, _chunk, new Vector3Int(ChunkDataSettings.XSize, ChunkDataSettings.YSize, ChunkDataSettings.ZSize));
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
