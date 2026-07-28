using System;

namespace DogCrush.Core
{
    /// <summary>
    /// Data for one playable level. Keeping this separate from the game loop
    /// lets new levels change dimensions, pacing and goals without code edits.
    /// </summary>
    [Serializable]
    public class LevelDefinition
    {
        public int level = 1;
        public int rows = 8;
        public int columns = 8;
        public float durationSeconds = 60f;
        public int targetScore = 5000;
        public int typeCount = 5;
        public int minChainLength = 3;
    }
}
