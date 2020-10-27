namespace Net5
{
    public class TargetTypeConditionals
    {
        public class Person { public string Name; }
        public class Student : Person { public int Grade; }
        public class Customer : Person { public bool HasMoney; }

        public void Method(Student s, Customer c, int b)
        {
            // The feature is in progress
            // Person person = s ?? c;

            // The feature is in progress
            // int? result = b ? 0 : null;
        }
    }
}
