using System;

class Program
{
    public static async Task<string> GetReferencedContent(string filename) // Compliant
    {
      string url = await BeginReadFromFile(filename);                      // Compliant
      string contentOfUrl = await BeginHttpGetFromUrl(url);                // Compliant
      return contentOfUrl;
    }


    static void Main(string[] args)
    {
        int async = 42;                                                    // Non-Compliant
        int await = 52;                                                    // Non-Compliant
    }
}
