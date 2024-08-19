namespace ClassLibrary
{
    public class NotMatchingBranchpoints_PartiallyCovered
    {
        public static string Method(bool b)
        {
            var s = $"{b}";
            if (b)
            {
                return s;
            }
            else
            {
                return "No";
            }
        }
    }
}
