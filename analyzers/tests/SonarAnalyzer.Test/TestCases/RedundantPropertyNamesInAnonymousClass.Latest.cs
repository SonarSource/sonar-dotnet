namespace CSharp13
{
    public partial class PartialPropertyClass
    {
        public partial int PartialProperty
        {
            get => 42;
            set { }
        }

        public partial int OtherPartialProperty
        {
            get;
            set;
        }

        public void M()
        {
            var anon = new
            {
                PartialProperty = PartialProperty, //Noncompliant
//              ^^^^^^^^^^^^^^^^^
            };

            var anon2 = new
            {
                OtherPartialProperty = OtherPartialProperty, //Noncompliant
            };
        }
    }
}
