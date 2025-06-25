using System.Collections.Generic;

namespace Data
{
    [System.Serializable]
    public class SubmitWrapper
    {
        public List<SubmitResponse> responses;
        public string level_time;

        public SubmitWrapper()
        {
            responses = new List<SubmitResponse>();
        }
    }
}
