using UnityEngine;
using System.Collections;
using Leap;

public class CubeCreater : MonoBehaviour {
	public GameObject grabbedCube;
	public GameObject cube;
	//public GameObject cubePos;
	public GameObject attachCube;
	//public GameObject s1;
	//public GameObject s2;
	Hand hand ;
	Operations operations;
	Menu menu;


	static RaycastHit hit= new RaycastHit();
	const int NUM_FINGERS = 5;
	const int NUM_JOINTS = 4;
	const int HAND_LAYER_INDEX = 11;
	const float PINCH_DISTANCE = 35.0f;
//	const float SPRING_CONSTANT = 30f;
	const float THUMB_TRIGGER_DISTANCE = 0.04f;

	private Vector2 pinch_position;
	private bool isPinching;
	private GameObject grabbed_;
	private bool isAttach = false;
	//private Vector3 lastPos = new Vector3(0,0,0);
	// Use this for initialization
	void Start () {
		operations = GameObject.Find ("Scripts").GetComponent<Operations> ();
		menu = GameObject.Find ("Scripts").GetComponent<Menu> ();
		isPinching = false;
		grabbed_ = null;
	}
	
	// Update is called once per frame
	void Update () {
		grabbedCube.GetComponent<Renderer> ().material.color = cube.GetComponent<Renderer> ().material.color;
		if (operations.opMode == 6) {
			bool trigger_pinch = false;
			hand = operations.hand;

			//pinch position.
			Vector3 thumb_tip = hand.Fingers [0].TipPosition.ToUnityScaled ();
		
			// Check thumb tip distance to joints on all other fingers.
			// If it's close enough, start pinching.

			for (int i = 1 ; i < NUM_FINGERS && !trigger_pinch ; ++i) {
				Finger finger = hand.Fingers [i];
			
				for (int j = 0 ; j < NUM_JOINTS && !trigger_pinch ; ++j) {
					Vector3 joint_position = finger.JointPosition ((Finger.FingerJoint)(j)).ToUnityScaled ();
					Vector3 distance = thumb_tip - joint_position;
					if (distance.magnitude < THUMB_TRIGGER_DISTANCE)
						trigger_pinch = true;
				}
			}

		
			//Vector3 pinch_position = transform.TransformPoint(thumb_tip);
			GameObject thumb = GameObject.Find ("CleanRobotFullRightHand(Clone)/thumb/bone3");
			if (thumb != null) {
				//Vector3 pinch = transform.TransformPoint (thumb_tip);

				//else attachCube.SetActive (false);
				if (hand.PalmVelocity.x * hand.PalmVelocity.x + hand.PalmVelocity.y * hand.PalmVelocity.y + hand.PalmVelocity.z * hand.PalmVelocity.z > 700) {
					DisplayAttachCube ();
				}
				pinch_position = Camera.main.WorldToScreenPoint (thumb.transform.position);
			
				if (trigger_pinch && !isPinching)
					OnPinch (pinch_position);
				else if (!trigger_pinch && isPinching)
					OnRelease ();	
		
				// Accelerate what we are grabbing toward the pinch.
				if (grabbed_ != null) {

					//Vector3 distance = pinch - grabbed_.transform.position;
					//grabbed_.GetComponent<Rigidbody> ().AddForce (SPRING_CONSTANT * distance);
					//grabbed_.GetComponent<Rigidbody>().AddForceAtPosition(SPRING_CONSTANT * distance,thumb.transform.position);
					grabbedCube.transform.position = new Vector3 (thumb.transform.position.x, thumb.transform.position.y - 0.07f, thumb.transform.position.z);

					//print (thumb.transform.position);
					//print (grabbedCube.transform.position);
				//	grabbedCube.transform.localScale = new Vector3 (0.007f, 0.007f, 0.007f);
					//grabbed_.GetComponent<Rigidbody>().AddForceAtPosition(SPRING_CONSTANT * distance,thumb.transform.position);
				
				}
				else {

				}
			}

			if(cube.transform.position.y <= -8 || cube.transform.position.z <= -20 || cube.transform.position.z >= 20 || cube.transform.position.x <= -70 ||cube.transform.position.x >= 60 ){
				cube.SetActive(true);
				grabbedCube.SetActive(false);

			}
			if (grabbedCube.transform.position.y <= -10) {
				cube.SetActive (true);
				grabbedCube.SetActive (false);
			}
		}
	}
	public bool isPinch(){
		return isPinching;
	}

	void OnPinch(Vector2 pinch_position) {
		//Debug.Log("PINCH");
		isPinching = true;

		// Check if we pinched a movable object and grab the closest one.
		//Collider[] close_things = Physics.OverlapSphere(pinch_position, PINCH_DISTANCE, layer_mask_);
//		Collider2D[] close_things = Physics2D.OverlapCircleAll (pinch_position, PINCH_DISTANCE);
		bool isGrabbed = Collision (pinch_position);

	/*	Vector3 distance = new Vector2(PINCH_DISTANCE, 0.0f);
		for (int j = 0; j < close_things.Length; ++j) {
			//if(close_things[j].name == "Cube"){
			Vector2 new_distance = (Vector2)pinch_position - (Vector2)close_things[j].transform.position;
			if (close_things[j].GetComponent<Rigidbody>() != null && new_distance.magnitude < distance.magnitude) {
				grabbed_ = close_things[j];
				print (grabbed_);
				distance = new_distance;
				}
			//}
		}*/

		if (isGrabbed) {
			grabbed_ = grabbedCube;
			grabbedCube.SetActive(true);
			cube.SetActive(false);
		}

	}
	
	void OnRelease() {
		//Debug.Log("RELEASE");


		//if (Physics.Raycast(point + cam.transform.forward, (point - cam.transform.position).normalized, out hit))
		//Vector3 point = grabbedCube.transform.position;
		//RaycastHit hit = new RaycastHit();
		//if(Physics.Raycast( Camera.main.ScreenPointToRay(new Vector3(point.x,point.y, 0)),out hit)){
		//if (Physics.Raycast (point + Camera.main.transform.forward, (point - Camera.main.transform.position).normalized, out hit) && hit.transform.name == "CubeObject(Clone)") {

		if (isAttach) {
			StartCoroutine (attach ());
		}
		cube.SetActive (true);
		grabbedCube.SetActive (false);
		attachCube.SetActive (false);

		grabbed_ = null;
		isPinching = false;
	}
	
	bool Collision(Vector2 pinch){ //2D screen collision
		float minX = pinch.x - PINCH_DISTANCE/2 , minY = pinch.y - PINCH_DISTANCE/2;
		Vector2 cubeScreenPos = Camera.main.WorldToScreenPoint(cube.transform.position);
		if(cubeScreenPos.x > minX && cubeScreenPos.y > minY && 
			(cubeScreenPos.x < (minX+PINCH_DISTANCE)) && (cubeScreenPos.y < (minY+PINCH_DISTANCE)))
			return true;
		return false;
	}

	void ChangeAttachCubeColor(){
		Transform[] allchildren = attachCube.GetComponentsInChildren<Transform> ();
		if (attachCube.activeSelf) {
			foreach (Transform child in allchildren) {
				child.GetComponent<Renderer> ().material.color = menu.GetPickedColor();
			}
		}
	}
	public void DisplayAttachCube(){
		Vector3 point = grabbedCube.transform.position;

		GameObject wrist = GameObject.Find ("CleanRobotFullRightHand(Clone)/wrist joint");
		GameObject palm = GameObject.Find ("CleanRobotFullRightHand(Clone)/palm");

		Vector3 offset = new Vector3(0.003f , 0.018f , 0f);
	//	s1.transform.position = wrist.transform.position;
	//	s2.transform.position = palm.transform.position-offset;
		Vector3 front = (palm.transform.position-offset - wrist.transform.position) .normalized;

		if (Physics.Raycast (point + front, front, out hit) && hit.transform.name == "CubeObject(Clone)") {
			attachCube.SetActive (true);
			ChangeAttachCubeColor ();
			Vector3 pos = hit.point + hit.normal * 0.5f - hit.transform.position;
			pos = new Vector3 (Mathf.RoundToInt (pos.x), Mathf.RoundToInt (pos.y), Mathf.RoundToInt (pos.z));
			attachCube.transform.position = pos;

		
			isAttach = true;
			//print (hand.PalmVelocity);

		} else {
			isAttach = false;
			attachCube.SetActive (false);
		}


	}
	IEnumerator attach(){
		

		//if(hit != null && hit.transform.name == "CubeObject(Clone)"){
			operations.Attach (hit);

			//cube.SetActive (true);
			//grabbedCube.SetActive (false);
			//print (hit.transform.name);

		//} 
		isAttach = false;
		yield return new WaitForSeconds (1);
	}

}
