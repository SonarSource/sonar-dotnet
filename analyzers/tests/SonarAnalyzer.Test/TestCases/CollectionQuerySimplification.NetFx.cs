using System.Linq;

public class MyEntity
{
    public int Id { get; set; }
}

public class DataLinq
{
    public void GetEntitiesFromLinqToSqlTable(System.Data.Linq.Table<MyEntity> entities)
    {
        // Materializing an IQueryable with ToList() is not equivalent to AsEnumerable(), so these must not be flagged.
        _ = entities.OrderBy(v => v.Id).ToList().Where(SomeTest).ToList(); // Compliant
        _ = entities.ToList().Where(SomeTest).ToList(); // Compliant
    }

    public bool SomeTest(MyEntity entity)
    {
        return true;
    }
}
