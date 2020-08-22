﻿using UnityEngine;

namespace NonStandard {
	public class CharacterCamera : MonoBehaviour
	{
		[Tooltip("which transform to follow with the camera")]
		public Transform target;
		[Tooltip("if false, camera can pass through walls")]
		public bool clipAgainstWalls = true;

		/// <summary>how far the camera wants to be from the target</summary>
		private float targetDistance = 10;
		/// <summary>calculate how far to clip the camera in the Update, to keep LateUpdate as light as possible
		private float distanceBecauseOfObstacle;
		/// <summary>how the camera should be rotated, calculated in Update, to keep LateUpdate as light as possible</summary>
		private Quaternion targetRotation;
		/// <summary>for fast access to transform</summary>
		private Transform t;

		/// <summary>keep track of rotation, so it can be un-rotated and cleanly re-rotated</summary>
		private float pitch, yaw;

		public float maxVerticalAngle = 100, minVerticalAngle = -100;

		/// publicly accessible variables that can be modified by external scripts or UI
		[HideInInspector] public float horizontalRotateInput, verticalRotateInput, zoomInput;
		public float HorizontalRotateInput { get { return horizontalRotateInput; } set { horizontalRotateInput = value; } }
		public float VerticalRotateInput { get { return verticalRotateInput; } set { verticalRotateInput = value; } }
		public float ZoomInput { get { return zoomInput; } set { zoomInput = value; } }
		public void AddToTargetDistance(float value) { targetDistance += value; }

	#if UNITY_EDITOR
		/// called when created by Unity Editor
		void Reset() {
			if (target == null) {
				CharacterMove body = null;
				if (body == null) { body = t.GetComponentInParent<CharacterMove>(); }
				if (body == null) { body = FindObjectOfType<CharacterMove>(); }
				if (body != null) { target = body.head; }
			}
		}
	#endif
		private void Awake() { t = transform; }

		private void Start() {
			if(target != null) {
				Vector3 delta = t.position - target.position;
				targetDistance = delta.magnitude;
			}
			Vector3 right = Vector3.Cross(t.forward, Vector3.up);
			Vector3 straightForward = Vector3.Cross(Vector3.up, right).normalized;
			pitch = Vector3.Angle(straightForward, t.forward);
			yaw = Vector3.Angle(Vector3.forward, straightForward);
			if (Vector3.Dot(straightForward, Vector3.right) < 0) { yaw *= -1; }
			if (Vector3.Dot(Vector3.up, t.forward) > 0) { pitch *= -1; }
		}

		void Update() {
			float rotH = horizontalRotateInput * Time.deltaTime, rotV = verticalRotateInput * Time.deltaTime, zoom = zoomInput * Time.deltaTime;
			targetDistance += zoom;
			if (rotH != 0 || rotV != 0)
			{
				targetRotation = Quaternion.identity;
				yaw += rotH;
				pitch -= rotV;
				if (yaw < -180) { yaw += 360; }
				if (yaw >= 180) { yaw -= 360; }
				if (pitch < -180) { pitch += 360; }
				if (pitch >= 180) { pitch -= 360; }
				pitch = Mathf.Clamp(pitch, minVerticalAngle, maxVerticalAngle);
				targetRotation *= Quaternion.Euler(pitch, yaw, 0);
			}
			if (targetDistance < 0) { targetDistance = 0; }
			if (target != null) {
				RaycastHit hitInfo;
				bool usuallyHitsTriggers = Physics.queriesHitTriggers;
				Physics.queriesHitTriggers = false;
				if (clipAgainstWalls && Physics.Raycast(target.position, -t.forward, out hitInfo, targetDistance))
				{
					distanceBecauseOfObstacle = hitInfo.distance;
				} else {
					distanceBecauseOfObstacle = targetDistance;
				}
				Physics.queriesHitTriggers = usuallyHitsTriggers;
			}
		}

		private void LateUpdate() {
			t.rotation = targetRotation;
			if(target != null) {
				t.position = target.position - (targetRotation * Vector3.forward) * distanceBecauseOfObstacle;
			}
		}
	}
}