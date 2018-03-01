using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.VoxelEngine.Containers
{
    public class Grid3D<T> : IEnumerable<KeyValuePair<Vector3Int,T>> where T: class
    {
        private readonly Dictionary<int, Dictionary<int, Dictionary<int, T>>> _nodes = new Dictionary<int, Dictionary<int, Dictionary<int, T>>>();
        private Vector3Int _size;
        
        public T this[int xPos, int yPos, int zPos]
        {
            get { return Get(xPos, yPos, zPos); }
            set { Set(xPos, yPos, zPos, value);}
        }

        public T Get(int xPos, int yPos, int zPos)
        {
            return _nodes.ContainsKey(xPos) && _nodes[xPos].ContainsKey(yPos) && _nodes[xPos][yPos].ContainsKey(zPos) ? _nodes[xPos][yPos][zPos] : null;
        }

        public void Set(int xPos, int yPos, int zPos, T value)
        {
            _size = default(Vector3Int);
            if (!_nodes.ContainsKey(xPos))
                _nodes[xPos] = new Dictionary<int, Dictionary<int, T>>();
            if (!_nodes[xPos].ContainsKey(yPos))
                _nodes[xPos][yPos] = new Dictionary<int, T>();
            _nodes[xPos][yPos][zPos] = value;
        }
        
        public T GetNearestItem(Vector3Int pos, int maxRadius)
        {
            for (var dX = 0; dX <= maxRadius; dX++)
            {
                for (var dY = 0; dY <= maxRadius; dY++)
                {
                    for (var dZ = 0; dZ <= maxRadius; dZ++)
                    {
                        var node = this[pos.x + dX, pos.y + dY, pos.z + dZ] ?? this[pos.x - dX, pos.y + dY, pos.z + dZ] ??
                                   this[pos.x + dX, pos.y - dY, pos.z + dZ] ?? this[pos.x - dX, pos.y - dY, pos.z + dZ] ??
                                   this[pos.x + dX, pos.y + dY, pos.z - dZ] ?? this[pos.x - dX, pos.y + dY, pos.z - dZ] ??
                                   this[pos.x + dX, pos.y - dY, pos.z - dZ] ?? this[pos.x - dX, pos.y - dY, pos.z - dZ];
                        if (node != null)
                            return node;

                    }
                }
            }
            return null;
        }
        
        public Vector3Int GetSize()
        {
            if (_size == default(Vector3Int))
            {
                if (_nodes.Count == 0)
                {
                    _size = new Vector3Int(0, 0, 0);
                }
                else
                {
                    var x = _nodes.Max(v => v.Key) + 1;
                    var y = _nodes.Values.Max(yDics => yDics.Max(v => v.Key)) + 1;
                    var z = _nodes.Values.Max(yDics => yDics.Values.Max(zDics => zDics.Max(v => v.Key))) + 1;
                    _size = new Vector3Int(x, y, z);
                }
            }
            return _size;
        }

        public IEnumerator<KeyValuePair<Vector3Int, T>> GetEnumerator()
        {
            return _nodes.SelectMany(x => x.Value.SelectMany(y => y.Value.Select(z => new KeyValuePair<Vector3Int, T>(new Vector3Int(x.Key, y.Key, z.Key), z.Value)))).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Remove(Vector3Int position)
        {
            _size = default(Vector3Int);
            if (!_nodes.ContainsKey(position.x))
                return;
            if (!_nodes[position.x].ContainsKey(position.y))
                return;
            _nodes[position.x][position.y].Remove(position.z);
            if (_nodes[position.x][position.y].Count == 0)
                _nodes[position.x].Remove(position.y);
            if (_nodes[position.x].Count == 0)
                _nodes.Remove(position.x);
        }
    }
}