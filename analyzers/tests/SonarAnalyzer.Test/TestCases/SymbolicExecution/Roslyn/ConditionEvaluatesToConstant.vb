Imports System
Imports System.IO
Imports System.Runtime.InteropServices

Public Class ConditionEvaluatesToConstant
    Private Const t As Boolean = True
    Private Const f As Boolean = False

    Public Sub LoopsWithBreak(ByVal o1 As Object, ByVal o2 As Object, ByVal o3 As Object)
        Dim c1, c2 As Boolean
        c1 = c2 = True
        While c1 ' Noncompliant
            If o1 IsNot Nothing Then Exit While ' Secondary
        End While

        Do
            If o2 IsNot Nothing Then Exit Do
        Loop While c2 ' Noncompliant

    End Sub

    Public Sub IfStatement()
        Dim c1 = True
        If c1 Then ' Noncompliant
            Console.WriteLine("Always True")
        End If
    End Sub

    Private Sub UsingStatement()
        Using writer As TextWriter = Nothing
            If writer IsNot Nothing Then              ' Noncompliant
                Console.WriteLine("Hello world")      ' Secondary
            End If
        End Using
    End Sub

    Public Sub DoesNotRaiseForConst()
        If t Then ' Compliant - no issue is raised for const fields.
            Console.WriteLine("Do stuff")
        End If
    End Sub

    Public Sub NotExecutedLoops(ByVal o1 As Object, ByVal o2 As Object, ByVal o3 As Object)
        Dim c1, c2, c3 As Boolean
        c1 = c2 = c3 = False

        While c1                                        ' Noncompliant {{Change this condition so that it does not always evaluate to 'False'. Some code paths are unreachable.}}
            If o1 IsNot Nothing Then Exit While         ' Secondary
        End While

        Do
            If o2 IsNot Nothing Then Exit Do

        Loop While c2   ' Noncompliant {{Change this condition so that it does not always evaluate to 'False'.}}
        '          ^^
    End Sub

    Public Sub BreakInLoop(ByVal o As Object)
        Dim c = True
        While c   ' Noncompliant
            If o IsNot Nothing Then Exit While
        End While
    End Sub

    Public Sub ReturnInLoop(ByVal o As Object)
        Dim c = True
        While c   ' Noncompliant
            If o IsNot Nothing Then Return
        End While
    End Sub

    Public Sub ThrowInLoop(ByVal o As Object)
        Dim c = True
        While c   ' Noncompliant
            If o IsNot Nothing Then Throw New Exception()
        End While
    End Sub

    Public Sub ConstField(ByVal a As Boolean, ByVal b As Boolean)

        Dim x = t OrElse a OrElse b ' Compliant t is const

        If t = True Then            ' Noncompliant
            Console.WriteLine("")
        End If

    End Sub

    Public Sub Foo1(ByVal a As Boolean, ByVal b As Boolean)
        Dim l = True
        Dim x = l OrElse a OrElse b
        '       ^                       Noncompliant
        '                ^^^^^^^^^^     Secondary@-1
    End Sub

    Public Sub Foo2(ByVal a As Boolean, ByVal b As Boolean)
        Dim l = True
        Dim x = l OrElse a OrElse b
        '       ^                       Noncompliant
        '                ^^^^^^^^^^     Secondary@-1

    End Sub

    Public Sub Foo3(ByVal a As Boolean, ByVal b As Boolean)
        Dim l = True
        Dim x = l OrElse a OrElse b
        '       ^                     Noncompliant
        '                ^^^^^^^^^^     Secondary@-1
    End Sub

    Public Sub Foo4(ByVal a As Boolean, ByVal b As Boolean)
        Dim l = True
        Dim m = False
        Dim x = m OrElse l OrElse a OrElse b
        '       ^                               Noncompliant
        '                ^                      Noncompliant@-1
        '                         ^^^^^^^^^^     Secondary@-2

    End Sub

    Public Sub Foo5(ByVal a As Boolean, ByVal b As Boolean)
        Dim l = True
        Dim m = False
        Dim x = m AndAlso l OrElse a OrElse b
        '       ^                          Noncompliant
        '                 ^                Secondary@-1
    End Sub

    Public Sub Foo6(ByVal a As Boolean, ByVal b As Boolean)
        Dim l = True
        Dim x = If(l OrElse a, a, b)
        '          ^                       Noncompliant
        '                   ^              Secondary@-1
        '                         ^       Secondary@-2
    End Sub

    Public Sub Foo7(ByVal a As Boolean, ByVal b As Boolean)
        Dim l = True
        If If(l OrElse a, a, b) OrElse b Then
            ' ^                              Noncompliant
            '          ^                     Secondary@-1
            '                ^               Secondary@-2
        End If
    End Sub

    Public Sub Method1()
        Dim b = True
        If b Then                      ' Noncompliant
            Console.WriteLine()
        Else
            ' secondary location covers all unreachable code blocks:
            Console.WriteLine(1)       ' Secondary ^13#103
            While b
                Console.WriteLine(2)
            End While   ' the secondary location ends at the end of the above line
        End If

        Console.WriteLine()
    End Sub

    Public Sub Method2()
        Dim b = True
        If b Then                      ' Noncompliant
            Console.WriteLine()
        End If

        If Not b Then                   ' Noncompliant
            Console.WriteLine()         ' Secondary
        End If

        Console.WriteLine()
    End Sub

    Public Sub Method2Literals()
        If True Then   ' Compliant
            Console.WriteLine()
        End If

        If False Then  ' Compliant
            Console.WriteLine()
        End If

        Console.WriteLine()
    End Sub

    Public Sub Method3()
        Dim b As Boolean
        TryGet(b)
        If b Then
        End If
    End Sub

    Private Sub TryGet(<Out> ByRef b As Boolean)
        b = False
    End Sub

    Public Sub Method3(ByVal cond As Boolean)
        While cond
            Console.WriteLine()
        End While

        Dim b = True
        While b               ' Noncompliant
            Console.WriteLine()
        End While

        Console.WriteLine()    ' Secondary
    End Sub

    Public Sub Method4(ByVal cond As Boolean)
        Dim i = 10
        While i < 20
            i = i + 1
        End While

        Dim b = True
        While b               ' Noncompliant
            Console.WriteLine()
        End While

        Console.WriteLine()    ' Secondary
    End Sub

    Public Sub Method5()
        While True            ' Compliant
            Console.WriteLine()
        End While

        Console.WriteLine()
    End Sub


    Public Sub Method_Select()
        Dim i = 10
        Dim b = True
        Select Case i
            Case 1             ' Noncompliant
                b = False      ' Secondary
        End Select

        If b Then              ' Noncompliant
        Else
        End If
    End Sub

    Public Sub Method_Select_Learn(ByVal cond As Boolean)
        Select Case cond
            Case True
                If cond Then ' Noncompliant
                    Console.WriteLine()
                End If
        End Select
    End Sub

    Public Sub RelationshipWithConstraint(ByVal a As Boolean, ByVal b As Boolean)
        If a = b AndAlso a Then
            If b Then                      ' FN: requires relation support
            '  ~
            End If
        End If

        If a <> b AndAlso a Then
            If b Then                     ' FN: requires relation support
            End If
        End If

        If a AndAlso b Then
            If a = b Then                 ' Noncompliant
            End If
        End If

        If a AndAlso b AndAlso a = b Then
        '                      ^^^^^        Noncompliant
        End If

        a = True
        b = False
        If a AndAlso b Then
        '  ^                                Noncompliant
        '            ^                      Noncompliant@-1

        End If
    End Sub


    Public Property Property1 As Boolean
        Get
            Dim a = New Action(Sub()
                                   Dim b = True
                                   If b Then                      ' Noncompliant
                                       Console.WriteLine()
                                   Else
                                       Console.WriteLine()        ' Secondary
                                   End If
                               End Sub)
            Return True
        End Get
        Set(ByVal value As Boolean)
            value = True
            If value Then                  ' Noncompliant
                Console.WriteLine()
            Else
                Console.WriteLine()        ' Secondary
            End If
        End Set
    End Property

    Public Shared ReadOnly Property Prop As Boolean
        Get
            Return Prop
        End Get
    End Property

    Public Sub Method_Complex()
        Dim guard1 = True
        Dim guard2 = True
        Dim guard3 = True
        Dim guard4 = True

        While GetCondition()
            If guard1 Then
                guard1 = False
            Else
                If guard2 Then          ' Compliant
                    guard2 = False
                Else
                    If guard3 Then      ' Noncompliant FP
                        guard3 = False
                    Else
                        guard4 = False  ' Secondary FP
                    End If
                End If
            End If
        End While

        If guard4 Then                 ' Noncompliant FP: loop is only analyzed three times
            Console.WriteLine()
        End If
    End Sub

    Public Sub Method_Complex_2()
        Dim x = False
        Dim y = False

        While GetCondition()
            While GetCondition()
                If x Then
                    If y Then
                    End If
                End If
                y = True
            End While
            x = True
        End While
    End Sub
    Private Shared Function GetObject() As Object
        Return Nothing
    End Function
    Public Sub M()

        Dim o1 = GetObject()
        Dim o2 As Object = Nothing

        If o1 IsNot Nothing Then
            If Not Equals(o1.ToString(), Nothing) Then
                o2 = New Object()
            End If
        End If

        If o2 Is Nothing Then
        End If

    End Sub

    Public Sub NullableStructs()
        Dim i As Integer? = Nothing

        If i Is Nothing Then              ' Noncompliant, always true
            Console.WriteLine(i)
        End If

        i = New Integer?()
        If i Is Nothing Then              ' Noncompliant
        End If

        Dim ii = ""
        If ii Is Nothing Then             ' Noncompliant, always false
            Console.WriteLine(ii)         ' Secondary
        End If
    End Sub

    Private Shared Function GetCondition() As Boolean
        Return True
    End Function

    Public Sub Lambda(ByVal condition As Boolean)

        Dim fail = False
        Dim a As Action = New Action(Sub() fail = condition)
        a()

        If fail Then ' Noncompliant FP
        End If
    End Sub

    Public Sub Constraint(ByVal cond As Boolean)
        Dim a = cond
        Dim b = a
        If a Then
            If b Then ' FN: requires relation support

            End If
        End If
    End Sub

    Public Sub Stack(ByVal cond As Boolean)
        Dim a = cond
        Dim b = a

        If Not a Then
            If b Then    ' FN: requires relation support
            End If
        End If

        Dim fail = False
        Dim ac As Action = New Action(Sub() fail = cond)

        ac()

        If Not fail Then ' Noncompliant FP
        End If
    End Sub

    Public Sub BooleanBinary(ByVal a As Boolean, ByVal b As Boolean)
        If a And Not b Then
            If a Then       ' FN: engine doesn't learn BoolConstraints from binary operators
            End If
            If b Then       ' FN: engine doesn't learn BoolConstraints from binary operators
            End If

        End If

        If Not (a Or b) Then
            If a Then                  ' FN: engine doesn't learn BoolConstraints from binary operators
            End If

        End If

        If a Xor b Then
            If Not a Xor Not b Then    ' FN: engine doesn't learn BoolConstraints from binary operators
            End If
        End If

        a = False
        If a And b Then      ' Noncompliant
        End If

        a = a Or True
        If a Then           ' Noncompliant
        End If

        a = a Or True
        If a Then           ' Noncompliant
        End If

        a = a Xor True
        If a Then           ' Noncompliant
        End If

        a = a Xor True
        If a Then           ' Noncompliant
        End If
    End Sub

    Public Sub CompoundAssignment(ByVal a As Boolean, ByVal b As Boolean)
        ' https://learn.microsoft.com/en-us/dotnet/visual-basic/language-reference/operators/assignment-operators
        a &= True
        If a Then           ' FN
        End If

        a ^= True
        If a Then           ' FN
        End If
    End Sub

    Public Sub IsAsExpression(ByVal o As Object)

        If TypeOf o Is String Then
        End If

        Dim oo As Object = TryCast(o, String)
        If oo Is Nothing Then
        End If

        o = Nothing
        If TypeOf o Is Object Then   ' Noncompliant
        End If

        oo = TryCast(o, Object)
        If oo Is Nothing Then        ' Noncompliant
        End If
    End Sub

    Public Overloads Sub Equals(ByVal b As Boolean)
        Dim a = True
        If a = b Then
            If b Then ' Noncompliant
            End If
        Else
            If b Then ' Noncompliant
            End If
        End If

        If Not a = b Then
            If b Then ' Noncompliant
            End If
        Else
            If b Then ' Noncompliant
            End If
        End If
    End Sub

    Public Sub NotEquals(ByVal b As Boolean)
        Dim a = True
        If a <> b Then
            If b Then ' Noncompliant
            End If
        Else
            If b Then ' Noncompliant
            End If
        End If

        If Not a <> b Then
            If b Then ' Noncompliant
            End If
        Else
            If b Then ' Noncompliant
            End If
        End If
    End Sub

    Public Sub EqRelations(ByVal a As Boolean, ByVal b As Boolean)
        If a = b Then
            If b = a Then              ' FN: requires relation support
            End If
            If b = Not a Then          ' FN: requires relation support
            End If
            If Not b = Not Not a Then  ' FN: requires relation support
            End If
            If Not a = b Then          ' FN: requires relation support
            End If
        Else
            If b <> a Then             ' FN: requires relation support
            End If
            If b <> Not a Then         ' FN: requires relation support
            End If
            If Not b <> Not Not a Then ' FN: requires relation support
            End If

        End If

        If a <> b Then
            If b = a Then  ' FN: requires relation support
            End If
        Else
            If b <> a Then ' FN: requires relation support
            End If
        End If
    End Sub

    Private Shared Sub BackPropagation(ByVal a As Object, ByVal b As Object)
        If a Is b AndAlso b Is Nothing Then
            a.ToString()
        End If
    End Sub

    Public Sub RefEqualsImpliesValueEquals(ByVal a As Object, ByVal b As Object)
        If Object.ReferenceEquals(a, b) Then
            If Equals(a, b) Then ' FN
            End If
            If Equals(a, b) Then ' FN
            End If
            If a.Equals(b) Then  ' FN
            End If
        End If

        If Me Is a Then
            If Equals(a) Then  ' FN
            End If
            If Equals(a) Then  ' FN
            End If
        End If
    End Sub

    Public Sub ValueEqualsDoesNotImplyRefEquals(ByVal a As Object, ByVal b As Object)
        If Equals(a, b) Then ' 'a' could override Equals, so this is not a ref equality check
            If a Is b Then
            End If ' Compliant
        End If
    End Sub

    Public Sub ReferenceEqualsMethodCalls(ByVal a As Object, ByVal b As Object)
        If Object.ReferenceEquals(a, b) Then
            If a Is b Then
            End If ' FN
        End If

        If a Is b Then
            If Object.ReferenceEquals(a, b) Then
            End If ' FN
        End If
    End Sub

    Public Sub ReferenceEqualsMethodCallWithOpOverload(ByVal a As ConditionEvaluatesToConstant, ByVal b As ConditionEvaluatesToConstant)
        If Object.ReferenceEquals(a, b) Then
            If a Is b Then ' FN
            End If
        End If

        If a Is b Then
            If Object.ReferenceEquals(a, b) Then ' Compliant, == is doing a value comparison above. FIX
            End If
        End If
    End Sub

    Public Sub ReferenceEquals(ByVal a As Object, ByVal b As Object)
        If Object.ReferenceEquals(a, b) Then
        End If

        If Object.ReferenceEquals(a, a) Then
        End If ' FN

        a = Nothing
        If Object.ReferenceEquals(Nothing, a) Then            ' Noncompliant
        End If
        If Object.ReferenceEquals(a, a) Then                  ' Noncompliant
        End If

        If Object.ReferenceEquals(Nothing, New Object()) Then ' Noncompliant
        End If

        ' Because of boxing:
        Dim i = 10
        If Object.ReferenceEquals(i, i) Then ' FN
        End If

        Dim ii As Integer? = Nothing
        Dim jj As Integer? = Nothing
        If Object.ReferenceEquals(ii, jj) Then ' Noncompliant
        End If

        jj = 10
        If Object.ReferenceEquals(ii, jj) Then ' Noncompliant
        End If
    End Sub

    Public Sub ReferenceEqualsBool()
        Dim a, b As Boolean
        a = b = True
        If Object.ReferenceEquals(a, b) Then ' FN
        End If
        If Object.Equals(a, b) Then ' Noncompliant
        End If
    End Sub

    Public Overrides Function Equals(ByVal obj As Object) As Boolean
        Return MyBase.Equals(obj)
    End Function

    Public Shared Operator =(ByVal a As ConditionEvaluatesToConstant, ByVal b As ConditionEvaluatesToConstant) As Boolean
        Return False
    End Operator

    Public Shared Operator <>(ByVal a As ConditionEvaluatesToConstant, ByVal b As ConditionEvaluatesToConstant) As Boolean
        Return False
    End Operator

    Public Sub StringEmpty(ByVal s1 As String)
        Dim s As String = Nothing
        If String.IsNullOrEmpty(s) Then      ' Noncompliant
        End If
        If String.IsNullOrWhiteSpace(s) Then ' Noncompliant
        End If
        If Not Equals(String.IsInterned(s), Nothing) Then
        End If
        s = ""
        If String.IsNullOrEmpty(s) Then      ' FN
        End If

        If String.IsNullOrWhiteSpace(s) Then ' FN
        End If

        If String.IsNullOrEmpty(s1) Then      ' Compliant, we don't know anything about the argument
        End If

        If String.IsNullOrWhiteSpace(s1) Then ' Compliant
        End If

        If String.IsNullOrEmpty(s1) Then
            If String.IsNullOrEmpty(s1) Then  ' FN
            End If
        End If
    End Sub

    Public Sub Comparisons(ByVal i As Integer, ByVal j As Integer)
        If i < j Then
            If j < i Then  ' FN
            End If
            If j <= i Then ' FN
            End If
            If j = i Then  ' FN
            End If
            If j <> i Then ' FN
            End If
        End If
    End Sub

    Private Sub ValueEquals(ByVal i As Integer, ByVal j As Integer)
        If i = j Then
            If Equals(i, j) Then ' FN
            End If
            If i.Equals(j) Then  ' FN
            End If
        End If
    End Sub

    Private Sub DefaultExpression(ByVal o As Object)
        If Nothing Is Nothing Then     ' Noncompliant
        End If

        Dim nullableInt As Integer? = Nothing
        If nullableInt Is Nothing Then ' Noncompliant
        End If

        If Nothing Is Nothing Then     ' Noncompliant
        End If

        If Nothing IsNot Nothing Then  ' Noncompliant
        End If

        If Nothing IsNot Nothing Then  ' Noncompliant
        End If
    End Sub

    Private Sub Cast()
        Dim i = 5
        Dim o = CObj(i)
        If o Is Nothing Then ' Noncompliant
        End If

        Dim x = CType(o, ConditionEvaluatesToConstant) ' This would throw and invalid cast exception
        If x Is Nothing Then ' Noncompliant
        End If

    End Sub

    Public Async Function NotNullAfterAccess(ByVal o As Object, ByVal arr As Integer(,), ByVal coll As IEnumerable(Of Integer), ByVal task As Task) As Task
        Console.WriteLine(o.ToString())
        If o Is Nothing Then    ' Noncompliant
        End If


        Console.WriteLine(arr(42, 42))
        If arr Is Nothing Then  ' Noncompliant
        End If


        For Each item In coll
        Next
        If coll Is Nothing Then ' Noncompliant
        End If


        Await task
        If task Is Nothing Then ' FN
        End If

    End Function

    Public Sub EqualsTest32(ByVal o As Object)
        Dim o2 = o
        If o Is o2 Then                       ' FN
        End If
        If Object.ReferenceEquals(o, o2) Then ' FN
        End If
        If o.Equals(o2) Then                  ' FN
        End If
        If Equals(o2, o) Then                 ' FN
        End If


        Dim i = 1
        Dim j = i
        If i = j Then                         ' Noncompliant
        End If

        If i.Equals(j) Then  ' Noncompliant
        End If

        If Equals(i, j) Then ' Noncompliant
        End If
    End Sub

    Private Async Function Test_Await_Constraint(ByVal t As Task(Of String)) As Task
        If t IsNot Nothing Then
            Dim o = Await t
            If Equals(o, Nothing) Then ' Compliant, might be null
            End If
        End If
    End Function

    Public Sub Assert(ByVal condition As Boolean, ByVal o1 As Object)
        Debug.Assert(condition)

        If condition Then       ' Noncompliant
        End If

        Trace.Assert(condition) ' Compliant

        If o1 IsNot Nothing Then
            Debug.Assert(o1 Is Nothing, "Some message", "More details", 1, 2, 3) ' Compliant
        End If
    End Sub

    Public Sub Assert(ByVal o1 As Object)
        Debug.Assert(o1 IsNot Nothing)
        Debug.Assert(o1 Is Nothing)
    End Sub

    Private Sub ComparisonTransitivity(ByVal a As Integer, ByVal b As Integer, ByVal c As Integer)
        If a = b AndAlso b < c Then
            If a >= c Then ' FN
            End If

        End If
        If a = b AndAlso b <= c Then
            If a > c Then  ' FN
            End If

        End If
        If a > b AndAlso b > c Then
            If a <= c Then ' FN
            End If

        End If
        If a > b AndAlso b >= c Then
            If a <= c Then ' FN
            End If

        End If
        If a >= b AndAlso b >= c Then
            If a < c Then  ' FN
            End If

        End If
        If a >= b AndAlso c <= b Then
            If a < c Then  ' FN
            End If

        End If
        If a >= b AndAlso c >= b Then
            If a < c Then
            End If
        End If
    End Sub

    Friend Class Comp
        Public Shared Operator <(ByVal a As Comp, ByVal b As Comp) As Boolean
            Return True
        End Operator
        Public Shared Operator >(ByVal a As Comp, ByVal b As Comp) As Boolean
            Return True
        End Operator
        Public Shared Operator >=(ByVal a As Comp, ByVal b As Comp) As Boolean
            Return True
        End Operator
        Public Shared Operator <=(ByVal a As Comp, ByVal b As Comp) As Boolean
            Return True
        End Operator
    End Class

    Private Sub RefEqTransitivity(ByVal a As Comp, ByVal b As Comp, ByVal c As Comp)
        If a Is b AndAlso b Is c Then
            If a IsNot c Then       ' FN
            End If
        End If

        If a.Equals(b) AndAlso b Is c Then
            If a IsNot c Then
            End If
            If a Is c Then
            End If
            If a.Equals(c) Then     ' FN
            End If
            If Not a.Equals(c) Then ' FN
            End If
        End If

        If a > b AndAlso b Is c Then
            If a <= c Then          ' FN
            End If
        End If
    End Sub

    Private Sub ValueEqTransitivity(ByVal a As Comp, ByVal b As Comp, ByVal c As Comp)
        If a Is b AndAlso b.Equals(c) Then
            If a.Equals(c) Then     ' FN
            End If
        End If

        If a.Equals(b) AndAlso b.Equals(c) Then
            If a IsNot c Then
            End If
            If a Is c Then
            End If
            If a.Equals(c) Then     ' FN
            End If
            If Not a.Equals(c) Then ' FN
            End If
        End If

        If a > b AndAlso b.Equals(c) Then
            If a > c Then  ' FN
            End If
            If a <= c Then ' FN
            End If
        End If

        If Not a.Equals(b) AndAlso b.Equals(c) Then
            If a.Equals(c) Then ' FN
            End If
            If a Is c Then      ' FN
            End If
        End If

        If a IsNot b AndAlso b.Equals(c) Then
            If a.Equals(c) Then
            End If
            If a Is c Then
            End If
        End If
    End Sub

    Private Sub NeqEqTransitivity(ByVal a As Object, ByVal b As Object, ByVal c As Object)
        If a Is c AndAlso a IsNot b Then
            If b Is c Then ' FN
            End If

            If b.Equals(c) Then
            End If
        End If

        If a Is c AndAlso Not a.Equals(b) Then
            If b Is c Then      ' FN
            End If

            If b.Equals(c) Then ' FN
            End If

        End If
    End Sub

    Public Sub LiftedOperator()
        Dim i As Integer? = Nothing
        Dim j As Integer? = 5

        If i < j Then        ' Noncompliant
        End If

        If i <= j Then       ' Noncompliant
        End If

        If i > j Then        ' Noncompliant
        End If

        If i >= j Then       ' Noncompliant
        End If

        If i > 0 Then        ' Noncompliant
        End If

        If i >= 0 Then       ' Noncompliant
        End If

        If i < 0 Then        ' Noncompliant
        End If

        If i <= 0 Then       ' Noncompliant
        End If

        If j > Nothing Then  ' Noncompliant
        End If

        If j >= Nothing Then ' Noncompliant
        End If

        If j < Nothing Then  ' Noncompliant
        End If

        If j <= Nothing Then ' Noncompliant
        End If
    End Sub

    Friend Class Singleton
        Private Shared syncRoot As Object = New Object()

        Private Shared instanceField As Singleton

        Public Shared ReadOnly Property Instance As Singleton
            Get
                If instanceField Is Nothing Then
                    SyncLock syncRoot
                        If instanceField Is Nothing Then ' We don't raise in conditions in synclock blocks as it's raising many FPs.
                            instanceField = New Singleton()
                        End If
                    End SyncLock
                End If
                Return instanceField
            End Get
        End Property
    End Class

    Friend Structure MyStructWithNoOperator
        Public Shared Sub M(ByVal a As MyStructWithNoOperator)
            If a Is Nothing Then    ' Noncompliant, also a compiler error
                                    ' Error@-1 [BC30020]
            End If
        End Sub
    End Structure

    Public Class NullableCases
        Private Sub Case1()
            Dim b1 As Boolean? = True
            If b1 = True Then    ' Noncompliant
            End If
        End Sub

        Private Sub Case2(ByVal i As Boolean?)
            If i Is Nothing Then

            End If
            If i = True Then

            End If
            If i = False Then

            End If

            i = Nothing
            If i Is Nothing Then        ' Noncompliant

            End If
            If i = True Then            ' Noncompliant

            End If
            If i = False Then           ' Noncompliant

            End If

            i = True
            If i Is Nothing Then        ' Noncompliant

            End If
            If i = True Then            ' Noncompliant

            End If
            If i = False Then           ' Noncompliant

            End If

            i = False
            If i Is Nothing Then        ' Noncompliant

            End If
            If i = True Then            ' Noncompliant

            End If
            If i = False Then           ' Noncompliant

            End If

            Dim b2 As Boolean? = True
            If b2 = False Then          ' Noncompliant
            End If

            Dim b3 As Boolean? = True
            If b3 Is Nothing Then       ' Noncompliant
            End If

            Dim b4 As Boolean? = Nothing
            If b4 = True Then            ' Noncompliant
            End If

            Dim b5 As Boolean? = Nothing
            If b5 = False Then          ' Noncompliant
            End If


            Dim b6 As Boolean? = Nothing
            If b6 Is Nothing Then       ' Noncompliant
            End If
            Dim b7 As Boolean? = True
            If b7 = True Then           ' Noncompliant
            End If
            Dim b8 As Boolean? = False
            If b8 = False Then          ' Noncompliant
            End If

        End Sub

        Private Sub Case3(ByVal b As Boolean?)
            If b Is Nothing Then
                If Nothing Is b Then    ' Noncompliant
                    b.ToString()
                End If
            Else
                If b IsNot Nothing Then ' Noncompliant
                    b.ToString()
                End If
            End If
        End Sub

        Private Sub Case4(ByVal b As Boolean?)
            If b = True Then
                If True = b Then        ' Noncompliant
                    b.ToString()
                End If
            End If
        End Sub

        Private Sub Case5(ByVal b As Boolean?)
            If b = True Then
            ElseIf b = False Then
            Else
            End If
        End Sub

        Private Sub Case6(ByVal b As Boolean?)
            If b Is Nothing Then
            ElseIf b = True Then
            Else
            End If
        End Sub

        Private Sub Case7(ByVal b As Boolean?)
            If b Is Nothing Then
                If If(b, False) Then    ' Noncompliant

                End If
            End If
        End Sub

        Private Sub Case8(ByVal b As Boolean?)
            If b IsNot Nothing Then
                If b.HasValue Then      ' Noncompliant
                End If
            End If
        End Sub

        Private Sub Case9(ByVal b As Boolean?)
            If b = True Then
                Dim x = b.Value
                If x = True Then        ' Noncompliant
                End If
            End If
        End Sub

        Private Sub Case10(ByVal i As Integer?)
            If i Is Nothing Then
                If i.HasValue Then      ' Noncompliant
                End If
            End If
        End Sub

        Public Sub IfElseIfElseFlow_DirectValue(ByVal b As Boolean?)
            If b = True Then
                Console.WriteLine("true")
            ElseIf b = False Then
                Console.WriteLine("false")
            Else
                Console.WriteLine("null")
            End If
        End Sub

        Public Sub IfElseIfElseFlow_KnownNull()
            Dim b As Boolean? = Nothing
            If b = True Then                  ' Noncompliant
                Console.WriteLine("true")     ' Secondary
            ElseIf b = False Then             ' Noncompliant
                Console.WriteLine("false")    ' Secondary
            Else
                Console.WriteLine("null")
            End If
        End Sub
    End Class

    Public Class ConstantExpressionsAreExcluded
        Const T As Boolean = True
        Const F As Boolean = False

        Private Sub LocalConstants()
            Const t = True
            If t Then                                      ' Noncompliant
                Console.WriteLine()
            End If
            Const f = False
            If f Then                                      ' Noncompliant
                Console.WriteLine()                        ' Secondary
            End If
        End Sub

        Private Sub Constants()
            If T Then               ' Compliant it's a constant
                Console.WriteLine()
            End If
            If F Then               ' Compliant it's a constant
                Console.WriteLine()
            End If
        End Sub
        Private Sub WhileTrue()
            While T                 ' Compliant it's a constant
                Console.WriteLine()
            End While
        End Sub
        Private Sub WhileFalse()
            Do
                Console.WriteLine()
            Loop While F           ' Compliant it's a constant
        End Sub
        Private Sub Condition()
            Dim x = If(T, 1, 2)    ' Compliant, T is constant
        End Sub
    End Class

End Class

Public Class GuardedTests
    Public Sub Guarded(ByVal s1 As String)
        Guard1(s1)

        If Equals(s1, Nothing) Then  ' Noncompliant, always false
            ' this branch is never executed
        Else
        End If
    End Sub

    Public Sub Guard1(Of T As Class)(
        <ValidatedNotNull> ByVal value As T)
    End Sub

    <AttributeUsage(AttributeTargets.Parameter)>
    Public NotInheritable Class ValidatedNotNullAttribute
        Inherits Attribute
    End Class
End Class

Public Class CatchFinally
    Public Sub ObjectsShouldNotBeDisposedMoreThanOnce()
        Dim stream As Stream = Nothing
        Try
            stream = File.Open("file", FileMode.Open)
            Using reader = New StreamReader(stream)
                ' read the file here

                ' StreamReader will dispose the stream automatically; set stream to null
                ' to prevent it from disposing twice (the recommended pattern for S3966)
                stream = Nothing
            End Using

        Finally
            If stream IsNot Nothing Then
                stream.Dispose()
            End If
        End Try
    End Sub

    Public Sub FalseNegatives()
        Dim o As Object = Nothing
        Try
            Console.WriteLine("Could throw")
        Catch
            If o IsNot Nothing Then ' Noncompliant
            End If
            If o Is Nothing Then    ' Noncompliant
            End If

        Finally
            If o IsNot Nothing Then ' Noncompliant
            End If
            If o Is Nothing Then    ' Noncompliant
            End If
        End Try
    End Sub
End Class

Friend Class UsingStatement
    Public Sub Method()
        Dim isTrue = True
        If isTrue Then     ' Noncompliant
        End If
        Using reader = New StreamReader("")
            If isTrue Then ' Noncompliant
            End If
        End Using
        If isTrue Then     ' Noncompliant
        End If
    End Sub
End Class

Friend Class AsyncAwait
    Private _foo1 As Object
    Private Async Function Foo(ByVal t As Task) As Task
        Dim o As Object = Nothing
        _foo1 = o
        Await t ' awaiting clears the constraints
        If _foo1 IsNot Nothing Then ' FN
        End If
        If _foo1 Is Nothing Then    ' FN
        End If
        If o IsNot Nothing Then     ' Noncompliant
        End If
        If o Is Nothing Then        ' Noncompliant
        End If
    End Function
End Class

Public Class StaticMethods
    Private _foo1 As Object
    ' https://github.com/SonarSource/sonar-dotnet/issues/947
    Private Sub CallToMonitorWaitShouldResetFieldConstraints()
        Dim o As Object = Nothing
        _foo1 = o
        Threading.Monitor.Wait(o) ' This is a multi-threaded application, the fields could change
        If _foo1 IsNot Nothing Then  ' Noncompliant FP
        End If
        If _foo1 Is Nothing Then     ' Noncompliant FP
        End If
        If o IsNot Nothing Then      ' Noncompliant
        End If
        If o Is Nothing Then         ' Noncompliant
        End If
    End Sub

    Private Sub CallToStaticMethodsShouldResetFieldConstraints()
        Dim o As Object = Nothing
        _foo1 = o
        Console.WriteLine() ' This particular method has no side effects
        If _foo1 IsNot Nothing Then  ' Noncompliant
        End If
        If _foo1 Is Nothing Then    ' Noncompliant
        End If
        If o IsNot Nothing Then     ' Noncompliant
        End If
        If o Is Nothing Then        ' Noncompliant
        End If
    End Sub
End Class


Public Class TestNullCoalescing
    Public Sub CompliantMethod(ByVal input As Boolean?)
        If If(input, False) Then                           ' Compliant
            Console.WriteLine("input is true")
        Else
            Console.WriteLine("input is false")
        End If
    End Sub

    Public Sub CompliantMethod1(ByVal input As Boolean?)
        While If(input, False)                             ' Compliant
            Console.WriteLine("input is true")
        End While
    End Sub

    Public Sub CompliantMethod2(ByVal input As Boolean?, ByVal input1 As Boolean)
        While If(input, False) AndAlso input1             ' Compliant
            Console.WriteLine("input is true")
        End While
    End Sub

    Public Sub CompliantMethod3(ByVal input As Boolean?, ByVal input1 As Boolean)

        If If(If(input, False), input1, False) Then      ' Compliant
            Console.WriteLine("input is true")
        End If
    End Sub

    Public Sub NonCompliantMethod()
        Dim input As Boolean? = True
        If If(input, False) Then                         ' Noncompliant
            Console.WriteLine("input is true")
        Else
            Console.WriteLine("input is false")          ' Secondary
        End If
    End Sub

    Public Sub NonCompliantMethod1()
        Dim input As Boolean? = True
        While If(input, False)                          ' Noncompliant
            Console.WriteLine("input is true")
        End While
    End Sub

    Public Sub NonCompliantMethod2(ByVal input As Boolean?)
        While If(input, False) OrElse True              ' Compliant
            Console.WriteLine("input is true")
        End While
    End Sub

    Public Sub NonCompliantMethod3(ByVal input As Boolean?, ByVal input1 As Boolean)
        If If(If(input, False), False, False) Then   ' Compliant
            Console.WriteLine("input is true")
        End If
    End Sub
End Class

Friend Class Program
    Public Shared Function CompliantMethod4(ByVal parameter As String) As String
        Dim useParameter = If(parameter, "non-empty")
        If Equals(useParameter, Nothing) OrElse Equals(useParameter, "") Then Return "non-empty" ' Noncompliant
        ' we don't know if this going to be excuted or not

        Return "empty"
    End Function

    Public Shared Function Method1347(ByVal parameter As String) As String
        Dim useParameter = If(parameter, "non-empty")
        If Not String.IsNullOrEmpty(useParameter) Then
            Return "non-empty"
        Else
        End If
        Return "empty"
    End Function
End Class

Public Class RefArgTest
    Public Sub Method(ByRef s As String, ByVal x As Integer)
    End Sub
    Public Sub Method1(ByVal infixes As String)
        If Not Equals(infixes, Nothing) Then
            Method(infixes, infixes.Length)
            If Equals(infixes, Nothing) Then        ' Noncompliant FP: ref
                Return
            End If
        End If
    End Sub

    Public Sub Method2(ByVal infixes As String)
        If Not Equals(infixes, Nothing) Then
            Method(infixes, infixes.Length)
            If Not Equals(infixes, Nothing) Then    ' Noncompliant FP: ref
                Return
            End If
        End If
    End Sub

    Public Sub Method3(ByVal infixes As String)
        If Equals(infixes, Nothing) Then
            Method(infixes, infixes.Length)
            If Equals(infixes, Nothing) Then        ' Noncompliant FP: ref
                Return
            End If
        End If
    End Sub

    Public Sub Method4(ByVal infixes As String)
        If Equals(infixes, Nothing) Then
            Method(infixes, infixes.Length)
            If Not Equals(infixes, Nothing) Then    ' Noncompliant FP: ref
                Return
            End If
        End If
    End Sub

End Class

Public Class StringComparision
    Public Sub Method(ByVal parameterString As String)
        Dim emptyString1 = ""
        Dim emptyString2 = ""
        Dim nullString1 As String = Nothing
        Dim nullString2 As String = Nothing
        Dim fullStringa1 = "a"
        Dim fullStringa2 = "a"
        Dim fullStringb = "b"
        Dim whiteSpaceString1 = " "
        Dim whiteSpaceString2 = " "
        Dim doubleWhiteSpaceString1 = "  "
        Dim doubleWhiteSpaceString2 = "   "

        If Equals(emptyString1, emptyString2) Then                       ' FN

        End If
        If Equals(nullString1, nullString2) Then                         ' Noncompliant

        End If

        If Equals(fullStringa1, fullStringa2) Then

        End If

        If Equals(fullStringa1, fullStringb) Then

        End If

        If Equals(parameterString, emptyString1) Then

        End If

        If Equals(parameterString, nullString1) Then

        End If

        If Equals(emptyString1, nullString1) Then                        ' Noncompliant

        End If

        If Equals(emptyString1, fullStringa1) Then                       ' FN

        End If

        If Equals(nullString1, fullStringa1) Then                        ' Noncompliant

        End If

        If Equals(fullStringa1, "") Then                                 ' FN

        End If

        If Equals(fullStringa1, Nothing) Then                            ' Noncompliant

        End If

        If Equals(whiteSpaceString1, whiteSpaceString2) Then             ' FN

        End If

        If Equals(whiteSpaceString1, " ") Then                           ' FN

        End If

        If Equals(whiteSpaceString1, emptyString1) Then                  ' FN

        End If

        If Equals(whiteSpaceString1, nullString1) Then                   ' Noncompliant

        End If

        If Equals(whiteSpaceString1, fullStringa1) Then                  ' FN

        End If

        If Equals(whiteSpaceString1, parameterString) Then

        End If

        If Equals(doubleWhiteSpaceString1, doubleWhiteSpaceString2) Then ' FN

        End If

        If Equals(doubleWhiteSpaceString1, emptyString1) Then            ' FN

        End If

        If Equals(doubleWhiteSpaceString1, nullString1) Then             ' Noncompliant

        End If

        If Equals(doubleWhiteSpaceString1, fullStringa1) Then            ' FN

        End If

        If Equals(doubleWhiteSpaceString1, parameterString) Then

        End If

    End Sub
End Class

Public Class NullOrEmpty
    Public Sub Method1(ByVal s As String)
        Dim s1 As String = Nothing
        Dim s2 = ""
        Dim s3 = "a"
        If String.IsNullOrEmpty(s1) Then ' Noncompliant
        End If
        If String.IsNullOrEmpty(s2) Then ' FN
        End If

        If String.IsNullOrEmpty(s) Then
        End If

        If String.IsNullOrEmpty(s3) Then ' FN
        End If
    End Sub

    Public Sub Method2(ByVal s As String)

        If String.IsNullOrEmpty(s) Then
        End If

        s = ""
        If String.IsNullOrEmpty(s) Then ' FN
        End If

        s = Nothing
        If String.IsNullOrEmpty(s) Then ' Noncompliant
        End If

        s = "a"
        If String.IsNullOrEmpty(s) Then ' FN
        End If
    End Sub

    Public Sub Method3(ByVal s1 As String)
        If String.IsNullOrEmpty(s1) Then
            s1.ToString()
        Else
            If Equals(s1, Nothing) Then     ' Noncompliant
                s1.ToString()               ' Secondary
            End If
        End If

    End Sub

    Public Sub Method4(ByVal s1 As String)
        If Not String.IsNullOrEmpty(s1) Then
            If Equals(s1, Nothing) Then     ' Noncompliant
                s1.ToString()               ' Secondary
            End If
        Else
            s1.ToString()
        End If

    End Sub

    Public Sub Method5(ByVal s1 As String)
        If Not String.IsNullOrEmpty(s1) Then
            If Equals(s1, Nothing) Then     ' Noncompliant
                s1.ToString()               ' Secondary
            End If
        Else
            s1.ToString()
        End If

    End Sub

    Public Sub Method6(ByVal s1 As String)
        If String.IsNullOrEmpty(s1) OrElse Equals(s1, "a") Then
            s1.ToString()
        Else
            If Equals(s1, Nothing) Then     ' Noncompliant
                s1.ToString()               ' Secondary
            End If
        End If

    End Sub

    Public Sub Method7(ByVal s1 As String)
        If String.IsNullOrEmpty(s1) AndAlso Equals(s1, "a") Then ' FN
            s1.ToString()
        Else
            If Equals(s1, Nothing) Then
                s1.ToString()
            End If
        End If

    End Sub


    Public Function Method8(ByVal message As String) As String
        If Equals(message, Nothing) Then
            Throw New ArgumentNullException($"{NameOf(message)} shouldn't be null!")
        End If

        If String.IsNullOrEmpty(message) Then
            Throw New ArgumentNullException($"{NameOf(message)} shouldn't be empty!")
        End If

        Return String.Empty
    End Function

    Private Sub BinaryConditional_Useless(ByVal a As String, ByVal b As String, ByVal c As String, ByVal d As String)
        Dim isNothing As String = Nothing
        Dim isNotNothing = ""
        Dim notEmpty = "value"
        Dim ret As String

        ret = If(b, a)
        ret = If(b, isNotNothing)
        ret = If(c, notEmpty)
        ret = If(d, "N/A")

        'Left operand: Values notNull and notEmpty are known to be not Nothing
        ret = If(isNotNothing, a)                       ' Noncompliant
                                                        ' Secondary@-1
        ret = If(isNotNothing, a)                       ' Noncompliant
                                                        ' Secondary@-1
        ret = "Lorem " & If(isNotNothing, a) & " ipsum" ' Noncompliant
                                                        ' Secondary@-1
        ret = If(isNotNothing, "N/A")                   ' Noncompliant
                                                        ' Secondary@-1
        ret = If(notEmpty, "N/A")                       ' Noncompliant
                                                        ' Secondary@-1

        'Left operand: isNull is known to be null
        ret = If(Nothing, a)                            ' Noncompliant
        ret = If(isNothing, a)                          ' Noncompliant
        ret = "Lorem " & If(isNothing, a) & " ipsum"    ' Noncompliant

        'Right operand: isNull is known to be null, therefore binary conditional expression is not needed
        ret = If(a, Nothing)                            ' FN: NOOP
        ret = If(a, isNothing)                          ' FN: NOOP
        '           ~~~~~~~~~

        'Combo/Fatality
        ret = If(isNotNothing, isNothing)
        '        ^^^^^^^^^^^^                             Noncompliant {{Remove this unnecessary check for Nothing. Some code paths are unreachable.}}
        '                      ^^^^^^^^^                  Secondary@-1
        ret = If(isNothing, Nothing)                    ' Noncompliant {{Remove this unnecessary check for Nothing.}}
        '        ^^^^^^^^^
        ret = If("Value", a)
        '        ^^^^^^^                                Noncompliant {{Remove this unnecessary check for Nothing. Some code paths are unreachable.}}
        '                 ^                             Secondary@-1
    End Sub

    Private Function CoalesceCount(Of T)(ByVal arg As IList(Of T)) As Integer
        arg = If(arg, New List(Of T)())
        Return arg.Count
    End Function

    Public Class CoalesceProperty
        Private messageField As Object

        Public ReadOnly Property Message As Object
            Get
                Return If(messageField Is Nothing, New Object(), messageField)
            End Get
        End Property

    End Class
End Class

Public Class NullOrWhiteSpace
    Public Sub Method1(ByVal s As String)
        Dim s1 As String = Nothing
        Dim s2 = ""
        Dim s3 = If(s, "")
        Dim s4 = " "

        If String.IsNullOrWhiteSpace(s1) Then       ' Noncompliant
        End If


        If String.IsNullOrWhiteSpace(s2) Then       ' FN
            If Equals(s2, "a") Then                 ' FN

            End If
        End If

        If String.IsNullOrWhiteSpace(s3) Then

        End If

        If String.IsNullOrWhiteSpace(s4) Then       ' FN

        End If

        If Not String.IsNullOrWhiteSpace(s4) Then   ' FN

        End If

        If Not String.IsNullOrWhiteSpace(s) Then
            If Equals(s, "") Then                   ' FN

            End If

            If Equals(s, " ") Then                  ' FN

            End If
        End If

    End Sub
End Class

' Reproducer for https://github.com/SonarSource/sonar-dotnet/issues/8368
Public Class Repro_8368
    Public Sub Method()
        Dim lastException As Exception = Nothing

        Try
            DoSomeWork()
            Return
        Catch ex As Exception
            lastException = ex
        End Try

        If lastException IsNot Nothing Then ' Noncompliant - FP
            LogError(lastException)
        End If
    End Sub

    Private Sub DoSomeWork()
    End Sub

    Private Sub LogError(ByVal exception As Exception)
    End Sub
End Class
