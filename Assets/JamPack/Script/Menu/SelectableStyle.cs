using UnityEngine;

namespace MPJamPack {
	[CreateAssetMenu(menuName="MPJamPack/Selectable Style")]
	public class SelectableStyle : ScriptableObject {
		public Color NormalColor;
		public Color ActiveColor;
		public Color SelectedColor;
		public Color DisabledColor;
        public Material NormalMaterial;
		public Material SelectedMaterial;
	}
}