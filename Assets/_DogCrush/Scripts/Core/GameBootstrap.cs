using System.Collections.Generic;
using DogCrush.Board;
using DogCrush.Gameplay;
using DogCrush.Presentation;
using DogCrush.UI;
using UnityEngine;

namespace DogCrush.Core
{
    public class GameBootstrap : MonoBehaviour
    {
        [Header("Controllers")]
        public GameStateController stateController;
        public BoardController boardController;
        public BoardGravityController gravityController;
        public ChainSelectionController selectionController;
        public ScoreController scoreController;
        public GameTimer gameTimer;

        [Header("Presentation & UI")]
        public GameplayUIController uiController;
        public FeedbackController feedbackController;
        public ParticleEffectController particleController;
        public AudioPlaceholderController audioController;
        public HapticFeedbackController hapticController;

        [Header("Level Progress")]
        [Min(1)] public int currentLevel = 1;
        [Min(100)] public int baseTargetScore = 5000;
        [Min(0)] public int targetIncreasePerLevel = 2500;

        private int CurrentTargetScore =>
            baseTargetScore + Mathf.Max(0, currentLevel - 1) * targetIncreasePerLevel;
        private bool shuffleBoosterAvailable;
        private bool boneBoosterAvailable;
        private bool foodBoosterAvailable;
        private const string UnlockedLevelKey = "DogCrush_UnlockedLevel";
        private const string LevelStarsKeyPrefix = "DogCrush_LevelStars_";

        private void Start()
        {
            InitializeGame();
        }

        public void InitializeGame()
        {
            currentLevel = Mathf.Max(currentLevel, PlayerPrefs.GetInt(UnlockedLevelKey, 1));
            if (stateController == null) stateController = GetComponent<GameStateController>();
            if (audioController == null) audioController = GetComponent<AudioPlaceholderController>();
            if (hapticController == null)
                hapticController = GetComponent<HapticFeedbackController>() ??
                    gameObject.AddComponent<HapticFeedbackController>();

            // Subscribe Events
            if (selectionController != null)
            {
                selectionController.OnChainCompleted += HandleChainCompleted;
                selectionController.OnChainCancelled += HandleChainCancelled;
                selectionController.OnChainUpdated += HandleChainUpdated;
            }

            if (scoreController != null)
            {
                scoreController.OnScoreChanged += (current, added) =>
                {
                    if (uiController != null) uiController.UpdateScore(current);
                };
                scoreController.OnHighScoreChanged += (high) =>
                {
                    if (uiController != null) uiController.UpdateHighScore(high);
                };
                scoreController.OnComboTriggered += (mult, text) =>
                {
                    if (feedbackController != null)
                    {
                        feedbackController.TriggerCameraShake(0.15f, 0.25f);
                    }
                    if (uiController != null)
                    {
                        Color comboColor = mult >= 4 ? new Color(1f, 0.3f, 0.8f) : new Color(1f, 0.85f, 0.2f);
                        uiController.ShowComboBanner(text, comboColor);
                    }
                    if (audioController != null)
                    {
                        audioController.PlayComboSound();
                    }
                };
            }

            if (gameTimer != null)
            {
                gameTimer.OnTimerTick += (remaining) =>
                {
                    if (uiController != null) uiController.UpdateTimer(remaining, gameTimer.Progress01);
                };
                gameTimer.OnTenSecondsLeft += () =>
                {
                    if (audioController != null) audioController.PlayTimerWarningSound();
                };
                gameTimer.OnTimerExpired += HandleTimerExpired;
            }

            if (uiController != null)
            {
                uiController.OnRestartRequested += RestartGame;
                uiController.OnNextLevelRequested += StartNextLevel;
                uiController.OnShuffleBoosterRequested += UseShuffleBooster;
                uiController.OnBoneBoosterRequested += UseBoneBooster;
                uiController.OnFoodBoosterRequested += UseFoodBooster;
                uiController.OnLevelSelected += SelectLevel;
                uiController.OnLevelSelectVisibilityChanged += HandleLevelSelectVisibilityChanged;
                uiController.SetUnlockedLevel(PlayerPrefs.GetInt(UnlockedLevelKey, 1));
                uiController.OnSoundToggleRequested += HandleSoundToggleRequested;
                uiController.OnHapticsToggleRequested += HandleHapticsToggleRequested;
                uiController.OnSettingsVisibilityChanged += HandleSettingsVisibilityChanged;
                uiController.UpdateSettingsState(
                    audioController != null ? audioController.SfxVolume : 0f,
                    hapticController == null || hapticController.HapticsEnabled);
            }

            StartNewMatch();
        }

        public void StartNewMatch()
        {
            stateController.ChangeState(GameState.Initializing);

            if (uiController != null)
            {
                uiController.HideGameOver();
                uiController.UpdateChainInfo(0, "");
                uiController.SetLevelObjective(currentLevel, CurrentTargetScore);
                shuffleBoosterAvailable = true;
                boneBoosterAvailable = true;
                foodBoosterAvailable = true;
                uiController.SetBoosterAvailability(true, true, true);
                uiController.SetSettingsVisible(false);
            }

            if (scoreController != null)
            {
                scoreController.ResetScore();
            }

            if (boardController != null)
            {
                boardController.InitializeBoard();
            }

            if (gameTimer != null)
            {
                float duration = boardController != null && boardController.config != null
                    ? boardController.config.gameDurationSeconds
                    : gameTimer.durationSeconds;
                gameTimer.StartTimer(duration);
            }

            stateController.ChangeState(GameState.Playing);
        }

        private void HandleChainUpdated(int count, PieceType type)
        {
            if (!stateController.CanSelectPieces()) return;

            if (uiController != null)
            {
                List<PieceView> chain = selectionController != null
                    ? selectionController.SelectedChain
                    : null;
                Vector3 lastPiecePosition = chain != null && chain.Count > 0
                    ? chain[chain.Count - 1].transform.position
                    : Vector3.zero;
                uiController.UpdateChainInfo(count, type.ToString(), lastPiecePosition);
            }
            if (audioController != null && count > 1)
            {
                audioController.PlaySelectSound(count);
            }
            if (hapticController != null && count > 1)
            {
                hapticController.PulseSelection();
            }
        }

        private void HandleChainCancelled()
        {
            if (uiController != null)
            {
                uiController.UpdateChainInfo(0, "");
            }
        }

        private void HandleChainCompleted(List<PieceView> chain)
        {
            if (!stateController.CanSelectPieces()) return;

            stateController.ChangeState(GameState.Resolving);
            if (uiController != null)
            {
                uiController.UpdateChainInfo(0, "");
            }

            int pointsGained = scoreController != null ? scoreController.AddChainScore(chain.Count) : 0;

            if (chain != null && chain.Count > 0)
            {
                Vector3 centerPos = chain[chain.Count / 2].transform.position;

                if (feedbackController != null)
                {
                    feedbackController.SpawnFloatingText(centerPos, $"+{pointsGained:N0}", Color.yellow, 36f);
                    feedbackController.TriggerCameraShake(
                        Mathf.Clamp(0.025f + chain.Count * 0.004f, 0.035f, 0.075f),
                        0.14f);
                }

                if (particleController != null)
                {
                    int burstCount = Mathf.Clamp(6 + chain.Count, 9, 16);
                    foreach (var piece in chain)
                    {
                        if (piece != null)
                        {
                            particleController.PlayMatchBurst(
                                piece.transform.position,
                                GetPieceAccentColor(piece.type),
                                burstCount);
                        }
                    }
                }
            }

            if (audioController != null)
            {
                audioController.PlayMatchSound(chain != null ? chain.Count : 3);
            }
            if (hapticController != null)
            {
                hapticController.PulseMatch(chain != null ? chain.Count : 3);
            }

            if (gravityController != null)
            {
                StartCoroutine(gravityController.ProcessRemovalAndRefill(chain, () =>
                {
                    if (scoreController != null && scoreController.CurrentScore >= CurrentTargetScore)
                    {
                        EndMatch(true);
                    }
                    else if (gameTimer != null && gameTimer.RemainingTime <= 0)
                    {
                        EndMatch(false);
                    }
                    else
                    {
                        stateController.ChangeState(GameState.Playing);
                    }
                }));
            }
        }

        private static Color GetPieceAccentColor(PieceType type)
        {
            return type switch
            {
                PieceType.Dog => new Color(1f, 0.66f, 0.18f),
                PieceType.Bone => new Color(1f, 0.95f, 0.72f),
                PieceType.Ball => new Color(0.24f, 0.78f, 1f),
                PieceType.Food => new Color(1f, 0.34f, 0.28f),
                PieceType.Collar => new Color(0.32f, 0.95f, 0.48f),
                _ => new Color(1f, 0.85f, 0.2f)
            };
        }

        private void HandleTimerExpired()
        {
            if (gravityController != null && gravityController.IsResolving)
            {
                return;
            }
            EndMatch(false);
        }

        private void EndMatch(bool victory)
        {
            stateController.ChangeState(GameState.GameOver);

            if (gameTimer != null)
            {
                gameTimer.StopTimer();
            }

            if (audioController != null)
            {
                if (victory)
                    audioController.PlayComboSound();
                else
                    audioController.PlayGameOverSound();
            }
            if (hapticController != null)
            {
                if (victory)
                    hapticController.PulseMatch(8);
                else
                    hapticController.PulseGameOver();
            }

            int finalScore = scoreController != null ? scoreController.CurrentScore : 0;
            int highScore = scoreController != null ? scoreController.HighScore : 0;
            bool isNewRecord = finalScore > 0 && finalScore >= highScore;

            if (uiController != null)
            {
                int stars = CalculateStars();
                if (victory)
                {
                    string starsKey = LevelStarsKeyPrefix + currentLevel;
                    int previousStars = PlayerPrefs.GetInt(starsKey, 0);
                    PlayerPrefs.SetInt(starsKey, Mathf.Max(previousStars, stars));
                    PlayerPrefs.SetInt(UnlockedLevelKey, Mathf.Max(
                        PlayerPrefs.GetInt(UnlockedLevelKey, 1), currentLevel + 1));
                    PlayerPrefs.Save();
                }
                uiController.ShowLevelResult(victory, finalScore, isNewRecord, stars);
            }
        }

        private int CalculateStars()
        {
            if (gameTimer == null || gameTimer.durationSeconds <= 0f)
            {
                return 1;
            }

            float timeRatio = gameTimer.RemainingTime / gameTimer.durationSeconds;
            if (timeRatio >= 0.60f) return 3;
            if (timeRatio >= 0.30f) return 2;
            return 1;
        }

        public void RestartGame()
        {
            StartNewMatch();
        }

        public void StartNextLevel()
        {
            currentLevel++;
            StartNewMatch();
        }

        private void SelectLevel(int level)
        {
            int unlockedLevel = PlayerPrefs.GetInt(UnlockedLevelKey, 1);
            if (level < 1 || level > unlockedLevel) return;
            currentLevel = level;
            StartNewMatch();
        }

        private void HandleLevelSelectVisibilityChanged(bool visible)
        {
            gameTimer?.SetPaused(visible);
        }

        private void UseShuffleBooster()
        {
            if (!shuffleBoosterAvailable || stateController == null || !stateController.CanSelectPieces()) return;
            shuffleBoosterAvailable = false;
            boardController?.ShuffleBoardTypes();
            boardController?.EnsureHasValidMoves();
            uiController?.SetBoosterAvailability(false, boneBoosterAvailable, foodBoosterAvailable);
            audioController?.PlayUISound();
            hapticController?.PulseSelection();
        }

        private void UseFoodBooster()
        {
            if (!foodBoosterAvailable || stateController == null || !stateController.CanSelectPieces()) return;
            foodBoosterAvailable = false;
            gameTimer?.AddTime(10f);
            uiController?.SetBoosterAvailability(shuffleBoosterAvailable, boneBoosterAvailable, false);
            audioController?.PlayUISound();
            hapticController?.PulseSelection();
        }

        private void UseBoneBooster()
        {
            if (!boneBoosterAvailable || stateController == null || !stateController.CanSelectPieces() || boardController == null || gravityController == null) return;
            List<PieceView> row = boardController.GetRowPieces(boardController.Rows / 2);
            if (row.Count == 0) return;
            boneBoosterAvailable = false;
            stateController.ChangeState(GameState.Resolving);
            uiController?.SetBoosterAvailability(shuffleBoosterAvailable, false, foodBoosterAvailable);
            StartCoroutine(gravityController.ProcessRemovalAndRefill(row, () =>
            {
                if (gameTimer != null && gameTimer.RemainingTime <= 0f) EndMatch(false);
                else stateController.ChangeState(GameState.Playing);
            }));
            audioController?.PlayMatchSound(row.Count);
            hapticController?.PulseMatch(row.Count);
        }

        private void HandleSoundToggleRequested()
        {
            if (audioController == null)
            {
                return;
            }

            float volume = audioController.CycleSfxVolume();
            uiController?.UpdateSettingsState(
                volume,
                hapticController == null || hapticController.HapticsEnabled);
        }

        private void HandleHapticsToggleRequested()
        {
            if (hapticController == null)
            {
                return;
            }

            bool enabled = hapticController.ToggleHaptics();
            if (enabled)
            {
                hapticController.PulseSelection();
            }
            audioController?.PlayUISound();
            uiController?.UpdateSettingsState(
                audioController != null ? audioController.SfxVolume : 0f,
                enabled);
        }

        private void HandleSettingsVisibilityChanged(bool visible)
        {
            gameTimer?.SetPaused(visible);
            if (visible)
            {
                audioController?.PlayUISound();
                uiController?.UpdateSettingsState(
                    audioController != null ? audioController.SfxVolume : 0f,
                    hapticController == null || hapticController.HapticsEnabled);
            }
        }
    }
}
