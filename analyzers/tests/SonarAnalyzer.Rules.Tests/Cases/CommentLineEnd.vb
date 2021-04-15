Module Module1
  Sub Main() ' Noncompliant multiple words  
'            ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    ' Compliant multiple words
    System.Console.WriteLine("Hello, world!") ' Noncompliant - My first program! ' cont
    System.Console.WriteLine("Hello, world!") ' Compliant
    ' Compliant multiple words

    End Sub
End Module