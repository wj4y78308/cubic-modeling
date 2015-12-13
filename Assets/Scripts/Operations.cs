using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System.Runtime.Serialization.Formatters.Binary; 
using System.IO;
using Leap;
//using Hover.Cast.State;
using Hover.Cast.Items;
using Hover.Cast;
using Hover.Demo.CastCubes.Items;

public class Operations : MonoBehaviour {

	public Controller controller;
	public Frame frame;
	public Hand hand;
	public Finger finger;
	public Pointable pointable;
	public Pointable.Zone touchZone;
	public HandList handsInFrame;
	SwipeGesture swipeGesture;
	ScreenTapGesture screenTapGesture;
    Vector currentPosition;
    
    HovercastSetup menuState;

	public GameObject attach;
    public GameObject mainCast , loadCast;
	//public GameObject attachCube;

	float GrabStrength = 0.0f;
	Vector handSpeed;
	Vector swipeDirection;
	Vector screenTapDirection;
    bool isTouchedScreen = false;
    float isMoveScreen = 1.5f , isSetThickness = 1.5f ;
    GameObject menuActive;
	GameObject Index;

    GameObject thumb1;
    GameObject thumb2;
    GameObject thumb3;
    public GameObject handController;

    public int opMode = 0;
	public UnityEngine.UI.Image paper;
	public Camera cam;
	public GameObject cubeObject;
	public List<GameObject> cubeObjects = new List<GameObject> ();
	public GameObject startCube;
	public LineRenderer lineRenderer;
	public GameObject grid;
	public GameObject gridLine;
	public List<SaveData> saveList;
	public bool isLoading;
	Menu menu;
	CubeCreater cubeCreater;

	float thinningScale = 0.1f;
	float thinningProgressRatio = 1.7f;

	Texture2D tex;
	List<Vector2> pointArray = new List<Vector2>();
	
	public static CubeMesh[] cubeArray,cubeArraySym;
	CubeMesh selectedMesh;

	Vector2 prev = new Vector2();
	bool clickedMenu;
	List<Vector3> newlyAttached = new List<Vector3>();

	List<List<MeshData>> undoStack = new List<List<MeshData>> ();
	List<List<MeshData>> redoStack = new List<List<MeshData>> ();
	bool pushed;




	// Use this for initialization
	void Start () {
		menu = GetComponent<Menu> ();
		cubeCreater = GameObject.Find("barrel").GetComponent<CubeCreater> ();
		lineRenderer = GetComponent<LineRenderer> ();
		menuState = GameObject.Find("Cast").GetComponent<HovercastSetup> ();
        
        SetUpTexture ();
		Load ();
        
        menu.ShowLoadMenu(false);
        menu.ShowSaveDialog(false);
        controller = new Controller();
		
		controller.EnableGesture(Gesture.GestureType.TYPE_SWIPE);
		
		controller.Config.SetFloat("Gesture.Swipe.MinLength", 120.0f);
		controller.Config.SetFloat("Gesture.Swipe.MinVelocity", 650.0f);
		controller.Config.SetFloat("Gesture.Swipe.HistorySeconds", 10.0f);
		controller.Config.Save();
        
    }
	
	// Update is called once per frame
	void Update () {

        
		int touchCount = Input.touchCount;
		if(touchCount <= 1){

			if (Input.GetMouseButtonDown (0)) {
				if(touchCount==0? EventSystem.current.IsPointerOverGameObject():EventSystem.current.IsPointerOverGameObject(Input.touches[0].fingerId)) clickedMenu = true;
				//else if(opMode<1) menu.ShowMenu(false);
			}
			if (Input.GetMouseButton (0) && !isLoading) {
				if(clickedMenu) return;
				Vector2 curr = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
				if(prev!=curr){
					RaycastHit hit = new RaycastHit();
					if (opMode<=1 && cubeArray==null) {
						AddPoint ();
						if(menu.mainMenu.activeSelf) menu.ShowMainMenu(false);
                        
					}
					else if (Physics.Raycast( cam.ScreenPointToRay(new Vector3(curr.x, curr.y, 0)),out hit)) {
						if (opMode ==2 || opMode == 3 || opMode == 5) {
							if (!pushed) {
								PushTo (undoStack);
								redoStack.Clear ();
								pushed = true;
							}
						}

						if(opMode == 2) Attach (hit);
						else if(opMode == 3) Remove (hit);
						else if(opMode == 4 && selectedMesh==null) selectedMesh = hit.transform.GetComponent<CubeMesh>();
						else if(opMode == 5) Paint (hit);
					}

					if (opMode==4 && prev!=Vector2.zero && selectedMesh!=null) {
						float camDist = Vector3.Distance(cam.transform.position, Vector3.zero);
						Vector3 transVec = cam.ScreenToWorldPoint(new Vector3(curr.x, curr.y, camDist)) - cam.ScreenToWorldPoint(new Vector3(prev.x, prev.y, camDist));
						transVec = new Vector3 ((int)transVec.x, (int)transVec.y, (int)transVec.z);
						if (transVec==Vector3.zero) return;
						if (!pushed) {
							PushTo (undoStack);
							redoStack.Clear ();
							pushed = true;
						}
						if (cubeArray!=null && selectedMesh == cubeArray[(int)menu.slider.value-1]) {
							foreach(CubeMesh ca in cubeArray) {
								ca.transform.Translate(transVec);
								if(ca.symmetry!=null) ca.symmetry.transform.position = SymmetricVector(ca.transform.position);
							}
						}
						else if (cubeArraySym!=null && selectedMesh == cubeArraySym[(int)menu.slider.value-1]) {
							foreach(CubeMesh cas in cubeArraySym) {
								cas.transform.Translate(transVec);
								cas.symmetry.transform.position = SymmetricVector(cas.transform.position);
							}
						}
						else {
							selectedMesh.transform.Translate(transVec);
							if(selectedMesh.symmetry) selectedMesh.symmetry.transform.position = SymmetricVector(selectedMesh.transform.position);
						}
						UpdateGrid ();
					}

					prev = curr;
				}
			}
			if (Input.GetMouseButtonUp (0)) { //按完左鍵
				if(clickedMenu) {
					clickedMenu = false;
					return;
				}
				prev = Vector2.zero;
				pushed = false;
				if(isLoading) return;
				if(opMode<2) {
					EndPoint();
					menu.ShowMainMenu(true);
				}
				else if(opMode==2) newlyAttached = new List<Vector3>();
				else if(opMode==4) selectedMesh = null;
			}

			if (Input.GetAxis ("Mouse ScrollWheel")!=0) {
				float newDist = Vector3.Distance(cam.transform.position,Vector3.zero) - Input.GetAxis ("Mouse ScrollWheel") * 20;
				if(newDist >= 15 && newDist <= 25){
					cam.transform.Translate(Vector3.forward * Input.GetAxis ("Mouse ScrollWheel") * 20);
				}
			}

			if(Input.GetMouseButton (1)){ //按右鍵 旋轉
				if(menu.mainMenu.activeSelf) menu.ShowMainMenu(false);
				Vector2 curr = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
				if(prev!=curr){
					if(prev != Vector2.zero){
						float dist = Vector3.Distance(cam.transform.position, Vector3.zero);
						float angle = Vector3.Angle(cam.ScreenToWorldPoint(new Vector3(curr.x, curr.y,dist))-cam.transform.position, cam.ScreenToWorldPoint(new Vector3(prev.x,prev.y,dist))-cam.transform.position)*3;
						Vector3 axis = Vector3.Cross(cam.transform.forward,(cam.ScreenToWorldPoint(new Vector3(curr.x, curr.y,cam.nearClipPlane)) - cam.ScreenToWorldPoint(new Vector3(prev.x,prev.y,cam.nearClipPlane))).normalized);
						cam.transform.RotateAround(Vector3.zero,axis,angle);
					}
					prev = curr;
				}
			}

			if (Input.GetMouseButtonUp (1)) {
				prev = Vector2.zero;
				if(!isLoading) menu.ShowMainMenu(true);
			}
		}


		else if(touchCount == 2){
			if(opMode<2) lineRenderer.SetVertexCount (0);

			Touch[] touches = Input.touches;
			if (touches[0].phase!=TouchPhase.Ended && touches[1].phase!=TouchPhase.Ended) {
				if(menu.mainMenu.activeSelf) menu.ShowMainMenu(false);
				float num = ((touches[0].position-touches[0].deltaPosition)-(touches[1].position-touches[1].deltaPosition)).magnitude - (touches[0].position-touches[1].position).magnitude;
				float newDist = Vector3.Distance(cam.transform.position,Vector3.zero) + num * 0.1f;
				if(newDist >= 15 && newDist <= 25){
					cam.transform.Translate(Vector3.back * num * 0.1f);
				}

				float angle = Vector2.Angle((touches[0].position-touches[0].deltaPosition)-(touches[1].position-touches[1].deltaPosition), touches[0].position-touches[1].position)*5;
				Vector3 axis = Vector3.Cross((touches[0].position-touches[0].deltaPosition)-(touches[1].position-touches[1].deltaPosition), touches[0].position-touches[1].position).normalized.z * cam.transform.position;
				cam.transform.RotateAround(Vector3.zero,axis,angle);
			}
			else {
				prev = Vector2.zero;
				if(clickedMenu) clickedMenu = false;

				if (isLoading) return;
				menu.ShowMainMenu(true);
				if(opMode<2) pointArray.Clear ();
				else if(opMode==2) newlyAttached = new List<Vector3>();
				else if(opMode==4) selectedMesh = null;
			}
		}


		else if(touchCount == 3){
			if(opMode<2) lineRenderer.SetVertexCount (0);

			foreach (Touch touch in Input.touches) {
				if (touch.fingerId == 0 && (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Moved)) {
					if(menu.mainMenu.activeSelf) menu.ShowMainMenu(false);
					Vector2 curr = new Vector2(touch.position.x, touch.position.y);
					if(prev!=curr){
						if(prev != Vector2.zero){
							float dist = Vector3.Distance(cam.transform.position, Vector3.zero);
							float angle = Vector3.Angle(cam.ScreenToWorldPoint(new Vector3(curr.x, curr.y,dist))-cam.transform.position, cam.ScreenToWorldPoint(new Vector3(prev.x,prev.y,dist))-cam.transform.position)*3;								Vector3 axis = Vector3.Cross(cam.transform.forward,(cam.ScreenToWorldPoint(new Vector3(curr.x, curr.y,cam.nearClipPlane)) - cam.ScreenToWorldPoint(new Vector3(prev.x,prev.y,cam.nearClipPlane))).normalized);
							cam.transform.RotateAround(Vector3.zero,axis,angle);
						}
						prev = curr;
					}
				}
				if(touch.phase == TouchPhase.Ended) {
					prev = Vector2.zero;
					if(clickedMenu) clickedMenu = false;

					if (isLoading) return;
					menu.ShowMainMenu(true);
					if(opMode<2) pointArray.Clear ();
					else if(opMode==2) newlyAttached = new List<Vector3>();
					else if(opMode==4) selectedMesh = null;
				}
			}
		}
	
        ////////////////////////////////////////////////////////////////////////////

		frame = controller.Frame();
		handsInFrame = frame.Hands;
		pointable = frame.Pointables.Frontmost;
		touchZone = pointable.TouchZone;
		hand = pointable.Hand;

		Index = GameObject.Find("CleanRobotFullRightHand(Clone)/index/bone3");

        if (menu.operButton.gameObject.activeSelf)
        {
            mainCast.SetActive(true);
            loadCast.SetActive(false);
           

            if (menuState.State == null) print("null");
            else{
               
                menuActive = GameObject.Find("Main Camera/Cast/Menu/Arc/CurrLevel");

                if (!menuActive.activeSelf)
                {                

                    if (hand.IsValid)
                    {
                        //if(frame != null){

                        for (int g = 0; g < frame.Gestures().Count; g++)
                        {
                            if (frame.Gestures()[g].Type == Gesture.GestureType.TYPE_SWIPE)
                            {
                                swipeGesture = new SwipeGesture(frame.Gestures()[g]);
                                swipeDirection = swipeGesture.Direction;
                                int dir = Swipe();

                                if (dir == 1)
                                {
                                    cam.transform.RotateAround(cubeObject.transform.position, cam.transform.up, 1.5f*swipeDirection.x);
                                }
                                else if (dir == 2)
                                {
                                    cam.transform.RotateAround(cubeObject.transform.position, cam.transform.up, swipeDirection.x);
                                }

                                else if (dir == 3 || dir == 4)
                                {
                                    cam.transform.RotateAround(cubeObject.transform.position, -cam.transform.right, swipeDirection.x);
                                }
                            }
                        }

                        // Move cubeObject
                        if (opMode == 4)
                        {
                            if (!pushed)
                            {
                                PushTo(undoStack);
                                redoStack.Clear();
                                pushed = true;
                            }
                            Move();
                            UpdateGrid();
                        }
                        //Draw cubeObject
                        else
                        {
                            currentPosition = pointable.TipPosition;
                            //if( touchZone == Pointable.Zone.ZONE_TOUCHING && handsInFrame.Leftmost.GrabStrength == 1){
                            //print(currentPosition);
                            if (currentPosition.z - cam.transform.position.z <= -15)
                            {
                                RaycastHit hit = new RaycastHit();
                                
                                //if ((index = GameObject.Find("SkeletalRightRobotHand(Clone)/index/bone3")) != null)
								if(Index != null)
                                {
                                    //if ((finger = GameObject.Find ("SkeletalRightRobotHand(Clone)").transform.FindChild ("index").FindChild ("bone3").gameObject) != null){					
									Vector3 point = Index.transform.position;

                                    //point = GameObject.Find ("SkeletalRightRobotHand(Clone)").transform.FindChild ("index").FindChild ("bone3").position;
                                    //if(cubeArray != null)
                                    //print (cubeArray.Length);

                                    if (opMode <= 1 && cubeArray == null)
                                    {

										if( Physics.Raycast(point + cam.transform.forward, (point - cam.transform.position).normalized, out hit) &&
											(hit.transform.name == "Background" || hit.transform.name == "CubeObject(Clone)") ){
											//print ("hit");
                                        	AddPointLeap();
										}
                                        //if (menu.mainMenu.activeSelf)
                                        //	menu.ShowMainMenu (false);
                                    }
                                    //else if (Physics.Raycast( cam.ScreenPointToRay(new Vector3(point.x, point.y, 0)),out hit)) {
									else if (Physics.Raycast(point + cam.transform.forward, (point - cam.transform.position).normalized, out hit) && hit.transform.name == "CubeObject(Clone)"){

                                        if (opMode == 2 || opMode == 3 || opMode == 5)
                                        {
                                            if (!pushed)
                                            {
                                                PushTo(undoStack);
                                                redoStack.Clear();
                                                pushed = true;
                                            }
                                        }
										if (opMode == 2 && !cubeCreater.isPinch())
                                            Attach(hit);
                                        else if (opMode == 3)
                                            Remove(hit);
                                        else if (opMode == 5)
                                            Paint(hit);
                                    }
                                }
                                isTouchedScreen = true;
                            }

                            //else if( touchZone == Pointable.Zone.ZONE_HOVERING && isTouchedScreen == true && handsInFrame.Leftmost.GrabStrength <= 0.5){ // touching to hovering
                            else if (currentPosition.z - cam.transform.position.z > -15 && isTouchedScreen == true)
                            { // touching to hovering
                                isTouchedScreen = false;


                                if (clickedMenu)
                                {
                                    clickedMenu = false;
                                    return;
                                }
                                pushed = false;
                                if (isLoading)
                                    return;
                                if (opMode < 2)
                                {
                                    EndPoint();
                                }
                                else if (opMode == 2)
                                    newlyAttached = new List<Vector3>();
                                if (attach.GetComponent<OperationsListener>().enabled == false)
                                    attach.GetComponent<OperationsListener>().enabled = true;
                            }

                            if (menu.sliderPanel.gameObject.activeSelf)
                            {                      
                                ThumbDown();
                            }
                        }
                    }
                }
            }

        }
        else //load menu
        {

            mainCast.SetActive(false);
            loadCast.SetActive(true);


            if (hand.IsRight)
            {
                FingerList indexList = hand.Fingers.FingerType(Finger.FingerType.TYPE_INDEX);
                Finger index = indexList[0];
                FingerList middleList = hand.Fingers.FingerType(Finger.FingerType.TYPE_MIDDLE);
                Finger middle = middleList[0];

                if ((index.Direction.x >= 0.4f && middle.Direction.x >= 0.4f) || (index.Direction.x <= -0.4f && middle.Direction.x <= -0.4f)) // 食指中指方向
                    isMoveScreen -= Time.deltaTime * Mathf.Abs(index.Direction.x * 1.8f) * 3.0f;

                if (isMoveScreen <= 0)
                {
                    isMoveScreen = 1.5f;
                    if (index.Direction.x <= -0.4f && middle.Direction.x <= -0.4f) menu.SwitchLoadScreen(-1); //left
                    else if (index.Direction.x >= 0.4f && middle.Direction.x >= 0.4f) menu.SwitchLoadScreen(1); //right

                }
            }
            

        }
           
	}
    int Swipe() {
        
                if (!hand.IsRight)
                {
                    if (Mathf.Abs(swipeDirection.x) > Mathf.Abs(swipeDirection.y))
                    {
                        if (swipeDirection.x > 0) return 1; // left                
                        else if (swipeDirection.x < 0) return 2; //right
                    }
                    else
                    {
                        if (swipeDirection.y > 0) return 3; // up                
                        else if (swipeDirection.y < 0) return 4; //down
                    }
                }
     
        return 0;
    }

	void Move(){
		GrabStrength = hand.GrabStrength;
		
		if (GrabStrength == 1 && hand.IsRight) {
			handSpeed = hand.PalmVelocity;
            //cam.transform.Translate (-handSpeed.x / 10000.0f, -handSpeed.y / 10000.0f, handSpeed.z / 10000.0f);
            //handController.transform.Translate(-handSpeed.x / 10000.0f, -handSpeed.y / 10000.0f, handSpeed.z / 10000.0f);
          
            foreach (GameObject co in cubeObjects)
            {
                co.transform.Translate(-handSpeed.x / 1000.0f, -handSpeed.y / 1000.0f, handSpeed.z / 1000.0f);
            }
                
        }
	}

    float ThumbProduct() {
		thumb1 = GameObject.Find("CleanRobotFullRightHand(Clone)/thumb/joint1");
		thumb2 = GameObject.Find("CleanRobotFullRightHand(Clone)/thumb/joint4");
		thumb3 = GameObject.Find("CleanRobotFullRightHand(Clone)/index/joint4");
        if(thumb1!=null && thumb2!=null && thumb3!=null){
        	Vector3 v1 = thumb2.transform.position - thumb1.transform.position;
       		Vector3 v2 = thumb3.transform.position - thumb1.transform.position;
        	return Vector3.Dot(v1, v2);
        }
     
        return 0;      
    }

    void ThumbDown(){
        float dotProduct = ThumbProduct();
        
       	if(hand.IsRight){
	        if (dotProduct >= 0.01f /*&& !isThumbDown*/){
	            
	            if (hand.PalmNormal.y > -0.4f){
	                //isThumbDown = true;
	                isSetThickness+=Time.deltaTime*1.5f;
	                ThumbChangeThickness();
	            }
	            else if (hand.PalmNormal.y < -0.7f){
	                //isThumbDown = true;
	                isSetThickness-=Time.deltaTime*1.5f;
	                ThumbChangeThickness();
	            }
	        }
	        /*else if (dotProduct <= 0.0065f){
	            //isThumbDown = false;
	        }*/
	    }
        //return 0;
    }

	void ThumbChangeThickness(){
        if(isSetThickness <= 0.0f){
        	menu.slider.value -= 1;
            ChangeThickness();
            isSetThickness = 1.5f;
        }
        else if(isSetThickness >= 3.0f){
 			menu.slider.value += 1;
            ChangeThickness();
			isSetThickness = 1.5f;
        }

       /* if (hand.IsRight){
        	

           int flag = ThumbDown();
            if (flag == 1) //thumb up
            {
                menu.slider.value += 1;
                ChangeThickness();
            }
            else if (flag == 2) //thumb down
            {
                menu.slider.value -= 1;
                ChangeThickness();
            }

        }*/
	}

	public void AddPoint () {

		pointArray.Add (new Vector2 ((int)Input.mousePosition.x, (int)Input.mousePosition.y));
		lineRenderer.SetVertexCount(pointArray.Count);
		lineRenderer.SetPosition(pointArray.Count-1, cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x,Input.mousePosition.y,1.0f))); 
	}
	public void AddPointLeap () {

		if (attach.GetComponent<OperationsListener>().enabled == true)
			attach.GetComponent<OperationsListener>().enabled = false;
        
		if (Index != null){
			
			Vector3 point = Camera.main.WorldToScreenPoint(Index.transform.position);
			pointArray.Add (new Vector2 ((int)point.x, (int)point.y));
		
			lineRenderer.SetVertexCount (pointArray.Count);
			lineRenderer.SetPosition (pointArray.Count - 1, cam.ScreenToWorldPoint (new Vector3 (point.x, point.y, 1.0f))); 
		}
	}

	public void EndPoint () {
		if(pointArray.Count > 2){
			lineRenderer.SetVertexCount(pointArray.Count);
			lineRenderer.SetPosition(pointArray.Count-1, cam.ScreenToWorldPoint(new Vector3(pointArray [0].x,pointArray [0].y,1.0f)));
			CV.ResizePointArray (pointArray, thinningScale);
			CV.FillPoly (tex, pointArray, Color.black);
			AddThinningProcessCubeArray();
			UpdateGrid();
			ClearTex ();
		}
		pointArray.Clear ();
		lineRenderer.SetVertexCount (0);
	}

	public void Clean () {
		PushTo (undoStack);
		redoStack.Clear ();

		if (cubeArray != null) {
			foreach (CubeMesh ca in cubeArray) Destroy (ca.gameObject);
			cubeArray = null;
		}
		if (cubeArraySym != null) {
			foreach (CubeMesh cas in cubeArraySym) Destroy (cas.gameObject);
			cubeArraySym = null;
		}

		foreach (GameObject co in cubeObjects) Destroy(co);
		cubeObjects.Clear ();
		UpdateGrid ();
		startCube.SetActive (true);
		menu.HideSlider ();
		menu.ShowSaveButton (false);
		Resources.UnloadUnusedAssets ();
	}

	public void ChangeThickness () {
		int value = (int)menu.slider.value;
		for (int i=0; i<cubeArray.Length; i++) {
			cubeArray [i].gameObject.SetActive (i==value-1);
			if(cubeArraySym!=null) cubeArraySym [i].gameObject.SetActive (i==value-1);
		}

		menu.SetSliderText ();
	}

	public void SetThickness () {
		int value = (int)menu.slider.value;
		for (int i = 0; i < cubeArray.Length; i++) {
			if(i!=value-1) {
				Destroy(cubeArray[i].gameObject);
				if(cubeArraySym!=null) Destroy(cubeArraySym[i].gameObject);
			}
			else {
				cubeObjects.Add(cubeArray[i].gameObject);
				if(cubeArraySym!=null) cubeObjects.Add(cubeArraySym[i].gameObject);
			}
		}
		cubeArray = null;
		cubeArraySym = null;
		menu.HideSlider ();
		menu.ShowSaveButton (true);
	}

	public void SetCamPos (int num) {
		if (num == 0) {
			cam.transform.position = new Vector3(0,25,0);
			cam.transform.rotation = Quaternion.Euler(new Vector3(90,0,0));
		}
		else if (num == 1) {
			cam.transform.position = new Vector3(0,0,25);
			cam.transform.rotation = Quaternion.Euler(new Vector3(0,180,0));
		}
		else if (num == 2) {
			cam.transform.position = new Vector3(25,0,0);
			cam.transform.rotation = Quaternion.Euler(new Vector3(0,-90,0));
		}
	}

	public void SetUpTexture () {
		tex = new Texture2D ((int)(UnityEngine.Screen.width*thinningScale), (int)(UnityEngine.Screen.height*thinningScale));
		for(int i=0; i<UnityEngine.Screen.width; i++){
			for(int j=0; j<UnityEngine.Screen.height; j++){
				tex.SetPixel(i,j,Color.clear);
			}
		}
		//tex.Apply();
		//paper.color = Color.white;
		//paper.sprite = Sprite.Create(tex,new Rect(0,0,Screen.width, Screen.height),new Vector2(Screen.width/2, Screen.height/2));
	}

	public void UpdateGrid () {
		for (int i=0; i<grid.transform.childCount; i++)
			Destroy (grid.transform.GetChild (i).gameObject);

		if (cubeObjects.Count == 0 && cubeArray == null)
			return;

		float minX=int.MaxValue, minY=int.MaxValue, maxX=int.MinValue, maxY=int.MinValue;
		foreach (GameObject co in cubeObjects) {
			CubeMesh cm = co.GetComponent<CubeMesh> ();
			Vector3 pos = cm.transform.position;
			if (cm.minPos.x-0.5f+pos.x < minX) minX = cm.minPos.x-0.5f+pos.x;
			if (cm.minPos.y-0.5f+pos.y < minY) minY = cm.minPos.y-0.5f+pos.y;
			if (cm.maxPos.x+0.5f+pos.x > maxX) maxX = cm.maxPos.x+0.5f+pos.x;
			if (cm.maxPos.y+0.5f+pos.y > maxY) maxY = cm.maxPos.y+0.5f+pos.y;
		}
		if (cubeArray != null) {
			CubeMesh cm = cubeArray[cubeArray.Length-1];
			Vector3 pos = cm.transform.position;
			if (cm.minPos.x-0.5f+pos.x < minX) minX = cm.minPos.x-0.5f+pos.x;
			if (cm.minPos.y-0.5f+pos.y < minY) minY = cm.minPos.y-0.5f+pos.y;
			if (cm.maxPos.x+0.5f+pos.x > maxX) maxX = cm.maxPos.x+0.5f+pos.x;
			if (cm.maxPos.y+0.5f+pos.y > maxY) maxY = cm.maxPos.y+0.5f+pos.y;
		}

		for (float x=minX; x<=maxX; x++) {
			LineRenderer lr = Instantiate (gridLine).GetComponent<LineRenderer> ();
			lr.transform.parent = grid.transform;
			lr.SetPosition(0,new Vector3(x,minY,0));
			lr.SetPosition(1,new Vector3(x,maxY,0));
			if (x==maxX) lr.material.color = Color.blue;
		}
		for (float y=minY; y<=maxY; y++) {
			LineRenderer lr = Instantiate (gridLine).GetComponent<LineRenderer> ();
			lr.transform.parent = grid.transform;
			lr.SetPosition(0,new Vector3(minX,y,0));
			lr.SetPosition(1,new Vector3(maxX,y,0));
			if (y==maxY) lr.material.color = Color.white;
		}
	}

	public void DeleteModel () {
		saveList.RemoveAt (menu.GetCurrLoadScreen());
		menu.SwitchLoadScreen (-1);
		Save ();
	}

	public void SaveModel () {
		saveList.Add (new SaveData(menu.inputName.text, FindObjectsOfType<CubeMesh>()));
		Save ();
	}

	public void LoadModel () {
        
		PushTo (undoStack);
		redoStack.Clear ();
		
		for (int i=cubeObjects.Count-1; i>=0; i--) {
			Destroy (cubeObjects[i]);
			cubeObjects.RemoveAt(i);
		}
		foreach (CubeMesh cm in FindObjectsOfType<CubeMesh> ()) {
			cubeObjects.Add (cm.gameObject);
			cm.gameObject.SetActive (false);
		}
		UpdateGrid ();
	}

	public void Undo () {
		if (undoStack.Count == 0) return;
		PushTo (redoStack);

		if (cubeArray != null) {
			foreach (CubeMesh ca in cubeArray) Destroy (ca.gameObject);
			cubeArray = null;
		}
		if (cubeArraySym != null) {
			foreach (CubeMesh cas in cubeArraySym) Destroy (cas.gameObject);
			cubeArraySym = null;
		}
		foreach (GameObject co in cubeObjects) Destroy(co);
		cubeObjects.Clear ();
		
		List<MeshData> meshList = undoStack [undoStack.Count - 1];
		undoStack.RemoveAt (undoStack.Count - 1);
		List<CubeMesh> cubeMeshes = new List<CubeMesh> ();
		foreach (MeshData meshData in meshList) {
			CubeMesh cm = ((GameObject)Instantiate(cubeObject, meshData.position.ToVector3(), Quaternion.identity)).GetComponent<CubeMesh> ();
			cm.LoadFromMeshData (meshData);
			cubeMeshes.Add (cm);
			cubeObjects.Add (cm.gameObject);
		}
		for (int i=0; i<meshList.Count; i++) {
			if (meshList[i].symmetry>=0) cubeMeshes[i].symmetry = cubeMeshes[meshList[i].symmetry];
		}

		UpdateGrid ();
		menu.HideSlider ();
		startCube.SetActive (cubeObjects.Count == 0);
		menu.ShowSaveButton (cubeObjects.Count != 0);
	}

	public void Redo () {
		if (redoStack.Count == 0) return;
		PushTo (undoStack);

		foreach (GameObject co in cubeObjects) Destroy(co);
		cubeObjects.Clear ();
		
		List<MeshData> meshList = redoStack [redoStack.Count - 1];
		redoStack.RemoveAt (redoStack.Count - 1);
		List<CubeMesh> cubeMeshes = new List<CubeMesh> ();
		foreach (MeshData meshData in meshList) {
			CubeMesh cm = ((GameObject)Instantiate(cubeObject, meshData.position.ToVector3(), Quaternion.identity)).GetComponent<CubeMesh> ();
			cm.LoadFromMeshData (meshData);
			cubeMeshes.Add (cm);
			cubeObjects.Add (cm.gameObject);
		}
		for (int i=0; i<meshList.Count; i++) {
			if (meshList[i].symmetry>=0) cubeMeshes[i].symmetry = cubeMeshes[meshList[i].symmetry];
		}

		UpdateGrid ();
		menu.HideSlider ();
		startCube.SetActive (cubeObjects.Count == 0);
		menu.ShowSaveButton (cubeObjects.Count != 0);
	}

	void Save () {
		print(Application.persistentDataPath);
		BinaryFormatter bf = new BinaryFormatter ();
		FileStream file;
		if (File.Exists (Application.persistentDataPath + "/savedmodels.cm")) {
			//if(saveList.Count==0) Load ();
			file = File.Open (Application.persistentDataPath + "/savedmodels.cm", FileMode.Open);
		}
		else {
			file = File.Create (Application.persistentDataPath + "/savedmodels.cm");
			saveList = new List<SaveData> ();
		}
		bf.Serialize(file, saveList);
		file.Close ();
		print ("Saving Successful");
	}

	void Load() {
		BinaryFormatter bf = new BinaryFormatter();
		FileStream file;
		if (File.Exists (Application.persistentDataPath + "/savedmodels.cm")) {
			file = File.Open (Application.persistentDataPath + "/savedmodels.cm", FileMode.Open);
			saveList = bf.Deserialize (file) as List<SaveData>;
			file.Close ();
		}
		else {
			saveList = new List<SaveData> ();
		}
		print ("Loading Successful");
	}

	void PushTo (List<List<MeshData>> stack) {
		List<MeshData> temp = new List<MeshData> ();
		foreach (GameObject co in cubeObjects) {
			temp.Add (new MeshData(co.GetComponent<CubeMesh> ()));
		}
		if (cubeArray != null) temp.Add (new MeshData(cubeArray[(int)menu.slider.value - 1]));
		if (cubeArraySym != null) temp.Add (new MeshData(cubeArraySym[(int)menu.slider.value - 1]));
		stack.Add (temp);
		if (stack.Count > 30) {
			stack.RemoveRange (0,10);
			Resources.UnloadUnusedAssets ();
		}
	}

	public void Attach (RaycastHit hit) {

		Vector3 pos = hit.point + hit.normal * 0.5f - hit.transform.position;
		pos = new Vector3 (Mathf.RoundToInt (pos.x), Mathf.RoundToInt (pos.y), Mathf.RoundToInt (pos.z));
		foreach (Vector3 na in newlyAttached) if(pos-hit.normal == na) return;

		CubeMesh cubeMesh = hit.transform.GetComponent<CubeMesh> ();
		Vector3 minPos = cubeMesh.minPos, maxPos = cubeMesh.maxPos;

		cubeMesh.AddCube (pos,menu.GetPickedColor());
		cubeMesh.UpdateMesh ();
		if (cubeMesh.symmetry != null) {
			cubeMesh.symmetry.AddCube (SymmetricVector (pos), menu.GetPickedColor ());
			cubeMesh.symmetry.UpdateMesh();
		}
		newlyAttached.Add (pos);

		if (minPos != cubeMesh.minPos || maxPos != cubeMesh.maxPos) UpdateGrid ();
	}

	public void Remove (RaycastHit hit) {
		Vector3 pos = hit.point - hit.normal * 0.5f - hit.transform.position;
		pos = new Vector3 (Mathf.RoundToInt (pos.x), Mathf.RoundToInt (pos.y), Mathf.RoundToInt (pos.z));

		CubeMesh cubeMesh = hit.transform.GetComponent<CubeMesh> ();
		Vector3 minPos = cubeMesh.minPos, maxPos = cubeMesh.maxPos;
		cubeMesh.RemoveCube (pos);
		cubeMesh.UpdateMesh ();
		if (cubeMesh.symmetry != null) {
			cubeMesh.symmetry.RemoveCube (SymmetricVector (pos));
			cubeMesh.symmetry.UpdateMesh();
		}

		if (minPos != cubeMesh.minPos || maxPos != cubeMesh.maxPos) UpdateGrid ();

		if (cubeMesh.GetCubeCount() == 0) {
			if(cubeMesh.symmetry != null) {
				Destroy(cubeMesh.symmetry.gameObject);
				cubeObjects.Remove(cubeMesh.symmetry.gameObject);
			}
			Destroy(cubeMesh.gameObject);
			cubeObjects.Remove(cubeMesh.gameObject);
			if(cubeObjects.Count==0) Clean();
		}
	}

	public void Paint (RaycastHit hit) {
		Vector3 pos = hit.point - hit.normal * 0.5f - hit.transform.position;
		pos = new Vector3 (Mathf.RoundToInt (pos.x), Mathf.RoundToInt (pos.y), Mathf.RoundToInt (pos.z));
		CubeMesh cubeMesh = hit.transform.GetComponent<CubeMesh> ();
		cubeMesh.SetColor(pos,hit.normal,menu.GetPickedColor());
		cubeMesh.UpdateMesh ();
		if (cubeMesh.symmetry != null) {
			cubeMesh.symmetry.SetColor(SymmetricVector (pos),SymmetricVector (hit.normal),menu.GetPickedColor());
			cubeMesh.symmetry.UpdateMesh();
		}
	}

	void ClearTex (){
		for(int i=0; i<tex.width; i++){
			for(int j=0; j<tex.height; j++){
				if(tex.GetPixel(i,j)!=Color.clear) tex.SetPixel(i,j,Color.clear);
			}
		}
		//tex.Apply();
	}

	void SetCubeColor (GameObject c){
		Color pickedColor = menu.GetPickedColor();

		for (int i=0; i<6; i++){
			c.transform.GetChild(i).GetComponent<Renderer>().material.color = pickedColor;
		}

	}

	char MaxXYZ (float x, float y, float z) {
		float max = Mathf.Max(Mathf.Max(x,y),z);
		if (max == x) return 'x';
		else if (max == y) return 'y';
		else return 'z';
	}

	Vector3 SymmetricVector (Vector3 pos) {
		return new Vector3 (pos.x, pos.y, pos.z==0? pos.z:-pos.z);
	}

	void AddThinningProcessCubeArray () {
		bool isSymmetric = opMode == 1;
		Texture2D image = new Texture2D(tex.width, tex.height);
		image.SetPixels (tex.GetPixels());
		//CV.Resize (image, (int)(Screen.width * thinningScale), (int)(Screen.height * thinningScale));
		Texture2D prev = new Texture2D (image.width, image.height);

		int zForImage = 0, zForDepth = 0;
		float nextZ = 1.0f;

		List<Texture2D> texArray = new List<Texture2D>();

		do {
			zForImage++;

			Texture2D cubeTex = new Texture2D (image.width, image.height);
			CV.MeansDenoising(image);
			cubeTex.SetPixels(image.GetPixels());
			//CV.Resize(cubeTex, Screen.width, Screen.height);
			texArray.Add(cubeTex);

			if(zForImage >= nextZ) {
				List<Vector3> cubePos = new List<Vector3>();
				GetCubePos(cubeTex, zForDepth, cubePos);
				if(cubePos.Count > 0){
					zForDepth++;
				}
				nextZ *= thinningProgressRatio;
			}

			prev.SetPixels(image.GetPixels());
			thinningIteration(image, 0);
			thinningIteration(image, 1);
		} while (!CV.Equal(image,prev));

		int maxDepth = Mathf.Max(zForImage, zForDepth * 2);
		if (zForDepth > 0) {
			PushTo (undoStack);
			redoStack.Clear ();
			if (cubeObjects.Count==0) SetCamPos (1);
			startCube.SetActive(false);
			Color pickedColor = menu.GetPickedColor();

			cubeArray = new CubeMesh[maxDepth];
			if(isSymmetric) cubeArraySym = new CubeMesh[maxDepth];
			for(int i=0;i<maxDepth;i++){
				cubeArray[i] = Instantiate(cubeObject).GetComponent<CubeMesh>();
				if(isSymmetric) {
					cubeArraySym[i] = Instantiate(cubeObject).GetComponent<CubeMesh>();
					cubeArray[i].symmetry = cubeArraySym[i];
					cubeArraySym[i].symmetry = cubeArray[i];
				}
			}
			
			for(int num = 0; num < maxDepth; num++) {
				List<Vector3> cubePos = new List<Vector3>();
				float growNum = Mathf.Pow(zForImage, 1.0f/(num+1));
				
				int depth = 0;
				for(float index = 1; index <= texArray.Count; ) {
					GetCubePos(texArray[(int)index-1], depth++, cubePos);
					index *= growNum;
				}

				foreach ( Vector3 pos in cubePos ){
					cubeArray[num].AddCube(pos,pickedColor);
					if(isSymmetric) cubeArraySym[num].AddCube(new Vector3(pos.x,pos.y,-pos.z),pickedColor);
				}
				cubeArray[num].UpdateMesh();
				if(isSymmetric) cubeArraySym[num].UpdateMesh();
			}

			for(int i=0;i<maxDepth;i++){
				if(i!=zForDepth-1) {
					cubeArray[i].gameObject.SetActive(false);
					if(isSymmetric) cubeArraySym[i].gameObject.SetActive(false);
				}
			}
			menu.SetUpSlider(zForDepth, maxDepth);
		}
	}
	
	void thinningIteration(Texture2D img, int iter)
	{
		int[,] array = new int[img.height, img.width];
		for (int y=0; y<img.height; y++) {
			for (int x=0; x<img.width; x++) {
				array[y,x] = img.GetPixel(x,y)==Color.black? 1:0 ;
			}	
		}
		int[,] marker = new int[img.height ,img.width];

		int pAbove;
		int pCurr;
		int pBelow;
		int nw, no, ne;    // north (pAbove)
		int we, me, ea;
		int sw, so, se;    // south (pBelow)
		
		int pDst;
		
		// initialize row pointers
		pCurr  = 0;
		pBelow = 1;
		
		for (int y = 1; y < img.height-1; ++y) {
			// shift the rows up by one
			pAbove = pCurr;
			pCurr  = pBelow;
			pBelow = y+1;
			
			pDst = y;
			
			// initialize col pointers
			no = array[pAbove,0];
			ne = array[pAbove,1];
			me = array[pCurr,0];
			ea = array[pCurr,1];
			so = array[pBelow,0];
			se = array[pBelow,1];
			
			for (int x = 1; x < img.width-1; ++x) {
				// shift col pointers left by one (scan left to right)
				nw = no;
				no = ne;
				ne = array[pAbove,x+1];
				we = me;
				me = ea;
				ea = array[pCurr,x+1];
				sw = so;
				so = se;
				se = array[pBelow,x+1];
				
				int A  = ((no == 0 && ne == 1)?1:0) + ((ne == 0 && ea == 1)?1:0) +
					((ea == 0 && se == 1)?1:0) + ((se == 0 && so == 1)?1:0) +
						((so == 0 && sw == 1)?1:0) + ((sw == 0 && we == 1)?1:0) +
						((we == 0 && nw == 1)?1:0) + ((nw == 0 && no == 1)?1:0);
				int B  = no + ne + ea + se + so + sw + we + nw;
				int m1 = iter == 0 ? (no * ea * so) : (no * ea * we);
				int m2 = iter == 0 ? (ea * so * we) : (no * so * we);
				
				if (A == 1 && (B >= 2 && B <= 6) && m1 == 0 && m2 == 0)
					marker[pDst,x] = 1;
			}
		}

		for (int y=0; y<img.height; y++) {
			for (int x=0; x<img.width; x++) {
				array[y,x] &= ~marker[y,x] ;
				img.SetPixel(x,y,array[y,x]==1?Color.black:Color.clear);
			}	
		}
	}

	void GetCubePos(Texture2D img, int depth, List<Vector3> cubePos){
		float dist = Vector3.Distance (cam.transform.position, Vector3.zero) - depth;
		char max = MaxXYZ (Mathf.Abs(cam.transform.position.x), Mathf.Abs(cam.transform.position.y), Mathf.Abs(cam.transform.position.z));

		/*Vector2 midPoint = CV.GetMidPoint (img)/thinningScale;
		if (max == 'x') depth=(int)cam.ScreenToWorldPoint (new Vector3 (midPoint.x, midPoint.y, dist)).x;
		else if (max == 'y') depth=(int)cam.ScreenToWorldPoint (new Vector3 (midPoint.x, midPoint.y, dist)).y;
		else if (max == 'z') depth=(int)cam.ScreenToWorldPoint (new Vector3 (midPoint.x, midPoint.y, dist)).z;*/
		
		int x0 = max=='x'? depth:(int)cam.ScreenToWorldPoint (new Vector3 (0, 0, dist)).x;
		int x1 = max=='x'? depth:(int)cam.ScreenToWorldPoint (new Vector3 (UnityEngine.Screen.width, UnityEngine.Screen.height, dist)).x;
		int y0 = max=='y'? depth:(int)cam.ScreenToWorldPoint (new Vector3 (0, 0, dist)).y;
		int y1 = max=='y'? depth:(int)cam.ScreenToWorldPoint (new Vector3 (UnityEngine.Screen.width, UnityEngine.Screen.height, dist)).y;
		int z0 = max=='z'? depth:(int)cam.ScreenToWorldPoint (new Vector3 (0, 0, dist)).z;
		int z1 = max=='z'? depth:(int)cam.ScreenToWorldPoint (new Vector3 (UnityEngine.Screen.width, UnityEngine.Screen.height, dist)).z;
	
		for (int x=Mathf.Min(x0,x1); x<=Mathf.Max(x0,x1); x++) {
			for (int y=Mathf.Min(y0,y1); y<=Mathf.Max(y0,y1); y++) {
				for (int z=Mathf.Min(z0,z1); z<=Mathf.Max(z0,z1); z++) {
					Vector3 point = new Vector3(x, y, z);
					if(img.GetPixel((int)(cam.WorldToScreenPoint(point).x*thinningScale), (int)(cam.WorldToScreenPoint(point).y*thinningScale))==Color.black){
						cubePos.Add(point);
						if(depth!=0){
							switch(max){
							case 'x':
								cubePos.Add(new Vector3(-x,y,z));
								break;
							case 'y':
								cubePos.Add(new Vector3(x,-y,z));
								break;
							case 'z':
								cubePos.Add(new Vector3(x,y,-z));
								break;
							}
							
						}
					}
				}
			}
		}
	}
}

