using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using Leap;

public class GUI_Button : MonoBehaviour {
	//public Button[] opButton;

	Menu menu;
	Operations operations;


	bool buttonTouchable = true;
	//bool menuOpened = true;
	float buttonDeltaTime = 1.0f;


	// Use this for initialization
	void Start () {
		menu = GetComponent<Menu> ();
		operations = GetComponent<Operations> ();
	}

	// Update is called once per frame
	void Update () {
		/*
		for(int i = 0 ; i < 6 ; i++){
			if (i != operations.opMode) {
				menu.opButtons[i].image.color = Color.black;
				menu.opButtons[i].GetComponentInChildren<Text> ().color = Color.white;
			} else {
				menu.opButtons[i].image.color = Color.white;
				menu.opButtons[i].GetComponentInChildren<Text> ().color = Color.black;
				
			}
		}
		if(operations.opMode==0) operations.lineRenderer.material.color = Color.black;
		else if(operations.opMode==1) operations.lineRenderer.material.color = Color.blue;
*/

	}
	/*public Vector2 leapPositionToScreen(Vector leapVector)
	{
		return operations.cam.WorldToScreenPoint(leapVector.ToUnityTranslated());
	}*/

	public bool inActiveZoneFinger(Button button){
		if (operations.hand.IsRight) {

			RectTransform rectButton = button.GetComponent<RectTransform> (); 		
			Rect box = new Rect (
				Camera.main.WorldToScreenPoint (button.transform.position).x,
				Camera.main.WorldToScreenPoint (button.transform.position).y,
				//button.rect.x,
				//button.rect.y,
				//opButton.image.sprite.bounds.size.x,
				//opButton.image.sprite.bounds.size.y
				
				rectButton.rect.width,
				rectButton.rect.height
				);   
			//Vector3 point = Camera.main.WorldToScreenPoint (GameObject.Find ("SkeletalRightRobotHand(Clone)").transform.FindChild ("index").FindChild ("bone3").position);

			GameObject finger;
			if ((finger = GameObject.Find ("SkeletalRightRobotHand(Clone)/index/bone3")) != null){
				
				Vector3 point = Camera.main.WorldToScreenPoint(finger.transform.position);

				//if(point != null){	

					if (point.x <= box.x + box.width / 2 && point.y >= box.y + (-1 * (box.height)) / 2) {

					return true;
				}
			}
		}
		return false;
	}



	void OnGUI () {

		operations.pointable = operations.frame.Pointables.Frontmost;
		operations.touchZone = operations.pointable.TouchZone;
		/*
		if (operations.handsInFrame.Leftmost.GrabStrength == 1 && menuOpened == true) {//open/close menu
			menuOpened = false;
			if (menu.mainMenu.gameObject.activeSelf) {
				menu.ShowMainMenu (false);
			}else if (!menu.mainMenu.gameObject.activeSelf) { 
				menu.ShowMainMenu (true);
			}
		} 
		else if( operations.handsInFrame.Leftmost.GrabStrength <= 0.5 && menuOpened == false){
			menuOpened = true;
		}
*/


		if (buttonTouchable == true) {
			switch (operations.touchZone) {
			case Pointable.Zone.ZONE_NONE: // None
				break;
			case Pointable.Zone.ZONE_HOVERING: // Hovering
		

				break;
			case Pointable.Zone.ZONE_TOUCHING: // Touching
			
				for( int i = 0 ; i < 6 ; i++){ //operation
					if (inActiveZoneFinger (menu.opButtons[i])) {				
						buttonTouchable = false;
						menu.ChangeOpMode(i);
						menu.HideSlider ();
						//menu.ShowMainMenu (false);
						//menuOpened = false;

						return;
					}
				}
				for(int i = 0 ; i < 3; i++){ //camera
					if(inActiveZoneFinger (menu.camposButtons[i])){
						buttonTouchable = false;
						operations.SetCamPos(i);
						menu.HideSlider ();
						return;
					}
				}
				for(int i = 0 ; i < 3; i++){ //visual state
					if(inActiveZoneFinger (menu.visualButtons[i])){
						buttonTouchable = false;
						menu.ChangeVisualState(i);
						menu.HideSlider ();
						return;
					}
				}

				if(inActiveZoneFinger (menu.loadButton)){
				   menu.ShowLoadMenu(true);
					buttonTouchable = false;
					menu.HideSlider ();
				}
				else if(inActiveZoneFinger (menu.saveButton)){
				   menu.ShowSaveDialog(true);
					buttonTouchable = false;
					menu.HideSlider ();
				}
				else if(inActiveZoneFinger(menu.undoButton)){
					print ("undo");
					operations.Undo();
					buttonTouchable = false;
					menu.HideSlider ();
				}
				else if(inActiveZoneFinger(menu.redoButton)){
					operations.Redo();
					buttonTouchable = false;
					menu.HideSlider ();
				}
				else if(inActiveZoneFinger(menu.cleanButton)){
					operations.Clean();
					print ("clean");
					buttonTouchable = false;
					menu.HideSlider ();
				}
				/*if(operations.cubeArray != null){
					//if(inActiveZoneFinger(menu.sliderPanel.transform.FindChild("Button").GetComponent<Button>()))
					if(inActiveZoneFinger(menu.yesButton))	{
					//operations.SetThickness();
						print ("12");
					}
				}*/
				break;
			}
		}
		else {
			buttonDeltaTime -= Time.deltaTime;
			if(buttonDeltaTime <= 0){
				buttonTouchable = true;
				buttonDeltaTime = 1.0f;
			}

	
		}
	}
	/*
	Vector2 ToVector2 (Vector leap){
		leap = operations.frame.InteractionBox.NormalizePoint (leap);
		return new Vector2 (UnityEngine.Screen.width*leap.x,UnityEngine.Screen.height*leap.y);

	}*/



	Vector3 ToVector3 (Vector leap){
		return new Vector3 (leap.x,leap.y,-leap.z);
		
	}
}