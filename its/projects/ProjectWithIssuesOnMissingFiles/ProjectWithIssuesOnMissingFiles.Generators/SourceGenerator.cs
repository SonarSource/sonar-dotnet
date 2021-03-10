using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace ProjectWithIssuesOnMissingFiles.Generators
{
    [Generator]
    public class SourceGenerator : ISourceGenerator
    {
        private const string MessagesCode = @"namespace Generated
{
    public static class Messages
    {
        public const string Hello = ""Hello from generated code!"";

        private static void UnusedMethod() { } // Issue is raised here
    }
}
";
        public void Execute(GeneratorExecutionContext context) => context.AddSource("Greetings", SourceText.From(MessagesCode, Encoding.UTF8));

        public void Initialize(GeneratorInitializationContext context)
        {
            /* Nothing to do here */
        }
    }
}
