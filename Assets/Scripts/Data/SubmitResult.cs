using System.Collections.Generic;

namespace Data
{
    [System.Serializable]
    public class SubmitResult
    {
        public string sessionId;
        public int score;
        public bool passed;
        public List<IncorrectQuestion> incorrectQuestions;
    }
}
