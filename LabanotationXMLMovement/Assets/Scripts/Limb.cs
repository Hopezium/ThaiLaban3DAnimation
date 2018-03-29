using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Limb {

    public string _body;                       //Body Attribute
    public float _start;                       //Body Attribute
    public float _end;                         //Body Attribute
    public string _rotate;                     


    public Limb(string body, float start, float end, string rotate)
    {
        _body = body;
        _start = start;
        _end = end;
        _rotate = rotate;

    }
}
