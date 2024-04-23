namespace ClassLibrary
{
    public class MatchingBranchpoints_FullyCovered
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
