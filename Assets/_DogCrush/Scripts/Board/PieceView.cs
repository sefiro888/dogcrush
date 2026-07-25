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

        private Vector3 defaultScale = Vector3.one;
        private Coroutine moveCoroutine;
        private Coroutine pulseCoroutine;

        private void Awake()
        {
            if (mainRenderer == null)
                mainRenderer = GetComponent<SpriteRenderer>();
            defaultScale = transform.localScale;
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
                transform.localScale = defaultScale * 1.2f;
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
                float scale = 1.15f + Mathf.Sin(Time.time * 8f) * 0.08f;
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
                moveCoroutine = StartCoroutine(MoveRoutine(targetPos, speed, onComplete));
            }
            else
            {
                transform.position = targetPos;
                onComplete?.Invoke();
            }
        }

        private IEnumerator MoveRoutine(Vector3 targetPos, float speed, System.Action onComplete)
        {
            while (Vector3.Distance(transform.position, targetPos) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
                yield return null;
            }
            transform.position = targetPos;
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
            float duration = 0.2f;
            Vector3 initialScale = transform.localScale;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                transform.localScale = Vector3.Lerp(initialScale, Vector3.zero, t);
                yield return null;
            }

            transform.localScale = defaultScale;
            onComplete?.Invoke();
        }
    }
}
