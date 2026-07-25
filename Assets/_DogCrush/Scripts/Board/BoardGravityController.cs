using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DogCrush.Board
{
    public class BoardGravityController : MonoBehaviour
    {
        public BoardController boardController;
        public PieceSpawner spawner;

        public bool IsResolving { get; private set; }

        public IEnumerator ProcessRemovalAndRefill(List<PieceView> removedPieces, System.Action onComplete)
        {
            IsResolving = true;

            // 1. Despawn removed pieces
            int pendingDespawns = removedPieces.Count;
            foreach (var piece in removedPieces)
            {
                boardController.SetPieceAt(piece.gridX, piece.gridY, null);
                piece.AnimateDespawn(() =>
                {
                    spawner.RecyclePiece(piece);
                    pendingDespawns--;
                });
            }

            while (pendingDespawns > 0)
            {
                yield return null;
            }

            // 2. Compact columns downward
            int movingPiecesCount = 0;
            float fallSpeed = boardController.config != null ? boardController.config.fallSpeed : 12f;

            for (int x = 0; x < boardController.Columns; x++)
            {
                int emptySlotsBelow = 0;
                for (int y = 0; y < boardController.Rows; y++)
                {
                    PieceView current = boardController.GetPieceAt(x, y);
                    if (current == null)
                    {
                        emptySlotsBelow++;
                    }
                    else if (emptySlotsBelow > 0)
                    {
                        int newY = y - emptySlotsBelow;
                        boardController.SetPieceAt(x, y, null);
                        boardController.SetPieceAt(x, newY, current);

                        Vector3 targetWorldPos = boardController.GridToWorldPosition(x, newY);
                        movingPiecesCount++;
                        current.MoveToWorldPosition(targetWorldPos, fallSpeed, () =>
                        {
                            movingPiecesCount--;
                        });
                    }
                }

                // 3. Fill empty top slots
                for (int fillIndex = 0; fillIndex < emptySlotsBelow; fillIndex++)
                {
                    int targetY = boardController.Rows - emptySlotsBelow + fillIndex;
                    PieceType randomType = (PieceType)Random.Range(0, boardController.config.typeCount);

                    Vector3 spawnWorldPos = boardController.GridToWorldPosition(x, boardController.Rows + fillIndex + 1);
                    Vector3 targetWorldPos = boardController.GridToWorldPosition(x, targetY);

                    PieceView newPiece = spawner.SpawnPiece(randomType, x, targetY, spawnWorldPos);
                    boardController.SetPieceAt(x, targetY, newPiece);

                    movingPiecesCount++;
                    newPiece.MoveToWorldPosition(targetWorldPos, fallSpeed, () =>
                    {
                        movingPiecesCount--;
                    });
                }
            }

            while (movingPiecesCount > 0)
            {
                yield return null;
            }

            // Ensure grid has valid moves after refill
            boardController.EnsureHasValidMoves();

            IsResolving = false;
            onComplete?.Invoke();
        }
    }
}
