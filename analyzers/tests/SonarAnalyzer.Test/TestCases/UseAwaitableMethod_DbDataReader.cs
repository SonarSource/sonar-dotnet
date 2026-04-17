using System.Data.Common;
using System.IO;
using System.Threading.Tasks;

// https://sonarsource.atlassian.net/browse/NET-3564
public class DbDataReaderColumnReads
{
    public async Task Read(DbDataReader reader, int ordinal)
    {
        while (await reader.ReadAsync())                           // Compliant - ReadAsync is correct here
        {
            _ = reader.IsDBNull(ordinal);                          // Noncompliant FP - IsDBNullAsync exists but is not beneficial without SequentialAccess
            _ = reader.GetFieldValue<string>(ordinal);             // Noncompliant FP - GetFieldValueAsync exists but is not beneficial without SequentialAccess
            _ = reader.GetValue(ordinal);                          // Compliant - no GetValueAsync on DbDataReader
            _ = reader.GetStream(ordinal);                         // Compliant - no GetStreamAsync on DbDataReader
            _ = reader.GetTextReader(ordinal);                     // Compliant - no GetTextReaderAsync on DbDataReader

            var buffer = new byte[1024];
            _ = reader.GetBytes(ordinal, 0, buffer, 0, buffer.Length);  // Compliant - no GetBytesAsync on DbDataReader

            var charBuffer = new char[1024];
            _ = reader.GetChars(ordinal, 0, charBuffer, 0, charBuffer.Length); // Compliant - no GetCharsAsync on DbDataReader
        }
    }

    public async Task ReadSync(DbDataReader reader, int ordinal)
    {
        while (reader.Read())                                      // Noncompliant - ReadAsync is a valid and beneficial suggestion
        {
            _ = reader.IsDBNull(ordinal);                          // Noncompliant FP
            _ = reader.GetFieldValue<string>(ordinal);             // Noncompliant FP
        }
    }
}
