using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MPJamPack {
    public class SelectableButton : Selectable
    {
        public override SelectableType Type { get { return SelectableType.Button; } }

        [SerializeField]
        private UnityEvent submitEvent;

        /// <summary>
        /// Navigate to Left selectable
        /// </summary>
        /// <param name="menuSelected">Refrence varrible from manager</param>
        /// <returns>Wether refrence selectable changed<</returns>
        public override bool Left(ref Selectable menuSelected) { return ChangeNav(ref menuSelected, left); }

        /// <summary>
        /// Navigate to Right selectable
        /// </summary>
        /// <param name="menuSelected">Refrence varrible from manager</param>
        /// <returns>Wether refrence selectable changed<</returns>
        public override bool Right(ref Selectable menuSelected) { return ChangeNav(ref menuSelected, right); }

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