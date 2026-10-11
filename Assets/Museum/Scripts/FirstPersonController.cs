using UnityEngine;
using UnityEngine.InputSystem;

namespace Museum
{
    [RequireComponent(typeof(CharacterController))]
    public class FirstPersonController : MonoBehaviour
    {
        [SerializeField] InputActionAsset actions;
        [SerializeField] Transform cameraPivot;
        [SerializeField] float walkSpeed = 2.5f;
        [SerializeField] float sprintSpeed = 4.5f;
        [SerializeField] float lookSensitivity = 0.1f;
        [SerializeField] float gravity = -9.81f;

        CharacterController cc;
        InputActionMap map;
        InputAction move, look, sprint;
        float pitch, verticalVelocity;

        public bool Frozen { get; set; }

        void OnEnable()
        {
            if (actions == null || cameraPivot == null)
            {
                Debug.LogError("FirstPersonController: 'actions' and 'cameraPivot' must be assigned.", this);
                enabled = false;
                return;
            }

            cc = GetComponent<CharacterController>();
            map = actions.FindActionMap("Player", true);
            move = map.FindAction("Move", true);
            look = map.FindAction("Look", true);
            sprint = map.FindAction("Sprint", true);
            map.Enable();
            SetCursorLocked(true);
        }

        void OnDisable()
        {
            map?.Disable();
        }

        static void SetCursorLocked(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }

        void Update()
        {
            var kb = Keyboard.current;
            var mouse = Mouse.current;
            if (kb != null && kb.escapeKey.wasPressedThisFrame) SetCursorLocked(false);
            else if (mouse != null && mouse.leftButton.wasPressedThisFrame) SetCursorLocked(true);

            if (!Frozen && Cursor.lockState == CursorLockMode.Locked)
            {
                Vector2 d = look.ReadValue<Vector2>() * lookSensitivity;
                transform.Rotate(0f, d.x, 0f);
                pitch = Mathf.Clamp(pitch - d.y, -85f, 85f);
                cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
            }

            Vector2 m = Frozen ? Vector2.zero : move.ReadValue<Vector2>();
            float speed = sprint.IsPressed() ? sprintSpeed : walkSpeed;
            Vector3 vel = (transform.right * m.x + transform.forward * m.y) * speed;

            if (cc.isGrounded && verticalVelocity < 0f) verticalVelocity = -2f;
            verticalVelocity += gravity * Time.deltaTime;
            vel.y = verticalVelocity;

            cc.Move(vel * Time.deltaTime);
        }
    }
}
