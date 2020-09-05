using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportAfterFall : MonoBehaviour
{
	Vector3 startPosition;
	public float minimumY = -20;
    // Start is called before the first frame update
    void Start()
    {
		startPosition = transform.position;
	}

    // Update is called once per frame
    void Update()
    {
        if(transform.position.y < minimumY)
		{
			Restart();
		}
    }

	public void Restart()
	{
		transform.position = startPosition;
	}
}
