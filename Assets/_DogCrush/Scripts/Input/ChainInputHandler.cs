using UnityEngine;

namespace DogCrush.InputSystem
{
    public class ChainInputHandler : MonoBehaviour
    {
        public System.Action<Vector2> OnPointerDownEvent;
        public System.Action<Vector2> OnPointerDragEvent;
        public System.Action OnPointerUpEvent;

        public bool IsPointerPressed { get; private set; }
        private Camera mainCamera;

        private void Awake()
        {
            mainCamera = Camera.main;
        }

        private void Update()
        {
            if (mainCamera == null)
                mainCamera = Camera.main;

            // Touch Input check
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                Vector2 worldPos = GetWorldPosition(touch.position);

                if (touch.phase == TouchPhase.Began)
                {
                    IsPointerPressed = true;
                    OnPointerDownEvent?.Invoke(worldPos);
                }
                else if ((touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary) && IsPointerPressed)
                {
                    OnPointerDragEvent?.Invoke(worldPos);
                }
                else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    IsPointerPressed = false;
                    OnPointerUpEvent?.Invoke();
                }
                return;
            }

            // Mouse Input check
            if (Input.GetMouseButtonDown(0))
            {
                IsPointerPressed = true;
                Vector2 worldPos = GetWorldPosition(Input.mousePosition);
                OnPointerDownEvent?.Invoke(worldPos);
            }
            else if (Input.GetMouseButton(0) && IsPointerPressed)
            {
                Vector2 worldPos = GetWorldPosition(Input.mousePosition);
                OnPointerDragEvent?.Invoke(worldPos);
            }
            else if (Input.GetMouseButtonUp(0) && IsPointerPressed)
            {
                IsPointerPressed = false;
                OnPointerUpEvent?.Invoke();
            }
        }

        public Vector2 GetWorldPosition(Vector3 screenPos)
        {
            if (mainCamera == null) return Vector2.zero;
            Vector3 world = mainCamera.ScreenToWorldPoint(screenPos);
            return new Vector2(world.x, world.y);
        }
    }
}
