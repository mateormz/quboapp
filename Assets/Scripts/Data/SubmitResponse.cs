namespace Data
{
    [System.Serializable]
    public class SubmitResponse
    {
        public string questionId;
        public int selectedIndex;

        public SubmitResponse(string id, int index)
        {
            questionId = id;
            selectedIndex = index;
        }
    }
}
