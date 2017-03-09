using System;
using System.IO;

// Some comment
namespace Foo
{
  using System.Net;
  
  // This does not parse - not in a method - using statements should not be dropped
  using (foo = "foo") {}
}

// This partial using directive will be dropped
using
