using UnityEngine;
using System.Collections;
using UnityEditor; 


[CustomEditor (typeof(World))]
public class EditWorld : Editor {

	public override void OnInspectorGUI ()
	{
		World _theWorld = (World)target;
		DrawDefaultInspector (); 
		if(GUILayout.Button("Update Adjectives")){
			_theWorld.UpdateAllAdjs(); 
		}
		if(GUILayout.Button("Update Things")){
			_theWorld.UpdateAllThings(); 
		}
	}
}
