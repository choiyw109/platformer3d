using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TiledWorldGenerator : MonoBehaviour
{
	public GameObject[] tiles;
	[ContextMenuItem("Generate","Generate"), ContextMenuItem("Clear", "Clear"), Tooltip("How many units wide each tile (square) is")]
	public float tileSize = 10;
	[ContextMenuItem("Generate", "Generate"), ContextMenuItem("Clear", "Clear"), Tooltip("randomly rotate tile during random placement")]
	public bool randZAngleOrthogonal = true;
	[ContextMenuItem("Generate", "Generate"), ContextMenuItem("Clear", "Clear"), Tooltip("How many tiles to place along each dimension")]
	public Vector3 tileAmount = new Vector3(10,1,10);

	public GameObject[][][] generated = null;

	private void OnValidate() {
		// keep dimensions of the game world positive
		for(int i = 0; i < 3; ++i) { tileAmount[i] = (int)Mathf.Max(1, tileAmount[i]); }
	}

	public GameObject GetNewRandomTile()
	{
		GameObject go = Instantiate(tiles[Random.Range(0, tiles.Length)]);
		return go;
	}

	public void Clear()
	{
		if(generated != null) {
			for (int z = 0; z < tileAmount.z; ++z)
			{
				for (int y = 0; y < tileAmount.y; ++y)
				{
					for (int x = 0; x < tileAmount.z; ++z) {
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
		generated = new GameObject[(int)tileAmount.z][][];
		for (int z = 0; z < tileAmount.z; ++z)
		{
			generated[z] = new GameObject[(int)tileAmount.y][];
			for (int y = 0; y < tileAmount.y; ++y)
			{
				generated[z][y] = new GameObject[(int)tileAmount.x];
				for (int x = 0; x < tileAmount.x; ++x)
				{
					GameObject go = GetNewRandomTile();
					Transform t = go.transform;
					t.SetParent(transform);
					if (randZAngleOrthogonal)
					{
						t.Rotate(new Vector3(0, Random.Range(0, 4) * 90, 0));
					}
					t.localPosition = new Vector3(x, y, z) * tileSize;
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
	}
}
