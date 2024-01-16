Imports System.Globalization
Imports System.DateTime

Class Test
    ReadOnly formatProviderField As IFormatProvider = New CultureInfo("en-US")

    Sub DifferentSyntaxScenarios()
        Dim dt = Date.Parse("01/02/2000")                                           ' Noncompliant
        '        ^^^^^^^^^^^^^^^^^^^^^^^^
        Date.Parse("01/02/2000")                                                    ' Noncompliant

        Parse("01/02/2000")                                                         ' Noncompliant {{Use a format provider when parsing date and time.}}
        System.DateTime.Parse("01/02/2000")                                         ' Noncompliant
        DATETIME.PARSE("01/02/2000")                                                ' Noncompliant

        Dim parsedDate As Date = Nothing

        If Date.TryParse("01/02/2000", parsedDate) Then                             ' Noncompliant
        End If

        Date.Parse(provider:=Nothing, s:="02/03/2000", styles:=DateTimeStyles.None) ' Noncompliant

        dt = Date.Parse("01/02/2000").AddDays(1)                                    ' Noncompliant

        Dim parsedDates = {"01/02/2000"}.Select(Function(x) Date.Parse(x))          ' Noncompliant
    End Sub

    Sub CallWithNullIFormatProvider()
        Date.Parse("01/02/2000", Nothing)                                               ' Noncompliant
        Date.Parse("01/02/2000", Nothing, DateTimeStyles.None)                          ' Noncompliant

        Date.Parse("01/02/2000", (Nothing))                                             ' FN
        Date.Parse("01/02/2000", If(True, CType(Nothing, IFormatProvider), Nothing))    ' FN

        Dim nullFormatProvider As IFormatProvider = Nothing
        Date.Parse("01/02/2000", nullFormatProvider)                                    ' FN
    End Sub

    Sub CallWithFormatProvider()
        Date.Parse("01/02/2000", CultureInfo.InvariantCulture)                 ' Compliant
        Date.Parse("01/02/2000", CultureInfo.CurrentCulture)                   ' Compliant
        Date.Parse("01/02/2000", CultureInfo.GetCultureInfo("en-US"))          ' Compliant
        Date.Parse("01/02/2000", formatProviderField)                          ' Compliant
        Date.Parse("01/02/2000", formatProviderField)                          ' Compliant
    End Sub

    Sub ParseMethodsOfNonTemporalTypes()
        Integer.Parse("1")                                                      ' Compliant - this rule only deals with temporal types
        Dim parsedDouble = Nothing
        Double.TryParse("1.1", parsedDouble)
    End Sub
End Class

Class CustomTypeCalledDateTime
    Public Structure DateTime
        Public Shared Function Parse(s As String) As DateTime
            Return New DateTime()
        End Function
    End Structure

    Sub New()
        Dim currentTime = DateTime.Parse("01/02/2000")                          ' Compliant - this is not System.DateTime
    End Sub
End Class
