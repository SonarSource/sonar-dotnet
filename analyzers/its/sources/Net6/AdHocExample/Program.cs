// See https://aka.ms/new-console-template for more information
using System.Runtime.CompilerServices;

int a = 5;
SomeMethod(a == 42);

// CallerArgumentExpression
void SomeMethod(object param, [CallerArgumentExpression("param")] string message = null)
{
    Console.WriteLine(message);
}
