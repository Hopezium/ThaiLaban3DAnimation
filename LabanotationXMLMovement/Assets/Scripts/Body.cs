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
    //public string _fold;
    //public float 

	public Body(string body, float start, float end, string direction, string level)
	{
		_body = body;
		_start = start;
		_end = end;
		_direction = direction;
		_level = level;
	}
}
