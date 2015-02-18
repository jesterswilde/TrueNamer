using UnityEngine;
using System.Collections;
using UnityEditor; 


[CustomEditor (typeof(Thing))]
public class EditThing : Editor {

	public override void OnInspectorGUI ()
	{
		Thing _theThing = (Thing)target; 
		DrawDefaultInspector (); 
		if(GUILayout.Button("Remove Adjective Influence")){
			_theThing.RemoveAdjectiveInfluence(); 
		}
		if(GUILayout.Button ("Set as Modified Scale")){
			_theThing.SetAsModifiedScale(); 
		}
		if(GUILayout.Button("Apply Local Adejctives")){
			_theThing.ApplyLocalAdjectives(); 
		}
	}
}
