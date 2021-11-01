namespace Tests
{
    public class Cases
    {
        public void M()
        {
            var y = string.Empty;
            (y, var z) = ("a", 'x');
        }
    }
}
