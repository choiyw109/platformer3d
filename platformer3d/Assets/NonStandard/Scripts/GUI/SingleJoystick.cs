using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using NonStandard.Inputs;

namespace NonStandard {
	public class SingleJoystick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
	{
		[Tooltip("When checked, this joystick will stay in a fixed position.")]
		public bool joystickStaysInFixedPosition = false;
		[Tooltip("If true, make sure horizontal and vertical inputs never leave the boundary of a unit circle")]
		public bool normalizeAxisInput = false;
		private bool inputNeedsNormalization = true;

		[Tooltip("Distance handle (knob) can be dragged from the center of the joystick. 2 is the most accurate value, but 4 'feels' better.")]
		public int handleDistance = 4;
		/// just a number that looks good
		private static float stickUISpeed = 8;
		/// background of the joystick, this is the part of the joystick that recieves input
		private Image bgImage;
		/// the "knob" part of the joystick, moves to provide feedback, does not receive input from the touch
		private Image joystickKnobImage;
		/// normalized direction vector ouput from joystick. can be accessed using GetInputDirection()
		private Vector3 inputVector;
		/// unormalized direction vector (has a magnitude)
		[HideInInspector] public Vector3 unNormalizedInput;
		private Vector3 lastInputValues;

		public float Horizontal {
			get { return unNormalizedInput.x; }
			set {
				inputNeedsNormalization = normalizeAxisInput;
				unNormalizedInput.x = value;
			}
		}
		public float Vertical {
			get { return unNormalizedInput.y; }
			set {
				inputNeedsNormalization = normalizeAxisInput;
				unNormalizedInput.y = value;
			}
		}
	
		[System.Serializable]
		public struct JoystickOutput {
			[System.Serializable]
			public class UnityEventFloat : UnityEngine.Events.UnityEvent<float> { }
			public UnityEventFloat onHorizontalChange, onVerticalChange;
			[Tooltip("Output of horizontal and vertical values will be in a range -1 to +1 multiplied by this value")]
			public float outputMultiplier;

			private float _lastH, _lastV;
			public void NotifyValue(float horizontal, float vertical) {
				onHorizontalChange.Invoke(horizontal * outputMultiplier);
				onVerticalChange.Invoke(vertical * outputMultiplier);
			}
		}
		public JoystickOutput joystickOutput = new JoystickOutput { outputMultiplier = 1 };

		private void Start()
		{
			if(EventSystem.current == null)
			{
				AppInput.GetEventSystem();
			}
			if (GetComponent<Image>() == null)
			{
				Debug.LogError("There is no joystick image attached to this script.");
			}
			if (transform.GetChild(0).GetComponent<Image>() == null)
			{
				Debug.LogError("There is no joystick handle image attached to this script.");
			}

			if (GetComponent<Image>() != null && transform.GetChild(0).GetComponent<Image>() !=null)
			{
				bgImage = GetComponent<Image>();
				joystickKnobImage = transform.GetChild(0).GetComponent<Image>();
				bgImage.rectTransform.SetAsLastSibling(); // ensures that this joystick will always render on top of other UI elements
				Vector2 idealPivot = new Vector2(0.5f, 0.5f);
				Vector2 offsetFromIdeal = (idealPivot - bgImage.rectTransform.pivot);
				offsetFromIdeal.Scale(bgImage.rectTransform.sizeDelta);
				bgImage.rectTransform.pivot = idealPivot;
				bgImage.rectTransform.anchoredPosition += offsetFromIdeal;
			}
		}

		public void Update()
		{
			//float h = 0, v = 0;
			if ((EventSystem.current != null && EventSystem.current.currentSelectedGameObject != gameObject) ){//&& ReadAxis(ref h, ref v)) {
				//inputVector = new Vector3(h, v, 0);
				//unNormalizedInput = inputVector;
				if(lastInputValues != unNormalizedInput) {
					inputVector = unNormalizedInput;
					if (inputNeedsNormalization) {
						inputVector.Normalize();
						inputNeedsNormalization = false;
					}
					joystickOutput.NotifyValue(inputVector.x, inputVector.y);
					lastInputValues = unNormalizedInput;
				}
				Vector3 oldPosition = joystickKnobImage.rectTransform.anchoredPosition;
				Vector3 newPosition =
					new Vector3(inputVector.x * (bgImage.rectTransform.sizeDelta.x / handleDistance),
								inputVector.y * (bgImage.rectTransform.sizeDelta.y / handleDistance));
				Vector3 delta = newPosition - oldPosition;
				joystickKnobImage.rectTransform.anchoredPosition = oldPosition + delta * (Time.deltaTime * stickUISpeed);
			}
		}

		// this event happens when there is a drag on screen
		public virtual void OnDrag(PointerEventData ped)
		{
			Vector2 localPoint = Vector2.zero; // resets the localPoint out parameter of the RectTransformUtility.ScreenPointToLocalPointInRectangle function on each drag event

			// if the point touched on the screen is within the background image of this joystick
			if (RectTransformUtility.ScreenPointToLocalPointInRectangle(bgImage.rectTransform, ped.position, ped.pressEventCamera, out localPoint))
			{
				localPoint.x = (localPoint.x / bgImage.rectTransform.sizeDelta.x);
				localPoint.y = (localPoint.y / bgImage.rectTransform.sizeDelta.y);

				inputVector = localPoint * 2;

				unNormalizedInput = inputVector;
				inputVector = (inputVector.magnitude > 1.0f) ? inputVector.normalized : inputVector;
				joystickOutput.NotifyValue(inputVector.x, inputVector.y);

				// moves the joystick handle "knob" image
				joystickKnobImage.rectTransform.anchoredPosition =
				 new Vector3(inputVector.x * (bgImage.rectTransform.sizeDelta.x / handleDistance),
							 inputVector.y * (bgImage.rectTransform.sizeDelta.y / handleDistance));
			}
		}

		// this event happens when there is a touch down (or mouse pointer down) on the screen
		public virtual void OnPointerDown(PointerEventData ped)
		{
			OnDrag(ped); // sent the event data to the OnDrag event
		}

		// this event happens when the touch (or mouse pointer) comes up and off the screen
		public virtual void OnPointerUp(PointerEventData ped)
		{
			Recenter();
		}

		public void Recenter()
		{
			inputVector = Vector3.zero;
			joystickKnobImage.rectTransform.anchoredPosition = Vector3.zero;
			joystickOutput.NotifyValue(inputVector.x, inputVector.y);
		}

		public Vector3 GetInputDirection()
		{
			return new Vector3(inputVector.x, inputVector.y, 0);
		}
	}
}