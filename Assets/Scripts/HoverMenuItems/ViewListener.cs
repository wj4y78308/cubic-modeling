using UnityEngine;
using System.Collections;
using Hover.Cast.Items;
using Hover.Common.Items;
using Hover.Common.Items.Types;

namespace Hover.Demo.CastCubes.Items {
	public class ViewListener : BaseListener<IRadioItem> {

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
			
			if (this.gameObject.name == "Top View")
				operations.SetCamPos(0);
			else if (this.gameObject.name == "Front View")
				operations.SetCamPos(1);
			else if (this.gameObject.name == "Side View")
				operations.SetCamPos(2);

		}		
	}
}