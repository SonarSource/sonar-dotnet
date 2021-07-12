namespace Net5
{
    public class S3776
    {
        bool ChainedSimilarConditionsWithParentheses(int one, int two) =>
            (one is 1 or 2) || (two is 3 or 5) || (two is 3 or 5) || (two is 3 or 5)
            || (two is 3 or 5) || (two is 3 or 5) || (two is 3 or 5) || (two is 3 or 5)
            || (two is 3 or 5) || (two is 3 or 5) || (two is 3 or 5) || (two is 3 or 5)
            || (two is 3 or 5) || (two is 3 or 5) || (two is 3 or 5) || (two is 3 or 5)
            || (two is 3 or 5) || (two is 3 or 5) || (two is 3 or 5) || (two is 3 or 5);
    }
}
