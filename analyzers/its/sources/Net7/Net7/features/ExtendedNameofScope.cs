namespace Net7.features
{
    [Obsolete(nameof(LogMessage))]
    internal class ExtendedNameofScope
    {
        [Obsolete(nameof(message))]
        public static void LogMessage(string? message)
        {
            Console.WriteLine(message.Length);
        }
    }
}
