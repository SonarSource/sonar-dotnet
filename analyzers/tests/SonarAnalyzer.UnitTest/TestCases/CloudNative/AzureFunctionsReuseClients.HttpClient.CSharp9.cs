namespace DifferentAssignments
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;

    public class FunctionApp1
    {
        private static HttpClient field;

        [FunctionName("Sample")]
        public static void NullConditionalAssignment()
        {
            field ??= new HttpClient(); // Compliant
        }

        [FunctionName("Sample")]
        public static void NullConditionalAssignmentToLocal()
        {
            var local = default(HttpClient);
            local ??= new HttpClient(); // Noncompliant
        }

        [FunctionName("Sample")]
        public static void AssignmentToDiscard()
        {
            _ = new HttpClient(); // Noncompliant
        }

        [FunctionName("Sample")]
        public static void WrapInUsingDeclaration()
        {
            using var local = new HttpClient(); // Noncompliant
        }

        [FunctionName("Sample")]
        public static void TargetTypedNewForLocal()
        {
            HttpClient local = new(); // Noncompliant
        }

        [FunctionName("Sample")]
        public static void TargetTypedNewForField()
        {
            field = new(); // Compliant
        }
    }
}
