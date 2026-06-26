using System.Data.Common;
using System.IO;
using System.Threading.Tasks;

// https://sonarsource.atlassian.net/browse/NET-3564
public class DbDataReaderColumnReads
{
    public async Task Read(DbDataReader reader, int ordinal)
    {
        while (await reader.ReadAsync())
        {
            _ = reader.IsDBNull(ordinal);                                        // Compliant - synchronous version preferred without SequentialAccess
            _ = reader.GetFieldValue<string>(ordinal);                           // Compliant - synchronous version preferred without SequentialAccess
            _ = reader.GetValue(ordinal);                                        // Compliant
            _ = reader.GetStream(ordinal);                                       // Compliant
            _ = reader.GetTextReader(ordinal);                                   // Compliant

            var buffer = new byte[1024];
            _ = reader.GetBytes(ordinal, 0, buffer, 0, buffer.Length);          // Compliant

            var charBuffer = new char[1024];
            _ = reader.GetChars(ordinal, 0, charBuffer, 0, charBuffer.Length);  // Compliant
        }
    }

    public async Task ReadSync(DbDataReader reader, int ordinal)
    {
        while (reader.Read())                                                    // Noncompliant {{Await ReadAsync instead.}}
        {
            _ = reader.IsDBNull(ordinal);                                        // Compliant
            _ = reader.GetFieldValue<string>(ordinal);                           // Compliant
        }
    }

    public async Task ReadSubtype(CustomDbDataReader reader, int ordinal)
    {
        while (await reader.ReadAsync())
        {
            _ = reader.IsDBNull(ordinal);                                        // Compliant - overridden in subtype, still excluded
            _ = reader.GetFieldValue<string>(ordinal);                           // Compliant - overridden in subtype, still excluded
            _ = reader.GetValue(ordinal);                                        // Compliant
            _ = reader.GetStream(ordinal);                                       // Compliant
            _ = reader.GetTextReader(ordinal);                                   // Compliant

            var buffer = new byte[1024];
            _ = reader.GetBytes(ordinal, 0, buffer, 0, buffer.Length);          // Compliant

            var charBuffer = new char[1024];
            _ = reader.GetChars(ordinal, 0, charBuffer, 0, charBuffer.Length);  // Compliant
        }
    }

    public async Task ReadSyncSubtype(CustomDbDataReader reader, int ordinal)
    {
        while (reader.Read())                                                    // Noncompliant {{Await ReadAsync instead.}}
        {
            _ = reader.IsDBNull(ordinal);                                        // Compliant
            _ = reader.GetFieldValue<string>(ordinal);                           // Compliant
        }
    }
}

// Abstract subclass overriding the excluded methods.
// When these are overridden, methodSymbol.ContainingType is CustomDbDataReader (not DbDataReader),
// so the exclusion must use DerivesFrom to walk the inheritance chain.
public abstract class CustomDbDataReader : DbDataReader
{
    public override bool IsDBNull(int ordinal) => false;
    public override T GetFieldValue<T>(int ordinal) => default(T);
    public override object GetValue(int ordinal) => null;
    public override Stream GetStream(int ordinal) => Stream.Null;
    public override TextReader GetTextReader(int ordinal) => TextReader.Null;
    public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length) => 0;
    public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length) => 0;
}
