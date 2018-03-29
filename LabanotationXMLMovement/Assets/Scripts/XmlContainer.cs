using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Linq;
using System.Linq;
using System.IO;

public class XmlContainer : MonoBehaviour {

	public List<Measure> Full_Dance = new List<Measure>();
	public static XmlContainer instance;

	// Use this for initialization
	void Awake () {
		instance = this;

		Full_Dance = parseFile("Basic");
		
	}
	
	public static List<Measure> parseFile(string name)
	{
        TextAsset _xml = Resources.Load<TextAsset>(name);

        var doc = XDocument.Parse(_xml.text);

        var all_measure = doc.Element("ThaiDance").Element("Attribute").Elements("Measure");
        List<Measure> measure = new List<Measure>();
        foreach (var one_measure in all_measure)
 	   {
		    //Create List of Body Xml Data
		    List<Body> body = new List<Body>();

		    //Store Measure Number
		    int attribute_num;                                                      
            var num = one_measure.Attribute("num");
            string str_attribute_num = num.Value;
            int.TryParse(str_attribute_num, out attribute_num);

            //Number Beat per Measure
            float attribute_beat;
            var beat = one_measure.Attribute("beat");
            string str_attribute_beat = beat.Value;
            float.TryParse(str_attribute_beat, out attribute_beat);

            //Beat per Minute
            float attribute_bpm;
            string str_attribute_bpm;
            if (one_measure.Attribute("bpm") != null)
            {
                var bpm = one_measure.Attribute("bpm");
                str_attribute_bpm = bpm.Value;
                float.TryParse(str_attribute_bpm, out attribute_bpm);                
            }
            else
                attribute_bpm = 60.0f; 

			//Store Body Data
			var all_moves = one_measure.Elements("Body");

			foreach (var one_move in all_moves)
			{
				//Store Body name
				var part = one_move.Attribute("part");
				string attribute_bodypart = part.Value;

				//Store starting time of Move
				float attribute_start;
				var start = one_move.Attribute("start");
				string str_attribute_start = start.Value;
				float.TryParse(str_attribute_start, out attribute_start);

				//Store ending time of Move
				float attribute_end;
				var end = one_move.Attribute("end");
				string str_attribute_end = end.Value;
				float.TryParse(str_attribute_end, out attribute_end);

				//Store direction of Move
				string element_direction;
				if(one_move.Element("Direction")!= null)
				{
					var direction = one_move.Element("Direction");
					element_direction = direction.Value;
				}
				else
				element_direction = null;

				//Store level of Move
				string element_level;
				if(one_move.Element("Level")!= null)
				{
					var level = one_move.Element("Level");
					element_level = level.Value;
				}
				else
				element_level = null;

				body.Add(new Body(attribute_bodypart, attribute_start, attribute_end, element_direction, element_level));
				

			}	
			//Debug.Log("Measure body count" + body.Count);

			measure.Add( new Measure(body, attribute_num, attribute_beat, attribute_bpm));

        }

		//Debug.Log("Total Measure " + measure.Count);
		return measure;

    } 

}
