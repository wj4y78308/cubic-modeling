using UnityEngine;
using System.Collections;
using Leap;

public class CubeCreater : MonoBehaviour {
	public GameObject grabbedCube;
	public GameObject cube;
	public GameObject cubePos;
	//public GameObject barrels;

	//Menu menu;
	Operations operations;
	static bool isHolding;

	const int NUM_FINGERS = 5;
	const int NUM_JOINTS = 4;
	const int HAND_LAYER_INDEX = 11;
	const float PINCH_DISTANCE = 30.0f;
	const float SPRING_CONSTANT = 30f;
	const float THUMB_TRIGGER_DISTANCE = 0.035f;
	//Vector3 cubePos = new Vector3(0.02f,0,1.09f);

	private Vector2 pinch_position;
	public static bool isPinching;
	private GameObject grabbed_;
	//private int layer_mask_;

	// Use this for initialization
	void Start () {
//		menu = GameObject.Find("Scripts").GetComponent<Menu> ();
		operations = GameObject.Find ("Scripts").GetComponent<Operations> ();
		//this.transform.localScale = new Vector3 (0.385f, 0.385f, 0.385f);
		isPinching = false;
		grabbed_ = null;
	//	layer_mask_ = 1 << HAND_LAYER_INDEX;
	//	layer_mask_ = ~layer_mask_;
	}
	
	// Update is called once per frame
	void Update () {
		grabbedCube.GetComponent<Renderer> ().material.color = cube.GetComponent<Renderer> ().material.color;
		if (operations.opMode == 2) {
			bool trigger_pinch = false;
			//Hand hand = GetComponent<HandModel>().GetLeapHand();
			Hand hand = operations.hand;
			// Thumb tip is the pinch position.
			Vector3 thumb_tip = hand.Fingers [0].TipPosition.ToUnityScaled ();
		
			// Check thumb tip distance to joints on all other fingers.
			// If it's close enough, start pinching.
			for (int i = 1; i < NUM_FINGERS && !trigger_pinch; ++i) {
				Finger finger = hand.Fingers [i];
			
				for (int j = 0; j < NUM_JOINTS && !trigger_pinch; ++j) {
					Vector3 joint_position = finger.JointPosition ((Finger.FingerJoint)(j)).ToUnityScaled ();
					Vector3 distance = thumb_tip - joint_position;
					if (distance.magnitude < THUMB_TRIGGER_DISTANCE)
						trigger_pinch = true;
				}
			}
		
			//Vector3 pinch_position = transform.TransformPoint(thumb_tip);
			GameObject thumb = GameObject.Find ("SkeletalRightRobotHand(Clone)/thumb/bone3");
			if (thumb != null) {
				//Vector3 pinch = transform.TransformPoint (thumb_tip);


				pinch_position = Camera.main.WorldToScreenPoint (thumb.transform.position);
				// Only change state if it's different.
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
				//Instantiate(cube,cubePos.transform.position,Quaternion.identity);
				cube.SetActive(true);
				grabbedCube.SetActive(false);
				//this.transform.localScale = new Vector3 (0.385f, 0.385f, 0.385f);
				//Destroy(this.gameObject);

			}
		}
	}

	void OnPinch(Vector2 pinch_position) {
		//Debug.Log("PINCH");
		isPinching = true;

		// Check if we pinched a live human.
		//GameObject human = GameObject.Find("HumanGame");
		/*if (human != null && (human.transform.position - pinch_position).magnitude < PINCH_DISTANCE) {
			GameObject.Find("HumanGame").GetComponent<RagdollInstantiator>().Die();
			Debug.Log("DEAD");
		}*/
		/*if (cube != null && (cube.transform.position - pinch_position).magnitude < PINCH_DISTANCE) {
		
		}
		*/
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
		//grabbedCube = (GameObject)Instantiate(operations.attachCube,cubePos,Quaternion.identity);
		if (isGrabbed) {
			grabbed_ = grabbedCube;
			grabbedCube.SetActive(true);
			cube.SetActive(false);
			//grabbed_ = grabbedCube;
		}
	}
	
	void OnRelease() {
		//Debug.Log("RELEASE");
		grabbed_ = null;
		isPinching = false;

		RaycastHit hit = new RaycastHit();
		//if (Physics.Raycast(point + cam.transform.forward, (point - cam.transform.position).normalized, out hit))
		Vector3 point = operations.attachCube.transform.position;

		//if(Physics.Raycast( Camera.main.ScreenPointToRay(new Vector3(point.x,point.y, 0)),out hit)){
		if (Physics.Raycast (point + Camera.main.transform.forward, (point - Camera.main.transform.position).normalized, out hit) && hit.transform.name == "CubeObject(Clone)") {
			//print (hit.transform.name);
			operations.Attach (hit);
			//Instantiate(cube,cubePos.transform.position,Quaternion.identity);
			//this.transform.localScale = new Vector3 (0.385f, 0.385f, 0.385f);
			//Destroy(this.gameObject);
			cube.SetActive(true);
			grabbedCube.SetActive(false);
			//Instantiate(operations.attachCube,cubePos,Quaternion.identity);
		} else {
			cube.SetActive(true);
			grabbedCube.SetActive(false);
		}
		//operations.attachCube.transform.position = cubePos;

	}
	
	bool Collision(Vector2 pinch){ //2D screen collision
		float minX = pinch.x - PINCH_DISTANCE/2 , minY = pinch.y - PINCH_DISTANCE/2;
		Vector2 cubeScreenPos = Camera.main.WorldToScreenPoint(cube.transform.position);
		if(cubeScreenPos.x > minX && cubeScreenPos.y > minY && (cubeScreenPos.x < (minX+PINCH_DISTANCE)) && (cubeScreenPos.y < (minY+PINCH_DISTANCE)))
			return true;
		return false;
	}


}
