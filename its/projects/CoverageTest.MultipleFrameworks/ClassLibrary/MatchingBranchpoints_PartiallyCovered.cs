namespace ClassLibrary
{
    public class MatchingBranchpoints_PartiallyCovered
    {
        public static string Method(bool b)
        {
            var s = b.ToString();
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
