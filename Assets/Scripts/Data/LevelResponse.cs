using System;

namespace Data
{
    [Serializable]
    public class LevelResponse
    {
        public string game_id;
        public int level_number;
        public string[] questions;
    }
}
