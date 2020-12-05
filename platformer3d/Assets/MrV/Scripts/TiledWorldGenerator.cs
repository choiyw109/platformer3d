using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TiledWorldGenerator : MonoBehaviour
{
	[TextArea(3,10)]
	public string map;
/*
....
.f#.
.ff.
....
.  .
  . 
    
.  .
*/
	public Vector3Int size = new Vector3Int(4, 2, 4);

	[System.Serializable]
	public class TileSource {
		public string name;
		public GameObject prefab;
	}
	[ContextMenuItem("Generate", "Generate"), ContextMenuItem("Clear", "Clear"), Tooltip("How many tiles to place along each dimension")]
	public TileSource[] tileSources;

	[ContextMenuItem("Generate","Generate"), ContextMenuItem("Clear", "Clear"), Tooltip("How many units wide each tile (square) is")]
	public float tileSize = 10;
	[ContextMenuItem("Generate", "Generate"), ContextMenuItem("Clear", "Clear"), Tooltip("randomly rotate tile during random placement")]
	public bool randYAngleOrthogonal = true;

	public GameObject[][][] generated = null;

	public GameObject GetNewRandomTile()
	{
		GameObject go = tileSources[Random.Range(0, tileSources.Length)].prefab;
		return go ? Instantiate(go) : null;
	}

	public void Clear()
	{
		if(generated != null) {
			for (int z = 0; z < size.z; ++z)
			{
				for (int y = 0; y < size.y; ++y)
				{
					for (int x = 0; x < size.z; ++z) {
						if (!Application.isPlaying) {
							DestroyImmediate(generated[z][y][z]);
						} else {
							Destroy(generated[z][y][z]);
						}
					}
					generated[z][y] = null;
				}
				generated[z] = null;
			}
			generated = null;
		}
		Transform t = transform;
		for(int i = t.childCount-1; i >= 0; --i)
		{
			Transform child = t.GetChild(i);
			if(child != null)
			if (!Application.isPlaying) {
				DestroyImmediate(child.gameObject);
			} else {
				Destroy(child.gameObject);
			}
		}
	}

	public void Generate() {
		Clear();
		generated = new GameObject[size.z][][];
		string correctedMap = map.Replace("\n", "").Replace("\r", "");
		for (int z = 0; z < size.z; ++z)
		{
			generated[z] = new GameObject[size.y][];
			for (int y = 0; y < size.y; ++y)
			{
				generated[z][y] = new GameObject[size.x];
				for (int x = 0; x < size.x; ++x)
				{
					GameObject go = null;
					if (string.IsNullOrEmpty(map)) {
						go = GetNewRandomTile();
						Transform t = go.transform;
						t.SetParent(transform);
						if (randYAngleOrthogonal) {
							t.Rotate(new Vector3(0, Random.Range(0, 4) * 90, 0));
						}
						t.localPosition = new Vector3(x, y, z) * tileSize;
					} else {
						int index = y * ((size.x) * size.z) + z * (size.x) + x;
						char letter = correctedMap[index];
						//Debug.Log(x+" "+y+" "+z+" : "+index+" '"+letter+"'");
						TileSource ts = System.Array.Find(tileSources, t => t.name[0] == letter);
						if(ts != null && ts.prefab != null) {
							go = Instantiate(ts.prefab);
							if (go) {
								go.transform.SetParent(transform);
								go.transform.localPosition = new Vector3(x, y, z) * tileSize;
							}
						}
					}
					generated[z][y][x] = go;
				}
			}
		}
	}

	public bool IsMissingWorld() {
		return (generated == null || generated.Length == 0 || generated[0].Length == 0
			|| generated[0][0].Length == 0 || generated[0][0][0] == null);
	}

	private void Start()
	{
		if (IsMissingWorld()) {
			Generate();
		}
		//Noisy.PlaySound("song");
	}
}
