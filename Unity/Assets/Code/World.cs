using UnityEngine;
using System.Collections;
using System.Collections.Generic; 

public class World : MonoBehaviour {

	public static PlayerController PlayerCon;
	public static Transform PlayerTran; 
	[SerializeField]
	PlayerController _player; 
	public static Transform CamTrans;
	public static CameraController CamCon;
	public static World T; 
	[SerializeField]
	CameraController _cameraController; 
	
	Adjective[] _theAdjectives; 
	public Adjective[] AllAdj { get { return _theAdjectives; } }
	static bool _isPaused = false;
	public static bool IsPaused { get { return _isPaused; } }
	static bool _oneFrameForward = false; 
	static float _frameForwardTimer = 0; 

	[SerializeField] 
	GameObject _adjParent; 
	Dictionary<string,Adjective> _baseAdjs = new Dictionary<string, Adjective> (); 

	[SerializeField]
	int _chaos;
	public static int Chaos;

	static MadeOf[] _allMadeOfs;

	static int _id = 0; 



	public void UpdateAdjList(){ //This updates the 'base' adjectives. THey are children of hte world game object.
		_baseAdjs.Clear ();  //THey are used as reference for all other adjectives
		Adjective[] _theAdjs = _adjParent.GetComponentsInChildren<Adjective> (); 
		foreach (Adjective _adj in _theAdjs) {
			_baseAdjs.Add(_adj.adjName,_adj);
		}
	}
	public void UpdateAllAdjs(){ //This goes through all adjectives and sets the to be the same as the 'base' adjectives
		UpdateAdjList (); 
		Adjective[] _theAdjs = FindObjectsOfType<Adjective> (); 
		foreach (Adjective _adj in _theAdjs) {
			if(_adj.transform.parent != _adjParent.transform){ //If they are not a child of the _adjParent, they are not a base adjective
				Adjective _originalAdj = _baseAdjs[_adj.adjName]; 
				if(_originalAdj == null){
					Debug.Log ("You have an adjective name does not match a base adjective on the object  " + _adj.gameObject.name); 
				}
				else{
					Adjective.CopyValues(_originalAdj,_adj); 
				}
			}
		}
	}
	public void UpdateAllThings(){ //This goes through and applies adjectives to things. Used in editor. 
		if (World.T == null) {
			World.T = this; 
		}
		Thing[] _theThings = FindObjectsOfType<Thing> (); 
		foreach (Thing _thing in _theThings) {
			_thing.ClearAdjList(); 		
		}
		MadeOfGameStart (); 
		AdjectivesInGameStart (); 
		foreach (Thing _thing in _theThings) {
			_thing.UpdateThing(); 		
		}
	}

	void ThingStartGame(){
		Thing[] _allThings = FindObjectsOfType<Thing>(); 
		foreach (Thing _thing in _allThings){
			_thing.StartGame(); 
		}
	}
	void AdjectivesInGameStart(){
		_theAdjectives = GetComponentsInChildren<Adjective> (); 
		Adjective[] _allAdjectives = FindObjectsOfType (typeof(Adjective)) as Adjective[]; 
		foreach (Adjective _adj in _allAdjectives) {
			_adj.GameStart(); 
		}
	}
	static void MadeOfGameStart(){
		_allMadeOfs = World.T.GetComponentsInChildren<MadeOf> (); 
	}

	public static MadeOf WhatAmIMadeOf(Thing _theThing){ //iterates through all 'made ofs' and finds out which one a thing is 'closest' to
		int _index = 0; 
		float _distance = 100000000; 
		for(int i = 0; i < _allMadeOfs.Length;i++){
			if(_allMadeOfs[i].MaterialDistance(_theThing) < _distance){
				_distance = _allMadeOfs[i].MaterialDistance(_theThing);
				_index = i; 
			}
		}
		return _allMadeOfs [_index];
	}
	public static void SwapAdj(){
		Adjective.SwapAdjectives (PlayerCon.SelectedThing, WorldUI.TopPanel.AdjI, WorldUI.InvenAdj.AdjI);
		WorldUI.RefreshThingUI (); 
	}
	public static void PauseTime(){
		Time.timeScale = 0; 
		_isPaused = true; 
	}
	public static void UnPauseTime(){
		Time.timeScale = 1; 
		_isPaused = false; 
	}

	public static int GetID(){
		_id ++; 
		return _id; 
	}

	void GameSettings(){
		Screen.showCursor = false; 
		Screen.lockCursor = true; 
	}


	public static void StepForwardOneFrame(){
		_oneFrameForward = true; 
		_frameForwardTimer = 0; 
		Time.timeScale = 1f; 
	}
	void OneFrameForwardTimer(){
		if (_oneFrameForward) {
			_frameForwardTimer += Time.fixedDeltaTime; 
			if(_frameForwardTimer > 0){
				_oneFrameForward = false; 
				Time.timeScale = 0; 
			}
		}
	}

	void Awake(){
		if (_player != null) {
						PlayerCon = _player; 
						PlayerTran = _player.transform; 
				} else
						Debug.Log ("The world needs to know about the player, you know, likes/dislikes, and whehter it fucking exists"); 
		if (_cameraController != null) {
						CamTrans = _cameraController.transform; 
						CamCon = _cameraController; 
				} else
						Debug.Log ("Hook the camera up to the world node homie homie G"); 
		T = this;  
		Chaos = Mathf.Clamp ( _chaos,0,10); 
		MadeOfGameStart (); 
	}
	void Start(){
		ThingStartGame (); 
		//AdjectivesInGameStart (); 
		GameSettings (); 
	}
	void Update(){
		Screen.lockCursor = true; 
	}
	void FixedUpdate(){
		OneFrameForwardTimer (); 
	}


}
