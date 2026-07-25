using System.Collections.Generic;
using DogCrush.Board;
using UnityEngine;

namespace DogCrush.Presentation
{
    [RequireComponent(typeof(LineRenderer))]
    public class ChainLineView : MonoBehaviour
    {
        public LineRenderer lineRenderer;

        private void Awake()
        {
            if (lineRenderer == null)
                lineRenderer = GetComponent<LineRenderer>();

            lineRenderer.positionCount = 0;
            lineRenderer.useWorldSpace = true;
            lineRenderer.startWidth = 0.18f;
            lineRenderer.endWidth = 0.18f;
        }

        public void UpdateLine(List<PieceView> chain)
        {
            if (lineRenderer == null) return;

            if (chain == null || chain.Count < 2)
            {
                lineRenderer.positionCount = 0;
                return;
            }

            lineRenderer.positionCount = chain.Count;
            for (int i = 0; i < chain.Count; i++)
            {
                Vector3 pos = chain[i].transform.position;
                pos.z = -1f; // Bring line forward in 2D
                lineRenderer.SetPosition(i, pos);
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
