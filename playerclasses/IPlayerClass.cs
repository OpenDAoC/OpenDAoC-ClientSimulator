namespace ClientSimulator.PlayerClass
{
    public interface IPlayerClass
    {
        string Sql { get; }
        string AccountName { get; }
        string CharName { get; }
        string Password { get; }

        void Login();
    }
}
