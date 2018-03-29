using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Measure
{    
    public List<Body> body = new List<Body>();

    public int num;

    public float beat;

    public float bpm;

    public Measure(List<Body> _body, int _num, float _beat, float _bpm)
    {
        body = _body;
        num = _num;
        beat = _beat;
        bpm = _bpm;
    }

}
