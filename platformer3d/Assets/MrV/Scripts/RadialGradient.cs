using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadialGradient : MonoBehaviour
{
	[ContextMenuItem("Create","Create")]
	public TextureSize textureSize = TextureSize.bit256;
	public enum TextureSize
	{
		bit16 = 16,
		bit32 = 32,
		bit64 = 64,
		bit128 = 128,
		bit256 = 256,
		bit512 = 512,
	}
	public FilterMode filterMode = FilterMode.Bilinear;

	[ContextMenuItem("Save","Save")]
	public string filename;

	private Texture2D texture;

	public void Save() {
		if(texture != null) { SaveTextureAsPNG(texture, filename); }
	}

	public static void SaveTextureAsPNG(Texture2D _texture, string path)
	{
		path = Application.dataPath + "/" + path;
		Debug.Log("saving to " + path);
		byte[] _bytes = _texture.EncodeToPNG();
		System.IO.File.WriteAllBytes(path, _bytes);
		Debug.Log(_bytes.Length / 1024 + "Kb was saved as: " + path);
	}

	public void Create()
	{
		GenerateTexture((int)textureSize, filterMode);//, wrapMode);
	}

	[ContextMenuItem("Create", "Create")]
	public AnimationCurve falloff = new AnimationCurve();

	public void GenerateTexture(int size, FilterMode filter)//, TextureWrapMode wrap)
	{
		texture = new Texture2D(size, size);
		GetComponent<Renderer>().material.mainTexture = texture;

		Vector2 center = new Vector2(size / 2f, size / 2f);

		float radius = size / 2;

		for (int y = 0; y < texture.height; y++)
		{
			for (int x = 0; x < texture.width; x++)
			{
				float dist = Vector2.Distance(center, new Vector2(x, y));
				float currentAlpha = dist / radius;
				currentAlpha = falloff.Evaluate(currentAlpha);
				Color color = new Color(1, 1, 1, currentAlpha);
				texture.SetPixel(x, y, color);
			}
		}
		texture.Apply();
		texture.filterMode = filter;

		SpriteRenderer s = GetComponent<SpriteRenderer>();
		s.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(.5f, .5f));
		s.sprite.name = "New Sprite";
	}
}
