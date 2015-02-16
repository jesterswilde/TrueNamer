using UnityEngine;
using System.Collections;
using UnityEngine.UI; 
using System.Collections.Generic; 

public class WorldUI : MonoBehaviour {


	[SerializeField]
	Transform _canvasTrans;
	public static Transform CanvasTrans; 
	[SerializeField]
	Object _runeButtonPrefab; 
	public static Object RuneButton;

	[SerializeField]
	TopPanelUI _topPanelUI; 
	public static TopPanelUI TopPanel; 
	[SerializeField]
	InvenAdjUI _invenADjUI;
	public static InvenAdjUI InvenAdj;
	public static WorldUI T; 
	
	[SerializeField]
	Sprite _blankRune; 
	public Sprite BlankRune{ get { return _blankRune; } }

	static bool _thingUIOn = true; 
	public static bool IsThingUIOn { get { return _thingUIOn; } }


	public static Button MakeRuneButton(Adjective _adj, RectTransform _parent){
		GameObject _runeGO = Instantiate (RuneButton) as GameObject; 
		Button _button = _runeGO.GetComponent<Button> (); 
		Image _image = _runeGO.GetComponent<Image> (); 
		_image.sprite =_adj.symbol; 
		_runeGO.transform.SetParent (_parent); 
		return _button; 
	}
	public static Button MakeRuneButton(RectTransform _parent){
		GameObject _runeGO = Instantiate (RuneButton) as GameObject; 
		Button _button = _runeGO.GetComponent<Button> (); 
		Image _image = _runeGO.GetComponent<Image> (); 
		_image.sprite = WorldUI.T.BlankRune; 
		_runeGO.transform.SetParent (_parent); 
		return _button; 
	}
	public static void ClearButtonList(List<Button> _theList){ //clears a list of it's buttons, then clears it 
		foreach (Button _button in _theList) {
			Destroy(_button.gameObject); 
		}
		_theList.Clear (); 
	}


	public static void ShowThingUI(){
		TopPanel.ShowTopPanel ();
		InvenAdj.ShowInvenAdj (); 
		_thingUIOn = true; 
	}
	public static void ShowInventoryOnly(){
		InvenAdj.ShowInvenAdj (); 
	}
	public static void HideThingUI(){
		TopPanel.HideTopPanel (); 
		InvenAdj.HideInvenAdj (); 
		_thingUIOn = false; 
	}
	public static void HideInventoryOnly(){
		InvenAdj.HideInvenAdj ();
	}
	public static void RefreshThingUI(){
		TopPanel.RefreshTopPanel (); 
		InvenAdj.RefreshInvenAdj (); 
	}


	void Awake(){
		RuneButton = _runeButtonPrefab; 
		TopPanel = _topPanelUI; 
		InvenAdj = _invenADjUI; 
		CanvasTrans = _canvasTrans; 
		T = this; 
	}
	void Start(){
		HideThingUI (); 
	}

}
