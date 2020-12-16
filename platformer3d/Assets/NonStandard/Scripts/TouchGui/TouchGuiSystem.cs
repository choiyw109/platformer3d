using System.Collections.Generic;
using UnityEngine;

namespace NonStandard.TouchGui
{
	public class TouchGuiSystem : MonoBehaviour
	{
		public Touch[] _currentTouches = null;
		private Vector3 mousePosition;
		public TouchCollider prefab_touchCollider;
		public Transform touchCanvas;

		/// <summary>
		/// all of the colliders for current touches
		/// </summary>
		private List<TouchCollider> touchColliders = new List<TouchCollider>();

		private static TouchGuiSystem s_instance;
		public static TouchGuiSystem Instance() {
			if (s_instance != null) return s_instance;
			s_instance = NonStandard.Inputs.AppInput.GetEventSystem().gameObject.AddComponent<TouchGuiSystem>();
			return s_instance;
		}
		public int countTouches;
		private Touch[] GetCurrentMouseTouches() {
			Touch[] mouseTouches = null;
			bool isDown = Input.GetMouseButtonDown(0);
			bool isButton = Input.GetMouseButton(0);
			if (isButton && (_currentTouches == null || _currentTouches.Length == 0)) {
				isDown = true;
			}
			if (isButton || isDown) {
				if (isDown) { mousePosition = Input.mousePosition; }
				bool isStationary = mousePosition == Input.mousePosition;
				mouseTouches = new Touch[] { new Touch { fingerId = 0, rawPosition = Input.mousePosition, position = Input.mousePosition,
				deltaPosition = Input.mousePosition - mousePosition,
				phase = (isDown) ? TouchPhase.Began : ((isStationary) ? TouchPhase.Stationary : TouchPhase.Moved),
			}};
				mousePosition = Input.mousePosition;
			}
			return mouseTouches;
		}

		public TouchCollider GetTouch(int fingerId) {
			for (int i = 0; i < touchColliders.Count; ++i) {
				TouchCollider tc = touchColliders[i];
				if (tc.touch.phase < TouchPhase.Ended && tc.touch.fingerId == fingerId) {
					return tc;
				}
			}
			return null;
		}

		public Touch[] GetCurrentTouches() {
			Touch[] touches;
			// prioritize actual touch events over mouse events
			if (Input.touchCount != 0) {
				touches = Input.touches;
			} else {
				// don't generate mouse touch events more than once per update
				touches = GetCurrentMouseTouches();
			}
			return touches;
		}

		public void UpdateTouchCollisionModels(Touch[] touches) {
			// if a is no longer valid, we mark it that way manually
			if (touches == null || touches.Length <= touchColliders.Count) {
				int index = touches != null ? touches.Length : 0;
				for (int i = index; i < touchColliders.Count; ++i) {
					touchColliders[i].MarkValid(false);
				}
			}
			// move the touch colliders to match given locations
			if (touches != null) {
				for (int i = 0; i < touches.Length; ++i) {
					UpdateTouch(i, touches[i]);
				}
			}
		}

		public TouchCollider CreateTouchCollider() {
			if (prefab_touchCollider == null) {
				GameObject touchObj = new GameObject("touch");
				touchObj.layer = LayerMask.NameToLayer("UI");
				touchObj.AddComponent<CircleCollider2D>();
				Rigidbody2D r2d = touchObj.AddComponent<Rigidbody2D>();
				r2d.bodyType = RigidbodyType2D.Kinematic;
				prefab_touchCollider = touchObj.AddComponent<TouchCollider>();
				prefab_touchCollider.MarkValid(false);
			}
			GameObject go = Instantiate(prefab_touchCollider.gameObject);
			return go.GetComponent<TouchCollider>();
		}
		public void UpdateTouch(int index, Touch t) {
			if (touchColliders.Count <= index) {
				TouchCollider tc = CreateTouchCollider();
				tc.name = "touch " + index;
				tc.transform.SetParent(touchCanvas);
				touchColliders.Add(tc);
			}
			TouchCollider touch = touchColliders[index];
			touch.touch = t;
			touch.MarkValid(true);
			touch.transform.position = t.position;
		}

		void Start() {
			if (touchCanvas == null) { touchCanvas = transform; }
		}

		void FixedUpdate() {
			_currentTouches = GetCurrentTouches();
			countTouches = _currentTouches == null ? 0 : _currentTouches.Length;
			UpdateTouchCollisionModels(_currentTouches);
		}
	}
}