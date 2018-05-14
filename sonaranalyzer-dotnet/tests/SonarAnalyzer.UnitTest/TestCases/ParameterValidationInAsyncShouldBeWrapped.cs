using System;
using System.IO;
using System.Threading.Tasks;

namespace Tests.Diagnostics
{
    public class InvalidCases
    {
        public static async Task SkipLinesAsync(this TextReader reader, int linesToSkip) // Noncompliant {{Split this method into two, one handling parameters check and the other handling the iterator.}}
//                               ^^^^^^^^^^^^^^
        {
            if (reader == null) { throw new ArgumentNullException(nameof(reader)); }
//                                          ^^^^^^^^^^^^^^^^^^^^^ Secondary
            if (linesToSkip < 0) { throw new ArgumentOutOfRangeException(nameof(linesToSkip)); }
//                                           ^^^^^^^^^^^^^^^^^^^^^^^^^^^ Secondary

            for (var i = 0; i < linesToSkip; ++i)
            {
                var line = await reader.ReadLineAsync().ConfigureAwait(false);
                if (line == null) { break; }
            }
        }

        public async Task DoSomethingAsync(string value) // Noncompliant - this is an edge case that might be worth handling later on
        {
            await Task.Delay(0);

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value)); // Secondary
            }
        }

        public async void OnSomeEvent(object sender, EventArgs args) // Noncompliant - it might looks weird to throw from some event method but that's valid syntax
        {
            if (sender == null)
            {
                throw new ArgumentNullException(nameof(sender)); // Secondary
            }

            await Task.Yield();
        }

        public static Task SkipLinesAsync(this TextReader reader, int linesToSkip) // Noncompliant - Not sure what should be the expected result (i.e. do local functions suffer from this behavior?)
        {
            if (reader == null) { throw new ArgumentNullException(nameof(reader)); } // Secondary
            if (linesToSkip < 0) { throw new ArgumentOutOfRangeException(nameof(linesToSkip)); } // Secondary

            return reader.SkipLinesInternalAsync(linesToSkip);

            async Task SkipLinesInternalAsync(this TextReader reader, int linesToSkip)
            {
                for (var i = 0; i < linesToSkip; ++i)
                {
                    var line = await reader.ReadLineAsync().ConfigureAwait(false);
                    if (line == null) { break; }
                }
            }
        }

        public Task DoAsync(object o)
        {
            if (o == null)
            {
                throw new UnknownException(nameof(o));
            }

            await UnresolvedAsync();
        }
    }

    public class ValidCases
    {
        public static Task SkipLinesAsync(this TextReader reader, int linesToSkip)
        {
            if (reader == null) { throw new ArgumentNullException(nameof(reader)); }
            if (linesToSkip < 0) { throw new ArgumentOutOfRangeException(nameof(linesToSkip)); }

            return reader.SkipLinesInternalAsync(linesToSkip);
        }

        private static async Task SkipLinesInternalAsync(this TextReader reader, int linesToSkip)
        {
            for (var i = 0; i < linesToSkip; ++i)
            {
                var line = await reader.ReadLineAsync().ConfigureAwait(false);
                if (line == null) { break; }
            }
        }

        public async Task DoAsync() // Compliant - no args check
        {
            await Task.Delay(0);
        }

        public async Task FooAsync(int age) // Compliant - the exception doesn't derive from ArgumentException
        {
            if (age == 0)
            {
                throw new Exception("Wrong age");
            }
        }
    }
}
