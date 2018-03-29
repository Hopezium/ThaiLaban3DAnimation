using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class JointController : MonoBehaviour {
	[HideInInspector]
	public Transform JointToControllTrans;

	private Vector3 oldPos;
	private Quaternion oldRot , originalRot;
    float numOfFrame = 100;

	private void Start () {
		oldPos = transform.position;
		oldRot = transform.rotation;
        originalRot = transform.rotation;
	}

	private void Update () {
		if (oldPos != transform.position) {
			Vector3 PosToUpdate = transform.position;
			PosToUpdate.z -= 1;
			JointToControllTrans.position = PosToUpdate;
			oldPos = transform.position;
		}

		if (oldRot != transform.rotation) {
			Quaternion RotToUpdate = transform.rotation;
			JointToControllTrans.rotation = RotToUpdate;
			oldRot = transform.rotation;
		}

	}

	public void Task (float ST,float ED,Vector3 MaxRotate) {
		
			
		Action fToRun = () => Rotate (MaxRotate, (ED-ST));            // Rotate parse in MaxRotate value, the duration of the rotation
		StartCoroutine(RunFunctionAfterDelay(fToRun,ST));             // Parse in the action, which is the rotate and the delay before running which is the start time, ST
        //Debug.Log("Reached JointController" + this.gameObject.name);

		
	}

    public void Task2(float ST, float ED, Vector3 movePos)
    {
        Action fToRun = () => Translate(movePos, (ED - ST));
        StartCoroutine(RunFunctionAfterDelay(fToRun, ST));
    }

    public void Task3(float ST, float ED, Vector3 rotateTo)
    {
        Action fToRun = () => SupportRotate(rotateTo, (ED - ST));
        StartCoroutine(RunFunctionAfterDelay(fToRun, ST));
    }

    public void SupportRotate(Vector3 dir, float seconds)
    {
        Coroutine t = StartCoroutine(RotateSupport(dir, seconds));
        StartCoroutine(StopCorotineAfterTime(t, seconds));
    }

    public void Translate (Vector3 dir, float seconds)
    {
        Coroutine t = StartCoroutine(MovePosition(dir, seconds));
        StartCoroutine(StopCorotineAfterTime(t, seconds));
    }

    public void Rotate (Vector3 dir, float seconds) {
		Coroutine t = StartCoroutine (RotateJoint (dir, seconds));    // t  stores  RotateJoint Coroutine, seconds is the duration it run
		StartCoroutine (StopCorotineAfterTime (t, seconds));          // Stop Coroutine t, seconds is after the duration it run
	}

	IEnumerator RunFunctionAfterDelay(Action f,float delay )
	{
		yield return new WaitForSeconds(delay);                       //delay = ST
		f();                                                          //f() = fToRun = Rotate
	}

    IEnumerator RotateSupport (Vector3 point, float seconds)
    {
        Quaternion SR = transform.rotation;
        float t = 0.0f;
        float t2 = 0.0f;
        float increment = 1.0f / numOfFrame;
        while (true)
        {
            if (t < 1)
            {
                yield return new WaitForSeconds((seconds / numOfFrame) / 2);
                transform.rotation = Quaternion.Slerp(SR, Quaternion.Euler(point), t);
                t += increment;
            }
            if(t>= 1)
            {
                yield return new WaitForSeconds((seconds / numOfFrame) / 2);
                transform.rotation = Quaternion.Slerp(Quaternion.Euler(point), originalRot , t2);
                t2 += increment;
            }
        }
    }

	IEnumerator RotateJoint (Vector3 point,float seconds) {
		Quaternion SR = transform.rotation;
        //Debug.Log(SR + " to " + point);
        float t = 0.0f;
        float increment = 1.0f / numOfFrame;
		while (true) { 
			yield return new WaitForSeconds (seconds/numOfFrame);
			transform.rotation = Quaternion.Slerp(SR,Quaternion.Euler(point), t);
            t += increment;
		}
	}

    IEnumerator MovePosition (Vector3 point, float seconds)
    {
        Vector3 SR = transform.position;
        float t = 0.0f;
        float increment = 1.0f / numOfFrame;
        while (true)
        {
            yield return new WaitForSeconds(seconds / numOfFrame);
            transform.position = Vector3.Lerp(SR, point, t);
            t += increment;
        }
    }

	IEnumerator StopCorotineAfterTime (Coroutine c, float time) {               //Coroutine c = Coroutine t = Coroutine RotateJoint
		yield return new WaitForSeconds (time);
		StopCoroutine (c);
	}
}