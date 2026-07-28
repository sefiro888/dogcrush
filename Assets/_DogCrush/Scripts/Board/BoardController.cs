using System.Collections.Generic;
using UnityEngine;

namespace DogCrush.Board
{
    public class BoardController : MonoBehaviour
    {
        private static readonly Vector2Int[] OrthogonalDirections =
        {
            Vector2Int.up,
            Vector2Int.right,
            Vector2Int.down,
            Vector2Int.left
        };

        public BoardConfig config;
        public PieceSpawner spawner;

        private PieceView[,] grid;
        private Vector3 boardOrigin;
        private float activePieceSpacing;
        private float activeBoardCenterY;
        private AdaptiveBoardView adaptiveView;

        public PieceView[,] Grid => grid;
        public int Columns => config != null ? config.columns : 8;
        public int Rows => config != null ? config.rows : 8;
        public float ActivePieceSpacing => activePieceSpacing;
        public float ActiveBoardCenterY => activeBoardCenterY;

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
            adaptiveView = GetComponent<AdaptiveBoardView>();
            if (adaptiveView == null)
            {
                adaptiveView = gameObject.AddComponent<AdaptiveBoardView>();
            }
            CalculateBoardOrigin();
            adaptiveView.Rebuild(this);
            FillInitialBoard();
        }

        public void CalculateBoardOrigin()
        {
            AdaptiveBoardView.CalculateLayout(
                config.columns,
                config.rows,
                Camera.main,
                config.pieceSpacing,
                out activePieceSpacing,
                out activeBoardCenterY);

            float totalWidth = (config.columns - 1) * activePieceSpacing;
            float totalHeight = (config.rows - 1) * activePieceSpacing;
            // Keep pieces in front of the board frame in URP/WebGL. At z=0
            // both SpriteRenderers can share the same depth buffer value and
            // the opaque frame may hide the pieces despite their sort order.
            boardOrigin = new Vector3(
                -totalWidth / 2f,
                activeBoardCenterY - totalHeight / 2f,
                -1f);
        }

        public Vector3 GridToWorldPosition(int x, int y)
        {
            return boardOrigin + new Vector3(x * activePieceSpacing, y * activePieceSpacing, 0f);
        }

        public void RefreshAdaptiveLayout()
        {
            if (config == null || grid == null) return;

            CalculateBoardOrigin();
            adaptiveView?.Rebuild(this);

            for (int x = 0; x < config.columns; x++)
            {
                for (int y = 0; y < config.rows; y++)
                {
                    PieceView piece = grid[x, y];
                    if (piece != null)
                    {
                        piece.transform.position = GridToWorldPosition(x, y);
                    }
                }
            }
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

            int availableTypeCount = Mathf.Clamp(config.typeCount, 1, (int)PieceType.Collar + 1);

            for (int x = 0; x < config.columns; x++)
            {
                for (int y = 0; y < config.rows; y++)
                {
                    PieceType type = (PieceType)Random.Range(0, availableTypeCount);
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
            int existingColumns = grid.GetLength(0);
            int existingRows = grid.GetLength(1);
            for (int x = 0; x < existingColumns; x++)
            {
                for (int y = 0; y < existingRows; y++)
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
                    foreach (Vector2Int direction in OrthogonalDirections)
                    {
                        int nx = x + direction.x;
                        int ny = y + direction.y;
                        if (IsValidGridPos(nx, ny) &&
                            grid[nx, ny] != null &&
                            grid[nx, ny].type == current.type)
                        {
                            matchingNeighbors++;
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

            // A shuffled distribution can still be unlucky, especially on
            // small/custom boards. Never leave the player with a dead board:
            // create one guaranteed orthogonal triplet as a deterministic
            // fallback after the shuffle budget is exhausted.
            if (!HasAnyValidMove())
            {
                ForceValidMovePattern();
            }
        }

        private void ForceValidMovePattern()
        {
            if (grid == null || spawner == null || config == null ||
                config.columns < 2 || config.rows < 2) return;

            int x = Mathf.Clamp(config.columns / 2, 0, config.columns - 2);
            int y = Mathf.Clamp(config.rows / 2, 0, config.rows - 2);
            PieceType forcedType = PieceType.Dog;
            PieceView[] pattern =
            {
                grid[x, y],
                grid[x + 1, y],
                grid[x, y + 1]
            };

            foreach (PieceView piece in pattern)
            {
                if (piece != null)
                {
                    piece.Initialize(
                        forcedType,
                        piece.gridX,
                        piece.gridY,
                        spawner.GetSpriteForType(forcedType),
                        spawner.GetColorForType(forcedType));
                }
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
            return dx + dy == 1;
        }

        public void FillMissingCells()
        {
            if (config == null || grid == null || spawner == null) return;
            int availableTypeCount = Mathf.Clamp(config.typeCount, 1, (int)PieceType.Collar + 1);
            for (int x = 0; x < Columns; x++)
            {
                for (int y = 0; y < Rows; y++)
                {
                    if (grid[x, y] != null) continue;
                    PieceType type = (PieceType)Random.Range(0, availableTypeCount);
                    grid[x, y] = spawner.SpawnPiece(type, x, y, GridToWorldPosition(x, y));
                }
            }
        }

        public List<PieceView> GetRowPieces(int row)
        {
            var result = new List<PieceView>();
            if (config == null || row < 0 || row >= Rows) return result;
            for (int x = 0; x < Columns; x++)
            {
                if (grid[x, row] != null) result.Add(grid[x, row]);
            }
            return result;
        }

        public PieceView GetRandomPiece()
        {
            if (grid == null || Columns <= 0 || Rows <= 0) return null;
            var pieces = new List<PieceView>();
            for (int x = 0; x < Columns; x++)
            {
                for (int y = 0; y < Rows; y++)
                {
                    if (grid[x, y] != null) pieces.Add(grid[x, y]);
                }
            }
            return pieces.Count == 0 ? null : pieces[Random.Range(0, pieces.Count)];
        }
    }
}
