using Microsoft.Azure.Cosmos;

namespace Tests.Diagnostics
{
    class AzureCosmosTest
    {
        private Container container = null;

        public void GetItemQueryIterator_Compliant(string continuationToken)
        {
            // Compliant - constant query string
            container.GetItemQueryIterator<MyType>("SELECT * FROM c");
            container.GetItemQueryIterator<MyType>("SELECT * FROM c", continuationToken);
            container.GetItemQueryIterator<MyType>("SELECT * FROM c", continuationToken, null);
        }

        public void GetItemQueryIterator_Noncompliant(string userInput, string continuationToken)
        {
            container.GetItemQueryIterator<MyType>($"SELECT * FROM c WHERE c.id = '{userInput}'"); // Noncompliant
            container.GetItemQueryIterator<MyType>($"SELECT * FROM c WHERE c.id = '{userInput}'", continuationToken); // Noncompliant
            container.GetItemQueryIterator<MyType>($"SELECT * FROM c WHERE c.id = '{userInput}'", continuationToken, null); // Noncompliant

            container.GetItemQueryIterator<MyType>("SELECT * FROM c WHERE c.id = '" + userInput + "'"); // Noncompliant
            container.GetItemQueryIterator<MyType>("SELECT * FROM c WHERE c.id = '" + userInput + "'", continuationToken); // Noncompliant

            container.GetItemQueryIterator<MyType>(string.Format("SELECT * FROM c WHERE c.id = '{0}'", userInput)); // Noncompliant
            container.GetItemQueryIterator<MyType>(string.Format("SELECT * FROM c WHERE c.id = '{0}'", userInput), continuationToken); // Noncompliant

            container.GetItemQueryIterator<MyType>(string.Concat("SELECT * FROM c WHERE c.id = '", userInput, "'")); // Noncompliant
        }

        public void GetItemQueryIterator_VariableAssignment(string userInput, string continuationToken)
        {
            string query = $"SELECT * FROM c WHERE c.id = '{userInput}'"; // Secondary
            container.GetItemQueryIterator<MyType>(query); // Noncompliant

            // Compliant - variable assigned with constant
            string constQuery = "SELECT * FROM c";
            container.GetItemQueryIterator<MyType>(constQuery);
        }

        public void GetItemQueryStreamIterator_Compliant(string continuationToken)
        {
            // Compliant - constant query string
            container.GetItemQueryStreamIterator("SELECT * FROM c");
            container.GetItemQueryStreamIterator("SELECT * FROM c", continuationToken);
            container.GetItemQueryStreamIterator("SELECT * FROM c", continuationToken, null);
        }

        public void GetItemQueryStreamIterator_Noncompliant(string userInput, string continuationToken)
        {
            container.GetItemQueryStreamIterator($"SELECT * FROM c WHERE c.id = '{userInput}'"); // Noncompliant
            container.GetItemQueryStreamIterator($"SELECT * FROM c WHERE c.id = '{userInput}'", continuationToken); // Noncompliant
            container.GetItemQueryStreamIterator($"SELECT * FROM c WHERE c.id = '{userInput}'", continuationToken, null); // Noncompliant

            container.GetItemQueryStreamIterator("SELECT * FROM c WHERE c.id = '" + userInput + "'"); // Noncompliant
            container.GetItemQueryStreamIterator("SELECT * FROM c WHERE c.id = '" + userInput + "'", continuationToken); // Noncompliant

            container.GetItemQueryStreamIterator(string.Format("SELECT * FROM c WHERE c.id = '{0}'", userInput)); // Noncompliant
            container.GetItemQueryStreamIterator(string.Format("SELECT * FROM c WHERE c.id = '{0}'", userInput), continuationToken); // Noncompliant

            container.GetItemQueryStreamIterator(string.Concat("SELECT * FROM c WHERE c.id = '", userInput, "'")); // Noncompliant
        }

        public void GetItemQueryStreamIterator_VariableAssignment(string userInput, string continuationToken)
        {
            string query = $"SELECT * FROM c WHERE c.id = '{userInput}'"; // Secondary
            container.GetItemQueryStreamIterator(query); // Noncompliant

            // Compliant - variable assigned with constant
            string constQuery = "SELECT * FROM c";
            container.GetItemQueryStreamIterator(constQuery);
        }

        public void QueryDefinition_Compliant()
        {
            // Compliant - constant query string
            new QueryDefinition("SELECT * FROM c");
        }

        public void QueryDefinition_Noncompliant(string userInput)
        {
            new QueryDefinition($"SELECT * FROM c WHERE c.id = '{userInput}'"); // Noncompliant
            new QueryDefinition("SELECT * FROM c WHERE c.id = '" + userInput + "'"); // Noncompliant
            new QueryDefinition(string.Format("SELECT * FROM c WHERE c.id = '{0}'", userInput)); // Noncompliant
            new QueryDefinition(string.Concat("SELECT * FROM c WHERE c.id = '", userInput, "'")); // Noncompliant
        }

        public void QueryDefinition_VariableAssignment(string userInput)
        {
            string query = $"SELECT * FROM c WHERE c.id = '{userInput}'"; // Secondary
            new QueryDefinition(query); // Noncompliant

            // Compliant - variable assigned with constant
            string constQuery = "SELECT * FROM c";
            new QueryDefinition(constQuery);
        }
    }

    class MyType
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}

