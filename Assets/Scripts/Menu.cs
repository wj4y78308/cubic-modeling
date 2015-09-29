using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class Menu : MonoBehaviour {

	public Button[] opButtons;
	public Button[] visualButtons;
	public Button[] camposButtons;

	public Button redoButton;
	public Button undoButton;
	public Button cleanButton;
	public Button yesButton;

	public Button operButton;
    private Vector3 HSV = new Vector3(0.0f,1.0f,1.0f);

    //public GameObject opPanel;
    //public GameObject leftPanel;

    public Slider slider;
	public Text sliderText;
	public GameObject sliderPanel;
	public Image colorPicker;
	public Image colorRectsBG;
	public Image paper;
	public GameObject mainMenu;
	public GameObject loadMenu;
	public Button loadButton;
	public Button saveButton;
	public GameObject about;
	public Button switchL;
	public Button switchR;
	public GameObject saveDialog;
	public Text inputName;
	public GameObject deleteDialog;
	public Text modelName;

	Operations operations;
	Color pickedColor = new Color (1.0f ,0.0f ,0.0f);
	int hueColorNum = 20;
	int brightnessNum = 4;
	int sizeOfView = (int)Screen.height/32;
	int borderSize = (int)Screen.height/250;
	Rect[,] rectArray = new Rect[20, 4];
	GUIStyle[,] styleArray = new GUIStyle[20,4];
	Rect pickedColorRect;
	Texture2D pickedColorTex;
	bool showColorRects = true;
	int prevWidth=Screen.width, prevHeight=Screen.height;
	int currLoadScreen;

	// Use this for initialization
	void Start () {
		operations = GetComponent<Operations> ();
		SetUpColorArray ();
        
	}
	
	// Update is called once per frame
	void Update () {
		if (Screen.width != prevWidth || Screen.height != prevHeight) {
			SetUpColorArray();
			operations.SetUpTexture();
			prevWidth=Screen.width;
			prevHeight=Screen.height;
		}
	}

	void OnGUI () {
		if(showColorRects){
			for(int i = 0; i < hueColorNum; ++i) {
				for(int j = 0; j < brightnessNum; ++j) {
					if( GUI.Button(rectArray[i,j], GUIContent.none, styleArray[i,j]) ){
						ChangeColor( styleArray[i,j].normal.background.GetPixel(0,0) );
					}
				}
				GUI.DrawTexture(pickedColorRect, pickedColorTex);
			}
		}
	}
	/*public void HidePanel(GameObject m){
		m.gameObject.SetActive (false);
	}
	public void ShowPanel(GameObject m){
		m.gameObject.SetActive (true);
	}
*/
	public void ChangeOpMode (int mode){
		/*for (int i=0; i<6; i++) {
			if( i==mode ){
				opButtons[i].image.color = Color.white;
				opButtons[i].GetComponentInChildren<Text>().color = Color.black;
			}
			else{
				opButtons[i].image.color = Color.black;
				opButtons[i].GetComponentInChildren<Text>().color = Color.white;
			}
		}*/
		if(mode==0) operations.lineRenderer.material.color = Color.black;
		else if(mode==1) operations.lineRenderer.material.color = Color.blue;
		operations.opMode = mode;

		if (operations.opMode == 0)
			operButton.GetComponentInChildren<Text>().text = "Draw Simple";
		else if(operations.opMode == 1)
			operButton.GetComponentInChildren<Text>().text = "Draw Symmetry";
		else if(operations.opMode == 2)
			operButton.GetComponentInChildren<Text>().text = "Attach";
		else if(operations.opMode == 3)
			operButton.GetComponentInChildren<Text>().text = "Remove";
		else if(operations.opMode == 4)
			operButton.GetComponentInChildren<Text>().text = "Move";
		else if(operations.opMode == 5)
			operButton.GetComponentInChildren<Text>().text = "Paint";	
	}

	public void ShowMainMenu (bool show) {
		if (!show) {
			mainMenu.gameObject.SetActive(false);
			showColorRects = false;
		}
		else {
			mainMenu.gameObject.SetActive(true);
			if(!colorPicker.gameObject.activeSelf){
				showColorRects = true;
			}
		}
	}

	public void ShowLoadMenu (bool show) {
		if (show) {
			if (operations.saveList.Count == 0) return;
			ShowMainMenu (false);
			operations.startCube.SetActive(false);
			if (visualButtons[0].image.color == Color.white) operations.grid.SetActive (false);
			foreach (GameObject co in operations.cubeObjects)
				co.SetActive (false);
			currLoadScreen = 0;
			SwitchLoadScreen(0);
			loadMenu.gameObject.SetActive (true);
			operations.SetCamPos(1);
			operations.isLoading = true;
		}
		else {
			ShowMainMenu (true);
			foreach (CubeMesh cm in FindObjectsOfType<CubeMesh> ())
				Destroy(cm.gameObject);
			foreach (GameObject co in operations.cubeObjects)
				co.SetActive (true);
			if (operations.cubeObjects.Count==0) operations.startCube.SetActive(true);
			if (visualButtons[0].image.color == Color.white) operations.grid.SetActive (true);
			loadMenu.gameObject.SetActive (false);
			operations.SetCamPos(1);
			operations.isLoading = false;
		}
	}

	public void ShowSaveButton (bool show) {
		saveButton.gameObject.SetActive (show);
	}

	public void SwitchLoadScreen (int num) {
		currLoadScreen += (currLoadScreen + num < 0 || currLoadScreen + num >= operations.saveList.Count)? 0:num;
		switchL.gameObject.SetActive(currLoadScreen > 0);
		switchR.gameObject.SetActive(currLoadScreen < operations.saveList.Count-1);
		foreach (CubeMesh cubeMesh in FindObjectsOfType<CubeMesh> ())
			Destroy (cubeMesh.gameObject);
		if (operations.saveList.Count == 0) {
			ShowLoadMenu (false);
			return;
		}

		SaveData saveData = operations.saveList [currLoadScreen];
		List<CubeMesh> cubeMeshes = new List<CubeMesh> ();
		foreach (MeshData meshData in saveData.meshList) {
			CubeMesh cm = ((GameObject)Instantiate(operations.cubeObject, meshData.position.ToVector3(), Quaternion.identity)).GetComponent<CubeMesh> ();
			cm.LoadFromMeshData(meshData);
			cubeMeshes.Add(cm);
		}
		for (int i=0; i<saveData.meshList.Count; i++) {
			if (saveData.meshList[i].symmetry>=0) cubeMeshes[i].symmetry = cubeMeshes[saveData.meshList[i].symmetry];
		}
		modelName.text = saveData.name;
	}

	public void ChangeVisualState (int num){
		if (visualButtons[num].image.color == Color.black) {
			visualButtons[num].image.color = Color.white;
			visualButtons[num].GetComponentInChildren<Text>().color = Color.black;
			if (num==0) operations.grid.SetActive (true);
		}
		else{
			visualButtons[num].image.color = Color.black;
			visualButtons[num].GetComponentInChildren<Text>().color = Color.white;
			if (num==0) operations.grid.SetActive (false);
		}
	}

	public void SetUpSlider (int value, int maxValue) {
		slider.value = value;
		slider.maxValue = maxValue;
		SetSliderText ();
		sliderPanel.gameObject.SetActive (true);
		loadButton.gameObject.SetActive (false);
		saveButton.gameObject.SetActive (false);
		int mode = operations.opMode;
		for (int i=0; i<6; i++)
			//if (i != mode && i != 4)
			if (i != mode)
				opButtons [i].interactable = false;
	}

	public void SetSliderText (){
		sliderText.text = slider.value.ToString ();
	}

	public void HideSlider () {
		sliderPanel.gameObject.SetActive (false);
		loadButton.gameObject.SetActive (true);
		for (int i=0; i<6; i++)
				opButtons [i].interactable = true;
	}

	public void PickColorFromPalette (){
		ChangeColor (colorPicker.sprite.texture.GetPixel ( (int)(Input.mousePosition.x/Screen.width*colorPicker.sprite.texture.width)
		                                                  , (int)(Input.mousePosition.y/colorPicker.canvas.scaleFactor)));
	}

	public void ChangePalette (){
		showColorRects = !showColorRects;
		colorPicker.gameObject.SetActive(!colorPicker.gameObject.activeSelf);
		colorRectsBG.gameObject.SetActive(!colorRectsBG.gameObject.activeSelf);
	}

	public Color GetPickedColor () {
		return pickedColor;
	}

	public void ShowAbout (bool show) {
		about.SetActive (show);
	}

	public void ShowSaveDialog (bool show) {
		inputName.text = "";
		saveDialog.SetActive (show);
	}

	public void ShowDeleteDialog (bool show) {
		deleteDialog.SetActive (show);
	}

	public int GetCurrLoadScreen () {
		return currLoadScreen;
	}

	public void ChangeColor (Color color){
		pickedColor = color;
		//pickedColorImage.color = pickedColor;
		pickedColorTex.SetPixel(0, 0, pickedColor);
		pickedColorTex.Apply();
		operations.startCube.GetComponent<Renderer>().material.color = pickedColor;
	}

	public void SetUpColorArray (){
		Color color = new Color();
		for(int i = 0; i < hueColorNum; ++i) {
			for(int j = 0; j < brightnessNum; ++j) {
				if(i == hueColorNum - 1) {
					if(j == 3)
						color = CV.HSVToRGB((float)i/hueColorNum,0.0f,0.25f);
					else if(j == 2)
						color = CV.HSVToRGB((float)i/hueColorNum,0.0f,0.5f);
					else if(j == 1)
						color = CV.HSVToRGB((float)i/hueColorNum,0.0f,0.75f);
					else if(j == 0)
						color = CV.HSVToRGB((float)i/hueColorNum,0.0f,1.0f);
				} else {
					if(j == 3)
						color = CV.HSVToRGB((float)i/hueColorNum,1.0f,0.4f);
					else if(j == 2)
						color = CV.HSVToRGB((float)i/hueColorNum,1.0f,0.8f);
					else if(j == 1)
						color = CV.HSVToRGB((float)i/hueColorNum,0.7f,1.0f);
					else if(j == 0)
						color = CV.HSVToRGB((float)i/hueColorNum,0.5f,1.0f);
				}
				rectArray[i,j] = new Rect(0.5f*(Screen.width - hueColorNum * sizeOfView - (hueColorNum - 1) * borderSize) + i * sizeOfView + (i-1) * borderSize,
				                Screen.height - (brightnessNum - j) * sizeOfView - (brightnessNum - j - 1) * borderSize,
				                sizeOfView, sizeOfView);
				Texture2D texture = new Texture2D(1, 1);
				texture.SetPixel(0, 0, color);
				texture.Apply();
				styleArray[i,j] = new GUIStyle();
				styleArray[i,j].normal.background = texture;
			}
		}
		//pickedColorRect = new Rect(0.5f*(Screen.width - hueColorNum * sizeOfView - (hueColorNum - 1) * borderSize) + hueColorNum * sizeOfView + (hueColorNum-1) * borderSize,
		  //                   Screen.height - sizeOfView * 2, sizeOfView * 2, sizeOfView * 2);
		RectTransform rectButton = operButton.GetComponent<RectTransform> (); 
		pickedColorRect = new Rect(operButton.transform.position.x /*- rectButton.rect.width/2*/ +sizeOfView * 2 ,Screen.height - operButton.transform.position.y + rectButton.rect.height, sizeOfView * 2, sizeOfView * 2);
		pickedColorTex = new Texture2D(1, 1);
		ChangeColor (Color.red);
	}

    public Vector3 getHSV()
    {
        return HSV;
    }
    public void setHSV(Vector3 hsv) {
        HSV = hsv;
    }
}
