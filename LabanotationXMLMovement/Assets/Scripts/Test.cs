using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour 
{
	public bool tick = true;
	void Update () 
	{
		if(tick)
		{
          	transform.Translate(Vector3.forward*Time.deltaTime*0.2f);
		}
		else
		{
			transform.Translate(Vector3.back*Time.deltaTime*0.1f);
		}
			
	}
}
