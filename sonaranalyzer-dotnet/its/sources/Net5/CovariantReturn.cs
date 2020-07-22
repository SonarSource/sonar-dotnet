using System.Collections.Generic;

namespace Net5
{
    public class Food { public bool HasTaste; }
    public class Meat : Food { public bool IsRed; }

    public abstract class Animal
    {
        public abstract Food GetFood();
    }

    // The feature is in progress

    //public class Tiger : Animal
    //{
        // public override Meat GetFood() => null;
    //}

    public class CovariantReturn
    {
        public virtual IEnumerable<int> OverrideMe(string m) => null;

        public IEnumerable<int> DoNotOverrideMe(string m) => null;
    }

    public class InheritCovariantReturn : CovariantReturn
    {
        // The feature is in progress
        // public override List<int> OverrideMe(string m) => null;

        // hides the member
        public new List<int> DoNotOverrideMe(string m) => null;
    }
}
