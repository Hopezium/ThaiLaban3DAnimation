using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Body
{

    public string _body;
	public float _start;
	public float _end;
	public string _direction;
	public string _level;
    public bool _hold;
    public string _fold;
    public float _degree;
    public string _gesture;
    public string _space;
    public float _extent;

	public Body(string body, float start, float end, string direction, string level, bool hold, string fold, float degree, string gesture, string space, float extent)
	{
		_body = body;
		_start = start;
		_end = end;
		_direction = direction;
		_level = level;
        _hold = hold;
        _fold = fold;
        _degree = degree;
        _gesture = gesture;
        _space = space;
        _extent = extent;
	}
}
