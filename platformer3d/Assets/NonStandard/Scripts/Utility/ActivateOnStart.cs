using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateOnStart : MonoBehaviour
{
	public GameObject toActivateOnStart;
	public bool setActive = true;

	void Start()
    {
		toActivateOnStart.SetActive(setActive);
	}
}
