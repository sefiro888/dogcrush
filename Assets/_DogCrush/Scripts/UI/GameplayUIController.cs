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

        private Canvas runtimeCanvas;
        private Image timerBarGlow;
        private Image bottomPillBg;
        private TextMeshProUGUI bottomPillText;

        private void Awake()
        {
            // 1. Completely strip and hide ALL old UI elements in Canvas/SafeArea
            CleanOldSceneUI();

            // 2. Build gorgeous reference-matching UI from scratch
            BuildRuntimeUI();

            HideGameOver();
            if (comboBannerText != null) comboBannerText.gameObject.SetActive(false);
        }

        private void CleanOldSceneUI()
        {
            Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (Canvas canvas in canvases)
            {
                // Disable all pre-existing children in the canvas tree
                for (int i = canvas.transform.childCount - 1; i >= 0; i--)
                {
                    Transform child = canvas.transform.GetChild(i);
                    if (!child.name.EndsWith("_RT"))
                    {
                        // Clean inside sub-panels like SafeAreaPanel
                        DisableAllChildrenRecursive(child);
                        child.gameObject.SetActive(false);
                    }
                }
            }
        }

        private void DisableAllChildrenRecursive(Transform parent)
        {
            foreach (Transform t in parent)
            {
                if (!t.name.EndsWith("_RT"))
                {
                    t.gameObject.SetActive(false);
                    DisableAllChildrenRecursive(t);
                }
            }
        }

        private void BuildRuntimeUI()
        {
            runtimeCanvas = FindAnyObjectByType<Canvas>();
            if (runtimeCanvas == null)
            {
                GameObject canvasObj = new GameObject("Canvas_RT", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                runtimeCanvas = canvasObj.GetComponent<Canvas>();
                runtimeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 1920);
                scaler.matchWidthOrHeight = 0.5f;
            }

            RectTransform canvasRect = runtimeCanvas.GetComponent<RectTransform>();

            // === TOP TIME BAR CAPSULE (Matches reference image) ===
            GameObject topBarOuter = new GameObject("TopBarOuter_RT", typeof(RectTransform), typeof(Image));
            topBarOuter.transform.SetParent(canvasRect, false);
            RectTransform topOuterRect = topBarOuter.GetComponent<RectTransform>();
            topOuterRect.anchorMin = new Vector2(0.04f, 0.93f);
            topOuterRect.anchorMax = new Vector2(0.96f, 0.975f);
            topOuterRect.offsetMin = Vector2.zero;
            topOuterRect.offsetMax = Vector2.zero;
            
            // Outer dark metallic capsule frame
            Image topOuterImg = topBarOuter.GetComponent<Image>();
            topOuterImg.color = new Color(0.1f, 0.18f, 0.25f, 0.95f);

            // Inner dark track
            GameObject topBarTrack = new GameObject("TopBarTrack_RT", typeof(RectTransform), typeof(Image));
            topBarTrack.transform.SetParent(topOuterRect, false);
            RectTransform trackRect = topBarTrack.GetComponent<RectTransform>();
            trackRect.anchorMin = Vector2.zero;
            trackRect.anchorMax = Vector2.one;
            trackRect.offsetMin = new Vector2(4, 4);
            trackRect.offsetMax = new Vector2(-4, -4);
            Image trackImg = topBarTrack.GetComponent<Image>();
            trackImg.color = new Color(0.05f, 0.08f, 0.12f, 1f);

            // Green gradient fill bar
            GameObject fillObj = new GameObject("TimerBarFill_RT", typeof(RectTransform), typeof(Image));
            fillObj.transform.SetParent(trackRect, false);
            RectTransform fillRect = fillObj.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            timerBarFill = fillObj.GetComponent<Image>();
            timerBarFill.color = new Color(0.25f, 0.9f, 0.35f, 1f);
            timerBarFill.type = Image.Type.Filled;
            timerBarFill.fillMethod = Image.FillMethod.Horizontal;
            timerBarFill.fillAmount = 1f;

            // Timer text overlay (Center of top capsule)
            timerText = CreateText(topOuterRect, "TimerText_RT",
                "60s", 28f, Color.white,
                TextAlignmentOptions.Center,
                Vector2.zero, Vector2.one,
                Vector2.zero, Vector2.zero);
            timerText.fontStyle = FontStyles.Bold;

            // === BOTTOM INFO CAPSULE (Matches reference image) ===
            GameObject bottomPillObj = new GameObject("BottomPill_RT", typeof(RectTransform), typeof(Image));
            bottomPillObj.transform.SetParent(canvasRect, false);
            RectTransform bottomPillRect = bottomPillObj.GetComponent<RectTransform>();
            bottomPillRect.anchorMin = new Vector2(0.12f, 0.02f);
            bottomPillRect.anchorMax = new Vector2(0.88f, 0.07f);
            bottomPillRect.offsetMin = Vector2.zero;
            bottomPillRect.offsetMax = Vector2.zero;
            
            bottomPillBg = bottomPillObj.GetComponent<Image>();
            bottomPillBg.color = new Color(0.08f, 0.48f, 0.45f, 0.95f);

            // Inner gloss line
            GameObject glossObj = new GameObject("Gloss_RT", typeof(RectTransform), typeof(Image));
            glossObj.transform.SetParent(bottomPillRect, false);
            RectTransform glossRect = glossObj.GetComponent<RectTransform>();
            glossRect.anchorMin = new Vector2(0.02f, 0.55f);
            glossRect.anchorMax = new Vector2(0.98f, 0.92f);
            glossRect.offsetMin = Vector2.zero;
            glossRect.offsetMax = Vector2.zero;
            Image glossImg = glossObj.GetComponent<Image>();
            glossImg.color = new Color(1f, 1f, 1f, 0.2f);

            // Bottom capsule text (Score & Info)
            scoreText = CreateText(bottomPillRect, "ScoreText_RT",
                "PUNTOS: 0", 32f, new Color(1f, 0.96f, 0.4f),
                TextAlignmentOptions.Center,
                Vector2.zero, Vector2.one,
                Vector2.zero, Vector2.zero);
            scoreText.fontStyle = FontStyles.Bold;

            // High Score text floating top right
            highScoreText = CreateText(canvasRect, "HighScoreText_RT",
                "RÉCORD: 0", 24f, new Color(1f, 1f, 1f, 0.85f),
                TextAlignmentOptions.Right,
                new Vector2(0.5f, 0.89f), new Vector2(0.96f, 0.925f),
                Vector2.zero, Vector2.zero);
            highScoreText.fontStyle = FontStyles.Bold;

            // === CHAIN SELECTION FLOATING TEXT ===
            chainInfoText = CreateText(canvasRect, "ChainInfoText_RT",
                "", 32f, new Color(1f, 0.92f, 0.25f),
                TextAlignmentOptions.Center,
                new Vector2(0.1f, 0.88f), new Vector2(0.5f, 0.925f),
                Vector2.zero, Vector2.zero);
            chainInfoText.fontStyle = FontStyles.Bold;
            chainInfoText.gameObject.SetActive(false);

            // === COMBO BANNER ===
            comboBannerText = CreateText(canvasRect, "ComboBannerText_RT",
                "", 64f, new Color(1f, 0.85f, 0.15f),
                TextAlignmentOptions.Center,
                new Vector2(0.1f, 0.45f), new Vector2(0.9f, 0.55f),
                Vector2.zero, Vector2.zero);
            comboBannerText.fontStyle = FontStyles.Bold;
            comboBannerText.gameObject.SetActive(false);

            // === GAME OVER OVERLAY ===
            BuildGameOverPanel(canvasRect);
        }

        private void BuildGameOverPanel(RectTransform canvasRect)
        {
            GameObject overlay = new GameObject("GameOverPanel_RT", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            overlay.transform.SetParent(canvasRect, false);
            RectTransform overlayRect = overlay.GetComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = Vector2.zero;
            overlayRect.offsetMax = Vector2.zero;

            Image overlayImg = overlay.GetComponent<Image>();
            overlayImg.color = new Color(0.04f, 0.06f, 0.12f, 0.92f);
            gameOverPanel = overlay;

            // Central box
            GameObject centerBox = new GameObject("GOCenterBox_RT", typeof(RectTransform), typeof(Image));
            centerBox.transform.SetParent(overlayRect, false);
            RectTransform centerRect = centerBox.GetComponent<RectTransform>();
            centerRect.anchorMin = new Vector2(0.1f, 0.22f);
            centerRect.anchorMax = new Vector2(0.9f, 0.78f);
            centerRect.offsetMin = Vector2.zero;
            centerRect.offsetMax = Vector2.zero;

            Image boxImg = centerBox.GetComponent<Image>();
            boxImg.color = new Color(0.12f, 0.16f, 0.26f, 0.98f);

            // Title
            CreateText(centerRect, "GOTitle",
                "¡FIN DEL TIEMPO!", 48f, new Color(1f, 0.4f, 0.35f),
                TextAlignmentOptions.Center,
                new Vector2(0.05f, 0.75f), new Vector2(0.95f, 0.95f),
                Vector2.zero, Vector2.zero).fontStyle = FontStyles.Bold;

            // Final score label
            CreateText(centerRect, "FinalLabel",
                "PUNTUACIÓN OBTENIDA", 22f, new Color(0.8f, 0.85f, 0.95f),
                TextAlignmentOptions.Center,
                new Vector2(0.05f, 0.58f), new Vector2(0.95f, 0.72f),
                Vector2.zero, Vector2.zero);

            // Final score display
            finalScoreText = CreateText(centerRect, "FinalScoreText_RT",
                "0", 68f, new Color(1f, 0.92f, 0.25f),
                TextAlignmentOptions.Center,
                new Vector2(0.05f, 0.38f), new Vector2(0.95f, 0.58f),
                Vector2.zero, Vector2.zero);
            finalScoreText.fontStyle = FontStyles.Bold;

            // New Record banner
            newRecordBanner = CreateText(centerRect, "NewRecordBanner_RT",
                "🏆 ¡NUEVO RÉCORD! 🏆", 32f, new Color(0.3f, 0.95f, 0.4f),
                TextAlignmentOptions.Center,
                new Vector2(0.05f, 0.25f), new Vector2(0.95f, 0.36f),
                Vector2.zero, Vector2.zero);
            newRecordBanner.fontStyle = FontStyles.Bold;
            newRecordBanner.gameObject.SetActive(false);

            // Play Again button
            GameObject btnObj = new GameObject("PlayAgainBtn_RT", typeof(RectTransform), typeof(Image), typeof(Button));
            btnObj.transform.SetParent(centerRect, false);
            RectTransform btnRect = btnObj.GetComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.12f, 0.05f);
            btnRect.anchorMax = new Vector2(0.88f, 0.22f);
            btnRect.offsetMin = Vector2.zero;
            btnRect.offsetMax = Vector2.zero;

            Image btnImg = btnObj.GetComponent<Image>();
            btnImg.color = new Color(0.2f, 0.78f, 0.38f);

            TextMeshProUGUI btnText = CreateText(btnRect, "BtnLabel",
                "JUGAR DE NUEVO", 30f, Color.white,
                TextAlignmentOptions.Center,
                Vector2.zero, Vector2.one,
                Vector2.zero, Vector2.zero);
            btnText.fontStyle = FontStyles.Bold;

            playAgainButton = btnObj.GetComponent<Button>();
            playAgainButton.onClick.AddListener(() => OnRestartRequested?.Invoke());
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

        private void Update()
        {
            if (displayedScore != targetScore)
            {
                displayedScore = (int)Mathf.MoveTowards(displayedScore, targetScore,
                    Mathf.Max(100f, Mathf.Abs(targetScore - displayedScore) * 10f * Time.deltaTime));
                if (scoreText != null) scoreText.text = $"PUNTOS: {displayedScore:N0}";
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
            }

            if (timerBarFill != null)
            {
                timerBarFill.fillAmount = Mathf.Clamp01(progress01);

                if (remainingSeconds <= 10f)
                {
                    timerBarFill.color = Color.Lerp(
                        new Color(1f, 0.25f, 0.25f),
                        new Color(1f, 0.65f, 0.15f),
                        Mathf.Sin(Time.time * 10f) * 0.5f + 0.5f);
                }
                else if (remainingSeconds <= 30f)
                {
                    timerBarFill.color = new Color(1f, 0.78f, 0.2f);
                }
                else
                {
                    timerBarFill.color = new Color(0.25f, 0.9f, 0.35f);
                }
            }
        }

        public void UpdateChainInfo(int count, string typeName)
        {
            if (chainInfoText == null) return;

            if (count > 0)
            {
                chainInfoText.gameObject.SetActive(true);
                string icon = "🐾";
                switch (typeName)
                {
                    case "Dog": icon = "🐶"; break;
                    case "Bone": icon = "🦴"; break;
                    case "Ball": icon = "🔵"; break;
                    case "Food": icon = "🍖"; break;
                    case "Collar": icon = "🟢"; break;
                }
                chainInfoText.text = $"{icon} x{count}";
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
            Vector3 peakScale = Vector3.one * 1.3f;

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
