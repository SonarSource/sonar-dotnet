
Imports System
Imports System.Net
Imports System.Net.Security
Imports System.Security.Cryptography.X509Certificates

Namespace Tests.TestCases

    Class CertificateValidationChecks

        Private Sub FalseNegatives()
            'Values from properties are not inspected at all
            CreateRQ().ServerCertificateValidationCallback = AddressOf FalseNegativeValidatorWithProperty
            CreateRQ().ServerCertificateValidationCallback = DelegateProperty
            'Values from overriden operators are not inspected at all
            CreateRQ().ServerCertificateValidationCallback = New CertificateValidationChecks() + 42 'Operator + is overriden to return delegate. 
        End Sub

        Private Sub DirectAddHandlers()
            'Inline version
            'Secondary@+1
            CreateRQ().ServerCertificateValidationCallback = Function(sender, certificate, chain, SslPolicyErrors) True    'Noncompliant  {{Enable server certificate validation on this SSL/TLS connection}}
            '          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^                                                         ^^^^
            CreateRQ().ServerCertificateValidationCallback = Function(sender, certificate, chain, SslPolicyErrors) False
            CreateRQ().ServerCertificateValidationCallback = Function(sender, certificate, chain, SslPolicyErrors) certificate.Subject = "Test"

            'Lambda block syntax
            CreateRQ().ServerCertificateValidationCallback = Function(sender, certificate, chain, SslPolicyErrors)                       'Noncompliant
                                                                 Return True    'Secondary
                                                             End Function
            CreateRQ().ServerCertificateValidationCallback = Function(sender, certificate, chain, SslPolicyErrors)
                                                                 Return False
                                                             End Function

            'With variable
            Dim rq As HttpWebRequest = CreateRQ()
            rq.ServerCertificateValidationCallback = AddressOf InvalidValidation            'Noncompliant

            'Without variable
            CreateRQ().ServerCertificateValidationCallback = AddressOf InvalidValidation    'Noncompliant

            'Assignment syntax = instead of =
            CreateRQ().ServerCertificateValidationCallback = AddressOf InvalidValidation    'Noncompliant
            CreateRQ().ServerCertificateValidationCallback = Function(sender, certificate, chain, SslPolicyErrors) True     'Noncompliant
            'Secondary@-1

            'Do not test this one. It's .NET Standard 2.1 target only. It shoudl work since we're hunting RemoteCertificateValidationCallback and method signature
            'var ws = new System.Net.WebSockets.ClientWebSocket()
            'ws.Options.RemoteCertificateValidationCallback = InvalidValidation

            'Do not test this one. It's .NET Standard 2.1 target only. It shoudl work since we're hunting RemoteCertificateValidationCallback and method signature
            'var sslOpts = new System.Net.Security.SslClientAuthentication()
            'Security.SslClientAuthenticationOptions.RemoteCertificateValidationCallback
        End Sub

        Private Sub MultipleHandlers()
            ServicePointManager.ServerCertificateValidationCallback = AddressOf CompliantValidation
            ServicePointManager.ServerCertificateValidationCallback = AddressOf CompliantValidationPositiveA
            ServicePointManager.ServerCertificateValidationCallback = AddressOf InvalidValidation                          'Noncompliant
            ServicePointManager.ServerCertificateValidationCallback = AddressOf CompliantValidationPositiveB
            ServicePointManager.ServerCertificateValidationCallback = AddressOf CompliantValidationNegative
            ServicePointManager.ServerCertificateValidationCallback = AddressOf AdvInvalidTry                              'Noncompliant
            ServicePointManager.ServerCertificateValidationCallback = AddressOf AdvInvalidWithTryObstacles                 'Noncompliant
            ServicePointManager.ServerCertificateValidationCallback = AddressOf AdvCompliantWithTryObstacles
            ServicePointManager.ServerCertificateValidationCallback = AddressOf AdvInvalidWithObstacles                    'Noncompliant
            ServicePointManager.ServerCertificateValidationCallback = AddressOf AdvCompliantWithObstacles
        End Sub

        Private Sub GenericHandlerSignature()
            Dim HttpHandler As New System.Net.Http.HttpClientHandler()          'This is not RemoteCertificateValidationCallback delegate type, but Func<...>
            HttpHandler.ServerCertificateCustomValidationCallback = AddressOf InvalidValidation            'Noncompliant          

            'Generic signature check without RemoteCertificateValidationCallback
            Dim ShouldTrigger As New RelatedSignatureType()
            ShouldTrigger.Callback = AddressOf InvalidValidation                                           'Noncompliant
            ShouldTrigger.Callback = AddressOf CompliantValidation

            Dim ShouldNotTrigger As New NonrelatedSignatureType()
            ShouldNotTrigger.Callback = Function(sender, chain, certificate, SslPolicyErrors) True   'Compliant, because signature types are not in expected order for validation
            ShouldNotTrigger.Callback = Function(sender, chain, certificate, SslPolicyErrors) False
        End Sub

        Private Sub PassedAsArgument()
            Dim SingleAssignmentCB, FalseNegativeCB, CompliantCB As RemoteCertificateValidationCallback
            Dim DeclarationAssignmentCompliantCB As RemoteCertificateValidationCallback = Nothing
            If True Then
                'If there's only one assignemnt, we will inspect it
                'Secondary@+1
                SingleAssignmentCB = AddressOf InvalidValidationAsArgument              'Secondary
                FalseNegativeCB = AddressOf InvalidValidation                           'Compliant due to false negative, the second assignment is after usage of the variable
                CompliantCB = AddressOf InvalidValidation                               'Compliant due to further logic and more assingments
            End If
            If True Then 'Environment.ComputerName, Environment.CommandLine, Debugger.IsAttached, Config.TestEnvironment
                CompliantCB = Nothing                                                   'Compliant, there are more assignments, so there is a logic
                DeclarationAssignmentCompliantCB = AddressOf InvalidValidation          'This is compliant due to the more assignments, first one is in variable initialization
            End If
            'Secondary@+1
            InitAsArgument(SingleAssignmentCB)                                          'Secondary
            InitAsArgument(FalseNegativeCB)
            InitAsArgument(CompliantCB)
            InitAsArgument(DeclarationAssignmentCompliantCB)
            FalseNegativeCB = Nothing                                                   'False negative due to more assignments, but this one is after variable usage.
            'Secondary@+1
            InitAsArgument(AddressOf InvalidValidationAsArgument)                       'Secondary
            InitAsArgument(Function(sender, certificate, chain, SslPolicyErrors) False)
            InitAsArgument(Function(sender, certificate, chain, SslPolicyErrors) True)  'Secondary
            'Secondary@-1
            InitAsArgumentRecursive(AddressOf InvalidValidation, 1)                     'Secondary
            InitAsOptionalArgument()

            'Call in nested class from root (this)
            Dim Inner As New InnerAssignemntClass
            Inner.InitAsArgument(Function(sender, certificate, chain, SslPolicyErrors) True)  'Secondary           
        End Sub

        Private Sub DelegateReturnedByFunction()
            CreateRQ().ServerCertificateValidationCallback = FindInvalid(False)       'Noncompliant
            CreateRQ().ServerCertificateValidationCallback = FindInvalid()            'Noncompliant
            CreateRQ().ServerCertificateValidationCallback = FindLambdaValidator()    'Noncompliant
            CreateRQ().ServerCertificateValidationCallback = FindCompliant(True)
            CreateRQ().ServerCertificateValidationCallback = FindCompliantRecursive(3)
            CreateRQ().ServerCertificateValidationCallback = FindInvalidRecursive(3)  'Noncompliant
        End Sub

        Private Sub ConstructorArguments()
            Dim optA As New OptionalConstructorArguments(Me, cb:=AddressOf InvalidValidation)                 'Noncompliant
            Dim optB As New OptionalConstructorArguments(Me, cb:=AddressOf CompliantValidation)

            Using ms As New System.IO.MemoryStream                                                                                      'Secondary@+1
                Using ssl As New System.Net.Security.SslStream(ms, True, Function(sender, chain, certificate, SslPolicyErrors) True)    'Noncompliant
                End Using
                Using ssl As New System.Net.Security.SslStream(ms, True, AddressOf InvalidValidation)   'Noncompliant
                End Using
                Using ssl As New System.Net.Security.SslStream(ms, True, AddressOf CompliantValidation)
                End Using
            End Using
        End Sub

#Region "Helpers"

        Private Sub InitAsArgument(Callback As RemoteCertificateValidationCallback)   'This double-assigment will fire the seconday for each occurence twice
            Dim cb As RemoteCertificateValidationCallback = Callback    'Secondary
            CreateRQ.ServerCertificateValidationCallback = Callback     'Noncompliant
            CreateRQ.ServerCertificateValidationCallback = cb           'Noncompliant
        End Sub

        Private Sub InitAsOptionalArgument(Optional Callback As RemoteCertificateValidationCallback = Nothing)
            CreateRQ().ServerCertificateValidationCallback = Callback     'Compliant, it is called without argument
        End Sub

        Private Sub InitAsArgumentRecursive(Callback As RemoteCertificateValidationCallback, Cnt As Integer)
            If Cnt = 0 Then
                CreateRQ().ServerCertificateValidationCallback = Callback     'Noncompliant
            Else
                InitAsArgumentRecursive(Callback, Cnt - 1)
            End If
        End Sub

        Private Sub InitAsArgumentRecursiveNoInvocation(Callback As RemoteCertificateValidationCallback, Cnt As Integer)
            If Cnt = 0 Then
                CreateRQ().ServerCertificateValidationCallback = Callback     'Compliant, no one is invoking this
            Else
                InitAsArgumentRecursiveNoInvocation(Callback, Cnt - 1)
            End If
        End Sub

        Public Shared Function CreateRQ() As HttpWebRequest
            Return DirectCast(HttpWebRequest.Create("http:'localhost"), HttpWebRequest)
        End Function

        Private Function IsValid(Crt As X509Certificate) As Boolean
            Return Crt.Subject = "Test"   'We do not inspect inner logic, yet
        End Function

        Private Sub Log(Crt As X509Certificate)
            'Pretend to do some logging
        End Sub

#End Region

#Region "Basic Validators"

        Private Function InvalidValidation(Sender As Object, Certificate As X509Certificate, Chain As X509Chain, PolicyErrors As SslPolicyErrors) As Boolean
            Return True    'Secondary
            'Secondary@-1
            'Secondary@-2
            'Secondary@-3
            'Secondary@-4
            'Secondary@-5
            'Secondary@-6
            'Secondary@-7
            'Secondary@-8
            'Secondary@-9
            'Secondary@-10            
            'Secondary@-11
        End Function

        Private Function InvalidValidationAsArgument(Sender As Object, Certificate As X509Certificate, Chain As X509Chain, PolicyErrors As SslPolicyErrors) As Boolean
            Return True    'Secondary
            'Secondary@-1
            'Secondary@-2
            'Secondary@-3
        End Function

        Private Function CompliantValidation(Sender As Object, Certificate As X509Certificate, Chain As X509Chain, PolicyErrors As SslPolicyErrors) As Boolean
            Return False 'Compliant
        End Function

        Private Function CompliantValidationPositiveA(Sender As Object, Certificate As X509Certificate, Chain As X509Chain, PolicyErrors As SslPolicyErrors) As Boolean
            If Certificate.Subject = "Test" Then
                Return True 'Compliant, checks were done
            Else
                Return False
            End If
        End Function

        Private Function CompliantValidationPositiveB(Sender As Object, Certificate As X509Certificate, Chain As X509Chain, PolicyErrors As SslPolicyErrors) As Boolean
            Return Certificate.Subject = "Test"
        End Function

        Private Function CompliantValidationNegative(Sender As Object, Certificate As X509Certificate, Chain As X509Chain, PolicyErrors As SslPolicyErrors) As Boolean
            If Certificate.Subject <> "Test" Then
                Return False
            ElseIf DateTime.Parse(Certificate.GetExpirationDateString()) < DateTime.Now Then
                Return False
            Else
                Return True
            End If
        End Function

#End Region

#Region "Advanced Validators"

        Private Function AdvInvalidTry(Sender As Object, Certificate As X509Certificate, Chain As X509Chain, PolicyErrors As SslPolicyErrors) As Boolean
            Try
                System.Diagnostics.Trace.WriteLine(Certificate.Subject)
                Return True 'Secondary
            Catch ex As Exception
                System.Diagnostics.Trace.WriteLine(ex.Message)
                Return True 'Secondary 
            End Try
        End Function

        Private Function AdvInvalidWithTryObstacles(Sender As Object, Certificate As X509Certificate, Chain As X509Chain, PolicyErrors As SslPolicyErrors) As Boolean
            Try
                Console.WriteLine("Log something")
                System.Diagnostics.Trace.WriteLine("Log something")
                Log(Certificate)

                Return True 'Secondary
            Catch ex As Exception
                System.Diagnostics.Trace.WriteLine(ex.Message)
            End Try
            Return True 'Secondary
        End Function

        Private Function AdvCompliantWithTryObstacles(Sender As Object, Certificate As X509Certificate, Chain As X509Chain, PolicyErrors As SslPolicyErrors) As Boolean
            Try
                Console.WriteLine("Log something")
                System.Diagnostics.Trace.WriteLine("Log something")
                Log(Certificate)

                Return True 'Compliant, since Log(certificate) can also do some validation and throw exception resulting in return false. It's bad practice, but compliant.
            Catch ex As Exception
                System.Diagnostics.Trace.WriteLine(ex.Message)
            End Try
            Return False
        End Function

        Private Function AdvInvalidWithObstacles(Sender As Object, Certificate As X509Certificate, Chain As X509Chain, PolicyErrors As SslPolicyErrors) As Boolean
            Console.WriteLine("Log something")
            System.Diagnostics.Trace.WriteLine("Log something")
            Log(Certificate)

            Return True 'Secondary
        End Function

        Private Function AdvCompliantWithObstacles(Sender As Object, Certificate As X509Certificate, Chain As X509Chain, PolicyErrors As SslPolicyErrors) As Boolean
            Console.WriteLine("Log something")
            System.Diagnostics.Trace.WriteLine("Log something")
            Log(Certificate)
            Return IsValid(Certificate)
        End Function

#End Region

#Region "Find Validators"

        Private Function FindInvalid() As RemoteCertificateValidationCallback
            Return AddressOf InvalidValidation                                      'Secondary
        End Function

        Private Function FindLambdaValidator() As RemoteCertificateValidationCallback
            Return Function(sender, certificate, chain, SslPolicyErrors) True       'Secondary
        End Function

        Private Function FindInvalid(UseDelegate As Boolean) As RemoteCertificateValidationCallback   'All paths return noncompliant
            If UseDelegate Then
                Return AddressOf InvalidValidation                                  'Secondary
            Else
                Return Function(sender, certificate, chain, SslPolicyErrors) True   'Secondary
            End If
        End Function

        Private Function FindCompliant(Compliant As Boolean) As RemoteCertificateValidationCallback     'At least one path returns compliant => there is a logic and it is considered compliant
            If Compliant Then
                Return Nothing
            Else
                Return Function(sender, certificate, chain, SslPolicyErrors) True
            End If
        End Function

        Private Function FindCompliantRecursive(Index As Integer) As RemoteCertificateValidationCallback
            If Index <= 0 Then
                Return AddressOf CompliantValidation
            Else
                Return FindCompliantRecursive(Index - 1)
            End If
        End Function

        Private Function FindInvalidRecursive(Index As Integer) As RemoteCertificateValidationCallback
            If Index <= 0 Then
                Return AddressOf InvalidValidation                                  'Secondary
            Else
                Return FindInvalidRecursive(Index - 1)
            End If
        End Function

#End Region

#Region "False negatives"

        Private ReadOnly Property DelegateProperty As RemoteCertificateValidationCallback
            Get
                Return Function(sender, certificate, chain, SslPolicyErrors) True   'False negative
            End Get
        End Property

        Private Function FalseNegativeValidatorWithProperty(Sendera As Object, Certificate As X509Certificate, Chain As X509Chain, PolicyErrors As SslPolicyErrors) As Boolean
            Return TrueProperty    'False negative
        End Function

        Private ReadOnly Property TrueProperty As Boolean
            Get
                Return True 'False negative
            End Get
        End Property

        Public Shared Operator +(Instance As CertificateValidationChecks, Number As Integer) As RemoteCertificateValidationCallback
            Return Function(sender, certificate, chain, SslPolicyErrors) True
        End Operator

#End Region

#Region "Nested classes"

        Class RelatedSignatureType

            Public Property Callback As Func(Of NonrelatedSignatureType, X509Certificate2, X509Chain, SslPolicyErrors, Boolean)

        End Class

        Class NonrelatedSignatureType

            'Parameters are in order, that we do not inspect
            Public Property Callback As Func(Of NonrelatedSignatureType, X509Chain, X509Certificate2, SslPolicyErrors, Boolean)

        End Class

        Class OptionalConstructorArguments

            Public Sub New(Owner As Object, Optional A As Integer = 0, Optional b As Integer = 0, Optional cb As RemoteCertificateValidationCallback = Nothing)

            End Sub

        End Class

        Class InnerAssignemntClass

            Public Sub InitAsArgument(Callback As RemoteCertificateValidationCallback)
                CertificateValidationChecks.CreateRQ().ServerCertificateValidationCallback = Callback 'Noncompliant
            End Sub

        End Class

        Class NeighbourAssignemntClass

            Public Sub Init(Callback As RemoteCertificateValidationCallback)
                'Assignemnt from sibling class in nested tree
                Dim Value As New InnerAssignemntClass()
                Value.InitAsArgument(Function(sender, certificate, chain, SslPolicyErrors) True)  'Secondary           
            End Sub

        End Class

#End Region

    End Class

End Namespace
