class A
{
  void Foo() // Non-Compliant
  {
    bool a = 0 && 1 && 2 && 3 && 4 && 5 && 6 && 7 && 8 && 9 && 10;
  }

  void Bar()
  {
    bool a = 0 && 1 && 2 && 3;
  }

  void Baz();
}
