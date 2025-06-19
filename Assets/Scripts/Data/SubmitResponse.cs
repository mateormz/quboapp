namespace Data
{
    [System.Serializable]
    public class SubmitResponse
    {
        public string question_id;
        public int selectedIndex;

        public SubmitResponse(string id, int index)
        {
            question_id = id;
            selectedIndex = index;
        }
    }
}
