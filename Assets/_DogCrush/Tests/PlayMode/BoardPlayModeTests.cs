using System.Collections;
using DogCrush.Board;
using DogCrush.Core;
using DogCrush.Gameplay;
using DogCrush.InputSystem;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace DogCrush.Tests.PlayMode
{
    public class BoardPlayModeTests
    {
        [UnityTest]
        public IEnumerator GameplayScene_FillsConfiguredBoardWithInteractivePieces()
        {
            SceneManager.LoadScene("Gameplay", LoadSceneMode.Single);
            yield return null;
            yield return null;

            BoardController board = Object.FindAnyObjectByType<BoardController>();
            Assert.That(board, Is.Not.Null, "Gameplay scene must contain a BoardController.");
            Assert.That(board.Grid, Is.Not.Null, "BoardController must initialize its grid.");
            Assert.That(board.Columns * board.Rows, Is.EqualTo(64));

            int activePieces = 0;
            PieceView[] pieces = Object.FindObjectsByType<PieceView>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (PieceView piece in pieces)
            {
                if (piece != null && piece.gameObject.activeInHierarchy)
                {
                    activePieces++;
                    Assert.That(piece.GetComponent<Collider2D>(), Is.Not.Null,
                        "Every board piece must remain interactive.");
                    SpriteRenderer renderer = piece.GetComponent<SpriteRenderer>();
                    Assert.That(renderer, Is.Not.Null);
                    Assert.That(renderer.sprite, Is.Not.Null, "Every board piece must have a sprite.");
                    Assert.That(renderer.sprite.bounds.size.x, Is.GreaterThan(0.5f),
                        "Piece sprite geometry must be large enough to be visible on the board.");
                }
            }

            Assert.That(activePieces, Is.EqualTo(64), "The initial board must contain 64 active pieces.");
        }

        [UnityTest]
        public IEnumerator RestartingMatch_RecyclesPreviousPieces()
        {
            SceneManager.LoadScene("Gameplay", LoadSceneMode.Single);
            yield return null;
            yield return null;

            GameBootstrap bootstrap = Object.FindAnyObjectByType<GameBootstrap>();
            Assert.That(bootstrap, Is.Not.Null);

            bootstrap.RestartGame();
            yield return null;
            yield return null;

            PieceView[] pieces = Object.FindObjectsByType<PieceView>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            int activePieces = 0;
            foreach (PieceView piece in pieces)
            {
                if (piece != null && piece.gameObject.activeInHierarchy)
                {
                    activePieces++;
                }
            }

            Assert.That(activePieces, Is.EqualTo(64),
                "Restarting a match must leave exactly one active set of 64 pieces.");
        }

        [UnityTest]
        public IEnumerator DraggingThreeMatchingPieces_ScoresFallsAndRefills()
        {
            SceneManager.LoadScene("Gameplay", LoadSceneMode.Single);
            yield return null;
            yield return null;

            BoardController board = Object.FindAnyObjectByType<BoardController>();
            ChainSelectionController selection = Object.FindAnyObjectByType<ChainSelectionController>();
            ChainInputHandler input = Object.FindAnyObjectByType<ChainInputHandler>();
            ScoreController score = Object.FindAnyObjectByType<ScoreController>();
            BoardGravityController gravity = Object.FindAnyObjectByType<BoardGravityController>();

            Assert.That(board, Is.Not.Null);
            Assert.That(selection, Is.Not.Null);
            Assert.That(input, Is.Not.Null);
            Assert.That(score, Is.Not.Null);
            Assert.That(gravity, Is.Not.Null);

            PieceView first = null;
            PieceView middle = null;
            PieceView last = null;

            for (int x = 0; x < board.Columns && middle == null; x++)
            {
                for (int y = 0; y < board.Rows && middle == null; y++)
                {
                    PieceView candidate = board.GetPieceAt(x, y);
                    if (candidate == null) continue;

                    PieceView[] matchingNeighbors = new PieceView[8];
                    int neighborCount = 0;
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        for (int dy = -1; dy <= 1; dy++)
                        {
                            if (dx == 0 && dy == 0) continue;
                            PieceView neighbor = board.GetPieceAt(x + dx, y + dy);
                            if (neighbor != null && neighbor.type == candidate.type)
                            {
                                matchingNeighbors[neighborCount++] = neighbor;
                            }
                        }
                    }

                    if (neighborCount >= 2)
                    {
                        first = matchingNeighbors[0];
                        middle = candidate;
                        last = matchingNeighbors[1];
                    }
                }
            }

            Assert.That(middle, Is.Not.Null,
                "The initialized board must expose at least one valid three-piece chain.");

            Physics2D.SyncTransforms();
            foreach (PieceView piece in new[] { first, middle, last })
            {
                Collider2D hit = Physics2D.OverlapPoint(piece.transform.position);
                Assert.That(hit, Is.Not.Null,
                    "Each normalized piece must keep a finger-sized collider at its visual center.");
                Assert.That(hit.GetComponent<PieceView>(), Is.EqualTo(piece));
            }

            input.OnPointerDownEvent?.Invoke(first.transform.position);
            yield return null;
            input.OnPointerDragEvent?.Invoke(middle.transform.position);
            yield return null;
            input.OnPointerDragEvent?.Invoke(last.transform.position);
            yield return null;

            Assert.That(selection.SelectedChain.Count, Is.EqualTo(3),
                "The live selection must contain the three dragged pieces.");

            input.OnPointerUpEvent?.Invoke();

            float timeoutAt = Time.realtimeSinceStartup + 4f;
            while ((gravity.IsResolving || score.CurrentScore == 0) &&
                   Time.realtimeSinceStartup < timeoutAt)
            {
                yield return null;
            }

            Assert.That(score.CurrentScore, Is.GreaterThan(0),
                "Completing a valid chain must award points.");
            Assert.That(gravity.IsResolving, Is.False,
                "Removal, fall and refill must finish.");

            int activePieces = 0;
            for (int x = 0; x < board.Columns; x++)
            {
                for (int y = 0; y < board.Rows; y++)
                {
                    PieceView piece = board.GetPieceAt(x, y);
                    Assert.That(piece, Is.Not.Null,
                        $"Grid position ({x}, {y}) must be refilled.");
                    if (piece.gameObject.activeInHierarchy) activePieces++;
                }
            }

            Assert.That(activePieces, Is.EqualTo(64),
                "A completed chain must refill the board back to 64 active pieces.");
        }
    }
}
