using UnityEngine;
using System.Collections;
using Hover.Cast.Items;
using Hover.Common.Items;
using Hover.Common.Items.Types;

namespace Hover.Demo.CastCubes.Items {
	public class FileListener : BaseListener<IRadioItem> {

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
			
			if (this.gameObject.name == "Load")
				menu.ShowLoadMenu (true);
			else if (this.gameObject.name == "Save")
				menu.ShowSaveDialog(true);
		
		}
	}
}