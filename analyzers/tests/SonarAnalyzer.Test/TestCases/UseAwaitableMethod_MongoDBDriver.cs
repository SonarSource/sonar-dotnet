using System;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;

public record Person(int Id, string Name);

public class MongoDBDriver
{
    public async Task Query(IMongoCollection<Person> personCollection)
    {
        var sort = Builders<Person>.Sort.Descending(nameof(Person.Name));
        // * Find https://mongodb.github.io/mongo-csharp-driver/2.8/apidocs/html/M_MongoDB_Driver_IMongoCollectionExtensions_Find__1_3.htm
        // * FindAsync https://mongodb.github.io/mongo-csharp-driver/2.8/apidocs/html/M_MongoDB_Driver_IMongoCollectionExtensions_FindAsync__1_3.htm
        // Speculative binding finds "FindAsync" but the return type IAsyncCursor<> of FindAsync is not compatible with return type IFindFluent<,> of "Find"
        // Speculative binding does overload resolution according to the C# rules, which ignore return types.
        // It seems to ignore the compiler binding error for the following "Sort" which is only defined on IFindFluent, but not in IAsyncCursor.
        var snapshot = await personCollection.Find(s => s.Id > 10) // Compliant https://github.com/SonarSource/sonar-dotnet/issues/9265
            .Sort(sort) // Not defined on IAsyncCursor (return type of FindAsync)
            .FirstOrDefaultAsync().ConfigureAwait(false);
    }
}
