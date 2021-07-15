namespace Net5
{
    public class S3776
    {
        string field;
        private int one, two;
        string Property
        {
            init
            {
                if ((one is 1 or 2) || (two is 3 or 5) || (two is 3 or 5) || (two is 3 or 5)
                    || (two is 3 or 5) || (two is 3 or 5) || (two is 3 or 5) || (two is 3 or 5)
                    || (two is 3 or 5) || (two is 3 or 5) || (two is 3 or 5) || (two is 3 or 5)
                    || (two is 3 or 5) || (two is 3 or 5) || (two is 3 or 5) || (two is 3 or 5)
                    || (two is 3 or 5) || (two is 3 or 5) || (two is 3 or 5) || (two is 3 or 5))
                {
                    field = "";
                }
            }
        }
        bool ChainedSimilarConditionsWithParentheses(int one, int two) =>
            (one is 1 or 2) || (two is 3 or 5) || (two is 3 or 5) || (two is 3 or 5)
            || (two is 3 or 5) || (two is 3 or 5) || (two is 3 or 5) || (two is 3 or 5)
            || (two is 3 or 5) || (two is 3 or 5) || (two is 3 or 5) || (two is 3 or 5)
            || (two is 3 or 5) || (two is 3 or 5) || (two is 3 or 5) || (two is 3 or 5)
            || (two is 3 or 5) || (two is 3 or 5) || (two is 3 or 5) || (two is 3 or 5);
    }
}
