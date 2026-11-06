using System;

try
{
    return args[0].Length;
}
catch (NullReferenceException nre) // Noncompliant {{Do not catch NullReferenceException; test for null instead.}}
{
    throw;
}

try
{
    return args[0].Length;
}
catch (Exception e) when (((e is NullReferenceException))) // Noncompliant
{
    throw;
}

try
{
    return args[0].Length;
}
catch (Exception e) when (((e is not NullReferenceException))) // Compliant
{
    throw;
}

try
{
    return args[0].Length;
}
catch (Exception e) when (e is NullReferenceException nre) // FN NET-5019
{
    throw;
}

try
{
    return args[0].Length;
}
catch (Exception e) when (e is NullReferenceException _) // FN NET-5019
{
    throw;
}

try
{
    return args[0].Length;
}
catch (Exception e) when (e is NullReferenceException or ArgumentNullException) // FN NET-5019
{
    throw;
}

try
{
    return args[0].Length;
}
catch (Exception e) when (e is NullReferenceException { Message: not null }) // FN NET-5019
{
    throw;
}

try
{
    return args[0].Length;
}
catch (Exception e) when (e is (NullReferenceException)) // FN NET-5019
{
    throw;
}

try
{
    return args[0].Length;
}
catch (Exception e) when (e is not NullReferenceException) // Compliant
{
    throw;
}

record R
{
    int i;
    public string P
    {
        init
        {
            try
            {
                i = value.Length;
            }
            catch (NullReferenceException e) // Noncompliant
            {
                throw;
            }
        }
    }
}
