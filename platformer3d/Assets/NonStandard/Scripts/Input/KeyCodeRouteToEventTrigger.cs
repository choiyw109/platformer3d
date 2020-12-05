using UnityEngine;
using UnityEngine.EventSystems;

namespace NonStandard {
[RequireComponent(typeof(EventTrigger))]
public class KeyCodeRouteToEventTrigger : KeyCodeRoute {

	private EventTrigger eventTrigger;
	[HideInInspector] public bool on = false;

	void Start() {
		eventTrigger = GetComponent<EventTrigger>();
	}

	void Update () {
		if(key != KeyCode.None) {
			if (UnityEngine.Input.GetKeyDown(key) && IsModifiersSatisfied()) {
				PointerEventData edata = new PointerEventData(EventSystem.current);
				eventTrigger.OnPointerDown(edata);
				on = true;
			}
			if (UnityEngine.Input.GetKeyUp(key) && on) {
				PointerEventData edata = new PointerEventData(EventSystem.current);
				eventTrigger.OnPointerUp(edata);
				on = false;
			}
		}
	}
}
}