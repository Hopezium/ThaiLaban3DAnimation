using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BodyController : MonoBehaviour
{
    public GameObject JointControllerPrefab;
    private Animator Anim;
    private Dictionary<HumanBodyBones, JointController> AllJoints;
    float hipXValue, movePos = 1;
    Vector3 hipPos = Vector3.zero;
    List<Joint> timeSortedJoints = new List<Joint>();
    List<HipJoint> timeSortedHipPos = new List<HipJoint>();
    List<Support> timeSortedSupportJoint = new List<Support>();

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
        for (int i = 0; i < 60; i++)
        {
            string check;
            Transform BoneTrans = Anim.GetBoneTransform((HumanBodyBones)i);
            if (BoneTrans != null)
            {
                GameObject controller = Instantiate(JointControllerPrefab);              //Create Controller Prefab
                Vector3 ConPos = BoneTrans.position;                                    //Get position of Specific HumanBody Bones
                ConPos.z += 1;                                                             //1 Vector.z unit away from model
                controller.transform.position = ConPos;                                  // Set Controller Prefab position 1 Vector.z unit away from specific HumanBody Bones
                controller.transform.rotation = BoneTrans.rotation;
                //BoneTrans.transform.SetParent(controller.transform);
                controller.name = ((HumanBodyBones)i).ToString() + " JC";
                if(((HumanBodyBones)i).ToString() == "Hips")
                {
                    hipXValue = controller.transform.position.x;
                }
                JointController JC = controller.GetComponent<JointController>();
                JC.JointToControllTrans = BoneTrans;                                      //JC.JointToControllTrans = Anim.GetBoneTransform((HumanBodyBones)i)
                AllJoints.Add((HumanBodyBones)i, JC);
                check = ((HumanBodyBones)i).ToString();
                //if(check[0] == 'L')
                if (((HumanBodyBones)i).ToString().StartsWith("Left"))
                    controller.gameObject.tag = "Left";

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
                //Debug.Log(XmlContainer.instance.Full_Dance[i].body[j]._direction);
                if (XmlContainer.instance.Full_Dance[i].body[j]._body == "left_support" || XmlContainer.instance.Full_Dance[i].body[j]._body == "right_support")
                {
                    hipPos += getHipPosition(XmlContainer.instance.Full_Dance[i].body[j]._direction);
                    start = XmlContainer.instance.Full_Dance[i].body[j]._start * (60.0f / XmlContainer.instance.Full_Dance[i].bpm) + timePassed;
                    end = XmlContainer.instance.Full_Dance[i].body[j]._end * (60.0f / XmlContainer.instance.Full_Dance[i].bpm) + timePassed;
                    timeSortedHipPos.Add(new HipJoint(("Hips"), hipPos, start, end));
                    _rotateCap += getSupportDirection(XmlContainer.instance.Full_Dance[i].body[j]._direction);
                    string supportName = XmlContainer.instance.Full_Dance[i].body[j]._body;
                    if (supportName == "left_support")
                    {
                        timeSortedSupportJoint.Add(new Support(("LeftUpperLeg"), _rotateCap, start, end));                        
                        float xValue = _rotateCap.x;
                        float zValue = _rotateCap.z;
                        _rotateCap.x = -xValue;
                        _rotateCap.z = -zValue;
                        timeSortedSupportJoint.Add(new Support(("RightUpperLeg"), _rotateCap, start, end));
                    }
                    else
                    {
                        timeSortedSupportJoint.Add(new Support(("RightUpperLeg"), _rotateCap, start, end));
                        float xValue = _rotateCap.x;
                        float zValue = _rotateCap.z;
                        _rotateCap.x = -xValue;
                        _rotateCap.z = -zValue;
                        timeSortedSupportJoint.Add(new Support(("LeftUpperLeg"), _rotateCap, start, end));
                    }
                }
                else
                {
                    string[] _nameJC = getName(XmlContainer.instance.Full_Dance[i].body[j]._body);
                    _rotateCap += getDirection(XmlContainer.instance.Full_Dance[i].body[j]._direction);
                    _rotateCap += getLevel(XmlContainer.instance.Full_Dance[i].body[j]._level);
                    start = XmlContainer.instance.Full_Dance[i].body[j]._start * (60.0f / XmlContainer.instance.Full_Dance[i].bpm) + timePassed;
                    end = XmlContainer.instance.Full_Dance[i].body[j]._end * (60.0f / XmlContainer.instance.Full_Dance[i].bpm) + timePassed;
                    //Debug.Log(_rotateCap);
                    moves.Add(new Joint(_nameJC, _rotateCap, start, end));
                }
            }

        

            timePassed += XmlContainer.instance.Full_Dance[i].beat * (60.0f / XmlContainer.instance.Full_Dance[i].bpm);
        }
        //Debug.Log("moves: " + moves.Count);
        timeSortedJoints = moves;
        //Debug.Log("moves: " + timeSortedJoints.Count);
    }

    void AssignTaskToJoints()
    {

        for (int i = 0; i < timeSortedJoints.Count; i++)
        {
            Joint JD = timeSortedJoints[i];
            for (int j = 0; j < timeSortedJoints[i].bodyname.Length; j++)
            {
                JointController JC = AllJoints[(HumanBodyBones)Enum.Parse(typeof(HumanBodyBones), JD.bodyname[j], true)];
                JC.Task(JD.start, JD.end, JD.rotateCap);
            }
        }

        for (int i = 0; i < timeSortedHipPos.Count; i++)
        {
            HipJoint HJ = timeSortedHipPos[i];
            JointController JC = AllJoints[(HumanBodyBones)Enum.Parse(typeof(HumanBodyBones), HJ.bodyname, true)];
            JC.Task2(HJ.start, HJ.end, HJ.addPos);
        }

        for (int i = 0; i < timeSortedSupportJoint.Count; i ++)
        {
            Support SP = timeSortedSupportJoint[i];
            JointController JC = AllJoints[(HumanBodyBones)Enum.Parse(typeof(HumanBodyBones), SP.bodyname, true)];
            JC.Task3(SP.start, SP.end, SP.rotateTo);
        }
    }

    Vector3 getDirection(string direction)
    {
        Vector3 dir = Vector3.zero;
        switch (direction)
        {
            case "forward":
                dir.y = 260.0f;
                break;

            case "backward":
                dir.y = 125.0f;
                break;

            case "left":
                dir.y = 180.0f;
                break;

            case "right":
                dir.y = 320.0f;
                break;

            case "forward_right":
                dir.y = 225.0f;
                break;

            case "forward_left":
                dir.y = 300.0f;
                break;

            case "backward_right":
                dir.y = 60.0f;
                break;

            case "backward_left":
                dir.y = 130.0f;
                break;
        }
        return dir;

    }

    Vector3 getLevel(string level)
    {
        Vector3 lvl = Vector3.zero;
        switch (level)
        {
            case "high":
                lvl.z = 70.0f;
                break;

            case "middle":
                lvl.z = 0.0f;
                break;

            case "low":
                lvl.z = -70.0f;
                break;
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
                dir = new Vector3(-25.0f,-180.0f,0.0f);
                break;

            case "backward":
                dir = new Vector3(25.0f, -180.0f, 0.0f);
                break;

            case "left":
                dir = new Vector3(0.0f, -180.0f, -10.0f);
                break;

            case "right":
                dir = new Vector3(0.0f, -180.0f, 10.0f);
                break;

            case "forward_right":
                dir = new Vector3(-15.0f, -180.0f, 15.0f);
                break;

            case "forward_left":
                dir = new Vector3(-15.0f, -180.0f, -15.0f);
                break;

            case "backward_right":
                dir = new Vector3(15.0f, -180.0f, 15.0f);
                break;

            case "backward_left":
                dir = new Vector3(15.0f, -180.0f, -15.0f);
                break;
        }
        return dir;
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
                _name = new string[] { "Chest", "Spine" };
                break;

            case "right_torso":
                _name = new string[] { "Chest", "Spine" };
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

    public class Joint
    {
        public string[] bodyname;
        public Vector3 rotateCap;
        public float start;
        public float end;

        public Joint(string[] bN, Vector3 rC, float strt, float nd)
        {
            bodyname = bN;
            rotateCap = rC;
            start = strt;
            end = nd;
        }
    }

    public class HipJoint
    {
        public string bodyname;
        public Vector3 addPos;
        public float start;
        public float end;

        public HipJoint(string _bodyname, Vector3 _addPos, float _start, float _end)
        {
            bodyname = _bodyname;
            addPos = _addPos;
            start = _start;
            end = _end;
        }
    }

    public class Support
    {
        public string bodyname;
        public Vector3 rotateTo;
        public float start;
        public float end;

        public Support(string _bodyname, Vector3 _rotateTo, float _start, float _end)
        {
            bodyname = _bodyname;
            rotateTo = _rotateTo;
            start = _start;
            end = _end;
        }
    }
}
