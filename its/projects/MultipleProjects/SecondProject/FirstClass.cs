
namespace SecondProject
{
    public class FirstClass
    {
        public string SwitchExpression(int number) =>
            number switch
            {
                10 => "ten",
                20 => "twenty",
                _ => "unknown"
            };

        public string IfElse(bool condition)
        {
            if (condition) // S3923 (conditional structure with same code on both branches)
            {
                return SwitchExpression(10);
            }
            else
            {
                return SwitchExpression(10);
            }
        }

    }
}
