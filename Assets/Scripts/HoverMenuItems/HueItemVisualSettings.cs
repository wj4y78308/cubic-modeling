using Hover.Cast.Custom;
using Hover.Cast.Custom.Standard;
using Hover.Cast.Display.Standard;
using Hover.Cast.Items;
using Hover.Common.Custom;
using Hover.Common.Items;
using Hover.Common.Items.Types;
using UnityEngine;

namespace Hover.Demo.CastCubes.Custom {

	/*================================================================================================*/
	public class HueItemVisualSettings : HovercastItemVisualSettings {

		private ItemVisualSettingsStandard vDefaultSettings;
		private ItemVisualSettingsStandard vHueSettings;
		private ISliderItem vHueSlider;
        protected Menu menu;


        ////////////////////////////////////////////////////////////////////////////////////////////////
        /*--------------------------------------------------------------------------------------------*/
        public void Awake() {
			vHueSettings = new ItemVisualSettingsStandard();
			
			vHueSlider = (ISliderItem)gameObject.GetComponent<HovercastItem>().GetItem();
			vHueSlider.OnValueChanged += HandleValueChanged;
            menu = GameObject.Find("Scripts").GetComponent<Menu>();
        }

		/*--------------------------------------------------------------------------------------------*/
		protected override IItemVisualSettings GetSettingsInner(IBaseItem pItem,
												IItemVisualSettings pDefault, bool pFillWithDefault) {
			vDefaultSettings = (ItemVisualSettingsStandard)pDefault;
			HandleValueChanged(null);
			return vHueSettings;
		}


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		private void HandleValueChanged(ISelectableItem pItem) {
           
            Color col = menu.GetPickedColor();
			Color colFade = col;
			colFade.a = 0.25f;

			vHueSettings.FillWith(vDefaultSettings, false);
			vHueSettings.Renderer = typeof(UiItemSliderRenderer);
			vHueSettings.SelectionColor = col;
			vHueSettings.SliderTrackColor = colFade;
			vHueSettings.SliderFillColor = colFade;
		}

	}

}
