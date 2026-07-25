using System.Collections.Generic;
using UnityEngine;

namespace DogCrush.Board
{
    public class PieceSpawner : MonoBehaviour
    {
        [Header("Prefab & Parent")]
        public PieceView piecePrefab;
        public Transform piecesContainer;

        [Header("Piece Sprites & Colors")]
        public Sprite dogSprite;
        public Sprite boneSprite;
        public Sprite ballSprite;
        public Sprite foodSprite;
        public Sprite collarSprite;

        public Color dogColor = new Color(0.95f, 0.65f, 0.25f);
        public Color boneColor = new Color(0.9f, 0.9f, 0.95f);
        public Color ballColor = new Color(0.3f, 0.75f, 0.95f);
        public Color foodColor = new Color(0.95f, 0.35f, 0.4f);
        public Color collarColor = new Color(0.45f, 0.85f, 0.45f);

        private readonly Queue<PieceView> pool = new Queue<PieceView>();

        private void Awake()
        {
            LoadSpritesIfNull();
        }

        public void LoadSpritesIfNull()
        {
            if (dogSprite == null) dogSprite = LoadResourceSprite("Pieces/dog_icon");
            if (boneSprite == null) boneSprite = LoadResourceSprite("Pieces/bone_icon");
            if (ballSprite == null) ballSprite = LoadResourceSprite("Pieces/ball_icon");
            if (foodSprite == null) foodSprite = LoadResourceSprite("Pieces/food_icon");
            if (collarSprite == null) collarSprite = LoadResourceSprite("Pieces/collar_icon");
        }

        private static Sprite LoadResourceSprite(string path)
        {
            Sprite sprite = Resources.Load<Sprite>(path);
            if (sprite != null) return sprite;

            Sprite[] sprites = Resources.LoadAll<Sprite>(path);
            return sprites != null && sprites.Length > 0 ? sprites[0] : null;
        }

        public PieceView SpawnPiece(PieceType type, int gridX, int gridY, Vector3 spawnWorldPos)
        {
            PieceView piece;
            if (pool.Count > 0)
            {
                piece = pool.Dequeue();
                piece.gameObject.SetActive(true);
            }
            else
            {
                piece = Instantiate(piecePrefab, piecesContainer != null ? piecesContainer : transform);
            }

            piece.transform.position = spawnWorldPos;
            Sprite icon = GetSpriteForType(type);
            Color color = GetColorForType(type);

            piece.Initialize(type, gridX, gridY, icon, color);
            return piece;
        }

        public void RecyclePiece(PieceView piece)
        {
            if (piece == null) return;
            piece.SetSelected(false);
            piece.gameObject.SetActive(false);
            pool.Enqueue(piece);
        }

        public Sprite GetSpriteForType(PieceType type)
        {
            switch (type)
            {
                case PieceType.Dog: return dogSprite;
                case PieceType.Bone: return boneSprite;
                case PieceType.Ball: return ballSprite;
                case PieceType.Food: return foodSprite;
                case PieceType.Collar: return collarSprite;
                default: return null;
            }
        }

        public Color GetColorForType(PieceType type)
        {
            switch (type)
            {
                case PieceType.Dog: return dogColor;
                case PieceType.Bone: return boneColor;
                case PieceType.Ball: return ballColor;
                case PieceType.Food: return foodColor;
                case PieceType.Collar: return collarColor;
                default: return Color.white;
            }
        }
    }
}
