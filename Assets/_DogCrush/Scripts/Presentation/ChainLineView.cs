using System.Collections.Generic;
using DogCrush.Board;
using UnityEngine;

namespace DogCrush.Presentation
{
    [RequireComponent(typeof(LineRenderer))]
    public class ChainLineView : MonoBehaviour
    {
        public LineRenderer lineRenderer;
        public float startWidth = 0.28f;
        public float endWidth = 0.28f;

        private void Awake()
        {
            if (lineRenderer == null)
                lineRenderer = GetComponent<LineRenderer>();

            lineRenderer.useWorldSpace = true;
            lineRenderer.startWidth = startWidth;
            lineRenderer.endWidth = endWidth;
            lineRenderer.positionCount = 0;
            lineRenderer.sortingOrder = 15;
            lineRenderer.textureMode = LineTextureMode.Tile;
            lineRenderer.numCapVertices = 8;
            lineRenderer.numCornerVertices = 8;
        }

        private void Update()
        {
            if (lineRenderer != null && lineRenderer.positionCount > 0)
            {
                // Pulsing spark glow effect along the chain line
                float pulse = 0.26f + Mathf.Sin(Time.time * 12f) * 0.04f;
                lineRenderer.startWidth = pulse;
                lineRenderer.endWidth = pulse;
            }
        }

        public void UpdateLine(List<PieceView> chain)
        {
            RenderChain(chain, Vector2.zero);
        }

        public void UpdateLine(List<PieceView> chain, Vector2 pointerWorldPos)
        {
            RenderChain(chain, pointerWorldPos);
        }

        public void RenderChain(List<PieceView> chain, Vector2 pointerWorldPos)
        {
            if (lineRenderer == null) return;

            if (chain == null || chain.Count == 0)
            {
                lineRenderer.positionCount = 0;
                return;
            }

            bool hasPointer = pointerWorldPos != Vector2.zero;
            int count = chain.Count + (hasPointer ? 1 : 0);
            lineRenderer.positionCount = count;

            for (int i = 0; i < chain.Count; i++)
            {
                Vector3 piecePos = chain[i].transform.position;
                piecePos.z = -1f; // Bring line slightly in front of pieces
                lineRenderer.SetPosition(i, piecePos);
            }

            if (hasPointer)
            {
                Vector3 dragPos = new Vector3(pointerWorldPos.x, pointerWorldPos.y, -1f);
                lineRenderer.SetPosition(count - 1, dragPos);
            }
        }

        public void ClearLine()
        {
            if (lineRenderer != null)
            {
                lineRenderer.positionCount = 0;
            }
        }
    }
}
