using UnityEngine;
using System.Collections;
using Hover.Cast.Items;
using Hover.Common.Items;
using Hover.Common.Items.Types;


namespace Hover.Demo.CastCubes.Items {

	public class OperationsDrawSimpleListener : BaseListener<IRadioItem> {
		
		//public HovercastItem HueSlider;

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

			if (this.gameObject.name == "Draw Simple")
				menu.ChangeOpMode (0);
			else if (this.gameObject.name == "Draw Symmetry")
				menu.ChangeOpMode (1);
			else if (this.gameObject.name == "Attach")
				menu.ChangeOpMode (2);
			else if (this.gameObject.name == "Remove")
				menu.ChangeOpMode (3);
			else if (this.gameObject.name == "Paint")
				menu.ChangeOpMode (4);

			//ISliderItem hue = (ISliderItem)HueSlider.GetItem();
		}		
	}	
}
