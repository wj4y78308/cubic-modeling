using Hover.Common.Items;
using Hover.Common.Items.Types;
using UnityEngine;

namespace Hover.Demo.CastCubes.Items
{

    public class OperationsListener : BaseListener<IRadioItem> {

		protected override void Setup() {
			base.Setup();
			Item.OnValueChanged += HandleValueChanged;
            
		}		

		protected override void BroadcastInitialValue() {
			HandleValueChanged(Item);
		}

		private void HandleValueChanged(ISelectableItem<bool> pItem) {
			
			if ( !pItem.Value ) {
				return;
			}
			if (gameObject.name == "Draw Simple")
				menu.ChangeOpMode (0);
			//else if (gameObject.name == "Draw Symmetry") {
				//menu.ChangeOpMode (1);
			//} 
			else if (gameObject.name == "Attach")
				menu.ChangeOpMode (2);
			else if (gameObject.name == "Remove")
				menu.ChangeOpMode (3);
			else if (gameObject.name == "Move")
				menu.ChangeOpMode (4);
			else if (gameObject.name == "Paint")
				menu.ChangeOpMode (5);
		//	else if (gameObject.name == "Grab")
			//	menu.ChangeOpMode (6);
			else if (gameObject.name == "X axis"){
				menu.ChangeOpMode (1);
				operations.symmetryDir = 1;
				menu.symmetryPlane.transform.eulerAngles = new Vector3 (90,0,0);
			} 
			else if (gameObject.name == "Y axis"){
				menu.ChangeOpMode (1);
				operations.symmetryDir = 2;
				menu.symmetryPlane.transform.eulerAngles = new Vector3 (0,90,90);
			} 
			else if (gameObject.name == "Z axis"){
				menu.ChangeOpMode (1);
				operations.symmetryDir = 3;
				menu.symmetryPlane.transform.eulerAngles = new Vector3 (0,180,90);
			}

            if ( menu.sliderPanel.gameObject.activeSelf){
				operations.SetThickness();
			}
			
		}		
	}	
}
