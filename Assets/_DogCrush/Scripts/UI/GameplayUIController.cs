using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DogCrush.UI
{
    public class GameplayUIController : MonoBehaviour
    {
        [Header("HUD Text Elements")]
        public TextMeshProUGUI scoreText;
        public TextMeshProUGUI highScoreText;
        public TextMeshProUGUI timerText;
        public TextMeshProUGUI chainInfoText;

        [Header("Game Over Overlay")]
        public GameObject gameOverPanel;
        public TextMeshProUGUI finalScoreText;
        public TextMeshProUGUI newRecordBanner;
        public Button playAgainButton;
        public Button hudRestartButton;

        public System.Action OnRestartRequested;

        private void Awake()
        {
            if (playAgainButton != null)
                playAgainButton.onClick.AddListener(() => OnRestartRequested?.Invoke());

            if (hudRestartButton != null)
                hudRestartButton.onClick.AddListener(() => OnRestartRequested?.Invoke());

            HideGameOver();
        }

        public void UpdateScore(int currentScore)
        {
            if (scoreText != null)
                scoreText.text = $"Puntos: {currentScore:N0}";
        }

        public void UpdateHighScore(int highScore)
        {
            if (highScoreText != null)
                highScoreText.text = $"Récord: {highScore:N0}";
        }

        public void UpdateTimer(float remainingSeconds)
        {
            if (timerText != null)
            {
                int seconds = Mathf.CeilToInt(remainingSeconds);
                timerText.text = $"{seconds}s";

                if (remainingSeconds <= 10f)
                {
                    timerText.color = new Color(1f, 0.2f, 0.2f);
                }
                else
                {
                    timerText.color = Color.white;
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

        public void ShowGameOver(int finalScore, bool isNewRecord)
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
            }

            if (finalScoreText != null)
            {
                finalScoreText.text = $"Puntuación Final:\n{finalScore:N0}";
            }

            if (newRecordBanner != null)
            {
                newRecordBanner.gameObject.SetActive(isNewRecord);
            }
        }

        public void HideGameOver()
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(false);
            }
        }
    }
}
