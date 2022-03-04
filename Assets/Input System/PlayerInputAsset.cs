using UnityEngine;
#if ENABLE_INPUT_SYSTEM && INPUT_ASSET_PACKAGES_CHECKED
using UnityEngine.InputSystem;
#endif

namespace Player
{
    public class PlayerInputAsset : MonoBehaviour
    {
        #region Variables
        [Header("Movement Information")]
        public Vector2 move;
        public Vector2 look;

        [Space(10)]
        public bool isAnalogMovementOn = false;

        [Space(10)]
        public bool jump = false;
        public bool sprint = false;

        [Header("Cursor Settings")]
        public bool isCursorLock = true;
        public bool isCursorOnScreen = true;
        #endregion

#if ENABLE_INPUT_SYSTEM && INPUT_ASSET_PACKAGES_CHECKED
        public void OnMove(InputValue value) => Move(value.Get<Vector2>());
        public void OnLook(InputValue value)
        {
            if (isCursorOnScreen)
                Look(value.Get<Vector2>());
        }
        public void OnJump(InputValue value) => Jump(value.isPressed);
        public void OnSprint(InputValue value) => Sprint(value.isPressed);
#endif
        public void Move(Vector2 newMoveDirection) => move = newMoveDirection;
        public void Look(Vector2 newLookDirection) => look = newLookDirection;
        public void Jump(bool newJumpCondition) => jump = newJumpCondition;
        public void Sprint(bool newSprintCondition) => sprint = newSprintCondition;
        public void SetCursorState(bool newState) => Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;

        private void OnApplicationFocus(bool focus) => SetCursorState(isCursorLock);
    }
}

