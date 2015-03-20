using UnityEngine;
using System.Collections;
using System.Collections.Generic; 

public class World : MonoBehaviour {

	#region Variable Decleration

	public static PlayerController PlayerCon;
	public static Transform PlayerTran; 
	[SerializeField]
	PlayerController _player; 
	public static Transform CamTrans;
	public static CameraController CamCon;
	public static World T; 
	[SerializeField]
	CameraController _cameraController; 
	[SerializeField]
	GroundDetection _groundD; 
	public static GroundDetection GroundD; 
	
	Adjective[] _theAdjectives; 
	public Adjective[] AllAdj { get { return _theAdjectives; } }
	static bool _isPaused = false;
	public static bool IsPaused { get { return _isPaused; } }
	static bool _oneFrameForward = false; 
	static float _frameForwardTimer = 0; 
	List<Thing> _heatingThings = new List<Thing> (); 
	List<Thing> _fireThings = new List<Thing>(); 

	[SerializeField] 
	GameObject _adjParent; 
	Dictionary<string,Adjective> _baseAdjs = new Dictionary<string, Adjective> (); 

	[SerializeField]
	int _chaos;
	public static int Chaos;
	static bool _canUpause = true; 
	public static bool CanUnpause { get { return _canUpause; } }

	static MadeOf[] _allMadeOfs;
	static GroundDetection[] _allGroundD; 

	static int _id = 0; 

	[SerializeField]
	LayerMask _noPlayerMask; 
	public static LayerMask NoPlayerMask; 

	#endregion

	#region Component Collection 

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
				Adjective _originalAdj = null;
				if(!_baseAdjs.ContainsKey(_adj.adjName)){
					Debug.Log("you spelled " + _adj.adjName + " on object " + _adj.name + " incorrectly, fix it batch."); 
				}
				else{
					_originalAdj = _baseAdjs[_adj.adjName]; 
				}
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
		MadeOfGameStart (); 
		Thing[] _theThings = FindObjectsOfType<Thing> (); 
		foreach (Thing _thing in _theThings) {
			_thing.StartGame(); 	
		}
	}

	void ThingStartGame(){ //Also used in buttons. Tell every thing to do their start up routine
		Thing[] _allThings = FindObjectsOfType<Thing>(); 
		foreach (Thing _thing in _allThings){
			_thing.StartGame(); 
		}
	}
	static void MadeOfGameStart(){ //gets the made of properties
		_allMadeOfs = World.T.GetComponentsInChildren<MadeOf> (); 
	}
	void GetAllGroundD(){ //get collect all ground detection 
		_allGroundD = FindObjectsOfType<GroundDetection> (); 
	}

	#endregion

	#region active public methods

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

	public static void RecalculateGroundD(){
		foreach (GroundDetection _groundD in _allGroundD) {
			if(!_groundD.IsTheGround){
				_groundD.TurnOff(.05f); 
			}
		}
	}

	#endregion



	#region Fire 
	public void AddToHeatingList(Thing _theThing){
		if (!_heatingThings.Contains(_theThing)) {
			_heatingThings.Add (_theThing);	
		}
	}

	public void RemoveFromHeatingThings(Thing _theThing){
		if (_heatingThings.Contains(_theThing)) {
			_heatingThings.Remove(_theThing); 	
		}
	}
	public void AddToFireList(Thing _theThing){
		if (!_fireThings.Contains(_theThing)) {
			_fireThings.Add(_theThing); 	
		}
	}
	public void RemoveFromFireList(Thing _theThing){
		if (_fireThings.Contains(_theThing)) {
			_fireThings.Remove(_theThing); 
		}
	}

	void CalculateFire(){
		foreach (Thing _thing in _fireThings) {
			_thing.OnFireHandling(); 	
		}
	}
	void CalculateHeating(){
		foreach (Thing _thing in _heatingThings) {
			_thing.HeatingHandling(); 
		}
	}
	void FireTimer(){ //callled at the start of the game, is the rate at which we check for fire things happening. 
		InvokeRepeating ("CalculateFire", 1f, 1f);
		InvokeRepeating ("CalculateHeating", 1.1f, 1f); 
	}
	#endregion



	#region Adjective and Time
	public static void SwapAdj(){ //the main function called to swap adj from player to thing
		Adjective.SwapAdjectives (PlayerCon.SelectedThing, WorldUI.TopPanel.AdjI, WorldUI.InvenAdj.AdjI);
		WorldUI.RefreshThingUI (); 
		PlayerCon.GetTouchedSurface (); //Do a ground test. 
	}
	public static void PauseTime(){ //used to pause the game
		Time.timeScale = 0; 
		PlayerCon.Pause (); 
		_isPaused = true; 
	}
	public static void UnPauseTime(){ //used to unpause the game. 
		if(_canUpause){
			Time.timeScale = 1; 
			_isPaused = false;
		}
	}

	public static int GetID(){
		_id ++; 
		return _id; 
	}

	void GameSettings(){
		Cursor.visible = false; 
		Screen.lockCursor = true; 
	}

	public static void SlowMo(){
		Debug.Log ("Enter slow mo"); 
		Time.timeScale = .3f; 
		_canUpause = false; 
	}
	public static void UnSlowMo(){
		Debug.Log ("Leaving Slow Mo"); 
		Time.timeScale = 0; 
		_canUpause = true; 
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
	#endregion




	#region Start Awake & Update
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
		GroundD = _groundD; 
		NoPlayerMask = _noPlayerMask; 
		Chaos = Mathf.Clamp ( _chaos,0,10); 
		MadeOfGameStart (); 
	}
	void Start(){
		ThingStartGame (); 
		GetAllGroundD (); 
		//AdjectivesInGameStart (); 
		GameSettings (); 
	}
	void Update(){
		Screen.lockCursor = true; 
	}
	void FixedUpdate(){
		OneFrameForwardTimer (); 
	}
	#endregion

}
