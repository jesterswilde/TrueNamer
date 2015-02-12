using UnityEngine;
using System.Collections;
using System.Reflection; 

public class Adjective : MonoBehaviour {
	
	public int SortPriority; 

	protected Thing _theThing; 



	public virtual void ModifyThing(){
	
	}

	protected virtual void Startup(){
		if(_theThing == null){
			_theThing = GetComponent<Thing> (); 
		}
	}
	public virtual void GameStart(){
		_theThing = GetComponent<Thing> (); 
		if (_theThing != null) {
			_theThing.AddAdjective(this); 
		}
	}
	void Start(){
		Startup (); 
	}

	public static Adjective CopyAdjective(Component original, GameObject destination, int slot){
		Adjective _theAdj = CopyComponent (original, destination) as Adjective; 
		_theAdj.Startup (); 
		_theAdj._theThing.ReplaceAdjective (_theAdj, slot); 
		return _theAdj; 
	}
	static Component CopyComponent(Component original, GameObject destination)
	{
		System.Type type = original.GetType();
		Component copy = destination.AddComponent(type);
		// Copied fields can be restricted with BindingFlags
		System.Reflection.FieldInfo[] fields = type.GetFields(); 
		foreach (System.Reflection.FieldInfo field in fields)
		{
			field.SetValue(copy, field.GetValue(original));
		}
		return copy;
	}
}
