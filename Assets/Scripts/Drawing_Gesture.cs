using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Leap;


public class Drawing_Gesture : MonoBehaviour {

	public static Controller controller;
	public static Frame frame;
	public GameObject startCube;
	public GameObject cubeObject;
	public Camera cam;

	public static Hand hand;
	public static Finger finger;
	public static Pointable pointable;
	public static Pointable.Zone touchZone;
	HandList handsInFrame;

	Operations operations;
	Menu menu;
	//CubeMesh selectedMesh;
	public LineRenderer lineRenderer;

	SwipeGesture swipeGesture;
	ScreenTapGesture screenTapGesture;

	int extendedFingers = 0;
	float GrabStrength = 0.0f;
	Vector handSpeed;
	Vector swipeDirection;
	Vector screenTapDirection;


	// Use this for initialization
	void Start () {
		operations = GetComponent<Operations> ();
		menu = GetComponent<Menu> ();
		lineRenderer = GetComponent<LineRenderer> ();


		controller = new Controller();

		controller.EnableGesture(Gesture.GestureType.TYPE_CIRCLE);
		controller.EnableGesture(Gesture.GestureType.TYPE_KEY_TAP);
		controller.EnableGesture(Gesture.GestureType.TYPE_SCREEN_TAP);
		controller.EnableGesture(Gesture.GestureType.TYPE_SWIPE);


		controller.Config.SetFloat("Gesture.ScreenTap.MinForwardVelocity", 0.1f);
		controller.Config.SetFloat("Gesture.ScreenTap.HistorySeconds", 1.0f);
		controller.Config.SetFloat("Gesture.ScreenTap.MinDistance", 0.1f);
		controller.Config.Save();

		controller.Config.SetFloat("Gesture.Swipe.MinLength", 120.0f);
		controller.Config.SetFloat("Gesture.Swipe.MinVelocity", 650.0f);
		controller.Config.SetFloat("Gesture.Swipe.HistorySeconds", 10.0f);
		controller.Config.Save();

	}


	void Move(){
		GrabStrength = hand.GrabStrength;

		if (GrabStrength == 1 && hand.IsRight) {
			handSpeed = hand.PalmVelocity;
			cam.transform.Translate (-handSpeed.x / 1000.0f, -handSpeed.y / 1000.0f, handSpeed.z / 1000.0f);
		}
	}



	// Update is called once per frame
	void Update () {




		frame = controller.Frame();
		pointable = frame.Pointables.Frontmost;
		touchZone = pointable.TouchZone;

		hand = pointable.Hand;
		handsInFrame = frame.Hands;

		for(int g = 0; g < frame.Gestures().Count; g++)
		{

			switch (frame.Gestures()[g].Type) {

			case Gesture.GestureType.TYPE_CIRCLE:
				//Handle circle gestures
				break;
			case Gesture.GestureType.TYPE_KEY_TAP:
				//Handle key tap gestures
				break;
			case Gesture.GestureType.TYPE_SCREEN_TAP:
				//Handle screen tap gestures

				//print ("screen tap");
				//screenTapGesture = new ScreenTapGesture(frame.Gestures()[g]);
				//screenTapDirection = screenTapGesture.Direction;


				extendedFingers = 0;
				for (int f = 0; f < hand.Fingers.Count; f++){
					finger = hand.Fingers[f];

					if(finger.IsExtended) {
						extendedFingers++;
					}
				}

				//print("number of finger:" + extendedFingers);
				/*

				handSpeed = hand.PalmVelocity;
				if(handSpeed.z > 0){
					print ("in");
				}
				else if(handSpeed.z < 0){
					print ("out");
				}
				*/

				break;

			case Gesture.GestureType.TYPE_SWIPE:
				//Handle swipe gestures
				
				swipeGesture = new SwipeGesture(frame.Gestures()[g]);
				swipeDirection = swipeGesture.Direction;

				if(hand.IsRight){


					if(Mathf.Abs(swipeDirection.x) > Mathf.Abs(swipeDirection.y)){

						if(swipeDirection.x > 0){
						//	print ("right");
							//cam.transform.RotateAround(cubeObject.transform.position,cam.transform.up,15*pointable.Direction.x);
							cam.transform.RotateAround(cubeObject.transform.position,cam.transform.up,1.5f*swipeDirection.x);
						}
						else if(swipeDirection.x < 0){
						//	print ("left");
							//cam.transform.RotateAround(cubeObject.transform.position,cam.transform.up,5*pointable.Direction.x);
							cam.transform.RotateAround(cubeObject.transform.position,cam.transform.up,swipeDirection.x);
						}
					}
					else{
					//	if(swipeDirection.y > 0){
						//	print ("up");
					//		cam.transform.RotateAround(cubeObject.transform.position,cam.transform.right,pointable.Direction.y);
					//	}
					//	else if(swipeDirection.y < 0){
						//	print ("down");
							cam.transform.RotateAround(cubeObject.transform.position,cam.transform.right,pointable.Direction.y);
						//}
					}
				}

				break;
			default:
				//Handle unrecognized gestures


				break;
			}
		}


		GrabStrength = hand.GrabStrength;
		//touchZone = handsInFrame.Rightmost

		// Move cubeObject
		if (operations.opMode == 4) Move ();
		

		//Draw cubeObject
		else {
			if( touchZone == Pointable.Zone.ZONE_TOUCHING && handsInFrame.Leftmost.GrabStrength == 1){

				RaycastHit hit = new RaycastHit();
				//pointable = frame.Pointables.Frontmost;
				//Vector3 point = Camera.main.WorldToScreenPoint (GameObject.Find ("SkeletalRightRobotHand(Clone)").transform.FindChild ("index").FindChild ("bone3").position);
				Vector3 point = GameObject.Find ("SkeletalRightRobotHand(Clone)").transform.FindChild ("index").FindChild ("bone3").position;

				/*if (operations.opMode<=1 && operations.cubeArray==null) {
					operations.AddPoint ();
					if(menu.mainMenu.activeSelf) menu.ShowMainMenu(false);
				}*/

				if (Physics.Raycast( point + cam.transform.forward, (point - cam.transform.position).normalized, out hit)) {
					/*if (operations.opMode ==2 || operations.opMode == 3 || operations.opMode == 5) {
						if (!operations.pushed) {
							PushTo (undoStack);
							redoStack.Clear ();
							pushed = true;
						}
					}
*/
					if(operations.opMode == 2) operations.Attach (hit);
					else if(operations.opMode == 3) operations.Remove (hit);
					else if(operations.opMode == 5) operations.Paint (hit);
				}
			}
			else{

				//if(operations.opMode==2) operations.newlyAttached = new List<Vector3>();
			}
		
		}




	}
}
