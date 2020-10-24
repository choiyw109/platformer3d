using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawn : MonoBehaviour
{
	public GameObject thingToSpawn;

	public KeyCode key = KeyCode.Space;

    void Update()
    {
		if (key != KeyCode.None && Input.GetKeyDown(key))
		{
			SpawnThing();
		}
    }

	public void SpawnThing()
	{
		Instantiate(thingToSpawn, transform.position, transform.rotation);
	}
}
