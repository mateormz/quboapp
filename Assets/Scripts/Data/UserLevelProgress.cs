namespace Data
{
    [System.Serializable]
    public class UserLevelProgress
    {
        public string user_id;
        public string email;
        public string role;
        public string classroom_id;
        public int skinSeleccionada;
        public System.Collections.Generic.List<int> skinsDesbloqueadas;
        public LevelProgressWrapper levelProgress;
    }
}
