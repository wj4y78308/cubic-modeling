using UnityEngine;
using System.Collections;
using Hover.Cast;
using Hover.Cast.Items;
using Hover.Cast.Custom;
using Hover.Common.Items;

namespace Hover.Demo.CastCubes.Items {
	public abstract class BaseListener<T> : HovercastItemListener<T> where T : ISelectableItem {


		protected HovercastSetup CastSetup { get; private set; }
		protected HovercastItemVisualSettings ItemSett { get; private set; }
		protected InteractionSettings InteractSett { get; private set; }
		protected Menu menu;

		protected override void Setup() {		
			CastSetup = GameObject.Find("Cast").GetComponent<HovercastSetup>();
			ItemSett = CastSetup.DefaultItemVisualSettings;
			InteractSett = CastSetup.InteractionSettings.GetSettings();
			menu = GameObject.Find("Scripts").GetComponent<Menu> ();
		}
	}
}
