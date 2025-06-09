using System.Collections.Generic;

namespace Data
{
    [System.Serializable]
    public class SubmitWrapper
    {
        public List<SubmitResponse> responses;

        public SubmitWrapper()
        {
            responses = new List<SubmitResponse>();
        }
    }
}
