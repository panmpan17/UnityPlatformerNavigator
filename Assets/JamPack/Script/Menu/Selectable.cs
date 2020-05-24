using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MPJamPack {
    public enum SelectableType { Button, SideSet }

    public abstract class Selectable : MonoBehaviour
    {
        static public bool ShowNavigationGizmos;
        
        [SerializeField]
        private Graphic[] targetGraphics;
        private TextMeshProUGUI[] textMeshUIs;
        private TextMeshProUGUI[] TextMeshUIs
        {
            get
            {
                if (textMeshUIs == null)
                {
                    List<TextMeshProUGUI> texts = new List<TextMeshProUGUI>();
                    for (int i = 0; i < targetGraphics.Length; i++)
                    {
                        TextMeshProUGUI text = targetGraphics[i].GetComponent<TextMeshProUGUI>();
                        if (text != null) texts.Add(text);
                    }
                    textMeshUIs = texts.ToArray();
                }

                return textMeshUIs;
            }
        }
        [SerializeField]
        protected SelectableStyle style;

        [SerializeField]
        protected Selectable left, right, up, down;

        [System.NonSerialized]
        protected bool selected, actived, disabled;

        public bool Disable
        {
            get { return disabled; }
            set
            {
                disabled = value;
                ApplyStyle();
            }
        }

        public abstract SelectableType Type { get; }

        public abstract bool Left(ref Selectable menuSelected);
        public abstract bool Right(ref Selectable menuSelected);
        public abstract bool Up(ref Selectable menuSelected);
        public abstract bool Down(ref Selectable menuSelected);
        public abstract void Submit();

        protected virtual void Awake()
        {
            ApplyStyle();
        }

        /// <summary>
        /// Change state of the selectable
        /// </summary>
        /// <value>Selected or not</value>
        public bool Select
        {
            get { return selected; }
            set
            {
                selected = value;
                ApplyStyle();
            }
        }

        public void SelectSilently()
        {
            selected = true;
            ApplyStyle();
        }

        /// <summary>
        /// Apply the style according to the state
        /// </summary>
		public void ApplyStyle()
        {
            if (style == null) return;

            Color color = style.NormalColor;

            if (disabled) color = style.DisabledColor;
            else if (actived) color = style.ActiveColor;
            else if (selected) color = style.SelectedColor;

            for (int i = 0; i < targetGraphics.Length; i++) targetGraphics[i].color = color;
        }

        /// <summary>
        /// Navigation function
        /// </summary>
        /// <param name="menuSelected">Refrence value from manager</param>
        /// <param name="selectable">The potencial nav selectable</param>
        /// <returns>Wether refrence selectable changed</returns>
        protected bool ChangeNav(ref Selectable menuSelected, Selectable selectable)
        {
            if (selectable == null) return false;
            selected = false;
            menuSelected = selectable;
            menuSelected.Select = true;
            ApplyStyle();
            return true;
        }

#if UNITY_EDITOR
        public void GenerateNavigation()
        {
            RectTransform rectT = GetComponent<RectTransform>();
            float minX = rectT.position.x - rectT.sizeDelta.x / 2;
            float maxX = rectT.position.x + rectT.sizeDelta.x / 2;
            float minY = rectT.position.y - rectT.sizeDelta.y / 2;
            float maxY = rectT.position.y + rectT.sizeDelta.y / 2;

            Selectable[] selectables = transform.root.GetComponentsInChildren<Selectable>();

            float bestRightDis = 0;
            Selectable bestRight = null;
            float bestLeftDis = 0;
            Selectable bestLeft = null;
            float bestUpDis = 0;
            Selectable bestUp = null;
            float bestDownDis = 0;
            Selectable bestDown = null;

            for (int j = 0; j < selectables.Length; j++)
            {
                if (selectables[j] == this) continue;

                RectTransform rectT2 = selectables[j].GetComponent<RectTransform>();
                float minX2 = rectT2.position.x - rectT2.sizeDelta.x / 2;
                float maxX2 = rectT2.position.x + rectT2.sizeDelta.x / 2;
                float minY2 = rectT2.position.y - rectT2.sizeDelta.y / 2;
                float maxY2 = rectT2.position.y + rectT2.sizeDelta.y / 2;

                if (Type == SelectableType.Button)
                {
                    if (right == null) {
                        float rightDis = minX2 - maxX;
                        if (rightDis > 0 && ((rightDis < bestRightDis) || (bestRight == null))) {
                            bestRightDis = rightDis;
                            bestRight = selectables[j];
                        }
                    }
                    if (left == null) {
                        float leftDis = minX - maxX2;
                        if (leftDis > 0 && ((leftDis < bestLeftDis) || (bestLeft == null))) {
                            bestLeftDis = leftDis;
                            bestLeft = selectables[j];
                        }
                    }
                }

                if (up == null) {
                    float upDis = minY2 - maxY;
                    if (upDis > 0 && ((upDis < bestUpDis) || (bestUp == null))) {
                        bestUpDis = upDis;
                        bestUp = selectables[j];
                    }
                }

                if (down == null) {
                    float downDis = minY - maxY2;
                    if (downDis > 0 && ((downDis < bestDownDis) || (bestDown == null))) {
                        bestDownDis = downDis;
                        bestDown = selectables[j];
                    }
                }
            }

            if (bestRight != null) right = bestRight;
            if (bestLeft != null) left = bestLeft;
            if (bestUp != null) up = bestUp;
            if (bestDown != null) down = bestDown;
        }

        protected void Reset()
        {
            targetGraphics = GetComponentsInChildren<Graphic>();
        }

        private void OnDrawGizmos() {
            if (!ShowNavigationGizmos) return;

            RectTransform rectT = GetComponent<RectTransform>();

            if (up != null) {
                RectTransform upRectT = up.GetComponent<RectTransform>();
                Gizmos.DrawLine(transform.position + new Vector3(10, rectT.sizeDelta.y / 2),
                                upRectT.position + new Vector3(10, -upRectT.sizeDelta.y / 2));
                Gizmos.DrawSphere(transform.position + new Vector3(10, rectT.sizeDelta.y / 2), 10f);
            }

            if (down != null) {
                RectTransform downRectT = down.GetComponent<RectTransform>();
                Gizmos.DrawLine(transform.position + new Vector3(-10, -rectT.sizeDelta.y / 2),
                                downRectT.position + new Vector3(-10, downRectT.sizeDelta.y / 2));
                Gizmos.DrawSphere(transform.position + new Vector3(-10, -rectT.sizeDelta.y / 2), 10f);
            }

            if (right != null) {
                RectTransform rightRectT = right.GetComponent<RectTransform>();
                Gizmos.DrawLine(transform.position + new Vector3(rectT.sizeDelta.x / 2, 10),
                                rightRectT.position + new Vector3(-rightRectT.sizeDelta.x / 2, 10));
                Gizmos.DrawSphere(transform.position + new Vector3(rectT.sizeDelta.x / 2, 10), 10f);
            }

            if (left != null) {
                RectTransform leftRectT = left.GetComponent<RectTransform>();
                Gizmos.DrawLine(transform.position + new Vector3(-rectT.sizeDelta.x / 2, -10),
                                leftRectT.position + new Vector3(leftRectT.sizeDelta.x / 2, -10));
                Gizmos.DrawSphere(transform.position + new Vector3(-rectT.sizeDelta.x / 2, -10), 10f);
            }
        }
#endif
    }
}