using UnityEngine;
using UnityEngine.InputSystem;

namespace BusinessAsUsual.Prototype
{
    // Each player owns its actions; no dependence on the template's global action asset.
    [DefaultExecutionOrder(-100)]
    public sealed class PrototypeInput : MonoBehaviour
    {
        public InputAction Move { get; private set; }
        public InputAction Look { get; private set; }
        public InputAction Sprint { get; private set; }
        public InputAction Jump { get; private set; }
        public InputAction Interact { get; private set; }
        public InputAction ReleaseCursor { get; private set; }
        public InputAction CaptureCursor { get; private set; }
        public bool GameplayActive => Cursor.lockState == CursorLockMode.Locked && !Blocked;
        // A local menu (Phase 7A pause) owns Esc and suspends this player's gameplay input.
        public bool EscapeHandledExternally { get; set; }
        public bool Blocked { get; set; }

        void Awake()
        {
            Move = new InputAction("Move", InputActionType.Value);
            Move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
            Look = new InputAction("Look", InputActionType.Value, "<Mouse>/delta");
            Sprint = new InputAction("Sprint", InputActionType.Button, "<Keyboard>/leftShift");
            Sprint.AddBinding("<Keyboard>/rightShift");
            Jump = new InputAction("Jump", InputActionType.Button, "<Keyboard>/space");
            Interact = new InputAction("Interact", InputActionType.Button, "<Keyboard>/e");
            ReleaseCursor = new InputAction("Release cursor", InputActionType.Button, "<Keyboard>/escape");
            CaptureCursor = new InputAction("Capture cursor", InputActionType.Button, "<Mouse>/leftButton");
        }
        void OnEnable()
        {
            foreach (var action in Actions()) action.Enable();
            SetCursor(true);
        }
        void Update()
        {
            if (Blocked) return;
            if (!EscapeHandledExternally && ReleaseCursor.WasPressedThisFrame()) SetCursor(false);
            else if (!GameplayActive && CaptureCursor.WasPressedThisFrame()) SetCursor(true);
        }
        public void SetCursor(bool captured)
        {
            Cursor.lockState = captured ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !captured;
        }
        void OnApplicationFocus(bool focus) { if (!focus) SetCursor(false); }
        void OnDisable()
        {
            foreach (var action in Actions()) action.Disable();
            SetCursor(false);
        }
        void OnDestroy() { foreach (var action in Actions()) action.Dispose(); }
        InputAction[] Actions() => new[] { Move, Look, Sprint, Jump, Interact, ReleaseCursor, CaptureCursor };
    }
}
