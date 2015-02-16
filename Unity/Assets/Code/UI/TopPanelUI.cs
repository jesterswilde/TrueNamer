using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic; 

public class TopPanelUI : MonoBehaviour {



	[SerializeField]
	GameObject _thingTopPanel;
	[SerializeField]
	RectTransform _topPanelRuneParent; 
	[SerializeField]
	Object _runeSelectorPrefab; 
	GameObject _runeSelectorGO; 
	bool _topPanelOn = true; //this keeps track of whether or not we should be drawing this menu

	List<Button> _topAdjButtons = new List<Button>(); 

	Thing _theThing;

	int _adjI = 0; 
	public int AdjI { get { return _adjI - 1; } }


	void MakeBlankRune(){ //make the blank rune, set it as teh first in the list
		_topAdjButtons.Add (WorldUI.MakeRuneButton (_topPanelRuneParent)); //makes the blank rune
	}
	void MakeThingRunes(){ //make all the other runes. Remember that each of these is one higher in the button index than on the adjective
		foreach (Adjective _adj in _theThing.LocalAdj) { //in with the new
			Button _theButton = WorldUI.MakeRuneButton(_adj,_topPanelRuneParent); 
			_topAdjButtons.Add (_theButton); 
		}
	}
	void MakeSelector(){ //makes a selector and places it over the currently selected rune
		if (_runeSelectorGO != null) {
			Destroy (_runeSelectorGO);	
		}
		if (_adjI - 1 >= World.PlayerCon.SelectedThing.LocalAdj.Count) {
			_adjI = World.PlayerCon.SelectedThing.LocalAdj.Count; 
		}
		if(_adjI < 0 ) _adjI = 0;
		_runeSelectorGO = Instantiate (_runeSelectorPrefab) as GameObject; 
		RectTransform _parent = _topAdjButtons [_adjI].transform as RectTransform; 
		RectTransform _child = _runeSelectorGO.transform as RectTransform; 
		_child.SetParent (_parent); 

	}
	
	
	public void ShowTopPanel(){
			_topPanelOn = true; 
			_thingTopPanel.SetActive (true);
			RefreshTopPanel (); 	
	}
	public void HideTopPanel(){
		if(_topPanelOn == true){
			_theThing = null; 
			_topPanelOn = false; 
			_thingTopPanel.SetActive(false); 
			WorldUI.ClearButtonList(_topAdjButtons); 
		}
	}
	public void RefreshTopPanel(){ //called whenever you need to draw the buttons
		if(_topPanelOn == true){
			_theThing = World.PlayerCon.SelectedThing;
			if ( _theThing != null) {
				WorldUI.ClearButtonList (_topAdjButtons); //out with the old
				MakeBlankRune();
				MakeThingRunes(); 
				MakeSelector(); 
			}
		}
	}
	void ChangeSelectedRune(){
		if(World.IsPaused){
			if(Input.GetKeyDown(KeyCode.D)){
				_adjI +=1;
				RefreshTopPanel(); 
			}
			if(Input.GetKeyDown (KeyCode.A)){
				_adjI -= 1; 
				RefreshTopPanel(); 
			}
		}
	}
	void Update(){
		ChangeSelectedRune (); 
	}

}
