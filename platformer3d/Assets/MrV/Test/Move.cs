using UnityEngine;
using Cursor = UnityEngine.Cursor;

public class Move : MonoBehaviour {
	private Rigidbody rb;
	public Transform _camera;
	public float mouseSensitivity = 4;
	public Transform playerBody;
	public float speed = 6;
	public bool jump = true;

	private float pitch;

	void Start() {
		rb = GetComponent<Rigidbody>();
		Cursor.lockState = CursorLockMode.Locked;
		rb.freezeRotation = true;
	}

	void Update() {
		if (jump) {
			if (Input.GetKeyDown(KeyCode.Space)) {
				rb.velocity += transform.up * speed;
				jump = false;
			}

			float fallingVelocity = rb.velocity.y;
			Vector3 newVelocity = Vector3.zero;
			if (Input.GetKey(KeyCode.W)) {
				newVelocity += transform.forward * speed;
			}
			if (Input.GetKey(KeyCode.A)) {
				newVelocity += transform.right * -speed;
			}
			if (Input.GetKey(KeyCode.S)) {
				newVelocity += transform.forward * -speed;
			}
			if (Input.GetKey(KeyCode.D)) {
				newVelocity += transform.right * speed;
			}
			newVelocity += Vector3.up * fallingVelocity;
			rb.velocity = newVelocity;
		}
		_camera.position = transform.position;
		_camera.rotation = transform.rotation;

		float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
		float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
		pitch -= mouseY;
		playerBody.Rotate(Vector3.up* mouseX);
		_camera.Rotate(Vector3.right, pitch);
	}

	void OnTriggerEnter(Collider other) {
		jump = true;
	}
	private void OnCollisionEnter(Collision collision) {
		jump = true; // whenever the player hits the ground (or a wall!), they are allowed to jump or move
	}
}