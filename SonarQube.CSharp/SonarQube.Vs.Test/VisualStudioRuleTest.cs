using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarQube.Analyzers.Rules;
using SonarQube.Analyzers.SonarQube.Settings;

namespace SonarQube.Rules.Test
{
	[TestClass]
	public class VisualStudioRuleTest
	{		
		[TestMethod]
		public void OnlyParameterlessRules()
		{

			var assembly = typeof(RuleParameterAttribute).Assembly;

			var analyzers = assembly
				.GetTypes()
				.Where(t => t.IsSubclassOf(typeof(DiagnosticAnalyzer)))
				.ToList();

			foreach (var analyzer in analyzers)
			{
				var hasParameter = analyzer.GetProperties().Any(p => p.GetCustomAttributes<RuleParameterAttribute>().Any());
				if (hasParameter)
				{
					throw new Exception(string.Format("Visual Studio rules cannot have parameters, remove DiagnosticAnalyzer '{0}'.", analyzer.Name));
				}
			}
		}

		[TestMethod]
		public void NoRuleTemplates()
		{
			var assembly = typeof(RuleParameterAttribute).Assembly;

			var analyzers = assembly
				.GetTypes()
				.Where(t => t.IsSubclassOf(typeof(DiagnosticAnalyzer)))
				.ToList();

			foreach (var analyzer in analyzers)
			{
				var ruleDescriptor = analyzer.GetCustomAttributes<RuleAttribute>().Single();
				if (ruleDescriptor.Template)
				{
					throw new Exception(string.Format("Visual Studio rules cannot be templates, remove DiagnosticAnalyzer '{0}'.", analyzer.Name));
				}
			}
		}
	}
}