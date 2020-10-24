using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class Aggro : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		if(other.tag == "Player")
		{
			Collider[] allTheColliders = Physics.OverlapSphere(transform.parent.position, 2);
			for(int i = 0; i < allTheColliders.Length; ++i)
			{
				AutoTurn at = allTheColliders[i].GetComponent<AutoTurn>();
				if (at != null) {
					at.isAggroed = true;
					at.whoToFollow = other.transform;
				}
			}
		}
	}
}
