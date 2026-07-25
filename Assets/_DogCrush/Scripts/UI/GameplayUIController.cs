using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DogCrush.UI
{
    public class GameplayUIController : MonoBehaviour
    {
        [Header("HUD Elements")]
        public TextMeshProUGUI scoreText;
        public TextMeshProUGUI highScoreText;
        public TextMeshProUGUI timerText;
        public Image timerBarFill;
        public TextMeshProUGUI chainInfoText;
        public TextMeshProUGUI comboBannerText;

        [Header("Game Over Overlay")]
        public GameObject gameOverPanel;
        public TextMeshProUGUI finalScoreText;
        public TextMeshProUGUI newRecordBanner;
        public Button playAgainButton;
        public Button secondaryRestartButton;
        public Button hudRestartButton;

        public System.Action OnRestartRequested;

        private int targetScore = 0;
        private int displayedScore = 0;
        private Coroutine comboRoutine;

        private void Awake()
        {
            if (playAgainButton != null)
                playAgainButton.onClick.AddListener(() => OnRestartRequested?.Invoke());

            if (secondaryRestartButton != null)
                secondaryRestartButton.onClick.AddListener(() => OnRestartRequested?.Invoke());

            if (hudRestartButton != null)
                hudRestartButton.onClick.AddListener(() => OnRestartRequested?.Invoke());

            HideGameOver();
            if (comboBannerText != null) comboBannerText.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (displayedScore != targetScore)
            {
                displayedScore = (int)Mathf.MoveTowards(displayedScore, targetScore, Mathf.Max(100f, Mathf.Abs(targetScore - displayedScore) * 10f * Time.deltaTime));
                if (scoreText != null) scoreText.text = $"{displayedScore:N0}";
            }
        }

        public void UpdateScore(int currentScore)
        {
            targetScore = currentScore;
        }

        public void UpdateHighScore(int highScore)
        {
            if (highScoreText != null)
                highScoreText.text = $"RÉCORD: {highScore:N0}";
        }

        public void UpdateTimer(float remainingSeconds, float progress01)
        {
            if (timerText != null)
            {
                int seconds = Mathf.CeilToInt(remainingSeconds);
                timerText.text = $"{seconds}s";
                if (remainingSeconds <= 10f)
                {
                    timerText.color = new Color(1f, 0.25f, 0.25f);
                }
                else
                {
                    timerText.color = Color.white;
                }
            }

            if (timerBarFill != null)
            {
                timerBarFill.fillAmount = Mathf.Clamp01(progress01);
                if (remainingSeconds <= 10f)
                {
                    timerBarFill.color = Color.Lerp(Color.red, Color.yellow, Mathf.Sin(Time.time * 10f) * 0.5f + 0.5f);
                }
                else
                {
                    timerBarFill.color = new Color(0.3f, 0.85f, 0.45f);
                }
            }
        }

        public void UpdateChainInfo(int count, string typeName)
        {
            if (chainInfoText == null) return;

            if (count > 0)
            {
                chainInfoText.gameObject.SetActive(true);
                chainInfoText.text = $"Cadena: {count} ({typeName})";
            }
            else
            {
                chainInfoText.gameObject.SetActive(false);
            }
        }

        public void ShowComboBanner(string comboText, Color color)
        {
            if (comboBannerText == null) return;

            if (comboRoutine != null) StopCoroutine(comboRoutine);
            comboBannerText.text = comboText;
            comboBannerText.color = color;
            comboBannerText.gameObject.SetActive(true);

            comboRoutine = StartCoroutine(AnimateComboBanner());
        }

        private System.Collections.IEnumerator AnimateComboBanner()
        {
            float duration = 1.2f;
            float elapsed = 0f;
            Transform tr = comboBannerText.transform;
            Vector3 startScale = Vector3.one * 0.7f;
            Vector3 targetScale = Vector3.one * 1.25f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                if (t < 0.2f)
                {
                    tr.localScale = Vector3.Lerp(startScale, targetScale, t / 0.2f);
                }
                else
                {
                    tr.localScale = Vector3.Lerp(targetScale, Vector3.one, (t - 0.2f) / 0.8f);
                }
                yield return null;
            }

            comboBannerText.gameObject.SetActive(false);
        }

        public void ShowGameOver(int finalScore, bool isNewRecord)
        {
            if (gameOverPanel != null) gameOverPanel.SetActive(true);
            if (finalScoreText != null) finalScoreText.text = $"Puntuación Final\n{finalScore:N0}";
            if (newRecordBanner != null) newRecordBanner.gameObject.SetActive(isNewRecord);
        }

        public void HideGameOver()
        {
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
        }
    }
}
