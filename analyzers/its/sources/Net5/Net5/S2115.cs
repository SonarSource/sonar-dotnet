using Microsoft.EntityFrameworkCore;

namespace Net5
{
    public static class S2115
    {
        public static void Foo(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=");
            optionsBuilder.UseNpgsql("Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=");
            optionsBuilder.UseMySQL("Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=");
            optionsBuilder.UseSqlite("Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=");
            optionsBuilder.UseOracle("Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=");
        }
    }
}
