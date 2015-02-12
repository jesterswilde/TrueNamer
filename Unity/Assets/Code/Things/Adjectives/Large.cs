using UnityEngine;
using System.Collections;

public class Large : Adjective {


	public override void ModifyThing ()
	{
		base.ModifyThing ();
		_theThing.Scale *= 2; 
		_theThing.Mass *= 4; 
	}

	void Start () {
		Startup (); 
	}

}
