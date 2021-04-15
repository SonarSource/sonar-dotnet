using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using SonarAnalyzer.Rules.Tests.Framework;
using SonarAnalyzer.UnitTest.TestFramework;

namespace VerifyAssembly
{
    public class Loads
    {
        private static readonly Rules.Verify Verifier = new Rules.Verify();
        private static readonly Type Type = typeof(Rules.Verify);
        private static readonly MethodInfo Method = Type.GetMethod(nameof(Rules.Verify.Cases));
        private static readonly IMethodInfo MethodInfo = new MethodWrapper(Type, Method);
        private static readonly TestFixture Suite = new TestFixture(new TypeWrapper(Type));

        [Test]
        public void CSharp_assembly()
        {
            var attr = new VerifyAssemblyAttribute(typeof(SonarAnalyzer.Rules.CSharp.ArrayCovariance));

            foreach (var testMethod in attr.BuildFrom(MethodInfo, Suite))
            {
                Console.WriteLine(testMethod.FullName);
                var x = Execute(testMethod);
                FixCSharp9ConsoleCases(x, testMethod);
                FixLanguageVersion(x, testMethod);
            }
        }

        [Test]
        public void VisualBasic_assembly()
        {
            var attr = new VerifyAssemblyAttribute(typeof(SonarAnalyzer.Rules.VisualBasic.ArrayCreationLongSyntax));

            var info = new MethodWrapper(GetType(), GetType().GetMethod(nameof(VisualBasic_assembly)));
            var suite = new TestMethod(info);
            var items = attr.BuildFrom(MethodInfo, Suite).ToArray();

            foreach (var item in items)
            {
                Console.WriteLine(item.Name);
            }
        }


        private static Exception Execute(TestMethod testMethod)
        {
            try
            {
                Method.Invoke(Verifier, testMethod.Arguments);
            }
            catch(TargetInvocationException x)
            {
                return x.InnerException;
            }
            catch(Exception x)
            {
                return x;
            }
            return null;
        }

        private static void FixCSharp9ConsoleCases(Exception x, TestMethod testMethod)
        {
            if (x is UnexpectedDiagnosticException unexpected && unexpected.Message.Contains("Program using top-level statements must be an executable"))
            {
                var location = GetFileLocation(testMethod);

                var target = new FileInfo(location.FullName.Replace(".CSharp9.", ".Console."));
                if (!target.Exists)
                {
                    AddVersionHeader(location, nameof(LanguageVersionInfo.CSharp9));
                    location.MoveTo(target.FullName);
                }
                else
                {
                    location.Delete();
                }
            }
        }

        private void FixLanguageVersion(Exception x, TestMethod testMethod)
        {
            if (x is null) { return; }

            var phrase = "Please use language version ";
            var pos = x.Message.IndexOf(phrase);
            if (pos != -1)
            {
                var location = GetFileLocation(testMethod);
                var version = x.Message.Substring(pos + phrase.Length, 3);

                switch (version)
                {
                    case "7.0": AddVersionHeader(location, "FromCSharp7"); break;
                    case "7.1": AddVersionHeader(location, "FromCSharp7.1"); break;
                    case "7.2": AddVersionHeader(location, "FromCSharp7.2"); break;
                    case "'8.":
                    case "8.0": AddVersionHeader(location, "FromCSharp8"); break;
                    case "9.0": AddVersionHeader(location, "FromCSharp9"); break;
                }
            }
        }

        private static void AddVersionHeader(FileInfo location, string version)
        {
            var stream = location.Open(FileMode.Open, FileAccess.ReadWrite);
            {
                var reader = new StreamReader(stream);
                var content = reader.ReadLine();

                content = content.StartsWith("// version:")
                    ? reader.ReadToEnd()
                    : content + reader.ReadToEnd();

                stream.Position = 0;

                using var writer = new StreamWriter(stream, Encoding.UTF8);
                {
                    writer.Write($"// version: {version}\r\n");
                    writer.Write(content);
                    writer.Flush();
                }
            }
        }

        private static FileInfo GetFileLocation(TestMethod testMethod)
        {
            var location = new FileInfo(testMethod.Arguments[0].ToString());
            location = new FileInfo(Path.Combine(location.Directory.Parent.Parent.Parent.Parent.FullName, "Cases", location.Name));
            return location;
        }
    }
}
