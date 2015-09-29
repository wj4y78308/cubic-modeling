using UnityEngine;
using Hover.Common.Items;
using Hover.Common.Items.Types;


namespace Hover.Demo.CastCubes.Items{

    public class LightnessListener : BaseListener<ISliderItem>{

        protected override void Setup() {
            base.Setup();
            Item.ValueToLabel = (s => Component.Label + ": " + s.RangeValue);
            Item.OnValueChanged += HandleValueChanged;
        }

        protected override void BroadcastInitialValue(){
            HandleValueChanged(Item);
        }

        private void HandleValueChanged(ISelectableItem<float> pItem){
            setLightness(Item.RangeValue);
        }

        private void setLightness(float lightness){
            Vector3 HSV = menu.getHSV();
            Color color = CV.HSVToRGB(HSV.x, lightness, HSV.z );
            menu.setHSV(new Vector3(HSV.x, lightness, HSV.z));
            menu.ChangeColor(color);
        }
    }
}
