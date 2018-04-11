using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class JointController : MonoBehaviour {
	[HideInInspector]
    //public Transform JointToControllTrans;

	//private Vector3 oldPos;
	private Quaternion originalRot, currentRot;  //oldRot, originalLocalRot
    float numOfFrame = 100, returnT = 0.0f;
    bool _hold = true;

	private void Start () {
        ////oldPos = transform.position;
        ////oldRot = transform.rotation;
        originalRot = transform.localRotation;
        //originalLocalRot = transform.localRotation;
	}

	private void Update () {

        ////if (_hold == false)
        ////{
        ////    UpdateDelay();
        ////    transform.localRotation = Quaternion.Lerp(currentRot, originalRot, returnT);
        ////    if (returnT < 1.0f)
        ////    {
        ////        returnT += (1.0f / numOfFrame);
        ////    }
        ////}

    }

	public void Task (float ST,float ED,Vector3 MaxRotate, bool Hold) {
		
			
		Action fToRun = () => Rotate (MaxRotate, (ED-ST), Hold);            // Rotate parse in MaxRotate value, the duration of the rotation
		StartCoroutine(RunFunctionAfterDelay(fToRun,ST));             // Parse in the action, which is the rotate and the delay before running which is the start time, ST
        //Debug.Log("Reached JointController" + this.gameObject.name);

		
	}

    public void Task2(float ST, float ED, Vector3 movePos)
    {
        Action fToRun = () => Translate(movePos, (ED - ST));
        StartCoroutine(RunFunctionAfterDelay(fToRun, ST));
    }

    public void Task3(float ST, float ED, Vector3 rotateTo, bool Hold)
    {
        Action fToRun = () => SupportRotate(rotateTo, (ED - ST), Hold);
        StartCoroutine(RunFunctionAfterDelay(fToRun, ST));
    }

    public void Task4(float ST, float ED, Vector3 fingerRotateTo, bool Hold)
    {
        Action fToRun = () => GestureRotate(fingerRotateTo, (ED - ST), Hold);
        StartCoroutine(RunFunctionAfterDelay(fToRun, ST));
    }

    public void GestureRotate(Vector3 dir, float seconds, bool hold)
    {
        Coroutine t = StartCoroutine(gestureRotate(dir));
        StartCoroutine(StopCorotineAfterTime(t, seconds, hold));
    }

    public void SupportRotate(Vector3 dir, float seconds, bool hold)
    {
        Coroutine t = StartCoroutine(RotateSupport(dir, seconds));
        StartCoroutine(StopCorotineAfterTime(t, seconds, true));
    }

    public void Translate (Vector3 dir, float seconds)
    {
        Coroutine t = StartCoroutine(MovePosition(dir, seconds));
        StartCoroutine(StopCorotineAfterTimeHip(t, seconds));
    }

    public void Rotate (Vector3 dir, float seconds, bool hold) {
		Coroutine t = StartCoroutine (RotateJoint (dir, seconds));    // t  stores  RotateJoint Coroutine, seconds is the duration it run
		StartCoroutine (StopCorotineAfterTime (t, seconds, hold));          // Stop Coroutine t, seconds is after the duration it run
	}

	IEnumerator RunFunctionAfterDelay(Action f,float delay )
	{
		yield return new WaitForSeconds(delay);                       //delay = ST
		f();                                                          //f() = fToRun = Rotate
	}

    IEnumerator RotateSupport (Vector3 point, float seconds)
    {
        Quaternion SR = transform.localRotation;
        float t = 0.0f;
        float t2 = 0.0f;
        float increment = 1.0f / numOfFrame;
        _hold = true;
        while (_hold)
        {
            if (t < 1)
            {
                yield return new WaitForSeconds((seconds / numOfFrame) / 2);
                transform.localRotation = Quaternion.Lerp(SR, Quaternion.Euler(point), t);
                t += increment;
            }
            if(t>= 1)
            {
                yield return new WaitForSeconds((seconds / numOfFrame) / 2);
                transform.localRotation = Quaternion.Lerp(Quaternion.Euler(point), originalRot , t2);
                t2 += increment;
            }
        }
    }

	IEnumerator RotateJoint (Vector3 point,float seconds) {
		Quaternion SR = transform.localRotation;
        //Debug.Log(gameObject.name + " " + point);
        //Debug.Log(SR + " to " + point);
        float t = 0.0f;
        float increment = 1.0f / numOfFrame;
        _hold = true;
		while (_hold) { 
			yield return new WaitForSeconds (seconds/numOfFrame);
			transform.localRotation = Quaternion.Lerp(SR,Quaternion.Euler(point), t);
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

    IEnumerator gestureRotate (Vector3 point)
    {
        Quaternion SR = transform.localRotation;
        float t = 0.0f;
        float increment = 1.0f / numOfFrame;
        _hold = true;
        while (true)
        {
            yield return new WaitForEndOfFrame();
            transform.rotation = Quaternion.Lerp(SR, Quaternion.Euler(point), t);
            t += increment;
        }
    }

	IEnumerator StopCorotineAfterTime (Coroutine c, float time, bool hold) {               //Coroutine c = Coroutine t = Coroutine RotateJoint
		yield return new WaitForSeconds (time);
		StopCoroutine (c);
        currentRot = transform.localRotation;
        returnT = 0.0f;
        _hold = hold;
    }

    IEnumerator StopCorotineAfterTimeHip(Coroutine c, float time)
    {               
        //Coroutine c = Coroutine t = Coroutine RotateJoint
        yield return new WaitForSeconds(time);
        StopCoroutine(c);
    }

    //IEnumerator UpdateDelay()
    //{
    //    yield return new WaitForSeconds(1.0f / numOfFrame);
    //}
}