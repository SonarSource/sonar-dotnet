namespace Net6
{
    internal class AnonymousTypesInWithExpressions
    {
        public void Example()
        {
            var person = new { FirstName = "Scott", LastName = "Hunter", Age = 25 };

            // Extend with expression to anonymous type
            var otherPerson = person with { LastName = "Hanselman" };
        }
    }
}
