namespace Credentials
{
    public class Credentials
    {
        private const string Username = "admin";
        private const string Password = "Password123";
        private const string Senha = "Password123";

        public override string ToString() =>
            $"{Username} {Password} {Senha}";
    }
}
