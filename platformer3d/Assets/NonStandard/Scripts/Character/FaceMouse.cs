using UnityEngine;

namespace NonStandard {
	public class FaceMouse : MonoBehaviour
	{
		public Camera _camera;
		public CharacterMove body;
		public float FaceX { get; set; }
		public float FaceY { get; set; }
		public float lookSpeed = 360;
	#if UNITY_EDITOR
		/// called when created by Unity Editor
		void Reset() {
			if (_camera == null) { _camera = GetComponent<Camera>(); }
			if (_camera == null) { _camera = Camera.main; }
			if (_camera == null) { _camera = FindObjectOfType<Camera>(); ; }
			if (body == null) { body = transform.GetComponentInParent<CharacterMove>(); }
			if (body == null) { body = FindObjectOfType<CharacterMove>(); }
			}
	#endif

		void AlignBodyWithFace()
		{
			Vector3 delta = lookingAt - body.transform.position;
			Vector3 right = Vector3.Cross(Vector3.up, delta);
			if(right == Vector3.zero) { right = Vector3.Cross(_camera.transform.up, delta); } // prevent bug when looking directly up or down
			Vector3 dirAlongHorizon = Vector3.Cross(right, Vector3.up).normalized;
			body.transform.rotation = Quaternion.LookRotation(dirAlongHorizon, Vector3.up);
		}

		[HideInInspector] public RaycastHit raycastHit;
		[HideInInspector] public Ray ray;
		[HideInInspector] public Vector3 lookingAt;
		[Tooltip("FaceMouse even while moving. Allows head to spin backwards around body in certain circumstances...")]
		public bool followMouseEvenWhileMoving;
		private Quaternion startingRotation;
		private Quaternion lookRotation;

		public void Start()
		{
			startingRotation = transform.rotation;// Quaternion.Inverse(transform.rotation);
		}

		public void LookAtMouse(Vector3 screenCoordinate)
		{
			if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject != null) return; // ignore user interface
			ray = _camera.ScreenPointToRay(screenCoordinate);
			if (Physics.Raycast(ray, out raycastHit)) {
				lookingAt = raycastHit.point;
				//NS.Lines.Make("ray "+name).Arrow(transform.position, raycastHit.point, Color.red);
			} else {
				lookingAt = ray.origin;// + ray.direction;
			}
			Look();
		}

		/// sets lookingAt and adjusts direction of transform, does not adjust ray or raycastHit
		public void ForceLookAt(Vector3 position)
		{
			lookingAt = position;
			Look();
		}

		void Look()
		{
			if (body != null && !body.move.lookForwardMoving) { AlignBodyWithFace(); }
			// if the position is behind the body
			Vector3 f = body.transform.forward;
			Vector3 dir = (lookingAt - transform.position);
			if (Vector3.Dot(f, dir) < 0) {
				// clamp the position to the body forward plane
				dir = Vector3.ProjectOnPlane(dir, f);
				// and push the look a little tiny bit forward, to prevent the character from spinning the head backwards
				dir += (f * (1f / 128));
			}
			dir.Normalize();
			if (dir != Vector3.zero) {
				Quaternion oldRotation = transform.rotation;
				Quaternion idealLook = Quaternion.LookRotation(dir);
				Vector3 euler = idealLook.eulerAngles;
				if (euler.x < -180) { euler.x += 360; }
				if (euler.x > 180) { euler.x -= 360; }
				euler.x = Mathf.Clamp(euler.x, -45, 45);
				idealLook = Quaternion.Euler(euler);
				idealLook *= startingRotation;
				lookRotation = Quaternion.RotateTowards(transform.rotation, idealLook, Time.deltaTime * lookSpeed);
				transform.rotation = lookRotation;
				euler = transform.localRotation.eulerAngles;
				if (euler.x < -180) { euler.x += 360; }
				if (euler.x > 180) { euler.x -= 360; }
				if (euler.y < -180 || euler.y > 180) {
					transform.rotation = oldRotation;
				}
			}
		}

		void TurnAccordingToJoystick(Vector3 joystickFace)
		{
			Vector3 f = (_camera != null) ? _camera.transform.forward : transform.forward;
			Vector3 up = Vector3.up;
			Vector3 right = Vector3.Cross(up, f);
			if (right == Vector3.zero) { right = Vector3.Cross(_camera.transform.up, f); } // prevent bug when looking directly up or down
			right.Normalize();
			if (_camera != null && Vector3.Dot(_camera.transform.up, Vector3.up) < 0) { right *= -1; } // handle upside-down turning
			Vector3 dirAlongHorizon = Vector3.Cross(right, up).normalized;
			Vector3 dir = joystickFace.x * right + joystickFace.y * dirAlongHorizon;
			lookingAt = transform.position + dir;
			transform.LookAt(lookingAt, up);
		}

		void LateUpdate()
		{
			Vector3 joystickFace = new Vector3(FaceX, FaceY);
			if (joystickFace != Vector3.zero)
			{
				TurnAccordingToJoystick(joystickFace);
			} else
			{
				if(body == null || !body.move.lookForwardMoving || body.move.moveDirection == Vector3.zero || followMouseEvenWhileMoving)
				{
					LookAtMouse(Input.mousePosition);
				} else {
					ForceLookAt(transform.position + body.transform.forward);
				}
			}
		}
	}
}