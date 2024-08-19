using Microsoft.Data.Sqlite;

namespace Net5
{
    public class S2077
    {
        void Foo(SqliteConnection connection, string query)
        {
            SqliteCommand command = new ($"SELECT * FROM mytable WHERE mycol={query}", connection);
        }
    }
}
