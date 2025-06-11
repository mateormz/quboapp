namespace Data
{
    [System.Serializable]
    public class LevelProgressEntry
    {
        public string gameId;
        public int unlockedLevel;

        public LevelProgressEntry(string gameId, int unlockedLevel)
        {
            this.gameId = gameId;
            this.unlockedLevel = unlockedLevel;
        }
    }
}
