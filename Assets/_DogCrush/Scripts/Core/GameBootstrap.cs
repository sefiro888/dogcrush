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

        private void Start()
        {
            InitializeGame();
        }

        public void InitializeGame()
        {
            if (stateController == null) stateController = GetComponent<GameStateController>();

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
                gameTimer.StartTimer(60f);
            }

            stateController.ChangeState(GameState.Playing);
        }

        private void HandleChainUpdated(int count, PieceType type)
        {
            if (!stateController.CanSelectPieces()) return;

            if (uiController != null)
            {
                uiController.UpdateChainInfo(count, type.ToString());
            }
            if (audioController != null && count > 1)
            {
                audioController.PlaySelectSound();
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

            int pointsGained = scoreController != null ? scoreController.AddChainScore(chain.Count) : 0;

            if (chain != null && chain.Count > 0)
            {
                Vector3 centerPos = chain[chain.Count / 2].transform.position;

                if (feedbackController != null)
                {
                    feedbackController.SpawnFloatingText(centerPos, $"+{pointsGained:N0}", Color.yellow, 36f);
                }

                if (particleController != null)
                {
                    foreach (var piece in chain)
                    {
                        if (piece != null)
                        {
                            particleController.PlayMatchBurst(piece.transform.position, Color.yellow, 10);
                        }
                    }
                }
            }

            if (audioController != null)
            {
                audioController.PlayMatchSound();
            }

            if (gravityController != null)
            {
                StartCoroutine(gravityController.ProcessRemovalAndRefill(chain, () =>
                {
                    if (gameTimer != null && gameTimer.RemainingTime <= 0)
                    {
                        EndMatch();
                    }
                    else
                    {
                        stateController.ChangeState(GameState.Playing);
                    }
                }));
            }
        }

        private void HandleTimerExpired()
        {
            if (gravityController != null && gravityController.IsResolving)
            {
                return;
            }
            EndMatch();
        }

        private void EndMatch()
        {
            stateController.ChangeState(GameState.GameOver);

            if (gameTimer != null)
            {
                gameTimer.StopTimer();
            }

            if (audioController != null)
            {
                audioController.PlayGameOverSound();
            }

            int finalScore = scoreController != null ? scoreController.CurrentScore : 0;
            int highScore = scoreController != null ? scoreController.HighScore : 0;
            bool isNewRecord = finalScore > 0 && finalScore >= highScore;

            if (uiController != null)
            {
                uiController.ShowGameOver(finalScore, isNewRecord);
            }
        }

        public void RestartGame()
        {
            StartNewMatch();
        }
    }
}
