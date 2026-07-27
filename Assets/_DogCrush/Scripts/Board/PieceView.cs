using System.Collections;
using UnityEngine;

namespace DogCrush.Board
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class PieceView : MonoBehaviour
    {
        public PieceType type = PieceType.None;
        public int gridX;
        public int gridY;

        [Header("Renderers & Visuals")]
        public SpriteRenderer mainRenderer;
        public SpriteRenderer selectionGlow;
        public SpriteRenderer shadowRenderer;

        private Vector3 defaultScale = Vector3.one * 0.95f;
        private CircleCollider2D interactionCollider;
        private Coroutine moveCoroutine;
        private Coroutine pulseCoroutine;

        private void Awake()
        {
            if (mainRenderer == null)
                mainRenderer = GetComponent<SpriteRenderer>();
            interactionCollider = GetComponent<CircleCollider2D>();

            defaultScale = transform.localScale;
            if (defaultScale == Vector3.zero) defaultScale = Vector3.one * 0.95f;

            SetSelected(false);
        }

        public void Initialize(PieceType pieceType, int x, int y, Sprite iconSprite, Color pieceColor)
        {
            type = pieceType;
            gridX = x;
            gridY = y;
            name = $"Piece_{x}_{y}_{pieceType}";

            if (mainRenderer != null)
            {
                mainRenderer.sprite = iconSprite;
                mainRenderer.color = pieceColor;
            }

            NormalizeVisualSize(pieceType, iconSprite);
            SetSelected(false);
        }

        private void NormalizeVisualSize(PieceType pieceType, Sprite iconSprite)
        {
            if (iconSprite == null)
            {
                defaultScale = Vector3.one;
                transform.localScale = defaultScale;
                return;
            }

            Vector2 spriteSize = iconSprite.bounds.size;
            float largestSide = Mathf.Max(spriteSize.x, spriteSize.y);
            float targetVisualSize = pieceType switch
            {
                PieceType.Dog => 0.60f,
                PieceType.Bone => 0.53f,
                PieceType.Ball => 0.55f,
                PieceType.Food => 0.57f,
                PieceType.Collar => 0.57f,
                _ => 0.55f
            };
            float uniformScale = largestSide > 0.001f
                ? targetVisualSize / largestSide
                : 1f;

            defaultScale = Vector3.one * uniformScale;
            transform.localScale = defaultScale;

            // The illustrations are normalized by scaling the piece root.
            // Compensate the collider so its world-space hit area remains
            // finger-friendly instead of shrinking to just a few pixels.
            if (interactionCollider != null && uniformScale > 0.001f)
            {
                const float desiredWorldRadius = 0.25f;
                interactionCollider.radius = desiredWorldRadius / uniformScale;
            }
        }

        public void SetGridPosition(int x, int y)
        {
            gridX = x;
            gridY = y;
        }

        public void SetSelected(bool isSelected)
        {
            if (selectionGlow != null)
            {
                selectionGlow.gameObject.SetActive(isSelected);
            }

            if (isSelected)
            {
                if (pulseCoroutine == null && gameObject.activeInHierarchy)
                {
                    pulseCoroutine = StartCoroutine(PulseAnimation());
                }
            }
            else
            {
                if (pulseCoroutine != null)
                {
                    StopCoroutine(pulseCoroutine);
                    pulseCoroutine = null;
                }
                transform.localScale = defaultScale;
            }
        }

        private IEnumerator PulseAnimation()
        {
            while (true)
            {
                float scale = 1.22f + Mathf.Sin(Time.time * 12f) * 0.08f;
                transform.localScale = defaultScale * scale;
                yield return null;
            }
        }

        public void MoveToWorldPosition(Vector3 targetPos, float speed, System.Action onComplete = null)
        {
            if (moveCoroutine != null)
                StopCoroutine(moveCoroutine);

            if (gameObject.activeInHierarchy)
            {
                moveCoroutine = StartCoroutine(MoveWithFluidBounceRoutine(targetPos, speed, onComplete));
            }
            else
            {
                transform.position = targetPos;
                onComplete?.Invoke();
            }
        }

        private IEnumerator MoveWithFluidBounceRoutine(Vector3 targetPos, float speed, System.Action onComplete)
        {
            Vector3 startPos = transform.position;
            float totalDistance = Vector3.Distance(startPos, targetPos);
            if (totalDistance < 0.001f)
            {
                transform.position = targetPos;
                onComplete?.Invoke();
                yield break;
            }

            float duration = Mathf.Clamp(totalDistance / speed, 0.08f, 0.45f);
            float elapsed = 0f;

            // Accelerated Ease-In Falling Curve (Candy Crush feel)
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float easedT = t * t; // Quadratic acceleration
                transform.position = Vector3.Lerp(startPos, targetPos, easedT);
                yield return null;
            }

            transform.position = targetPos;

            // Double Elastic Landing Bounce (Compress -> Overshoot -> Settle)
            float bounceTime = 0.16f;
            elapsed = 0f;
            Vector3 compressScale = new Vector3(defaultScale.x * 1.22f, defaultScale.y * 0.78f, defaultScale.z);
            Vector3 stretchScale = new Vector3(defaultScale.x * 0.90f, defaultScale.y * 1.12f, defaultScale.z);

            while (elapsed < bounceTime)
            {
                elapsed += Time.deltaTime;
                float b = elapsed / bounceTime;

                if (b < 0.4f)
                {
                    transform.localScale = Vector3.Lerp(defaultScale, compressScale, b / 0.4f);
                }
                else if (b < 0.75f)
                {
                    transform.localScale = Vector3.Lerp(compressScale, stretchScale, (b - 0.4f) / 0.35f);
                }
                else
                {
                    transform.localScale = Vector3.Lerp(stretchScale, defaultScale, (b - 0.75f) / 0.25f);
                }

                yield return null;
            }

            transform.localScale = defaultScale;
            moveCoroutine = null;
            onComplete?.Invoke();
        }

        public void AnimateDespawn(System.Action onComplete)
        {
            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(DespawnRoutine(onComplete));
            }
            else
            {
                onComplete?.Invoke();
            }
        }

        private IEnumerator DespawnRoutine(System.Action onComplete)
        {
            float elapsed = 0f;
            float duration = 0.18f;
            Vector3 startScale = transform.localScale;
            Vector3 popScale = startScale * 1.35f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                if (t < 0.3f)
                {
                    transform.localScale = Vector3.Lerp(startScale, popScale, t / 0.3f);
                }
                else
                {
                    transform.localScale = Vector3.Lerp(popScale, Vector3.zero, (t - 0.3f) / 0.7f);
                }

                yield return null;
            }

            transform.localScale = defaultScale;
            onComplete?.Invoke();
        }
    }
}
