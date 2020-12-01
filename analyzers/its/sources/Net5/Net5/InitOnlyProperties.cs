using System;

namespace Net5
{
    public class InitOnlyProperties
    {
        private readonly string firstName;
        private readonly string lastName;

        public string FirstName
        {
            get => firstName;
            init => firstName = (value ?? throw new ArgumentNullException(nameof(FirstName)));
        }

        public string LastName
        {
            get => lastName;
            init => lastName = value!;
        }

        public string Details { get; init; }

        public InitOnlyProperties()
        {
            Details = string.Empty;
        }
    }
}
