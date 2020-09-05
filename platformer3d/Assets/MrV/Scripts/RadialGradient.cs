using UnityEngine;

namespace Moback.UXP.AssetGeneration
{
	[ExecuteInEditMode]
	public class RadialGradient : MonoBehaviour
	{
		[ContextMenuItem("Create", "Create")]
		public TextureSize textureSize = TextureSize.bit256;
		public enum TextureSize { bit16 = 16, bit32 = 32, bit64 = 64, bit128 = 128, bit256 = 256, bit512 = 512, }
		public FilterMode filterMode = FilterMode.Bilinear;

		[ContextMenuItem("Save", "Save")]
		public string filename;

		[ContextMenuItem("Create", "Create")]
		public AnimationCurve alphaCurve = AnimationCurve.Linear(0, 1, 1, 0);

		public Gradient colorGradient;

		private Texture2D texture;

		private SpriteRenderer s;

		public enum Type {
			Radial = 0, Square, HoriztonalAndVertical,
			Bottom, Top, Left, Right, Horizontal, Vertical,
			TopLeft, TopRight, BottomLeft, BottomRight,
			TopAndLeft, TopAndRight, BottomAndLeft, BottomAndRight,
			Polygon
		}

		public bool curveIsAlpha = true;
		public bool realtimeEdit = true;
		private bool doRealtimeEditOneMoreTime = false;

		[ContextMenuItem("Create", "Create")]
		public Type type = Type.Radial;

		public float[] convexPointAngle = new float[] { };
		public float[] convexPointDistance = new float[] { };

		private Vector2[] edgeDirections;
		public Vector2[] edgeOrigins;

		// TODO center offset
		// TODO preset: tile [0,45,90,135,180,225,270,315], [1,1.3,1,1.3,1,1.3,1,1.3]
		// TODO preset: vertical gradient
		// TODO preset: horizontal gradient
		// TODO preset: triangle, square, pentagon, hexagon, octogon

		public void Save() {
			if (texture == null) return;
			if (!filename.ToLower().EndsWith(".png")) {
				filename += ".png";
			}
			SaveTextureAsPNG(texture, filename);
		}

		public void CreatePoints() {
			bool neededToUpdateArrayLength = false;
			if(convexPointAngle.Length != convexPointDistance.Length) {
				float[] d = new float[convexPointAngle.Length];
				for(int i = 0; i < d.Length && i < convexPointDistance.Length; ++i) { d[i] = convexPointDistance[i]; }
				convexPointDistance = d;
				neededToUpdateArrayLength = true;
			}
			bool needToAutoGenerateConvexPoints = true;
			if (!neededToUpdateArrayLength) {
				for (int i = 0; i < convexPointDistance.Length; ++i) {
					if(convexPointDistance[i] != 0) { needToAutoGenerateConvexPoints = false; }
					if(convexPointDistance[i] < 1f/1024) { convexPointDistance[i] = 1f/1024; }
				}
			}
			Vector2 center = new Vector2(.5f, .5f);
			if (needToAutoGenerateConvexPoints) {
				float anglePerTurn = 360f / convexPointAngle.Length;
				edgeOrigins = new Vector2[convexPointAngle.Length];
				for (int i = 0; i < convexPointAngle.Length; ++i) {
					convexPointDistance[i] = 1;
					convexPointAngle[i] = anglePerTurn * i;
					edgeOrigins[i] = center;
				}
			}
			edgeDirections = new Vector2[convexPointAngle.Length];
			// generate points
			for (int i = 0; i < edgeDirections.Length; ++i) {
				Quaternion q = Quaternion.AngleAxis(convexPointAngle[i], Vector3.forward);
				float multiplier = (1 / convexPointDistance[i]);
				Vector3 dir = Vector3.up * multiplier;
				edgeDirections[i] = (Vector2)(q * dir);
			}
		}

		public void Create() {
			if (s == null) { s = GetComponent<SpriteRenderer>(); }
			if (s == null) { s = gameObject.AddComponent<SpriteRenderer>(); }
			Material m = new Material(s.material) { mainTexture = texture };
			CreatePoints();
			texture = GenerateTexture((int)textureSize, filterMode, colorGradient, curveIsAlpha?alphaCurve:null, type, edgeOrigins, edgeDirections);
		}

		private void DrawConvexEdges() {
			float w = texture.width / 2f, h = texture.height / 2f, cx, cy, x, y;
			for (int i = 0; i < edgeDirections.Length; ++i) {
				cx = edgeOrigins[i].x * texture.width;
				cy = edgeOrigins[i].y * texture.height;
				x = w * edgeDirections[i].x + cx;
				y = h * edgeDirections[i].y + cy;
				NS.Lines.DrawLine(texture, (int)cx, (int)cy, (int)x, (int)y, Color.red);
			}
			texture.Apply();
		}

		private void OnValidate() {
			if(realtimeEdit || doRealtimeEditOneMoreTime) {
				Create();
				if (realtimeEdit) { DrawConvexEdges(); }
				doRealtimeEditOneMoreTime = realtimeEdit;
			}
		}

		public static void SaveTextureAsPNG(Texture2D _texture, string path) {
			path = Application.dataPath + "/" + path;
			Debug.Log("saving to " + path);
			byte[] _bytes = _texture.EncodeToPNG();
			System.IO.File.WriteAllBytes(path, _bytes);
			Debug.Log(_bytes.Length / 1024 + "Kb was saved as: " + path);
		}

		public static Texture2D GenerateTexture(int size, FilterMode filter, Gradient colorGradient, AnimationCurve alphaCurve, Type type, Vector2[] origin, Vector2[] dir) {
			Texture2D texture = new Texture2D(size, size);
			System.Func<int, int, float> f = (int x, int y) => { return 1; };
			float halfH = texture.height / 2f, halfW = texture.width / 2f;
			Vector2 center = new Vector2(size / 2f, size / 2f);
			Vector2 sizeDelta = new Vector2(size, size);
			float radius = size / 2f;
			switch (type) {
				case Type.Bottom: f = (x, y) =>    (float)y / texture.height;  break;
				case Type.Top:    f = (x, y) => 1-((float)y / texture.height); break;
				case Type.Left:   f = (x, y) =>    (float)x / texture.width;   break;
				case Type.Right:  f = (x, y) => 1-((float)x / texture.width);  break;
				case Type.Horizontal: f = (x, y) => Mathf.Abs((y-halfH) / halfH);   break;
				case Type.Vertical:   f = (x, y) => Mathf.Abs((x - halfW) / halfW); break;
				case Type.BottomAndLeft: f = (x, y) => Mathf.Min(  ((float)x / texture.width),  ((float)y / texture.height)); break;
				case Type.BottomLeft:    f = (x, y) => Mathf.Max(  ((float)x / texture.width),  ((float)y / texture.height)); break;
				case Type.TopAndLeft:    f = (x, y) => Mathf.Min(  ((float)x / texture.width),1-((float)y / texture.height)); break;
				case Type.TopLeft:       f = (x, y) => Mathf.Max(  ((float)x / texture.width),1-((float)y / texture.height)); break;
				case Type.BottomAndRight:f = (x, y) => Mathf.Min(1-((float)x / texture.width),  ((float)y / texture.height)); break;
				case Type.BottomRight:   f = (x, y) => Mathf.Max(1-((float)x / texture.width),  ((float)y / texture.height)); break;
				case Type.TopAndRight:   f = (x, y) => Mathf.Min(1-((float)x / texture.width),1-((float)y / texture.height)); break;
				case Type.TopRight:      f = (x, y) => Mathf.Max(1-((float)x / texture.width),1-((float)y / texture.height)); break;
				case Type.HoriztonalAndVertical: f = (x, y) => Mathf.Min(Mathf.Abs((y - halfH) / halfH), Mathf.Abs((x - halfW) / halfW)); break;
				case Type.Square:                f = (x, y) => Mathf.Max(Mathf.Abs((y - halfH) / halfH), Mathf.Abs((x - halfW) / halfW)); break;
				case Type.Radial:                f = (x, y) => Vector2.Distance(center, new Vector2(x, y)) / radius; break;
				case Type.Polygon: f = (x, y) => {
					float best = 0;
					for(int i = 0; i < dir.Length; ++i) {
						Vector2 c = origin[i] * sizeDelta;
						Vector2 pos = (new Vector2(x, y) - c) / radius;
						best = Mathf.Max(best, Vector2.Dot(dir[i], pos));
					}
					return best;
				}; break;
			}
			SetPixels(f, texture, colorGradient, alphaCurve);
			texture.Apply();
			texture.filterMode = filter;
			texture.wrapMode = TextureWrapMode.Clamp;
			return texture;
		}

		public void Update()
		{
			if(s != null && s.sprite != null && s.sprite.texture != texture) {
				s.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(.5f, .5f), (int)textureSize);
				s.sprite.name = type.ToString() + textureSize;
			}
		}

		public static void SetPixels(System.Func<int,int,float> PixelFunction, Texture2D texture, Gradient colorGradient, AnimationCurve alphaCurve) {
			for (int y = 0; y < texture.height; y++) {
				for (int x = 0; x < texture.width; x++) {
					float t = PixelFunction(x, y);//(float)y / texture.height;
					Color color = colorGradient.Evaluate(t);//new Color(1, 1, 1, currentAlpha);
					if (alphaCurve != null) {
						color.a = alphaCurve.Evaluate(t);
					}
					texture.SetPixel(x, y, color);
				}
			}
		}
	}
}