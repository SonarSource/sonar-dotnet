public class NullCoalescenceAssignment
{
    public void NullCoalescenceAssignment_Null()
    {
        int? i1 = null;
        i1 ??= (int)i1; // FIXME Non-compliant {{Nullable is known to be empty, this cast throws an exception.}}
    }

    public void NullCoalescenceAssignment_NotNull()
    {
        int? i1 = 1;
        i1 ??= (int)i1;
    }

    public void NullCoalescenceAssignmentResult_Null()
    {
        int? i = null;
        i ??= null;
        var r1 = (int)i; // FIXME Non-compliant
    }

    public void NullCoalescenceAssignmentResult_NotNull()
    {
        int? i = null;
        i ??= 1;
        var r1 = (int)i;
    }
}

public class SwitchExpressions
{
    public void Nullable_In_Arm_Noncompliant()
    {
        int? i = null;
        int? result = i switch
        {
            null => (int) i, // FIXME Non-compliant
            _ => 0
        };
    }

    public int AlwaysNull_Noncompliant(int val)
    {
        int? result = val switch
        {
            1 => null,
            2 => null,
            _ => null
        };
        return (int)result; // FIXME Non-compliant
    }

    public int AlwaysNonNull(int val)
    {
        int? result = val switch
        {
            1 => -1,
            2 => -2,
                _ => -5
        };
        return (int) result;
    }

    public int NullableHasValue(int? val)
    {
        return val.HasValue switch
        {
            true => (int)val,
            _ => 0
        };
    }

    public int NullableNoValue(int? val)
    {
        return val.HasValue switch
        {
            false => (int)val, // FN
            _ => 0
        };
    }
}

public interface IWithDefaultMembers
{
    void NoncompliantDefaultInterfaceMethod()
    {
        int? i1 = null;
        var i2 = (int)i1; // FIXME Non-compliant
    }

    void CompliantDefaultInterfaceMethod()
    {
        int? i1 = 1;
        var i2 = (int)i1;
    }
}

public class LocalStaticFunctions
{
    public void Method(object arg)
    {
        void LocalFunction()
        {
            int? i1 = null;
            var i2 = (int)i1; // FN
        }

        static void LocalStaticFunction()
        {
            int? i1 = null;
            var i2 = (int)i1; // FN
        }
    }
}
