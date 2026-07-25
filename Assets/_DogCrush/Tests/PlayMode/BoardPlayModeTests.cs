using System.Collections;
using DogCrush.Board;
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
            Assert.That(board.Columns * board.Rows, Is.EqualTo(63));

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

            Assert.That(activePieces, Is.EqualTo(63), "The initial board must contain 63 active pieces.");
        }
    }
}
