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

	[SerializeField]
	Adjective[] _theAdjectives; 
	public Adjective[] AllAdj { get { return _theAdjectives; } }

	void AdjectivesInGameStart(){
		_theAdjectives = GetComponentsInChildren<Adjective> (); 
		Adjective[] _allAdjectives = FindObjectsOfType (typeof(Adjective)) as Adjective[]; 
		foreach (Adjective _adj in _allAdjectives) {
			_adj.GameStart(); 
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
	}
	void Start(){
		AdjectivesInGameStart (); 
	}
}
