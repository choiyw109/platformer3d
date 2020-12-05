using UnityEngine;
using UnityEngine.UI;

namespace NonStandard {
public class KeyCodeRouteToToggle : KeyCodeRoute {

	private Toggle toggle;
	public bool onToggleOnRelease = false;
	private bool on = false;

	void Start() {
		toggle = GetComponent<Toggle>();
	}

	public void Toggle() {
		toggle.isOn = !toggle.isOn;
	}

	void Update () {
		if(key != KeyCode.None) {
			if (UnityEngine.Input.GetKeyDown(key) && IsModifiersSatisfied()) {
				Toggle();
				on = true;
			}
			if (onToggleOnRelease && UnityEngine.Input.GetKeyUp(key) && on) {
				Toggle();
				on = false;
			}
		}
	}
}
}