using System.Collections.Generic;
using UnityEngine;

namespace DogCrush.Board
{
    public class BoardController : MonoBehaviour
    {
        public BoardConfig config;
        public PieceSpawner spawner;

        private PieceView[,] grid;
        private Vector3 boardOrigin;

        public PieceView[,] Grid => grid;
        public int Columns => config != null ? config.columns : 8;
        public int Rows => config != null ? config.rows : 8;

        public void InitializeBoard()
        {
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<BoardConfig>();
            }

            // A restart replaces the grid array. Recycle the previous grid
            // first, otherwise its active PieceViews remain behind the new
            // board and the scene accumulates duplicate visual pieces.
            if (grid != null)
            {
                ClearBoard();
            }

            grid = new PieceView[config.columns, config.rows];
            CalculateBoardOrigin();
            FillInitialBoard();
        }

        public void CalculateBoardOrigin()
        {
            float totalWidth = (config.columns - 1) * config.pieceSpacing;
            float totalHeight = (config.rows - 1) * config.pieceSpacing;
            // Keep pieces in front of the board frame in URP/WebGL. At z=0
            // both SpriteRenderers can share the same depth buffer value and
            // the opaque frame may hide the pieces despite their sort order.
            // The canonical board is square. Keep the logical 8x8 grid
            // centered on its chocolate play area; UI spacing is handled by
            // the camera/HUD rather than by distorting the board geometry.
            const float boardCenterY = 0.55f;
            boardOrigin = new Vector3(-totalWidth / 2f, boardCenterY - totalHeight / 2f, -1f);
        }

        public Vector3 GridToWorldPosition(int x, int y)
        {
            return boardOrigin + new Vector3(x * config.pieceSpacing, y * config.pieceSpacing, 0f);
        }

        public bool IsValidGridPos(int x, int y)
        {
            return x >= 0 && x < config.columns && y >= 0 && y < config.rows;
        }

        public PieceView GetPieceAt(int x, int y)
        {
            if (!IsValidGridPos(x, y)) return null;
            return grid[x, y];
        }

        public void SetPieceAt(int x, int y, PieceView piece)
        {
            if (IsValidGridPos(x, y))
            {
                grid[x, y] = piece;
                if (piece != null)
                {
                    piece.SetGridPosition(x, y);
                }
            }
        }

        private void FillInitialBoard()
        {
            ClearBoard();

            for (int x = 0; x < config.columns; x++)
            {
                for (int y = 0; y < config.rows; y++)
                {
                    PieceType type = (PieceType)Random.Range(0, config.typeCount);
                    Vector3 targetWorldPos = GridToWorldPosition(x, y);
                    PieceView piece = spawner.SpawnPiece(type, x, y, targetWorldPos);
                    grid[x, y] = piece;
                }
            }

            EnsureHasValidMoves();
        }

        public void ClearBoard()
        {
            if (grid == null) return;
            for (int x = 0; x < config.columns; x++)
            {
                for (int y = 0; y < config.rows; y++)
                {
                    if (grid[x, y] != null)
                    {
                        spawner.RecyclePiece(grid[x, y]);
                        grid[x, y] = null;
                    }
                }
            }
        }

        public bool HasAnyValidMove()
        {
            for (int x = 0; x < config.columns; x++)
            {
                for (int y = 0; y < config.rows; y++)
                {
                    PieceView current = grid[x, y];
                    if (current == null) continue;

                    int matchingNeighbors = 0;
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        for (int dy = -1; dy <= 1; dy++)
                        {
                            if (dx == 0 && dy == 0) continue;
                            int nx = x + dx;
                            int ny = y + dy;
                            if (IsValidGridPos(nx, ny) && grid[nx, ny] != null && grid[nx, ny].type == current.type)
                            {
                                matchingNeighbors++;
                            }
                        }
                    }

                    if (matchingNeighbors >= 2)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void EnsureHasValidMoves()
        {
            int safetyCounter = 0;
            while (!HasAnyValidMove() && safetyCounter < 50)
            {
                ShuffleBoardTypes();
                safetyCounter++;
            }
        }

        public void ShuffleBoardTypes()
        {
            List<PieceType> allTypes = new List<PieceType>();
            for (int x = 0; x < config.columns; x++)
            {
                for (int y = 0; y < config.rows; y++)
                {
                    if (grid[x, y] != null)
                        allTypes.Add(grid[x, y].type);
                }
            }

            // Fisher-Yates shuffle
            for (int i = 0; i < allTypes.Count; i++)
            {
                int rnd = Random.Range(i, allTypes.Count);
                PieceType temp = allTypes[i];
                allTypes[i] = allTypes[rnd];
                allTypes[rnd] = temp;
            }

            int index = 0;
            for (int x = 0; x < config.columns; x++)
            {
                for (int y = 0; y < config.rows; y++)
                {
                    if (grid[x, y] != null)
                    {
                        PieceType newType = allTypes[index++];
                        grid[x, y].Initialize(newType, x, y, spawner.GetSpriteForType(newType), spawner.GetColorForType(newType));
                    }
                }
            }
        }

        public static bool AreAdjacent(int x1, int y1, int x2, int y2)
        {
            int dx = Mathf.Abs(x1 - x2);
            int dy = Mathf.Abs(y1 - y2);
            return dx <= 1 && dy <= 1 && !(dx == 0 && dy == 0);
        }
    }
}
