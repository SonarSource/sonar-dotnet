using System;

class Program
{
  // This line is fine, but the following is not
  //int Id = 0;

  /*
  The following line is bad

  void Test() { int dirId = this.Id++; }

  No violation on the following line, because there is at most one violation per comment
  MyMethod();
  */

  static void Main(string[] args)
  {
  }
}
