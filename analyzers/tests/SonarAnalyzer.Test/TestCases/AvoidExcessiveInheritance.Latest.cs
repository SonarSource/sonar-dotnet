namespace CSharp15
{
    public closed class Closed_0 { }
    public class Closed_1 : Closed_0 { }
    public class Closed_2 : Closed_1 { }
    public class Closed_3 : Closed_2 { }
    public class Closed_4 : Closed_3 { }
    public class Closed_5 : Closed_4 { }
    public class Closed_6 : Closed_5 { } // Noncompliant {{This class has 6 parents which is greater than 5 authorized.}}
}
