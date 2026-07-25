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
        private Coroutine moveCoroutine;
        private Coroutine pulseCoroutine;

        private void Awake()
        {
            if (mainRenderer == null)
                mainRenderer = GetComponent<SpriteRenderer>();

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

            SetSelected(false);
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
                float scale = 1.18f + Mathf.Sin(Time.time * 9f) * 0.08f;
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
                moveCoroutine = StartCoroutine(MoveWithBounceRoutine(targetPos, speed, onComplete));
            }
            else
            {
                transform.position = targetPos;
                onComplete?.Invoke();
            }
        }

        private IEnumerator MoveWithBounceRoutine(Vector3 targetPos, float speed, System.Action onComplete)
        {
            while (Vector3.Distance(transform.position, targetPos) > 0.02f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
                yield return null;
            }

            transform.position = targetPos;

            // Landing Bounce Effect (Squash & Stretch)
            float bounceTime = 0.12f;
            float elapsed = 0f;
            Vector3 squashScale = new Vector3(defaultScale.x * 1.15f, defaultScale.y * 0.85f, defaultScale.z);

            while (elapsed < bounceTime)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / bounceTime;
                transform.localScale = Vector3.Lerp(defaultScale, squashScale, Mathf.Sin(t * Mathf.PI));
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
            Vector3 popScale = startScale * 1.25f;

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
