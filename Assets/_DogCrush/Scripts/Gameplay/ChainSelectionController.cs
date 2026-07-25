using System.Collections.Generic;
using DogCrush.Board;
using DogCrush.InputSystem;
using UnityEngine;

namespace DogCrush.Gameplay
{
    public class ChainSelectionController : MonoBehaviour
    {
        public BoardController boardController;
        public ChainInputHandler inputHandler;
        public Presentation.ChainLineView lineView;

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
            PieceView piece = GetPieceAtPosition(worldPos);
            if (piece == null) return;

            IsSelecting = true;
            selectedChain.Clear();
            ActiveChainType = piece.type;

            AddPieceToChain(piece);
        }

        private void HandlePointerDrag(Vector2 worldPos)
        {
            if (!IsSelecting) return;

            PieceView piece = GetPieceAtPosition(worldPos);
            if (piece == null) return;

            if (selectedChain.Count == 0)
            {
                AddPieceToChain(piece);
                return;
            }

            // Check if player dragged back to the second-to-last piece (Backtrack / Undo)
            if (selectedChain.Count >= 2 && piece == selectedChain[selectedChain.Count - 2])
            {
                RemoveLastPieceFromChain();
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
        }

        private void HandlePointerUp()
        {
            if (!IsSelecting) return;
            IsSelecting = false;

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
            piece.SetSelected(true);

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

        private void UpdateLineView()
        {
            if (lineView != null)
            {
                lineView.UpdateLine(selectedChain);
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
