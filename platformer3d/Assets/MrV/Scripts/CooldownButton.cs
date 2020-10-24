using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class CooldownButton : MonoBehaviour
{
	public float duration = 2;
	float timer;

	public Image visual;
	public float progress = 0;

	public UnityEvent thingToDo;
	public KeyCode key = KeyCode.None;

	void Update()
    {
		timer += Time.deltaTime;
		progress = timer / duration;
		if(progress > 1) { progress = 1; }
		visual.fillAmount = progress;

		if(progress >= 1) {
			if (key != KeyCode.None && Input.GetKeyDown(key)) {
				DoTheThing();
			}
		}
	}

	public void DoTheThing() {
		if (progress < 1) return; // fail if cooldown still active
		thingToDo.Invoke();
		timer = 0;
	}
}
