public class GotoStatement
{
    void LabeledBreakAndContinue(int[][] matrix)
    {
        outer:
        foreach (var row in matrix)
        {
            foreach (var item in row)
            {
                if (item < 0)
                {
                    continue outer;     // Compliant
                }
                if (item == 0)
                {
                    break outer;        // Compliant
                }
            }
        }
    }
}
