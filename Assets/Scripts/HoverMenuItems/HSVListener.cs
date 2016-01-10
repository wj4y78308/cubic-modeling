using UnityEngine;
using System.Collections;
using Hover.Common.Items;
using Hover.Common.Items.Types;

namespace Hover.Demo.CastCubes.Items
{
	public class HSVListener : BaseListener<ISliderItem> {

		protected override void Setup() {
			base.Setup();
			Item.ValueToLabel = (s => Component.Label + ": " + s.RangeValue);
			Item.OnValueChanged += HandleValueChanged;
		}

		protected override void BroadcastInitialValue(){
			HandleValueChanged(Item);
		}

		private void HandleValueChanged(ISelectableItem<float> pItem){
			if (gameObject.name == "HueColor")
				setHueColor (Item.RangeValue);
			else if (gameObject.name == "Lightness")
				setLightness (Item.RangeValue);
			else if (gameObject.name == "Saturation")
				setSaturation (Item.RangeValue);
		}

		private void setHueColor(float hueColor) {
			Vector3 HSV = menu.getHSV();
			Color color = CV.HSVToRGB(hueColor, HSV.y, HSV.z);
			menu.setHSV(new Vector3(hueColor,HSV.y,HSV.z));         
			menu.ChangeColor(color);
		}

		private void setLightness(float lightness){
			Vector3 HSV = menu.getHSV();
			Color color = CV.HSVToRGB(HSV.x, HSV.y, lightness );
			menu.setHSV(new Vector3(HSV.x, HSV.y, lightness));
			menu.ChangeColor(color);
		}
		private void setSaturation(float saturation){
			Vector3 HSV = menu.getHSV();
			Color color = CV.HSVToRGB(HSV.x, saturation,HSV.z );
			menu.setHSV(new Vector3(HSV.x,saturation , HSV.z));
			menu.ChangeColor(color);	
		}
	}
}