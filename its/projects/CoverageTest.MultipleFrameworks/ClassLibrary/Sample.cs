namespace ClassLibrary
{
    public class Sample
    {
        public static string MatchingBranchpoints_FullyCovered(bool b)
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

        public static string MatchingBranchpoints_PartiallyCovered(bool b)
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

        public static string NotMatchingBranchpoints_FullyCovered(bool b)
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

        public static string NotMatchingBranchpoints_PartiallyCovered(bool b)
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
