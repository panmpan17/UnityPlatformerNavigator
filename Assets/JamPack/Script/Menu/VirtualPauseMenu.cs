using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MPJamPack {
    public enum UIInputState {
        None,
        Up,
        Down,
        Left,
        Right,
        Submit,
        Cancel
    }

    public class VirtualPauseMenu : MonoBehaviour
    {
        /// <summary>
        /// Current selected
        /// </summary>
        [SerializeField]
        protected Selectable selected;

        /// <summary>
        /// Store default selected Selectable
        /// </summary>
        protected Selectable defaultSelected;

        /// <summary>
        /// Select indicator
        /// </summary>
		[SerializeField]
        protected Transform selectedIndicator;

        /// <summary>
        /// Indicator moving animation
        /// </summary>
        [SerializeField]
        protected Vector2LerpTimer indicatorMovingLerp;

        /// <summary>
        /// The curve that use for animate indicator
        /// </summary>
		[SerializeField]
        protected AnimationCurve indicatorMoveCurve;

        /// <summary>
        /// Indicator animation finished event, set by derivative class
        /// </summary>
        protected System.Action indicatorMoveFinishedCall;

        protected virtual bool Submit {
            get {
                return Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return);
            }
        }

        protected virtual bool Cancel {
            get {
                return Input.GetKeyDown(KeyCode.Escape);
            }
        }

        protected virtual bool Up {
            get {
                return Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow);
            }
        }
        
        protected virtual bool Down {
            get {
                return Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow);
            }
        }

        protected virtual bool Right {
            get {
                return Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow);
            }
        }

        protected virtual bool Left {
            get {
                return Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow);
            }
        }

        private Canvas canvas;

        private void Awake() {
            canvas = GetComponent<Canvas>();
            canvas.enabled = enabled = false;
            defaultSelected = selected;
        }

        public virtual void Activate(bool resetDefaultSelect=true) {
            canvas.enabled = enabled = true;
            if (resetDefaultSelect) selected = defaultSelected;
            selected.SelectSilently();

            indicatorMovingLerp.Timer.Reset();
            indicatorMovingLerp.Timer.Running = false;
            selectedIndicator.position = selected.transform.position;
        }

        public virtual void Deactivate() {
            canvas.enabled = enabled = false;
        }

        protected virtual UIInputState UpdateInput() {
            if (Submit) return UIInputState.Submit;
            if (Cancel) return UIInputState.Cancel;
            if (Up) return UIInputState.Up;
            if (Down) return UIInputState.Down;
            if (Left) return UIInputState.Left;
            if (Right) return UIInputState.Right;

            return UIInputState.None;
        }

        protected virtual void Update() {
            if (indicatorMovingLerp.Timer.Running) {
                if (indicatorMovingLerp.Timer.UpdateEnd) {
                    selectedIndicator.position = indicatorMovingLerp.Value;
                    indicatorMovingLerp.Timer.Running = false;
                }
                else {
                    selectedIndicator.position = indicatorMovingLerp.CurvedValue(indicatorMoveCurve);
                    return;
                }
            }

            UIInputState state = UpdateInput();

            switch (state)
            {
                case UIInputState.Up:
                    if (selected.Up(ref selected))
                        MoveIndicator(selected.transform.position);
                    break;
                case UIInputState.Down:
                    if (selected.Down(ref selected))
                        MoveIndicator(selected.transform.position);
                    break;
                case UIInputState.Left:
                    if (selected.Left(ref selected) && selected.Type == SelectableType.Button)
                        MoveIndicator(selected.transform.position);
                    break;
                case UIInputState.Right:
                    if (selected.Right(ref selected) && selected.Type == SelectableType.Button)
                        MoveIndicator(selected.transform.position);
                    break;
                case UIInputState.Submit:
                    selected.Submit();
                    break;
                case UIInputState.Cancel:
                    Deactivate();
                    break;
            }
        }

        protected virtual void MoveIndicator(Vector3 position) {
            indicatorMovingLerp.From = selectedIndicator.position;
            indicatorMovingLerp.To = position;
            indicatorMovingLerp.Timer.Reset();
        }
    }
}