using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using DogCrush.Board;
using DogCrush.Core;
using DogCrush.Gameplay;
using DogCrush.InputSystem;
using DogCrush.Presentation;
using DogCrush.UI;

namespace DogCrush.EditorTool
{
    public static class DogCrushProjectSetup
    {
        [MenuItem("DOGCRUSH/Build Playable Prototype")]
        public static void BuildPrototype()
        {
            Debug.Log("[DOGCRUSH] Starting automated prototype setup...");

            EnsureDirectoriesExist();
            Sprite dogSprite = CreateIconTexture("dog_icon", 128, Color.white, IconShape.Dog);
            Sprite boneSprite = CreateIconTexture("bone_icon", 128, Color.white, IconShape.Bone);
            Sprite ballSprite = CreateIconTexture("ball_icon", 128, Color.white, IconShape.Ball);
            Sprite foodSprite = CreateIconTexture("food_icon", 128, Color.white, IconShape.Food);
            Sprite collarSprite = CreateIconTexture("collar_icon", 128, Color.white, IconShape.Collar);
            Sprite glowSprite = CreateGlowTexture("piece_glow", 128);

            BoardConfig config = CreateOrLoadBoardConfig();
            PieceView piecePrefab = CreateOrUpdatePiecePrefab(glowSprite);

            SetupGameplayScene(config, piecePrefab, dogSprite, boneSprite, ballSprite, foodSprite, collarSprite);

            AddSceneToBuildSettings("Assets/_DogCrush/Scenes/Gameplay.unity");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[DOGCRUSH] Prototype setup completed successfully! Open Assets/_DogCrush/Scenes/Gameplay.unity and press Play.");
        }

        private static void EnsureDirectoriesExist()
        {
            string[] dirs = new string[]
            {
                "Assets/_DogCrush/Art/Pieces",
                "Assets/_DogCrush/Art/Backgrounds",
                "Assets/_DogCrush/Art/UI",
                "Assets/_DogCrush/Data",
                "Assets/_DogCrush/Prefabs/Pieces",
                "Assets/_DogCrush/Prefabs/UI",
                "Assets/_DogCrush/Scenes"
            };

            foreach (var dir in dirs)
            {
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
            }
        }

        private enum IconShape { Dog, Bone, Ball, Food, Collar }

        private static Sprite CreateIconTexture(string name, int size, Color color, IconShape shape)
        {
            string path = $"Assets/_DogCrush/Art/Pieces/{name}.png";
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[size * size];

            float center = size / 2f;
            float radius = size * 0.42f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    bool draw = false;

                    switch (shape)
                    {
                        case IconShape.Dog:
                            // Circle head + two ear protrusions
                            draw = (dist <= radius) ||
                                   (dx < -center * 0.3f && dy > center * 0.1f && dist <= radius * 1.25f) ||
                                   (dx > center * 0.3f && dy > center * 0.1f && dist <= radius * 1.25f);
                            break;

                        case IconShape.Bone:
                            // Rounded bar + 4 corner circles
                            bool bar = Mathf.Abs(dy) <= radius * 0.3f && Mathf.Abs(dx) <= radius * 0.8f;
                            bool c1 = Vector2.Distance(new Vector2(dx, dy), new Vector2(-radius * 0.7f, radius * 0.4f)) <= radius * 0.35f;
                            bool c2 = Vector2.Distance(new Vector2(dx, dy), new Vector2(-radius * 0.7f, -radius * 0.4f)) <= radius * 0.35f;
                            bool c3 = Vector2.Distance(new Vector2(dx, dy), new Vector2(radius * 0.7f, radius * 0.4f)) <= radius * 0.35f;
                            bool c4 = Vector2.Distance(new Vector2(dx, dy), new Vector2(radius * 0.7f, -radius * 0.4f)) <= radius * 0.35f;
                            draw = bar || c1 || c2 || c3 || c4;
                            break;

                        case IconShape.Ball:
                            // Circle + tennis ball curved stripe
                            draw = dist <= radius;
                            break;

                        case IconShape.Food:
                            // Bowl shape (half circle)
                            draw = (dist <= radius && dy <= radius * 0.2f) || (Mathf.Abs(dx) <= radius * 0.8f && dy >= radius * 0.2f && dy <= radius * 0.4f);
                            break;

                        case IconShape.Collar:
                            // Ring shape
                            draw = dist <= radius && dist >= radius * 0.45f;
                            break;
                    }

                    if (draw)
                    {
                        // Add smooth anti-aliased edge & white inner icon highlight
                        pixels[y * size + x] = Color.white;
                    }
                    else
                    {
                        pixels[y * size + x] = Color.clear;
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();

            File.WriteAllBytes(path, tex.EncodeToPNG());
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static Sprite CreateGlowTexture(string name, int size)
        {
            string path = $"Assets/_DogCrush/Art/Backgrounds/{name}.png";
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[size * size];

            float center = size / 2f;
            float maxDist = size / 2f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    float alpha = Mathf.Clamp01(1f - (dist / maxDist));
                    alpha = Mathf.Pow(alpha, 2f);
                    pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static BoardConfig CreateOrLoadBoardConfig()
        {
            string path = "Assets/_DogCrush/Data/DefaultBoardConfig.asset";
            BoardConfig config = AssetDatabase.LoadAssetAtPath<BoardConfig>(path);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<BoardConfig>();
                config.columns = 7;
                config.rows = 9;
                config.typeCount = 5;
                config.pieceSpacing = 1.15f;
                config.fallSpeed = 14.0f;
                config.minChainLength = 3;
                config.gameDurationSeconds = 60.0f;
                AssetDatabase.CreateAsset(config, path);
            }
            return config;
        }

        private static PieceView CreateOrUpdatePiecePrefab(Sprite glowSprite)
        {
            string prefabPath = "Assets/_DogCrush/Prefabs/Pieces/PiecePrefab.prefab";

            GameObject go = new GameObject("PiecePrefab");
            SpriteRenderer mainSr = go.AddComponent<SpriteRenderer>();
            mainSr.sortingOrder = 5;

            CircleCollider2D col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.45f;

            GameObject glowGo = new GameObject("SelectionGlow");
            glowGo.transform.SetParent(go.transform, false);
            SpriteRenderer glowSr = glowGo.AddComponent<SpriteRenderer>();
            glowSr.sprite = glowSprite;
            glowSr.color = new Color(1f, 0.9f, 0.3f, 0.6f);
            glowSr.sortingOrder = 4;
            glowGo.transform.localScale = Vector3.one * 1.3f;
            glowGo.SetActive(false);

            PieceView view = go.AddComponent<PieceView>();
            view.mainRenderer = mainSr;
            view.selectionGlow = glowSr;

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            Object.DestroyImmediate(go);

            return prefab.GetComponent<PieceView>();
        }

        private static void SetupGameplayScene(BoardConfig config, PieceView piecePrefab,
            Sprite dog, Sprite bone, Sprite ball, Sprite food, Sprite collar)
        {
            string scenePath = "Assets/_DogCrush/Scenes/Gameplay.unity";
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 1. Camera Setup
            GameObject camGo = new GameObject("Main Camera");
            Camera cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 6.8f;
            cam.backgroundColor = new Color(0.08f, 0.12f, 0.18f); // Dark cozy background
            cam.tag = "MainCamera";
            camGo.AddComponent<AudioListener>();

            // 2. Game Managers
            GameObject managerGo = new GameObject("[GameManager]");
            GameStateController stateController = managerGo.AddComponent<GameStateController>();
            BoardController boardController = managerGo.AddComponent<BoardController>();
            BoardGravityController gravityController = managerGo.AddComponent<BoardGravityController>();
            ChainSelectionController selectionController = managerGo.AddComponent<ChainSelectionController>();
            ChainInputHandler inputHandler = managerGo.AddComponent<ChainInputHandler>();
            ScoreController scoreController = managerGo.AddComponent<ScoreController>();
            GameTimer gameTimer = managerGo.AddComponent<GameTimer>();
            FeedbackController feedbackController = managerGo.AddComponent<FeedbackController>();
            AudioPlaceholderController audioController = managerGo.AddComponent<AudioPlaceholderController>();
            GameBootstrap bootstrap = managerGo.AddComponent<GameBootstrap>();

            // 3. Piece Spawner
            GameObject spawnerGo = new GameObject("[PieceSpawner]");
            PieceSpawner spawner = spawnerGo.AddComponent<PieceSpawner>();
            spawner.piecePrefab = piecePrefab;
            spawner.piecesContainer = new GameObject("[BoardContainer]").transform;

            spawner.dogSprite = dog;
            spawner.boneSprite = bone;
            spawner.ballSprite = ball;
            spawner.foodSprite = food;
            spawner.collarSprite = collar;

            // 4. Line View
            GameObject lineGo = new GameObject("[ChainLineView]");
            LineRenderer lr = lineGo.AddComponent<LineRenderer>();
            ChainLineView lineView = lineGo.AddComponent<ChainLineView>();
            lineView.lineRenderer = lr;
            lr.startColor = new Color(1f, 0.85f, 0.2f, 0.85f);
            lr.endColor = new Color(1f, 0.5f, 0.1f, 0.85f);

            // Wire Controller Dependencies
            boardController.config = config;
            boardController.spawner = spawner;

            gravityController.boardController = boardController;
            gravityController.spawner = spawner;

            selectionController.boardController = boardController;
            selectionController.inputHandler = inputHandler;
            selectionController.lineView = lineView;

            bootstrap.stateController = stateController;
            bootstrap.boardController = boardController;
            bootstrap.gravityController = gravityController;
            bootstrap.selectionController = selectionController;
            bootstrap.scoreController = scoreController;
            bootstrap.gameTimer = gameTimer;
            bootstrap.feedbackController = feedbackController;
            bootstrap.audioController = audioController;

            // 5. Canvas UI Setup
            Canvas canvas = CreateGameplayCanvas(bootstrap, feedbackController);
            feedbackController.uiCanvas = canvas;
            feedbackController.mainCamera = cam;

            EditorSceneManager.SaveScene(scene, scenePath);
        }

        private static Canvas CreateGameplayCanvas(GameBootstrap bootstrap, FeedbackController feedbackController)
        {
            GameObject canvasGo = new GameObject("[Canvas]");
            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGo.AddComponent<GraphicRaycaster>();

            // Safe Area Panel
            GameObject safeGo = new GameObject("SafeAreaPanel", typeof(RectTransform));
            safeGo.transform.SetParent(canvasGo.transform, false);
            RectTransform safeRect = safeGo.GetComponent<RectTransform>();
            safeRect.anchorMin = Vector2.zero;
            safeRect.anchorMax = Vector2.one;
            safeRect.sizeDelta = Vector2.zero;
            safeGo.AddComponent<SafeAreaHandler>();

            GameplayUIController uiController = safeGo.AddComponent<GameplayUIController>();
            bootstrap.uiController = uiController;

            // Top Header Panel
            GameObject headerGo = new GameObject("HeaderHUD", typeof(RectTransform));
            headerGo.transform.SetParent(safeGo.transform, false);
            RectTransform headerRect = headerGo.GetComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0.05f, 0.88f);
            headerRect.anchorMax = new Vector2(0.95f, 0.98f);
            headerRect.sizeDelta = Vector2.zero;

            uiController.scoreText = CreateTMPText("ScoreText", headerGo.transform, "Puntos: 0", 38, TextAlignmentOptions.Left, new Vector2(0, 0.5f), new Vector2(0.5f, 1f));
            uiController.highScoreText = CreateTMPText("HighScoreText", headerGo.transform, "Récord: 0", 28, TextAlignmentOptions.Left, new Vector2(0, 0f), new Vector2(0.5f, 0.5f));
            uiController.timerText = CreateTMPText("TimerText", headerGo.transform, "60s", 52, TextAlignmentOptions.Right, new Vector2(0.6f, 0.2f), new Vector2(1f, 1f));

            // Chain Info Banner
            uiController.chainInfoText = CreateTMPText("ChainInfoText", safeGo.transform, "Cadena: 0", 32, TextAlignmentOptions.Center, new Vector2(0.2f, 0.82f), new Vector2(0.8f, 0.87f));

            // Restart Button in HUD
            GameObject btnGo = CreateSimpleButton("HUD_RestartButton", safeGo.transform, "Reiniciar", new Vector2(0.8f, 0.03f), new Vector2(0.95f, 0.08f));
            uiController.hudRestartButton = btnGo.GetComponent<Button>();

            // Game Over Panel Overlay
            GameObject gameOverGo = new GameObject("GameOverOverlay", typeof(RectTransform), typeof(Image));
            gameOverGo.transform.SetParent(safeGo.transform, false);
            RectTransform goRect = gameOverGo.GetComponent<RectTransform>();
            goRect.anchorMin = Vector2.zero;
            goRect.anchorMax = Vector2.one;
            goRect.sizeDelta = Vector2.zero;

            Image bgImg = gameOverGo.GetComponent<Image>();
            bgImg.color = new Color(0f, 0f, 0f, 0.85f);

            CreateTMPText("GameOverTitle", gameOverGo.transform, "¡TIEMPO AGOTADO!", 64, TextAlignmentOptions.Center, new Vector2(0.1f, 0.7f), new Vector2(0.9f, 0.85f));
            uiController.finalScoreText = CreateTMPText("FinalScoreText", gameOverGo.transform, "Puntuación Final:\n0", 42, TextAlignmentOptions.Center, new Vector2(0.1f, 0.5f), new Vector2(0.9f, 0.68f));
            uiController.newRecordBanner = CreateTMPText("NewRecordBanner", gameOverGo.transform, "★ ¡NUEVO RÉCORD! ★", 36, TextAlignmentOptions.Center, new Vector2(0.1f, 0.42f), new Vector2(0.9f, 0.49f));
            uiController.newRecordBanner.color = new Color(1f, 0.85f, 0.2f);

            GameObject playAgainBtn = CreateSimpleButton("PlayAgainButton", gameOverGo.transform, "¡JUGAR OTRA VEZ!", new Vector2(0.25f, 0.22f), new Vector2(0.75f, 0.35f));
            uiController.playAgainButton = playAgainBtn.GetComponent<Button>();

            uiController.gameOverPanel = gameOverGo;

            // Event System
            if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject es = new GameObject("[EventSystem]");
                es.AddComponent<UnityEngine.EventSystems.EventSystem>();
#if ENABLE_INPUT_SYSTEM
                es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
                es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
#endif
            }

            return canvas;
        }

        private static TextMeshProUGUI CreateTMPText(string name, Transform parent, string content, float size, TextAlignmentOptions align, Vector2 minAnchor, Vector2 maxAnchor)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);

            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = minAnchor;
            rect.anchorMax = maxAnchor;
            rect.sizeDelta = Vector2.zero;

            TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.text = content;
            tmp.fontSize = size;
            tmp.alignment = align;
            tmp.color = Color.white;
            tmp.fontStyle = FontStyles.Bold;

            return tmp;
        }

        private static GameObject CreateSimpleButton(string name, Transform parent, string label, Vector2 minAnchor, Vector2 maxAnchor)
        {
            GameObject btnGo = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            btnGo.transform.SetParent(parent, false);

            RectTransform rect = btnGo.GetComponent<RectTransform>();
            rect.anchorMin = minAnchor;
            rect.anchorMax = maxAnchor;
            rect.sizeDelta = Vector2.zero;

            Image img = btnGo.GetComponent<Image>();
            img.color = new Color(0.2f, 0.65f, 0.35f);

            CreateTMPText("Label", btnGo.transform, label, 28, TextAlignmentOptions.Center, Vector2.zero, Vector2.one);

            return btnGo;
        }

        private static void AddSceneToBuildSettings(string scenePath)
        {
            EditorBuildSettingsScene[] original = EditorBuildSettings.scenes;
            foreach (var s in original)
            {
                if (s.path == scenePath) return;
            }

            EditorBuildSettingsScene[] newScenes = new EditorBuildSettingsScene[original.Length + 1];
            System.Array.Copy(original, newScenes, original.Length);
            newScenes[original.Length] = new EditorBuildSettingsScene(scenePath, true);
            EditorBuildSettings.scenes = newScenes;
        }
    }
}
