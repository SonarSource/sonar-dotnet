using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.IO;

namespace NSonarQubeAnalyzer
{
    public class Program
    {
        static void Main(string[] args)
        {
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(File.ReadAllText(args[0], Encoding.UTF8));
            int lines = new Metrics(syntaxTree).Lines();
            Console.WriteLine(lines);
        }
    }
}
