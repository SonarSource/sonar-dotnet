using System;
using System.IO;
using System.Net;

var fs0 = new FileStream(@"c:\foo.txt", FileMode.Open); // Noncompliant
FileStream fs1 = new(@"c:\foo.txt", FileMode.Open); // FN

var fs2 = new FileStream(@"c:\foo.txt", FileMode.Open); // Compliant, passed to a method
NoOperation(fs2);

FileStream fs3; // Compliant - not instantiated
using var fs5 = new FileStream(@"c:\foo.txt", FileMode.Open); // Compliant

using FileStream fs6 = new(@"c:\foo.txt", FileMode.Open); // Compliant

void NoOperation(object x) { }

class Foo
{
    public void Bar(object cond)
    {
        var fs = new FileStream("", FileMode.Open); // FN, not disposed on all paths
        if (cond is 5)
        {
            fs.Dispose();
        }
        else if (cond is not 599)
        {
            fs.Dispose();
        }
    }

    public void Lambdas()
    {
        Action<int, int> a = static (int v, int w) => {
            var fs = new FileStream("", FileMode.Open); // Noncompliant
                                                        // Noncompliant@-1 - duplicate
        };
        Action<int, int> b = (_, _) => {
            var fs = new FileStream("", FileMode.Open);
            fs.Dispose();
        };
    }
}

record MyRecord
{
    private FileStream field_fs1; // Compliant - not instantiated
    public FileStream field_fs2 = new FileStream(@"c:\foo.txt", FileMode.Open); // Compliant - public
    private FileStream field_fs3 = new FileStream(@"c:\foo.txt", FileMode.Open); // Noncompliant {{Dispose 'field_fs3' when it is no longer needed.}}
            // Noncompliant@-1 - duplicate
    private FileStream field_fs4 = new FileStream(@"c:\foo.txt", FileMode.Open); // Compliant - disposed

    private FileStream backing_field;
    public FileStream Prop
    {
        init
        {
            backing_field = new FileStream("", FileMode.Open); // Noncompliant
            // Noncompliant@-1 - duplicate
        }
    }

    public void Foo()
    {
        field_fs4.Dispose();

        FileStream fs5; // Compliant - used properly
        using (fs5 = new FileStream(@"c:\foo.txt", FileMode.Open))
        {
            // do nothing but dispose
        }

        using (fs5 = new(@"c:\foo.txt", FileMode.Open))
        {
            // do nothing but dispose
        }

        FileStream fs1 = new(@"c:\foo.txt", FileMode.Open); // FN
        var fs2 = File.Open(@"c:\foo.txt", FileMode.Open); // Noncompliant - instantiated with factory method
            // Noncompliant@-1 - duplicate
        var s = new WebClient(); // Noncompliant - another tracked type
            // Noncompliant@-1 - duplicate
    }
}
