using UnityEngine;
using Hover.Common.Items;
using Hover.Common.Items.Types;


namespace Hover.Demo.CastCubes.Items
{

    public class HueColorListener : BaseListener<ISliderItem>{

        protected override void Setup(){
            base.Setup();       
            Item.ValueToLabel = (s => Component.Label + ": " + s.RangeValue);
            Item.OnValueChanged += HandleValueChanged;
          
        }

        protected override void BroadcastInitialValue(){
            HandleValueChanged(Item);
        }

        private void HandleValueChanged(ISelectableItem<float> pItem){
            
            setHueColor(Item.RangeValue);
        }

        private void setHueColor(float hueColor) {
            Vector3 HSV = menu.getHSV();
            Color color = CV.HSVToRGB(hueColor, HSV.y, HSV.z);
            menu.setHSV(new Vector3(hueColor,HSV.y,HSV.z));         
            menu.ChangeColor(color);
        }
    }
}
