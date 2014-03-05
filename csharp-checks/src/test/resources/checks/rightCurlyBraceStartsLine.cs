class C
{
   public void f1()
   {
     doSomthing();}                     // Noncompliant

   public void f2()
   {
     if (true)
     {
       doSomething();}                  // Noncompliant

     Foo f = delegate {
       doSomething();};                 // Noncompliant

     {
       doSomething();}                  // Noncompliant

     if (true) { doSomething(); }       // Compliant
    }


   public void f3()
   {
      doSomething(new[] { new Foo (1),
                          new Foo (2)});  // Compliant

      List<int> d = new List<int> { 0, 1, 2, 3, 4,
                                    5, 6, 7, 8, 9 }; // Compliant
      Bar b = new Bar(){ A = 1, B = 2,
                         C = 3, D = 4};   // Compliant
   }                                      // Compliant
}
