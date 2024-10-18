
namespace CSharp13
{
    public partial class PartialPropertyClass
    {
        public partial int PartialProperty  
        {
            get;
            set;
        }
        public partial int OtherPartialProperty
        {
            get => 42;
            set { }
        }
    }
}
