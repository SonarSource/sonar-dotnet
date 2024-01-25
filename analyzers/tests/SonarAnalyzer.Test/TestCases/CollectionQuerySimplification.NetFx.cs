using System.Data.Linq;
using System.Linq;

public class MyEntity
{
    public int Id { get; set; }
}

public class DataLinq
{
    public void GetEntities(Table<MyEntity> entities)
    {
        _ = entities.OrderBy(v => v.Id).ToList().Where(SomeTest).ToList(); // Noncompliant {{Use 'AsEnumerable' here instead.}}
        //                              ^^^^^^
        _ = entities.ToList().Where(SomeTest).ToList(); // Noncompliant {{Use 'AsEnumerable' here instead.}}
        //           ^^^^^^
    }

    public bool SomeTest(MyEntity entity)
    {
        return true;
    }
}
