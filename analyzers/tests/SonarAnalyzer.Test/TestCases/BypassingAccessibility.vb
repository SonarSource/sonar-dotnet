Imports System
Imports System.Reflection

Namespace Tests.TestCases
    Class Foo
        Public Sub Test()

            ' RSPEC: https:'jira.sonarsource.com/browse/RSPEC-3011
            Dim dynClass = Type.GetType("MyInternalClass")
            'Questionable.Using BindingFlags.NonPublic will return non-public members
            Dim bindingAttr As BindingFlags = BindingFlags.NonPublic Or BindingFlags.Static
'                                             ^^^^^^^^^^^^^^^^^^^^^^   {{Make sure that this accessibility bypass is safe here.}}

            Dim dynMethod As MethodInfo = dynClass.GetMethod("mymethod", bindingAttr)
            Dim result = dynMethod.Invoke(dynClass, Nothing)

        End Sub

        Public Function AdditionalChecks(t As System.Type) As BindingFlags

            ' Using other binding attributes should be ok
            Dim bindingAttr = BindingFlags.Static Or BindingFlags.CreateInstance Or BindingFlags.DeclaredOnly Or
                BindingFlags.ExactBinding Or BindingFlags.GetField Or BindingFlags.InvokeMethod ' et cetera...
            Dim dynMeth = t.GetMember("mymethod", bindingAttr)

            ' We don't detect casts to the forbidden value
            Dim nonPublic As BindingFlags = DirectCast(32, BindingFlags)
            nonPublic = CType(32, BindingFlags)
            dynMeth = t.GetMember("mymethod", bindingAttr)

            System.Enum.TryParse(Of BindingFlags)("NonPublic", nonPublic)
            dynMeth = t.GetMember("mymethod", bindingAttr)


            bindingAttr = (((BindingFlags.NonPublic)) Or BindingFlags.Static)
'                            ^^^^^^^^^^^^^^^^^^^^^^

            dynMeth = t.GetMember("mymethod", (BindingFlags.NonPublic))
'                                              ^^^^^^^^^^^^^^^^^^^^^^

            Dim val As Integer = CType(BindingFlags.NonPublic, Integer) ' Noncompliant
            Return BindingFlags.NonPublic ' Noncompliant
        End Function


        Public ReadOnly DefaultAccess As BindingFlags = BindingFlags.OptionalParamBinding Or BindingFlags.NonPublic
'                                                                                            ^^^^^^^^^^^^^^^^^^^^^^
        Private ReadOnly access1 = BindingFlags.NonPublic     ' Noncompliant

    End Class
End Namespace
