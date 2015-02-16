using UnityEngine;
using System.Collections;
using System.Reflection; 

public class Adjective : MonoBehaviour {
		
	public string adjName; 
	public Sprite symbol; 
	public Color adjTint; 

	public float scale = 1; 
	public float density = 1;
	public float stability = 1; 
	public float friction = 1;  
	public float life = 1; 
	public int SortPriority; 

	protected Thing _theThing; 



	public virtual void ModifyThing(){
		_theThing.Life *= life; 
		_theThing.Friction *= friction;
		_theThing.Density *= density; 
		_theThing.Stability *= stability; 
		_theThing.Scale *= scale; 
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



	public static void SwapAdjectives(Thing _theThing, int _thingSlot, int _playerSlot){ //Takes an adjective from the player, and an adjective from the thing and swaps them
		Adjective _thingAdj = _theThing.GetAdj (_thingSlot); 
		Adjective _playerAdj = World.PlayerCon.GetAdj (_playerSlot);   //put the new adj on the respective objects
		GameObject _tempGo = new GameObject (); //move the adjectives over the a temporary home. 
		Adjective _tempThingAdj = CopyAdjective (_thingAdj, _tempGo, 0);
		Adjective _tempPlayerAdj = CopyAdjective (_playerAdj, _tempGo, 0);//Done moveing them over
		Adjective _newThingAdj = CopyAdjective (_tempPlayerAdj, _theThing.gameObject, _thingSlot); 
		Adjective _newPlayerAdj = CopyAdjective (_tempThingAdj, World.PlayerCon.AdjHolder, _playerSlot); 
		//_theThing.SwapAdjectives (_newThingAdj, _thingSlot);//do the book keeping
		//World.PlayerCon.ReplaceAdjOnPlayer (_playerSlot, _newPlayerAdj); 
		Destroy (_tempGo); 
	}
	public static Adjective CopyAdjective(Component original, GameObject destination, int slot){
		Adjective _theAdj = CopyComponent (original, destination) as Adjective; 
		Thing _theThing = destination.GetComponent<Thing> (); 
		if(_theThing != null){
			if(_theAdj != null){
				_theAdj.Startup (); 
			}
			_theThing.SwapAdjectives(_theAdj,slot); 
		}
		else{ //we are assuming if the object doesn't have a thing component, then it's the player
			if(destination == World.PlayerCon.AdjHolder){
				World.PlayerCon.ReplaceAdjOnPlayer(slot,_theAdj);
			}
		}
		WorldUI.RefreshThingUI (); 
		return _theAdj; 
	}
	static Component CopyComponent(Component original, GameObject destination)
	{
		if(original != null){
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
		return null; 
	}
}
