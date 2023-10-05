using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;

[TestClass]
class ClassTest1(int primaryConstructor) // Noncompliant
//    ^^^^^^^^^^
{
}

[TestFixture]
class ClassTest2(int primaryConstructor) // Noncompliant
//    ^^^^^^^^^^
{
}
