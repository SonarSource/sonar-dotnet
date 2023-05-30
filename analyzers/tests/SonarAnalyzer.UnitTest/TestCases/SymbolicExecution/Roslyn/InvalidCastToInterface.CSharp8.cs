﻿public class NullCoalescenceAssignment
{
    public void NullCoalescenceAssignment_Null()
    {
        int? i1 = null;
        i1 ??= (int)i1; // Noncompliant {{Nullable is known to be empty, this cast throws an exception.}}
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
        var r1 = (int)i; // Noncompliant
    }

    public void NullCoalescenceAssignmentResult_NotNull()
    {
        int? i = null;
        i ??= 1;
        var r1 = (int)i;    // Noncompliant FP FIXME, this doesn't call property int?.HasValue, but method int?.HasValue.get
    }
}

public class SwitchExpressions
{
    public void Nullable_In_Arm_Noncompliant()
    {
        int? i = null;
        int? result = i switch
        {
            null => (int) i, // Noncompliant
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
        return (int)result; // Noncompliant
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
            false => (int)val, // Noncompliant
            _ => 0
        };
    }
}

public interface IWithDefaultMembers
{
    void NoncompliantDefaultInterfaceMethod()
    {
        int? i1 = null;
        var i2 = (int)i1; // Noncompliant
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
