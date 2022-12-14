using System;

namespace IncrementalPRAnalysisDuplication
{
    public class DuplicatedClassPart1
    {
        private string DuplicatedString = "DuplicatedString.";

        private void SomeMethod(int a, int b)
        {
            var someString = "This is some very high end sophisticated solution.";
            for (int i = 0; i < a; i++)
            {
                someString += b.ToString();
            }

            Console.WriteLine("I am wondering why I am writing this to the console at all?", someString);
        }

        private void SomeMethod2(object o)
        {
            if (o == "Have you ever seen such an important and complex codebase as this one?")
            {
                throw new ArgumentException("Like did you really call this method with this argument? What is wrong with you?");
            }
            else
            {
                SomeMethod(16, 23);
            }
        }

        private void SomeMethod3(object o)
        {
            if (o == "Have you ever seen such an important and complex codebase as this one?")
            {
                throw new ArgumentException("Like did you really call this method with this argument? What is wrong with you?");
            }
            else
            {
                SomeMethod(16, 23);
            }
        }
        private void SomeMethod4(object o)
        {
            if (o == "Have you ever seen such an important and complex codebase as this one?")
            {
                throw new ArgumentException("Like did you really call this method with this argument? What is wrong with you?");
            }
            else
            {
                SomeMethod(16, 23);
            }
        }

        private void SomeMethod5(object o)
        {
            if (o == "Have you ever seen such an important and complex codebase as this one?")
            {
                throw new ArgumentException("Like did you really call this method with this argument? What is wrong with you?");
            }
            else
            {
                SomeMethod(16, 23);
            }
        }

        private void SomeMethod6(object o)
        {
            if (o == "Have you ever seen such an important and complex codebase as this one?")
            {
                throw new ArgumentException("Like did you really call this method with this argument? What is wrong with you?");
            }
            else
            {
                SomeMethod(16, 23);
            }
        }

        private class SomeInnerClass
        {
            public const int a = 1;
        }
    }
}
