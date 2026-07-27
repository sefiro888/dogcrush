using System.Collections.Generic;
using DogCrush.Board;
using DogCrush.Core;
using DogCrush.InputSystem;
using UnityEngine;

namespace DogCrush.Gameplay
{
    public class ChainSelectionController : MonoBehaviour
    {
        public BoardController boardController;
        public ChainInputHandler inputHandler;
        public Presentation.ChainLineView lineView;
        public GameStateController stateController;

        public System.Action<List<PieceView>> OnChainCompleted;
        public System.Action OnChainCancelled;
        public System.Action<int, PieceType> OnChainUpdated;

        private readonly List<PieceView> selectedChain = new List<PieceView>();
        public List<PieceView> SelectedChain => selectedChain;

        public bool IsSelecting { get; private set; }
        public PieceType ActiveChainType { get; private set; } = PieceType.None;

        private void OnEnable()
        {
            if (inputHandler != null)
            {
                inputHandler.OnPointerDownEvent += HandlePointerDown;
                inputHandler.OnPointerDragEvent += HandlePointerDrag;
                inputHandler.OnPointerUpEvent += HandlePointerUp;
            }
        }

        private void Awake()
        {
            if (stateController == null)
                stateController = FindAnyObjectByType<GameStateController>();
        }

        private void OnDisable()
        {
            if (inputHandler != null)
            {
                inputHandler.OnPointerDownEvent -= HandlePointerDown;
                inputHandler.OnPointerDragEvent -= HandlePointerDrag;
                inputHandler.OnPointerUpEvent -= HandlePointerUp;
            }
        }

        private void HandlePointerDown(Vector2 worldPos)
        {
            if (stateController != null && !stateController.CanSelectPieces()) return;
            PieceView piece = GetPieceAtPosition(worldPos);
            if (piece == null) return;

            IsSelecting = true;
            selectedChain.Clear();
            ActiveChainType = piece.type;
            if (lineView != null)
            {
                lineView.SetChainType(ActiveChainType);
            }

            AddPieceToChain(piece);
        }

        private void HandlePointerDrag(Vector2 worldPos)
        {
            if (!IsSelecting) return;
            if (stateController != null && !stateController.CanSelectPieces())
            {
                CancelCurrentSelection();
                return;
            }

            PieceView piece = GetPieceAtPosition(worldPos);
            if (piece == null)
            {
                UpdateLineView(worldPos);
                return;
            }

            if (selectedChain.Count == 0)
            {
                AddPieceToChain(piece);
                UpdateLineView(worldPos);
                return;
            }

            // Check if player dragged back to the second-to-last piece (Backtrack / Undo)
            if (selectedChain.Count >= 2 && piece == selectedChain[selectedChain.Count - 2])
            {
                RemoveLastPieceFromChain();
                UpdateLineView(worldPos);
                return;
            }

            // Check validity: must match active type, must be adjacent to last, must not already be in chain
            PieceView lastPiece = selectedChain[selectedChain.Count - 1];
            if (piece.type == ActiveChainType &&
                !selectedChain.Contains(piece) &&
                BoardController.AreAdjacent(lastPiece.gridX, lastPiece.gridY, piece.gridX, piece.gridY))
            {
                AddPieceToChain(piece);
            }

            UpdateLineView(worldPos);
        }

        private void HandlePointerUp()
        {
            if (!IsSelecting) return;
            IsSelecting = false;

            if (stateController != null && !stateController.CanSelectPieces())
            {
                CancelCurrentSelection();
                return;
            }

            int minLength = boardController != null && boardController.config != null ? boardController.config.minChainLength : 3;

            if (selectedChain.Count >= minLength)
            {
                List<PieceView> completed = new List<PieceView>(selectedChain);
                ClearSelectionVisuals();
                OnChainCompleted?.Invoke(completed);
            }
            else
            {
                ClearSelectionVisuals();
                OnChainCancelled?.Invoke();
            }

            selectedChain.Clear();
            ActiveChainType = PieceType.None;
        }

        private void AddPieceToChain(PieceView piece)
        {
            selectedChain.Add(piece);
            piece.SetSelected(true, selectedChain.Count - 1);

            UpdateLineView();
            OnChainUpdated?.Invoke(selectedChain.Count, ActiveChainType);
        }

        private void RemoveLastPieceFromChain()
        {
            if (selectedChain.Count == 0) return;
            int lastIdx = selectedChain.Count - 1;
            PieceView lastPiece = selectedChain[lastIdx];
            lastPiece.SetSelected(false);
            selectedChain.RemoveAt(lastIdx);

            UpdateLineView();
            OnChainUpdated?.Invoke(selectedChain.Count, ActiveChainType);
        }

        private void ClearSelectionVisuals()
        {
            foreach (var piece in selectedChain)
            {
                if (piece != null)
                    piece.SetSelected(false);
            }
            if (lineView != null)
            {
                lineView.ClearLine();
            }
        }

        private void CancelCurrentSelection()
        {
            IsSelecting = false;
            ClearSelectionVisuals();
            selectedChain.Clear();
            ActiveChainType = PieceType.None;
            OnChainCancelled?.Invoke();
        }

        private void UpdateLineView()
        {
            if (lineView != null)
            {
                lineView.UpdateLine(selectedChain);
            }
        }

        private void UpdateLineView(Vector2 pointerWorldPos)
        {
            if (lineView != null)
            {
                lineView.UpdateLine(selectedChain, pointerWorldPos);
            }
        }

        private PieceView GetPieceAtPosition(Vector2 worldPos)
        {
            Collider2D hit = Physics2D.OverlapPoint(worldPos);
            if (hit != null)
            {
                return hit.GetComponent<PieceView>();
            }

            // Raycast fallback if collider is on child or offset
            RaycastHit2D hit3D = Physics2D.Raycast(worldPos, Vector2.zero);
            if (hit3D.collider != null)
            {
                return hit3D.collider.GetComponent<PieceView>();
            }

            return null;
        }
    }
}
