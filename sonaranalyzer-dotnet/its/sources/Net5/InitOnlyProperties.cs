using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Net5
{
    public class InitOnlyProperties
    {
        private readonly string firstName;
        public string FirstName
        {
            get => firstName;
            init => firstName = (value ?? throw new ArgumentNullException(nameof(FirstName)));
        }
    }
}
