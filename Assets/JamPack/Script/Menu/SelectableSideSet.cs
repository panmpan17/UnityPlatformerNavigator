using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MPJamPack
{
    public class SelectableSideSet : Selectable
    {
        public override SelectableType Type { get { return SelectableType.SideSet; } }

        [SerializeField]
        private UnityEvent submitEvent, leftEvent, rightEvent;

        /// <summary>
		/// Make left effector look like been selected, execute left event
		/// </summary>
		/// <param name="menuSelected">Refrence selectable will never be changed it here</param>
		/// <returns>Always false</returns>
		public override bool Left(ref Selectable menuSelected) {
			leftEvent.Invoke();
			return true;
		}

        /// <summary>
        /// Make right effector look like been selected, execute left event
        /// </summary>
        /// <param name="menuSelected">Refrence selectable will never be changed it here</param>
        /// <returns>Always false</returns>
        public override bool Right(ref Selectable menuSelected) {
            rightEvent.Invoke();
			return true;
		}

        /// <summary>
        /// Navigate to Up selectable
        /// </summary>
        /// <param name="menuSelected">Refrence varrible from manager</param>
        /// <returns>Wether refrence selectable changed<</returns>
        public override bool Up(ref Selectable menuSelected) { return ChangeNav(ref menuSelected, up); }

        /// <summary>
        /// Navigate to Down selectable
        /// </summary>
        /// <param name="menuSelected">Refrence varrible from manager</param>
        /// <returns>Wether refrence selectable changed<</returns>
        public override bool Down(ref Selectable menuSelected) { return ChangeNav(ref menuSelected, down); }

        /// <summary>
        /// Execute submit event
        /// </summary>
        public override void Submit()
        {
            if (disabled || actived) return;
            submitEvent.Invoke();
        }
    }
}