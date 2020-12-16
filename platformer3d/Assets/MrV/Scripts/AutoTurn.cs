using NonStandard;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NonStandard.Character;

public class AutoTurn : MonoBehaviour
{
	public bool isAggroed = false;
	public Transform whoToFollow;

	public float speed = 0.125f;
	Rigidbody rb;

	public Transform FindPlayer()
	{
		CharacterMove[] cm = FindObjectsOfType<CharacterMove>();
		for(int i = 0; i < cm.Length; ++i)
		{
			if(cm[i].tag == "Player") { return cm[i].transform; }
		}
		return null;
	}

	private void Start()
	{
		whoToFollow = FindPlayer();
		rb = GetComponent<Rigidbody>();
	}

	private void FixedUpdate()
	{
		if(isAggroed)
		{
			transform.LookAt(whoToFollow);
			rb.velocity = transform.forward * speed;
		}
	}
}
