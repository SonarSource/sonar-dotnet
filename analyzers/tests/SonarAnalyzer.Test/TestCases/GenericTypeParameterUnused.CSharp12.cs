public class Example<T>(T param) // Compliant
{
    bool IsNull() => param is null;
}
