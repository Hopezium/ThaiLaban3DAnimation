using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BodyController : MonoBehaviour
{
    //public GameObject JointControllerPrefab;
    private Animator Anim;
    private Dictionary<HumanBodyBones, JointController> AllJoints;
    float hipXValue, movePos = 1;

    //Level variable
    float high = 70.0f;
    float middle = 0.0f;
    float low = -70.0f;

    //Fold variables
    float kneeFold = -190.0f;
    float ankleFold = -40.0f;
    float elbowFold = 190.0f;
    float wristFold = 75;
    float torsoFold = 100.0f;
    float waistFold = 100.0f;
    float chestFold = 30.0f;
    float maxDegree = 6.0f;


    Vector3 hipPos = Vector3.zero;
    List<Joint> timeSortedJoints = new List<Joint>();
    List<HipJoint> timeSortedHipPos = new List<HipJoint>();
    List<Support> timeSortedSupportJoint = new List<Support>();
    List<gestureJoint> timeSortedGesture = new List<gestureJoint>();

    void Start()
    {
        Anim = GetComponentInChildren<Animator>();
        AllJoints = new Dictionary<HumanBodyBones, JointController>();
        ProcessAndCreateJoints();
        GetandProcessXMLTask();
        AssignTaskToJoints();
        Transform _hipPos = Anim.GetBoneTransform(HumanBodyBones.Hips);                                 //Get Hip starting position
        hipPos = _hipPos.position;                                                                      //Store hip starting position
    }

    void ProcessAndCreateJoints()
    {
        for (int i = 0; i < 55; i++)
        {
            string check;
            Transform BoneTrans = Anim.GetBoneTransform((HumanBodyBones)i);
            if (BoneTrans != null)
            {
                //GameObject controller = Instantiate(JointControllerPrefab);              //Create Controller Prefab
                //Vector3 ConPos = BoneTrans.position;                                    //Get position of Specific HumanBody Bones
                //ConPos.z += 1;                                                             //1 Vector.z unit away from model
                //controller.transform.position = ConPos;                                  // Set Controller Prefab position 1 Vector.z unit away from specific HumanBody Bones
                //controller.transform.rotation = BoneTrans.rotation;
                //BoneTrans.transform.SetParent(controller.transform);
                BoneTrans.name = ((HumanBodyBones)i).ToString() + " JC";
                if(((HumanBodyBones)i).ToString() == "Hips")
                {
                    hipXValue =BoneTrans.transform.position.x;
                }
                JointController JC = BoneTrans.gameObject.AddComponent<JointController>();
                //JC.JointToControllTrans = BoneTrans;                                      //JC.JointToControllTrans = Anim.GetBoneTransform((HumanBodyBones)i)
                AllJoints.Add((HumanBodyBones)i, JC);
                check = ((HumanBodyBones)i).ToString();
                //if(check[0] == 'L')
                if (((HumanBodyBones)i).ToString().StartsWith("Left"))
                    BoneTrans.gameObject.tag = "Left";

            }
       }
    }

    void GetandProcessXMLTask()
    {
        //Loop XML Object
        //Convert XML movement to unity values
        //Tell Joint to perform task
        float timePassed = 0f;
        List<Joint> moves = new List<Joint>();
        //Debug.Log("Xml Container measure count: " + XmlContainer.instance.Full_Dance[1].body.Count);
        for (int i = 0; i < XmlContainer.instance.Full_Dance.Count; i++)              //For each measure
        {

            for (int j = 0; j < XmlContainer.instance.Full_Dance[i].body.Count; j++)  //For each Body part stored in Measure
            {
                //Debug.Log("j: " + j);
                Vector3 _rotateCap = Vector3.zero;
                float start = 0.0f, end = 0.0f;
                if (XmlContainer.instance.Full_Dance[i].body[j]._direction != null)      //
                {
                    if (XmlContainer.instance.Full_Dance[i].body[j]._body == "left_support" || XmlContainer.instance.Full_Dance[i].body[j]._body == "right_support")
                    {
                        hipPos += getHipPosition(XmlContainer.instance.Full_Dance[i].body[j]._direction);
                        float extent = getSpace(XmlContainer.instance.Full_Dance[i].body[j]._space, XmlContainer.instance.Full_Dance[i].body[j]._extent);           //Get extent value
                        hipPos.x = hipPos.x * extent;
                        hipPos.z = hipPos.z * extent;
                        start = XmlContainer.instance.Full_Dance[i].body[j]._start * (60.0f / XmlContainer.instance.Full_Dance[i].bpm) + timePassed;
                        end = XmlContainer.instance.Full_Dance[i].body[j]._end * (60.0f / XmlContainer.instance.Full_Dance[i].bpm) + timePassed;
                        timeSortedHipPos.Add(new HipJoint(("Hips"), hipPos, start, end));
                        _rotateCap += getSupportDirection(XmlContainer.instance.Full_Dance[i].body[j]._direction);
                        bool hold = XmlContainer.instance.Full_Dance[i].body[j]._hold;
                        string supportName = XmlContainer.instance.Full_Dance[i].body[j]._body;
                        if (supportName == "left_support")
                        {
                            timeSortedSupportJoint.Add(new Support(("LeftUpperLeg"), _rotateCap, start, end, hold));
                            float xValue = _rotateCap.x;
                            float zValue = _rotateCap.z;
                            _rotateCap.x = -xValue;
                            _rotateCap.z = -zValue;
                            timeSortedSupportJoint.Add(new Support(("RightUpperLeg"), _rotateCap, start, end, hold));
                        }
                        else
                        {
                            timeSortedSupportJoint.Add(new Support(("RightUpperLeg"), _rotateCap, start, end, hold));
                            float xValue = _rotateCap.x;
                            float zValue = _rotateCap.z;
                            _rotateCap.x = -xValue;
                            _rotateCap.z = -zValue;
                            timeSortedSupportJoint.Add(new Support(("LeftUpperLeg"), _rotateCap, start, end, hold));
                        }
                    }
                    else if (XmlContainer.instance.Full_Dance[i].body[j]._body != "left_support" && XmlContainer.instance.Full_Dance[i].body[j]._body != "right_support" && XmlContainer.instance.Full_Dance[i].body[j]._body != "left_")
                    {
                        bool hold = XmlContainer.instance.Full_Dance[i].body[j]._hold;
                        string[] _nameJC = getName(XmlContainer.instance.Full_Dance[i].body[j]._body);
                        _rotateCap += getDirection(XmlContainer.instance.Full_Dance[i].body[j]._direction, XmlContainer.instance.Full_Dance[i].body[j]._body);
                        _rotateCap += getLevel(XmlContainer.instance.Full_Dance[i].body[j]._level, XmlContainer.instance.Full_Dance[i].body[j]._body);
                        start = XmlContainer.instance.Full_Dance[i].body[j]._start * (60.0f / XmlContainer.instance.Full_Dance[i].bpm) + timePassed;
                        end = XmlContainer.instance.Full_Dance[i].body[j]._end * (60.0f / XmlContainer.instance.Full_Dance[i].bpm) + timePassed;
                        moves.Add(new Joint(_nameJC, _rotateCap, start, end, hold));
                    }
                }
                else if(XmlContainer.instance.Full_Dance[i].body[j]._fold != null)
                {
                    string[] _nameJC = getName(XmlContainer.instance.Full_Dance[i].body[j]._body);
                    start = XmlContainer.instance.Full_Dance[i].body[j]._start * (60.0f / XmlContainer.instance.Full_Dance[i].bpm) + timePassed;
                    end = XmlContainer.instance.Full_Dance[i].body[j]._end * (60.0f / XmlContainer.instance.Full_Dance[i].bpm) + timePassed;
                    _rotateCap += getFold(XmlContainer.instance.Full_Dance[i].body[j]._fold, XmlContainer.instance.Full_Dance[i].body[j]._degree, XmlContainer.instance.Full_Dance[i].body[j]._body);
                    bool hold = XmlContainer.instance.Full_Dance[i].body[j]._hold;
                    moves.Add(new Joint(_nameJC, _rotateCap, start, end, hold));
                    Debug.Log(XmlContainer.instance.Full_Dance[i].body[j]._body + " " + XmlContainer.instance.Full_Dance[i].body[j]._degree);
                }

                if(XmlContainer.instance.Full_Dance[i].body[j]._gesture != null)
                {
                    start = XmlContainer.instance.Full_Dance[i].body[j]._start * (60.0f / XmlContainer.instance.Full_Dance[i].bpm) + timePassed;
                    end = XmlContainer.instance.Full_Dance[i].body[j]._end * (60.0f / XmlContainer.instance.Full_Dance[i].bpm) + timePassed;
                    bool hold = XmlContainer.instance.Full_Dance[i].body[j]._hold;
                    List<gestureJoint> temp = new List<gestureJoint>();
                    temp = getGesture(XmlContainer.instance.Full_Dance[i].body[j]._body, XmlContainer.instance.Full_Dance[i].body[j]._gesture, start, end, hold);
                    for(int k = 0; k < temp.Count; k++)
                    {
                        timeSortedGesture.Add(new gestureJoint(temp[k].fingerName, temp[k].position, temp[k].start, temp[k].end, temp[k].hold));
                    }
                }
            }

        

            timePassed += XmlContainer.instance.Full_Dance[i].beat * (60.0f / XmlContainer.instance.Full_Dance[i].bpm);
        }
        timeSortedJoints = moves;
    }

    void AssignTaskToJoints()
    {

        for (int i = 0; i < timeSortedJoints.Count; i++)
        {
            Joint JD = timeSortedJoints[i];
            for (int j = 0; j < timeSortedJoints[i].bodyName.Length; j++)
            {
                JointController JC = AllJoints[(HumanBodyBones)Enum.Parse(typeof(HumanBodyBones), JD.bodyName[j], true)];
                JC.Task(JD.start, JD.end, JD.rotateCap, JD.hold);
            }
        }

        for (int i = 0; i < timeSortedHipPos.Count; i++)
        {
            HipJoint HJ = timeSortedHipPos[i];
            JointController JC = AllJoints[(HumanBodyBones)Enum.Parse(typeof(HumanBodyBones), HJ.bodyName, true)];
            JC.Task2(HJ.start, HJ.end, HJ.addPos);
        }

        for (int i = 0; i < timeSortedSupportJoint.Count; i ++)
        {
            Support SP = timeSortedSupportJoint[i];
            JointController JC = AllJoints[(HumanBodyBones)Enum.Parse(typeof(HumanBodyBones), SP.bodyName, true)];
            JC.Task3(SP.start, SP.end, SP.rotateTo, SP.hold);
        }

        for (int i = 0; i < timeSortedGesture.Count; i ++)
        {
            gestureJoint GJ = timeSortedGesture[i];
            JointController JC = AllJoints[(HumanBodyBones)Enum.Parse(typeof(HumanBodyBones), GJ.fingerName, true)];
            JC.Task4(GJ.start, GJ.end, GJ.position, GJ.hold);
        }
    }

    Vector3 getDirection(string direction, string name)
    {
        Vector3 dir = Vector3.zero;
        if (name.ToString().StartsWith("left"))
        {
            switch (direction)
            {
                case "forward":
                    dir.y = 90.0f;
                    break;

                case "backward":
                    dir.y = -60.0f;
                    break;

                case "left":
                    dir.y = 0.0f;
                    break;

                case "right":
                    dir.y = 150.0f;
                    break;

                case "forward_right":
                    dir.y = 105.0f;
                    break;

                case "forward_left":
                    dir.y = 50.0f;
                    break;

                case "backward_right":
                    dir.y = -105.0f;
                    break;

                case "backward_left":
                    dir.y = -50.0f;
                    break;

                case "place":
                    dir.y = 0.0f;
                    break;
            }
        }
        else
        {
            switch (direction)
            {
                case "forward":
                    dir.y = -90.0f;
                    break;

                case "backward":
                    dir.y = 60.0f;
                    break;

                case "left":
                    dir.y = -150.0f;
                    break;

                case "right":
                    dir.y = 0.0f;
                    break;

                case "forward_right":
                    dir.y = -50.0f;
                    break;

                case "forward_left":
                    dir.y = -105.0f;
                    break;

                case "backward_right":
                    dir.y = 50.0f;
                    break;

                case "backward_left":
                    dir.y = 105.0f;
                    break;

                case "place":
                    dir.y = 0.0f;
                    break;
            }
        }
        return dir;

    }

    Vector3 getLevel(string level, string name)
    {
        Vector3 lvl = Vector3.zero;
        switch (level)
        {
            case "high":
                lvl.z = high;
                break;

            case "middle":
                lvl.z = middle;
                break;

            case "low":
                lvl.z = low;
                break;
        }
        if(name.ToString().StartsWith("left"))
        {
            lvl.z = -lvl.z;
        }
        return lvl;
    }

    Vector3 getHipPosition(string direction)
    {
        Vector3 _hipPos = Vector3.zero;
        switch(direction)
        {
            case "forward":
                _hipPos.z += movePos;
                break;

            case "backward":
                _hipPos.z += -movePos;
                break;

            case "right":
                _hipPos.x += movePos;
                break;

            case "left":
                _hipPos.x += -movePos;
                break;

            case "forward_right":
                _hipPos.z += movePos;
                _hipPos.x += movePos;
                break;

            case "forward_left":
                _hipPos.z += movePos;
                _hipPos.x += -movePos;
                break;

            case "backward_right":
                _hipPos.z += -movePos;
                _hipPos.x += movePos;
                break;

            case "backward_left":
                _hipPos.z += -movePos;
                _hipPos.x += -movePos;
                break;
        }
        return _hipPos;
    }

    Vector3 getSupportDirection(string direction)
    {
        Vector3 dir = Vector3.zero;
        switch (direction)
        {
            case "forward":
                dir = new Vector3(-25.0f, 0.0f,0.0f);
                break;

            case "backward":
                dir = new Vector3(25.0f, 0.0f, 0.0f);
                break;

            case "left":
                dir = new Vector3(0.0f, 0.0f, -15.0f);
                break;

            case "right":
                dir = new Vector3(0.0f, 0.0f, 15.0f);
                break;

            case "forward_right":
                dir = new Vector3(-25.0f, 0.0f, 15.0f);
                break;

            case "forward_left":
                dir = new Vector3(-25.0f, 0.0f, -15.0f);
                break;

            case "backward_right":
                dir = new Vector3(25.0f, 0.0f, 15.0f);
                break;

            case "backward_left":
                dir = new Vector3(25.0f, -0.0f, -15.0f);
                break;

            case "place":
                dir = Vector3.zero;
                break;
        }
        return dir;
    }

    Vector3 getFold(string fold, float degree, string name)
    {
        Vector3 _fold = Vector3.zero;
        switch(name)
        {
            case "right_elbow":
                _fold.y = (elbowFold / maxDegree) * degree;
                _fold.x = 0.0f;
                _fold.z = 0.0f;
                break;

            case "left_elbow":
                _fold.y = (-elbowFold / maxDegree) * degree;
                _fold.x = 0.0f;
                _fold.z = 0.0f;
                break;

            case "right_wrist":
                _fold.z = (wristFold / maxDegree) * degree;
                _fold.x = 0.0f;
                break;

            case "left_wrist":
                _fold.z = (-wristFold / maxDegree) * degree;
                _fold.x = 0.0f;
                break;

            case "right_knee":
                _fold.x = (kneeFold / maxDegree) * degree;
                _fold.z = 0.0f;
                break;

            case "left_knee":
                _fold.x = (kneeFold / maxDegree) * degree;
                _fold.z = 0.0f;
                break;

            case "right_ankle":
                _fold.x = (ankleFold / maxDegree) * degree;
                _fold.z = 0.0f;
                break;

            case "left_ankle":
                _fold.x = (ankleFold / maxDegree) * degree;
                _fold.z = 0.0f;
                break;

            case "right_waist":
                _fold.x = (waistFold / maxDegree) * degree;
                _fold.z = 0.0f;
                break;

            case "left_waist":
                _fold.x = (waistFold / maxDegree) * degree;
                _fold.z = 0.0f;
                break;

            case "right_torso":
                _fold.x = (torsoFold / maxDegree) * degree;
                _fold.z = 0.0f;
                break;

            case "left_torso":
                _fold.x = (torsoFold / maxDegree) * degree;
                _fold.z = 0.0f;
                break;

            case "right_chest":
                _fold.x = (chestFold / maxDegree) * degree;
                _fold.z = 0.0f;
                break;

            case "left_chest":
                _fold.x = (chestFold / maxDegree) * degree;
                _fold.z = 0.0f;
                break;
        }
        return _fold;
    }

    float getSpace(string space, float extent)
    {
        float _ex = 1.0f;
        switch(space)
        {
            case "narrow":
                _ex -= extent;
                break;

            case "wide":
                _ex += extent;
                break;
        }

        return _ex;
    }

    string[] getName(string name)
    {
        string[] _name = null;
        switch (name)
        {
            case "left_pelvis":
                _name = new string[] { "LeftUpperLeg", "RightUpperLeg" };
                break;

            case "right_pelvis":
                _name = new string[] { "LeftUpperLeg", "RightUpperLeg" };
                break;

            case "left_leg":
                _name = new string[] { "LeftUpperLeg" };
                break;

            case "right_leg":
                _name = new string[] { "RightUpperLeg" };
                break;

            case "left_knee":
                _name = new string[] { "LeftLowerLeg" };
                break;

            case "right_knee":
                _name = new string[] { "RightLowerLeg" };
                break;

            case "left_ankle":
                _name = new string[] { "LeftFoot" };
                break;

            case "right_ankle":
                _name = new string[] { "RightFoot" };
                break;

            case "left_hip":
                _name = new string[] { "LeftUpperLeg" };
                break;

            case "right_hip":
                _name = new string[] { "RightUpperLeg" };
                break;

            case "left_waist":
                _name = new string[] { "Spine" };
                break;

            case "right_waist":
                _name = new string[] { "Spine" };
                break;

            case "left_chest":
                _name = new string[] { "Chest" };
                break;

            case "right_chest":
                _name = new string[] { "Chest" };
                break;

            case "left_torso":
                _name = new string[] { "Spine" };
                break;

            case "right_torso":
                _name = new string[] { "Spine" };
                break;

            case "left_arm":
                _name = new string[] { "LeftUpperArm" };
                break;

            case "right_arm":
                _name = new string[] { "RightUpperArm" };
                break;

            case "left_elbow":
                _name = new string[] { "LeftLowerArm" };
                break;

            case "right_elbow":
                _name = new string[] { "RightLowerArm" };
                break;

            case "left_wrist":
                _name = new string[] { "LeftHand" };
                break;

            case "right_wrist":
                _name = new string[] { "RightHand" };
                break;

            case "head":
                _name = new string[] { "Head" };
                break;

            case "left_support":
                _name = new string[] { "RightUpperLeg" };
                break;

            case "right_support":
                _name = new string[] { "LeftUpperLeg" };
                break;
              
        }
        return _name;
    }

    List<gestureJoint> getGesture(string bodyName, string gestureName, float start, float end, bool hold)
    {
        List<gestureJoint> gesture = new List<gestureJoint>();
        string finger = null;
        Vector3 pos = Vector3.zero;
        float _st = start;
        float _ed = end;
        bool _hd = hold;
        if (bodyName.ToString().StartsWith("left"))
        {
            switch (gestureName)
            {
                case "jeeb":
                    finger = "LeftThumbProximal";               //Thumb
                    pos = new Vector3(-4.0f, -10.0f, -30.0f);
                    gesture.Add(new gestureJoint(finger, pos, _st, _ed, _hd));

                    finger = "LeftIndexProximal";                //Index Finger
                    pos = new Vector3(0.0f, 0.0f, 10.0f);
                    gesture.Add(new gestureJoint(finger, pos, _st, _ed, _hd));

                    finger = "LeftMiddleProximal";               //Middle Finger
                    pos = new Vector3(0.0f, 0.0f, -15.0f);
                    gesture.Add(new gestureJoint(finger, pos, _st, _ed, _hd));

                    finger = "LeftRingProximal";                 //Ring Finger
                    pos = new Vector3(0.0f, 0.0f, -30.0f);
                    gesture.Add(new gestureJoint(finger, pos, _st, _ed, _hd));

                    finger = "LeftLittleProximal";               //Pinky Finger
                    pos = new Vector3(0.0f, 0.0f, -55.0f);
                    gesture.Add(new gestureJoint(finger, pos, _st, _ed, _hd));
                    break;

                case "wong":
                    finger = "LeftThumbProximal";                 //Thumb 1st Joint
                    pos = new Vector3(0.0f, 0.0f, -40.0f);
                    gesture.Add(new gestureJoint(finger, pos, _st, _ed, _hd));

                    finger = "LeftThumbIntermediate";             //Thumb 2nd Joint
                    pos = new Vector3(-30.0f, -30.0f, 50.0f);
                    gesture.Add(new gestureJoint(finger, pos, _st, _ed, _hd));

                    finger = "LeftThumbDistal";                   //Thumb 3rd Joint
                    pos = new Vector3(-5.0f, -100.0f, 90.0f);
                    gesture.Add(new gestureJoint(finger, pos, _st, _ed, _hd));

                    finger = "LeftIndexProximal";
                    pos = new Vector3(0.0f, 0.0f, -30.0f);
                    gesture.Add(new gestureJoint(finger, pos, _st, _ed, _hd));

                    finger = "LeftMiddleProximal";
                    pos = new Vector3(0.0f, 0.0f, -30.0f);
                    gesture.Add(new gestureJoint(finger, pos, _st, _ed, _hd));

                    finger = "LeftRingProximal";
                    pos = new Vector3(0.0f, 0.0f, -30.0f);
                    gesture.Add(new gestureJoint(finger, pos, _st, _ed, _hd));

                    finger = "LeftLittleProximal";
                    pos = new Vector3(0.0f, 0.0f, -30.0f);
                    gesture.Add(new gestureJoint(finger, pos, _st, _ed, _hd));
                    break;
            }
        }
        else
        {
            switch (gestureName)
            {
                case "jeeb":
                    finger = "RightThumbProximal";                //Thumb
                    pos = new Vector3(4.0f, 10.0f, 30.0f);
                    gesture.Add(new gestureJoint(finger, pos, _st, _ed, _hd));

                    finger = "RightIndexProximal";                //Index Finger
                    pos = new Vector3(0.0f, 0.0f, -10.0f);
                    gesture.Add(new gestureJoint(finger, pos, _st, _ed, _hd));

                    finger = "RightMiddleProximal";               //Middle Finger
                    pos = new Vector3(0.0f, 0.0f, 15.0f);
                    gesture.Add(new gestureJoint(finger, pos, _st, _ed, _hd));

                    finger = "RightRingProximal";                 //Ring Finger
                    pos = new Vector3(0.0f, 0.0f, 30.0f);
                    gesture.Add(new gestureJoint(finger, pos, _st, _ed, _hd));

                    finger = "RightLittleProximal";               //Pinky Finger
                    pos = new Vector3(0.0f, 0.0f, 55.0f);
                    gesture.Add(new gestureJoint(finger, pos, _st, _ed, _hd));
                    break;

                case "wong":
                    finger = "RightThumbProximal";                 //Thumb 1st Joint
                    pos = new Vector3(0.0f, 0.0f, 40.0f);
                    gesture.Add(new gestureJoint(finger, pos, _st, _ed, _hd));

                    finger = "RightThumbIntermediate";             //Thumb 2nd Joint
                    pos = new Vector3(30.0f, 30.0f, -50.0f);
                    gesture.Add(new gestureJoint(finger, pos, _st, _ed, _hd));

                    finger = "RightThumbDistal";                   //Thumb 3rd Joint
                    pos = new Vector3(5.0f, 100.0f, -90.0f);
                    gesture.Add(new gestureJoint(finger, pos, _st, _ed, _hd));

                    finger = "RightIndexProximal";
                    pos = new Vector3(0.0f, 0.0f, 30.0f);
                    gesture.Add(new gestureJoint(finger, pos, _st, _ed, _hd));

                    finger = "RightMiddleProximal";
                    pos = new Vector3(0.0f, 0.0f, 30.0f);
                    gesture.Add(new gestureJoint(finger, pos, _st, _ed, _hd));

                    finger = "RightRingProximal";
                    pos = new Vector3(0.0f, 0.0f, 30.0f);
                    gesture.Add(new gestureJoint(finger, pos, _st, _ed, _hd));

                    finger = "RightLittleProximal";
                    pos = new Vector3(0.0f, 0.0f, 30.0f);
                    gesture.Add(new gestureJoint(finger, pos, _st, _ed, _hd));
                    break;
            }
        }
        return gesture;
           
    }

    public class Joint
    {
        public string[] bodyName;
        public Vector3 rotateCap;
        public float start;
        public float end;
        public bool hold;

        public Joint(string[] bN, Vector3 rC, float strt, float nd, bool hd)
        {
            bodyName = bN;
            rotateCap = rC;
            start = strt;
            end = nd;
            hold = hd;
        }
    }

    public class HipJoint
    {
        public string bodyName;
        public Vector3 addPos;
        public float start;
        public float end;

        public HipJoint(string _bodyname, Vector3 _addPos, float _start, float _end)
        {
            bodyName = _bodyname;
            addPos = _addPos;
            start = _start;
            end = _end;
        }
    }

    public class Support
    {
        public string bodyName;
        public Vector3 rotateTo;
        public float start;
        public float end;
        public bool hold;

        public Support(string _bodyname, Vector3 _rotateTo, float _start, float _end, bool _hold)
        {
            bodyName = _bodyname;
            rotateTo = _rotateTo;
            start = _start;
            end = _end;
            hold = _hold;
        }
    }

    public class gestureJoint
    {
        public string fingerName;
        public Vector3 position;
        public float start;
        public float end;
        public bool hold;

        public gestureJoint(string finger, Vector3 pos, float st, float ed, bool hd)
        {
            fingerName = finger;
            position = pos;
            start = st;
            end = ed;
            hold = hd;
        }
    }
    
}
