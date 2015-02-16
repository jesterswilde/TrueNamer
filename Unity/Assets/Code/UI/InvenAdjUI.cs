using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI; 

public class InvenAdjUI : MonoBehaviour {

	bool _invenAdjOn = true; 
	[SerializeField]
	RectTransform _invenPanel; 
	int _index = 0; 
	float _timer = 0; 
	float _scrollThreshold = .05f; 
	bool _canScroll = true; 
	
	[SerializeField]
	GameObject[] _blankButtons; 
	List<Button> _runeButtons = new List<Button> (); 

	Animator _anim; 
	bool _scrolling = false; 

	int _adjI = 0; //index of selected adj
	public int AdjI { get { return GetAdjIndex (2); } }

	public void ShowInvenAdj(){
		_invenAdjOn = true; 
		_invenPanel.gameObject.SetActive (true); 
		RefreshInvenAdj (); 
	}
	public void HideInvenAdj(){
		WorldUI.ClearButtonList (_runeButtons); 
		_invenAdjOn = false; 
		_invenPanel.gameObject.SetActive (false); 
	}
	int GetAdjIndex(int i){
		int _slot = (i + _adjI - 2) % World.PlayerCon.LocalAdj.Count; //get the rune that should be displayed in this slot
		if(_slot < 0) _slot += World.PlayerCon.LocalAdj.Count; 
		return _slot; 
	}
	public void RefreshInvenAdj(){
		WorldUI.ClearButtonList (_runeButtons); 
		for (int i = 0; i < _blankButtons.Length; i++) {
			int _slot = GetAdjIndex(i); 
			if(World.PlayerCon.LocalAdj[_slot] != null){
				Button _theButton = WorldUI.MakeRuneButton(World.PlayerCon.LocalAdj[_slot], _blankButtons[i].transform as RectTransform);
				_runeButtons.Add(_theButton); 
				//_theButton.transform.SetParent(_blankButtons[i].transform, true); 
				_theButton.transform.position = _blankButtons[i].transform.position; 
			}
		}
	}
	public void ScrollDown(){
		_adjI -= 1; 
		//_scrolling = true; 
		//_anim.SetBool ("Down", true); 
		_timer = 0; 
		_canScroll = false; 
		RefreshInvenAdj (); 
	}
	public void ScrollUp(){
		_adjI ++ ; 
		//_scrolling = true; 
		//_anim.SetBool ("Up", true); 
		_timer = 0;
		_canScroll = false; 
		RefreshInvenAdj (); 
	}
	public Adjective SelectedAdj(){
		return World.PlayerCon.LocalAdj [_adjI % World.PlayerCon.LocalAdj.Count]; 
	}
	void AnimUpdate(){
		if(_invenPanel.gameObject.activeSelf){
			if (Input.GetAxis ("Mouse ScrollWheel") > 0 && _canScroll) {
				ScrollDown(); 	
			}
			if (Input.GetAxis ("Mouse ScrollWheel") < 0 && _canScroll) {
				ScrollUp(); 
			}
			if(_scrolling){
				if (_anim.IsInTransition (0)) {
					_anim.SetBool ("Down", false);
					_anim.SetBool ("Up",false); 
				}
				if (_anim.GetCurrentAnimatorStateInfo (0).IsTag("End") && _scrolling == true) { //if you land on the base
					RefreshInvenAdj(); 
					_scrolling = false; 
				}
			}
		}
	}

	void TimerUpdate(){
		if(_invenPanel.gameObject.activeSelf){
			if (!_canScroll) {
				_timer+= Time.unscaledDeltaTime;
				if(_timer >= _scrollThreshold){
					_canScroll = true; 
				}
			}
		}
	}

	void Update(){
		TimerUpdate (); 
		AnimUpdate (); 
	}
	void Start(){
		_anim = GetComponent<Animator> (); 
	}
}
