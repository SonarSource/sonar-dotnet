using System;
using System.IO;

class Test {
    private static void Main() {
        for (int i = 0; i < 5000; i++) {
            using (TextWriter w = File.CreateText("C:\\tmp\\test\\log" + i + ".txt")) {
                string msg = DateTime.Now + ", " + i;
                w.WriteLine(msg); //NOSONAR
                Console.Out.WriteLine(msg);
            }
        }
        Console.In.ReadLine();
    }
    
      // public static void Main(string[] args) {
        //    string[] lines = System.IO.File.ReadAllLines(@"C:\t1");
          //  Console.Out.WriteLine("contents = " + lines.Length);
          //  Console.In.ReadLine();
        //}    
}