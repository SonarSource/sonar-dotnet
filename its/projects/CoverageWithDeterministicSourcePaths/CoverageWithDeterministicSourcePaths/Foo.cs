namespace Repro3362
{
    public class Foo
    {
        public void Covered(bool b)
        {
            int x = 1 + 1 + (b ? 2 : 4);
        }

        public void NotCovered()
        {
            int x = 1 + 1;
        }
    }
}
