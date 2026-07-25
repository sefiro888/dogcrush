using System.Collections;
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

        // Runtime-created UI references
        private Canvas runtimeCanvas;
        private Image timerBarBg;
        private RectTransform headerPanel;
        private CanvasGroup gameOverCanvasGroup;

        private void Awake()
        {
            // Disable old scene UI elements if they exist
            CleanOldSceneUI();

            // Always build clean runtime UI to ensure correct scaling and design
            BuildRuntimeUI();

            if (playAgainButton != null)
                playAgainButton.onClick.AddListener(() => OnRestartRequested?.Invoke());

            if (secondaryRestartButton != null)
                secondaryRestartButton.onClick.AddListener(() => OnRestartRequested?.Invoke());

            if (hudRestartButton != null)
                hudRestartButton.onClick.AddListener(() => OnRestartRequested?.Invoke());

            HideGameOver();
            if (comboBannerText != null) comboBannerText.gameObject.SetActive(false);
        }

        private void CleanOldSceneUI()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null) canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            foreach (Transform child in canvas.transform)
            {
                if (child.name != "SafeAreaPanel" && !child.name.EndsWith("_RT"))
                {
                    child.gameObject.SetActive(false);
                }
            }
        }

        private void BuildRuntimeUI()
        {
            // Find or create canvas
            runtimeCanvas = GetComponentInParent<Canvas>();
            if (runtimeCanvas == null)
            {
                runtimeCanvas = FindAnyObjectByType<Canvas>();
            }
            if (runtimeCanvas == null) return;

            RectTransform canvasRect = runtimeCanvas.GetComponent<RectTransform>();

            // === HEADER PANEL (Score + Timer) ===
            headerPanel = CreatePanel(canvasRect, "HeaderHUD_Runtime",
                new Vector2(0, 1), new Vector2(1, 1), // stretch top
                new Vector2(0, -10), new Vector2(0, -10), 180f);

            // Score display
            GameObject scoreContainer = CreateRoundedPanel(headerPanel, "ScoreContainer",
                new Color(0.08f, 0.08f, 0.18f, 0.85f),
                new Vector2(0.02f, 0.05f), new Vector2(0.48f, 0.55f));

            scoreText = CreateText(scoreContainer.GetComponent<RectTransform>(), "ScoreText_RT",
                "0", 42f, new Color(1f, 0.95f, 0.3f),
                TextAlignmentOptions.Center,
                new Vector2(0, 0), new Vector2(1, 1),
                new Vector2(0, -5), new Vector2(0, 5));

            // Score label
            CreateText(scoreContainer.GetComponent<RectTransform>(), "ScoreLabel",
                "PUNTOS", 16f, new Color(1f, 1f, 1f, 0.5f),
                TextAlignmentOptions.Top,
                new Vector2(0, 0.7f), new Vector2(1, 1),
                new Vector2(5, 0), new Vector2(-5, -3));

            // High Score display  
            GameObject highScoreContainer = CreateRoundedPanel(headerPanel, "HighScoreContainer",
                new Color(0.08f, 0.08f, 0.18f, 0.85f),
                new Vector2(0.52f, 0.05f), new Vector2(0.98f, 0.55f));

            highScoreText = CreateText(highScoreContainer.GetComponent<RectTransform>(), "HighScoreText_RT",
                "RÉCORD: 0", 22f, new Color(0.7f, 0.85f, 1f),
                TextAlignmentOptions.Center,
                new Vector2(0, 0), new Vector2(1, 1),
                new Vector2(5, 0), new Vector2(-5, 0));

            // Timer bar
            GameObject timerContainer = CreateRoundedPanel(headerPanel, "TimerContainer",
                new Color(0.05f, 0.05f, 0.12f, 0.9f),
                new Vector2(0.02f, 0.62f), new Vector2(0.98f, 0.95f));

            // Timer bar background
            RectTransform timerContainerRect = timerContainer.GetComponent<RectTransform>();

            GameObject timerBgObj = new GameObject("TimerBarBg", typeof(RectTransform), typeof(Image));
            timerBgObj.transform.SetParent(timerContainerRect, false);
            RectTransform timerBgRect = timerBgObj.GetComponent<RectTransform>();
            timerBgRect.anchorMin = new Vector2(0.02f, 0.15f);
            timerBgRect.anchorMax = new Vector2(0.82f, 0.85f);
            timerBgRect.offsetMin = Vector2.zero;
            timerBgRect.offsetMax = Vector2.zero;
            Image timerBgImg = timerBgObj.GetComponent<Image>();
            timerBgImg.color = new Color(0.15f, 0.15f, 0.25f, 1f);

            // Timer bar fill
            GameObject timerFillObj = new GameObject("TimerBarFill_RT", typeof(RectTransform), typeof(Image));
            timerFillObj.transform.SetParent(timerBgRect, false);
            RectTransform timerFillRect = timerFillObj.GetComponent<RectTransform>();
            timerFillRect.anchorMin = Vector2.zero;
            timerFillRect.anchorMax = Vector2.one;
            timerFillRect.offsetMin = new Vector2(2, 2);
            timerFillRect.offsetMax = new Vector2(-2, -2);
            timerBarFill = timerFillObj.GetComponent<Image>();
            timerBarFill.color = new Color(0.3f, 0.85f, 0.45f);
            timerBarFill.type = Image.Type.Filled;
            timerBarFill.fillMethod = Image.FillMethod.Horizontal;
            timerBarFill.fillAmount = 1f;

            // Timer text
            timerText = CreateText(timerContainerRect, "TimerText_RT",
                "60s", 28f, Color.white,
                TextAlignmentOptions.Center,
                new Vector2(0.83f, 0.1f), new Vector2(0.98f, 0.9f),
                Vector2.zero, Vector2.zero);

            // === CHAIN INFO TEXT (center screen) ===
            chainInfoText = CreateText(canvasRect, "ChainInfoText_RT",
                "", 28f, new Color(1f, 0.9f, 0.3f),
                TextAlignmentOptions.Center,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-200, 200), new Vector2(200, 240));
            chainInfoText.gameObject.SetActive(false);
            chainInfoText.fontStyle = FontStyles.Bold;
            chainInfoText.enableAutoSizing = true;
            chainInfoText.fontSizeMin = 18;
            chainInfoText.fontSizeMax = 36;

            // === COMBO BANNER TEXT ===
            comboBannerText = CreateText(canvasRect, "ComboBannerText_RT",
                "", 56f, new Color(1f, 0.85f, 0.2f),
                TextAlignmentOptions.Center,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-300, 50), new Vector2(300, 120));
            comboBannerText.fontStyle = FontStyles.Bold;
            comboBannerText.enableAutoSizing = true;
            comboBannerText.fontSizeMin = 32;
            comboBannerText.fontSizeMax = 64;
            AddOutline(comboBannerText.gameObject, new Color(0.6f, 0.1f, 0.1f), 3f);
            comboBannerText.gameObject.SetActive(false);

            // === GAME OVER PANEL ===
            BuildGameOverPanel(canvasRect);
        }

        private void BuildGameOverPanel(RectTransform canvasRect)
        {
            // Full screen overlay
            GameObject overlay = new GameObject("GameOverPanel_RT", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            overlay.transform.SetParent(canvasRect, false);
            RectTransform overlayRect = overlay.GetComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = Vector2.zero;
            overlayRect.offsetMax = Vector2.zero;
            Image overlayImg = overlay.GetComponent<Image>();
            overlayImg.color = new Color(0.05f, 0.02f, 0.1f, 0.88f);
            gameOverPanel = overlay;
            gameOverCanvasGroup = overlay.GetComponent<CanvasGroup>();

            // Center container
            GameObject centerBox = CreateRoundedPanel(overlayRect, "GOCenterBox",
                new Color(0.12f, 0.1f, 0.22f, 0.95f),
                new Vector2(0.1f, 0.2f), new Vector2(0.9f, 0.8f));
            RectTransform centerRect = centerBox.GetComponent<RectTransform>();
            AddOutline(centerBox, new Color(1f, 0.7f, 0.2f, 0.5f), 2f);

            // "GAME OVER" title
            CreateText(centerRect, "GOTitle",
                "¡TIEMPO!", 52f, new Color(1f, 0.4f, 0.3f),
                TextAlignmentOptions.Center,
                new Vector2(0.05f, 0.72f), new Vector2(0.95f, 0.95f),
                Vector2.zero, Vector2.zero).fontStyle = FontStyles.Bold;

            // Final Score
            finalScoreText = CreateText(centerRect, "FinalScoreText_RT",
                "0", 72f, new Color(1f, 0.95f, 0.3f),
                TextAlignmentOptions.Center,
                new Vector2(0.05f, 0.42f), new Vector2(0.95f, 0.72f),
                Vector2.zero, Vector2.zero);
            finalScoreText.fontStyle = FontStyles.Bold;

            CreateText(centerRect, "FinalScoreLabel",
                "PUNTUACIÓN FINAL", 20f, new Color(1f, 1f, 1f, 0.5f),
                TextAlignmentOptions.Center,
                new Vector2(0.1f, 0.36f), new Vector2(0.9f, 0.44f),
                Vector2.zero, Vector2.zero);

            // New Record banner
            newRecordBanner = CreateText(centerRect, "NewRecordBanner_RT",
                "🏆 ¡NUEVO RÉCORD! 🏆", 30f, new Color(1f, 0.85f, 0.1f),
                TextAlignmentOptions.Center,
                new Vector2(0.05f, 0.28f), new Vector2(0.95f, 0.38f),
                Vector2.zero, Vector2.zero);
            newRecordBanner.fontStyle = FontStyles.Bold;

            // Play Again Button
            GameObject btnObj = new GameObject("PlayAgainBtn_RT", typeof(RectTransform), typeof(Image), typeof(Button));
            btnObj.transform.SetParent(centerRect, false);
            RectTransform btnRect = btnObj.GetComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.15f, 0.06f);
            btnRect.anchorMax = new Vector2(0.85f, 0.24f);
            btnRect.offsetMin = Vector2.zero;
            btnRect.offsetMax = Vector2.zero;
            Image btnImg = btnObj.GetComponent<Image>();
            btnImg.color = new Color(0.2f, 0.75f, 0.35f);

            TextMeshProUGUI btnText = CreateText(btnRect, "BtnLabel",
                "JUGAR DE NUEVO", 28f, Color.white,
                TextAlignmentOptions.Center,
                Vector2.zero, Vector2.one,
                new Vector2(5, 2), new Vector2(-5, -2));
            btnText.fontStyle = FontStyles.Bold;

            playAgainButton = btnObj.GetComponent<Button>();
            playAgainButton.onClick.AddListener(() => OnRestartRequested?.Invoke());

            // Ensure it's on top
            overlay.transform.SetAsLastSibling();
        }

        // === HELPER METHODS ===

        private RectTransform CreatePanel(RectTransform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, float height)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(0.5f, 1f);
            rect.offsetMin = new Vector2(offsetMin.x, 0);
            rect.offsetMax = new Vector2(offsetMax.x, 0);
            rect.sizeDelta = new Vector2(rect.sizeDelta.x, height);
            rect.anchoredPosition = new Vector2(0, offsetMin.y);
            return rect;
        }

        private GameObject CreateRoundedPanel(RectTransform parent, string name, Color color,
            Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            Image img = go.GetComponent<Image>();
            img.color = color;
            return go;
        }

        private TextMeshProUGUI CreateText(RectTransform parent, string name,
            string text, float fontSize, Color color, TextAlignmentOptions alignment,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = alignment;
            tmp.enableWordWrapping = false;
            tmp.overflowMode = TextOverflowModes.Overflow;
            return tmp;
        }

        private void AddOutline(GameObject go, Color color, float width)
        {
            Outline outline = go.AddComponent<Outline>();
            outline.effectColor = color;
            outline.effectDistance = new Vector2(width, -width);
        }

        // === UPDATE ===

        private void Update()
        {
            if (displayedScore != targetScore)
            {
                displayedScore = (int)Mathf.MoveTowards(displayedScore, targetScore,
                    Mathf.Max(100f, Mathf.Abs(targetScore - displayedScore) * 10f * Time.deltaTime));
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
                timerText.color = remainingSeconds <= 10f ? new Color(1f, 0.25f, 0.25f) : Color.white;

                // Pulse effect when low time
                if (remainingSeconds <= 10f)
                {
                    float pulse = 1f + Mathf.Sin(Time.time * 8f) * 0.1f;
                    timerText.transform.localScale = Vector3.one * pulse;
                }
                else
                {
                    timerText.transform.localScale = Vector3.one;
                }
            }

            if (timerBarFill != null)
            {
                timerBarFill.fillAmount = Mathf.Clamp01(progress01);

                if (remainingSeconds <= 10f)
                {
                    timerBarFill.color = Color.Lerp(
                        new Color(1f, 0.2f, 0.2f),
                        new Color(1f, 0.6f, 0.1f),
                        Mathf.Sin(Time.time * 10f) * 0.5f + 0.5f);
                }
                else if (remainingSeconds <= 30f)
                {
                    timerBarFill.color = new Color(1f, 0.75f, 0.2f);
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
                string emoji = "";
                switch(typeName)
                {
                    case "Dog": emoji = "🐕"; break;
                    case "Bone": emoji = "🦴"; break;
                    case "Ball": emoji = "⚽"; break;
                    case "Food": emoji = "🍖"; break;
                    case "Collar": emoji = "📿"; break;
                }
                chainInfoText.text = $"{emoji} Cadena: {count}";
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

        private IEnumerator AnimateComboBanner()
        {
            float duration = 1.4f;
            float elapsed = 0f;
            Transform tr = comboBannerText.transform;
            Vector3 startScale = Vector3.one * 0.5f;
            Vector3 peakScale = Vector3.one * 1.35f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                if (t < 0.15f)
                {
                    tr.localScale = Vector3.Lerp(startScale, peakScale, t / 0.15f);
                }
                else if (t < 0.3f)
                {
                    tr.localScale = Vector3.Lerp(peakScale, Vector3.one, (t - 0.15f) / 0.15f);
                }
                else
                {
                    tr.localScale = Vector3.one;
                    float alpha = Mathf.Lerp(1f, 0f, (t - 0.7f) / 0.3f);
                    comboBannerText.alpha = Mathf.Max(0, alpha);
                }
                yield return null;
            }

            comboBannerText.alpha = 1f;
            comboBannerText.gameObject.SetActive(false);
        }

        public void ShowGameOver(int finalScore, bool isNewRecord)
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
                gameOverPanel.transform.SetAsLastSibling();
            }
            if (finalScoreText != null)
            {
                finalScoreText.text = $"{finalScore:N0}";
                StartCoroutine(AnimateScoreCount(finalScore));
            }
            if (newRecordBanner != null) newRecordBanner.gameObject.SetActive(isNewRecord);
        }

        private IEnumerator AnimateScoreCount(int target)
        {
            float duration = 1.5f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                // Ease out
                float easedT = 1f - (1f - t) * (1f - t);
                int displayVal = Mathf.RoundToInt(Mathf.Lerp(0, target, easedT));
                if (finalScoreText != null) finalScoreText.text = $"{displayVal:N0}";
                yield return null;
            }
            if (finalScoreText != null) finalScoreText.text = $"{target:N0}";
        }

        public void HideGameOver()
        {
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
        }
    }
}
