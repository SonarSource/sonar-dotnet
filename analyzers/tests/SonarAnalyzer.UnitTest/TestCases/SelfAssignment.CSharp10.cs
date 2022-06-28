int x = 42;

  (x, var y) = (x, 42);
// ^                    Noncompliant
//              ^       Secondary@-1
