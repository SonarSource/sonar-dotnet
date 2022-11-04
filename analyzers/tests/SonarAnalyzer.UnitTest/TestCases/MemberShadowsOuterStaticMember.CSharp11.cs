public interface IMath<T>
{
    public static abstract T AbstractMethod(T t);

    public static virtual T VirtualMethod(T t)
    {
        return t;
    }

    public class Inner
    {
        public T AbstractMethod(T t) // Compliant, FN
        {
            return t;
        }

        public T VirtualMethod(T t) // Compliant, FN
        {
            return t;
        }
    }
}
