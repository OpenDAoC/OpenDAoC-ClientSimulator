using System.Threading;

namespace AtlasSimulator.playerclasses
{
    public interface IPlayerClass
    {
        public string sql { get; }
        string accountName { get; }
        string charname { get; }
        string password { get; }
        
        public void login();
    }
}