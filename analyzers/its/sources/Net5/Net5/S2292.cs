namespace Net5
{
    public class S2292
    {
        public record Record
        {
            private string field;

            public string PropWithGetAndInit
            {
                get { return field; }
                init { field = value; }
            }
        }
    }
}
