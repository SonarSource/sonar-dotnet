Imports System
Imports System.Collections.Generic
Imports System.Runtime.InteropServices
Imports MaybeInt = System.Nullable(Of System.Int32)

Class Basics
    Sub NullAssignment()
        Dim i As Integer? = Nothing
        If i.HasValue Then
            Console.WriteLine(i.Value)
        End If
        Console.WriteLine(i.Value)  ' Noncompliant {{'i' is Nothing on at least one execution path.}}
        '                 ^
    End Sub

    Sub DereferenceOnValueResult(i As Integer?)
        Dim x = i.Value.ToString()  ' Compliant, unknown
        i = Nothing
        x = i.Value.ToString()      ' Noncompliant
    End Sub

    Sub NonEmpty()
        Dim i As Integer? = 42
        If i.HasValue Then
            Console.WriteLine(i.Value)
        End If
        Console.WriteLine(i.Value)  ' Compliant
    End Sub

    Sub EmptyConstructor()
        Dim i As Integer? = New Nullable(Of Integer)()
        If i.HasValue Then
            Console.WriteLine(i.Value)
        End If
        Console.WriteLine(i.Value)  ' Noncompliant
    End Sub

    Sub NonEmptyConstructor()
        Dim i As Integer? = New Nullable(Of Integer)(42)
        If i.HasValue Then
            Console.WriteLine(i.Value)
        End If
        Console.WriteLine(i.Value)
    End Sub

    Sub Assignment1(i1 As Integer?)
        Dim i2, i3 As Integer?
        i2 = Nothing
        If i1 = i2 Then
            Dim x = i1.Value    ' Noncompliant
        End If

        i1 = 42
        If i1 <> i2 Then        ' Compliant
            Dim x = i1.Value
        End If

        If i1 <> i2 Then
            Dim x = i2.Value    ' Noncompliant
        End If

        i3 = Nothing
        i2 = i3
        If i1 <> i2 Then
            Dim x = i2.Value    ' Noncompliant
        End If

        Dim b1 As Boolean? = Nothing
        If b1 Then              ' Compliant
        End If

        If Not b1 Then          ' Compliant
        End If
    End Sub

    Sub AssignmentAndNullComparison(o As Object)
        Dim b = CTypeDynamic(Of Boolean?)(o)
        If b IsNot Nothing Then
            Dim x = b.Value     ' Compliant
        End If
    End Sub

    Sub AssignmentTransitivity()
        Dim b1 As Boolean? = Nothing
        Dim b2 As Boolean? = b1
        Dim x = b2.Value        ' Noncompliant
    End Sub

    Sub TwoWaySwappingViaTemporaryVar()
        Dim b1 As Boolean? = Nothing
        Dim b2 As Boolean? = True
        Dim t As Boolean? = b1
        b1 = b2
        b2 = t
        Dim x = b1.Value    ' Compliant
        x = b2.Value        ' Noncompliant
    End Sub

    Function IfOperator(param As Integer?) As Integer
        param = If(param, 42)
        Return param.Value      ' Compliant
    End Function
End Class

Class TestLoopWithBreak
    Public Shared Sub LoopWithBreak(list As System.Collections.Generic.IEnumerable(Of String), condition As Boolean)
        Dim i1 As Integer? = Nothing
        For Each x As String In list
            Try
                If condition Then
                    Console.WriteLine(i1.Value) ' Noncompliant
                End If
                Exit For
            Catch e As Exception
                Continue For
            End Try
        Next
    End Sub
End Class

Class NullableOfCustomTypes
    Sub Assignment(nullable As AStruct?)
        Dim x = nullable.Value                                              ' Compliant, unknown
        x = CType(nullable, AStruct)                                        ' Compliant
        x = New AStruct?(nullable.Value).Value                              ' Compliant

        nullable = Nothing
        x = nullable.Value                                                  ' Noncompliant
        nullable = New AStruct()
        x = nullable.Value                                                  ' Compliant
        nullable = New AStruct?(New AStruct())
        x = nullable.Value                                                  ' Compliant

        nullable = New AStruct?()
        x = nullable.Value                                                  ' Noncompliant
        nullable = New Nullable(Of AStruct)()
        x = nullable.Value                                                  ' Noncompliant

        x = (New Nullable(Of AStruct)()).Value                              ' FN
        x = (CTypeDynamic(Of AStruct?)(Nothing)).Value                      ' FN

        nullable = Nothing
        x = (CType(nullable, AStruct?)).Value                               ' FN
        x = (CTypeDynamic(Of AStruct?)(nullable)).Value                     ' Compliant, when reached .Value above implies nullable is not null
        x = (CType((CTypeDynamic(Of AStruct?)(nullable)), AStruct?)).Value  ' Compliant, same as above
    End Sub

    Sub ForeachCast()
        For Each x As AStruct In New AStruct?() {Nothing}                   ' FN
        Next

        For Each x As AStruct In New AStruct?() {New AStruct()}             ' Compliant
        Next

        For Each x As AStruct In New AStruct?() {New AStruct(), Nothing}    ' FN
        Next

        For Each x As AStruct? In New AStruct?() {New AStruct(), Nothing}   ' Compliant
        Next

        For Each x In New AStruct?() {New AStruct(), Nothing}               ' Compliant
        Next
    End Sub

    Structure AStruct
    End Structure
End Class

Class Arithmetic
    Function MultiplicationByZeroAndAssignment(i As Integer?) As Integer?
        i = Nothing
        Return 0 * i * i.Value  ' Noncompliant
    End Function
End Class

Class ComplexConditionsSingleNullable
    Function LogicalAndLearningNonNull1(b As Boolean?) As Boolean
        Return b.HasValue AndAlso b.Value
    End Function

    Function LogicalAndLearningNonNull2(b As Boolean?) As Boolean
        Return b.HasValue = True AndAlso b.Value
    End Function

    Function LogicalAndLearningNonNull3(b As Boolean?) As Boolean
        Return b.HasValue <> False AndAlso b.Value
    End Function

    Function LogicalAndLearningNonNull4(b As Boolean?) As Boolean
        Return b.HasValue = Not False AndAlso b.Value
    End Function

    Function LogicalAndLearningNonNull5(b As Boolean?) As Boolean
        Return Not Not b.HasValue AndAlso b.Value
    End Function

    Function LogicalAndLearningNonNull6(b As Boolean?) As Boolean
        Return b.Value AndAlso b.HasValue
    End Function

    Function LogicalAndLeaningNull1(b As Boolean?) As Boolean
        Return Not b.HasValue AndAlso b.Value                       ' Noncompliant
    End Function

    Function LogicalAndLeaningNull2(b As Boolean?) As Boolean
        Return b.HasValue = False AndAlso b.Value                   ' Noncompliant
    End Function

    Function LogicalAndLeaningNull3(b As Boolean?) As Boolean
        Return b.HasValue = Not True AndAlso b.Value                ' Noncompliant
    End Function

    Function LogicalAndLeaningNull4(b As Boolean?) As Boolean
        Return Not Not Not b.HasValue AndAlso b.Value               ' Noncompliant
    End Function

    Function BitAndLearningNull(b As Boolean?) As Boolean
        Return Not b.HasValue And b.Value                           ' Noncompliant
    End Function

    Function BitAndLearningNonNull(b As Boolean?) As Boolean
        Return b.HasValue And b.Value                               ' Noncompliant
    End Function

    Function LogicalOrLearningNonNull(b As Boolean?) As Boolean
        Return b.HasValue OrElse b.Value                            ' Noncompliant
    End Function

    Function BitOrLearningNonNull(b As Boolean?) As Boolean
        Return b.HasValue Or b.Value                                ' Noncompliant
    End Function

    Function Tautology(b As Boolean?) As Boolean
        Return (Not b.HasValue OrElse b.HasValue) AndAlso b.Value   ' Noncompliant
    End Function

    Function ShortCircuitedOr(b As Boolean?) As Boolean
        Return (True OrElse b.HasValue) AndAlso b.Value             ' Compliant
    End Function

    Function ShortCircuitedAnd(b As Boolean?) As Boolean
        Return (False AndAlso b.HasValue) OrElse b.Value            ' Compliant
    End Function

    Function XorWithTrue(b As Boolean?) As Boolean
        Return (True Xor b.HasValue) AndAlso b.Value                ' Noncompliant
    End Function

    Function XorWithFalse(b As Boolean?) As Boolean
        Return (False Xor b.HasValue) AndAlso b.Value               ' Compliant
    End Function

    Sub ReachabilityIsTakenIntoAccount()
        Dim b As Boolean? = Nothing
        Dim x = True OrElse b.Value ' Compliant, short-circuited
        x = b.Value                 ' Noncompliant
        x = b.Value                 ' Compliant, when reached previous b.Value implies b is not null
    End Sub

    Sub ReachabilityStateIsPreserved()
        Dim b As Boolean? = Nothing
        Dim x = True Or b.Value     ' Noncompliant, always evaluating both sides
        x = b.Value                 ' Compliant, when reached previous b.Value implies b is not null
    End Sub

    Function LiftedNot(b As Boolean?) As Boolean
        Return Not b = b AndAlso b.Value    ' FN, b.Value reached only when b is empty
    End Function
End Class

Class ComplexConditionMultipleNullables
    Function IndependentConditions1(d As Double?, f As Single?) As Boolean
        Return f Is Nothing AndAlso d.Value = 42.0
    End Function

    Function IndependentConditions2(d As Double?, f As Single?) As Boolean
        Return f IsNot Nothing AndAlso d.Value = 42.0
    End Function

    Function IndependentConditions3(d As Double?, f As Single?) As Boolean
        Return f.HasValue AndAlso d.Value = 42.0
    End Function

    Function IndependentConditions4(d As Double?, f As Single?) As Boolean
        Return f Is Nothing AndAlso d.Value = 42.0
    End Function

    Function IndependentConditions5(d As Double?, f As Single?) As Boolean
        Return f Is Nothing AndAlso d.Value = 42.0
    End Function

    Function DependentConditions1(d As Double?, f As Single?) As Boolean
        Return Not d.HasValue AndAlso d.Value = 42.0                                    ' Noncompliant
    End Function

    Function DependentConditions2(d As Double?, f As Single?) As Boolean
        Return d.Value = 42.0 AndAlso d.HasValue                                        ' Compliant
    End Function

    Function DependentConditions3(d As Double?, f As Single?) As Boolean
        Return d.Value = 42.0 AndAlso d.HasValue = f.HasValue AndAlso f.Value = 42.0    ' Compliant
    End Function

    Function DependentConditions4(d As Double?, f As Single?) As Boolean
        Return d.Value = 42.0 AndAlso d.HasValue <> f.HasValue AndAlso f.Value = 42.0   ' Noncompliant
    End Function

    Sub ThirdExcluded(b1 As Boolean?, b2 As Boolean?)
        If b1 = b2 AndAlso b1 <> b2 Then
            Dim x = (CTypeDynamic(Of Integer?)(Nothing)).Value                          ' Compliant, logically unreachable
        End If
    End Sub

    Sub Transitivity(b1 As Boolean?, b2 As Boolean?, b3 As Boolean?)
        If b1 = b2 AndAlso b1 <> b3 AndAlso b2 = b3 Then
            Dim x = (CTypeDynamic(Of Integer?)(Nothing)).Value                          ' Compliant, logically unreachable
        End If

        If b1 <> b2 AndAlso b1 <> b3 AndAlso b2 <> b3 AndAlso b1 IsNot Nothing AndAlso b2 IsNot Nothing Then
            Dim x = b3.Value                                                            ' FN: b3 is empty
        End If
    End Sub

    Sub RelationsEquality(b1 As Boolean?)
        Dim b2 As Boolean? = Nothing
        If b1 = b2 Then
            Dim x = b1.Value            ' Noncompliant
            x = b2.Value                ' Noncompliant, FP: when reached previous b1.Value should imply b1 is not null, hence b2
        End If
    End Sub

    Sub RelationsBitOr(b1 As Boolean?, b2 As Boolean?)
        Dim x = b1.Value Or b2.Value
        x = b1.Value                    ' Compliant, both sides are evaluated
        x = b2.Value                    ' Compliant, both sides are evaluated
    End Sub
End Class

Class IfOperator
    Function Truth(b As Boolean?) As Boolean
        Return If(True, b.Value, False)                             ' Compliant
    End Function
    Function Tautology(b As Boolean?) As Boolean
        Return If(b.HasValue OrElse Not b.HasValue, b.Value, False) ' Noncompliant
    End Function
    Function Falsity(b As Boolean?) As Boolean
        Return If(False, b.Value, b.Value)                          ' Compliant
    End Function
    Function HasValue1(b As Boolean?) As Boolean
        Return If(b.HasValue, b.Value, False)                       ' Compliant
    End Function
    Function HasValue2(b As Boolean?) As Boolean
        Return If(b.HasValue, True, b.Value)                        ' Noncompliant
    End Function
    Function TruthAndAssignment(b As Boolean?) As Boolean
        b = Nothing
        Return If(True OrElse b Is Nothing, b.Value, False)         ' Noncompliant
    End Function
End Class

Class WithKeyword
    Function WithKeyword(nullable As Integer?)
        If nullable.HasValue Then
            Console.WriteLine(nullable.Value)
        End If
        With nullable   ' Noncompliant
            Dim x = .Value
        End With
    End Function
End Class

Class Linq
    Private Numbers As IEnumerable(Of TestClass) =
    {
        New TestClass() With {.Number = 42},
        New TestClass(),
        New TestClass() With {.Number = 1},
        New TestClass() With {.Number = Nothing}
    }

    ReadOnly Property EnumerableOfEmptyNullableValues1 As IEnumerable(Of Integer)
        Get
            Return Numbers.Where(Function(x) Not x.Number.HasValue).Select(Function(x) x.Number.Value)          ' FN
        End Get
    End Property

    ReadOnly Property EnumerableOfEmptyNullableValues2 As IEnumerable(Of Integer)
        Get
            Return Numbers.Select(Function(x) CTypeDynamic(Of Integer?)(Nothing)).Select(Function(x) x.Value)   ' FN
        End Get
    End Property

    Class TestClass
        Public Property Number As Integer?
    End Class
End Class

Class MemberAccessSequence
    Sub Basics(dt As DateTime?)
        Dim x = dt.Value.ToString() ' Compliant
        dt = Nothing
        x = dt.Value.ToString()     ' Noncompliant
        x = dt.Value.ToString()     ' Compliant, when reached previous dt.Value implies dt is empty
    End Sub
End Class

' https//github.com/SonarSource/sonar-dotnet/issues/4573
Class Repro_4573
    Private foo As DateTime?

    Overridable Property Bar As DateTime?
        Get
            Return foo
        End Get
        Set(value As DateTime?)
            If value.HasValue Then
                ' HasValue and NoValue constraints are set here
            End If
            If foo <> value OrElse foo.HasValue Then
                foo = value
            End If
        End Set
    End Property

    Sub Sequence(value As DateTime?)
        If value.HasValue Then
            ' HasValue and NoValue constraints are set here
        End If
        If foo = value Then ' Relationship is added here
            If foo.HasValue Then
                Console.WriteLine(foo.Value.ToString())
            End If
            If Not foo.HasValue Then
                Console.WriteLine(foo.Value.ToString()) ' Noncompliant
            End If
            If foo Is Nothing Then
                Console.WriteLine(foo.Value.ToString()) ' Compliant, when reached, foo is not empty
            End If
            If foo IsNot Nothing Then
                Console.WriteLine(foo.Value.ToString())
            End If
        End If
    End Sub

    Sub NestedIsolated1(value As DateTime?)
        If foo = value Then                                 ' Relationship is added here
            If value.HasValue Then
                If Not foo.HasValue Then
                    Console.WriteLine(foo.Value.ToString()) ' Noncompliant, FP: when reached, foo is not empty
                End If
            End If
        End If
    End Sub

    Sub NestedIsolated2(value As DateTime?)
        If foo = value Then                                 ' Relationship is added here
            If Not value.HasValue Then
                If foo Is Nothing Then
                    Console.WriteLine(foo.Value.ToString()) ' Noncompliant
                End If
            End If
        End If
    End Sub

    Sub NestedConditionsWithHasValue(value As DateTime?)
        If foo = value Then                                 ' Relationship should be created here
            If value.HasValue Then
                If foo.HasValue Then
                    Console.WriteLine(foo.Value.ToString()) ' Compliant, non-empty
                End If
                If Not foo.HasValue Then
                    Console.WriteLine(foo.Value.ToString()) ' Noncompliant, FP: logically unreachable
                End If
            Else
                If foo.HasValue Then
                    Console.WriteLine(foo.Value.ToString()) ' Compliant, logically unreachable
                End If
                If Not foo.HasValue Then
                    Console.WriteLine(foo.Value.ToString()) ' Noncompliant
                End If
            End If
        End If
    End Sub

    Sub NestedConditionsUsingNullComparisons(ByVal value As DateTime?)
        If foo = value Then                                 ' Relationship should be created here
            If value.HasValue Then
                If foo Is Nothing Then
                    Console.WriteLine(foo.Value.ToString()) ' Noncompliant, FP: logically unreachable
                End If
                If foo IsNot Nothing Then
                    Console.WriteLine(foo.Value.ToString()) ' Compliant, non-empty
                End If
            Else
                If foo IsNot Nothing Then
                    Console.WriteLine(foo.Value.ToString()) ' Compliant, logically unreachable
                End If
                If foo Is Nothing Then
                    Console.WriteLine(foo.Value.ToString()) ' Noncompliant
                End If
            End If
        End If
    End Sub
End Class

Class Assignments
    Sub Assignment(nullable As Integer?)
        Dim x As Integer = nullable     ' Compliant
        nullable = Nothing
        Dim y As Integer = nullable     ' Noncompliant
    End Sub
    Sub Assignment2(nullable As Integer?)
        If nullable Is Nothing Then
            Dim x As Integer = nullable     ' Noncompliant
        End If
    End Sub
End Class

Class Casts
    Sub CastWithCType()
        Dim i As Integer? = Nothing
        Dim x = CType(i, Integer)   ' Noncompliant
    End Sub

    Sub CastWithPredefinedCastExpression()
        Dim i As Integer? = Nothing
        Dim x = CType(i, Integer)   ' Noncompliant
    End Sub

    Sub DowncastWithReassignment(i As Integer?)
        Dim x = CInt(i)
        i = Nothing
        x = CInt(i) ' Noncompliant
        i = 42
        x = CInt(i)
    End Sub

    Sub DowncastAfterLearnedNotNullViaLiteral(i As Integer?)
        i = 42
        Dim x = CInt(i)
    End Sub

    Sub DowncastAfterLearnedNotNullViaValue()
        Dim i As Integer? = Nothing
        Dim x = i.Value ' Noncompliant
        x = CInt(i)     ' Compliant, when reached i.Value above implies i is not null
    End Sub

    Sub UpcastWithNull()
        Dim x = (CType(Nothing, Integer?)).Value    ' FN
    End Sub

    Sub UpcastWithReassignment(i As Integer?)
        Dim x = (CType(Nothing, Integer?)).Value    ' FN
        i = Nothing
        x = (CType(i, Integer?)).Value              ' FN
    End Sub

    Sub UpcastWithNonNullLiteral(i As Integer?)
        Dim x = (CType(42, Integer?)).Value
    End Sub

    Sub CTypeDynamicAndUnreachability(i As Integer?)
        Dim x = (CTypeDynamic(Of Integer?)(Nothing)).Value  ' FN
        i = Nothing
        x = (CTypeDynamic(Of Integer?)(i)).Value            ' FN
    End Sub

    Sub CTypeDynamicWithUnknownAndReassignment(i As Integer?)
        Dim x = (CTypeDynamic(Of Integer?)(i)).Value   ' Compliant
        i = Nothing
        x = (CTypeDynamic(Of Integer?)(i)).Value       ' FN
    End Sub

    Sub CTypeDynamicWithNonNullLiteral(i As Integer?)
        Dim x = (CTypeDynamic(Of Integer?)(42)).Value
    End Sub

    Function ReturnStatement(nullable As Integer?) As Integer
        If nullable Is Nothing Then
            Return nullable ' Noncompliant
        End If
    End Function

    Function ReturnAssignment(nullable As Integer?) As Integer
        If nullable Is Nothing Then
            ReturnAssignment = nullable   ' Noncompliant
        End If
    End Function

    Sub Method(nullable As Integer?)
        If nullable Is Nothing Then
            Method2(nullable)   ' Noncompliant
        End If
    End Sub

    Sub Method2(i As Integer)
    End Sub

    Sub IfExpression(nullable As Integer?)
        If nullable Is Nothing Then
            Dim x As Integer = If(True, nullable, nullable)    ' Noncompliant
        End If
    End Sub
End Class

Class WithAliases
    Private Sub Basics(ByVal i As MaybeInt)
        Dim x = i.Value
        i = Nothing
        x = (CTypeDynamic(Of Integer?)(i)).Value    ' FN
    End Sub
End Class

Class EqualsOperator
    Sub EqualsNothing(nullable As Integer?)
        If nullable = Nothing Then      ' Always evaluates to Nothing (=> false) due to null propagation
            Dim x As Integer = nullable ' Noncompliant FP, nullable was not actually checked for null
        End If
    End Sub

    Sub UnequalsNothing(nullable As Integer?)
        If nullable <> Nothing Then     ' Always evaluates to Nothing (=> false) due to null propagation
            Dim x As Integer = nullable ' Compliant
        End If
    End Sub

    Sub UqualsOrUnequals(nullable As Integer?)
        If (nullable <> Nothing) OrElse (nullable = Nothing) Then
            Dim x As Integer = nullable ' Noncompliant FP
        End If
    End Sub
End Class

Namespace TypeWithValueProperty
    Class TypeWithValueProperty
        Private Sub Basics1()
            Dim i As ClassWithValueProperty = Nothing
            Dim x = i.Value
        End Sub

        Private Sub Basics2()
            Dim i As ClassWithValueProperty = Nothing
            Dim x = i.APropertyNotCalledValue
        End Sub

        Private Sub ImplicitCast()
            Dim i As StructWithValuePropertyAndCastOperators = Nothing
            Dim x = i.Value
        End Sub

        Private ReadOnly Property ExplicitCast1 As Integer
            Get
                Return (CType((CTypeDynamic(Of Long?)(Nothing)), StructWithValuePropertyAndCastOperators)).Value
            End Get
        End Property

        Private ReadOnly Property ExplicitCast2 As StructWithValuePropertyAndCastOperators
            Get
                Return (CTypeDynamic(Of StructWithValuePropertyAndCastOperators?)(Nothing)).Value
            End Get
        End Property

        ReadOnly Property ExplicitCast3 As Integer
            Get
                Return (CTypeDynamic(Of StructWithValuePropertyAndCastOperators?)(Nothing)).Value.Value
            End Get
        End Property
    End Class

    Class ClassWithValueProperty
        Public ReadOnly Property Value As Integer
            Get
                Return 42
            End Get
        End Property

        Public ReadOnly Property APropertyNotCalledValue As Integer
            Get
                Return 42
            End Get
        End Property
    End Class

    Structure StructWithValuePropertyAndCastOperators
        Public ReadOnly Property Value As Integer
            Get
                Return 42
            End Get
        End Property

        Public ReadOnly Property APropertyNotCalledValue As Integer
            Get
                Return 42
            End Get
        End Property

        Public Shared Widening Operator CType(value As Integer?) As StructWithValuePropertyAndCastOperators
            Return New StructWithValuePropertyAndCastOperators()
        End Operator

        Public Shared Narrowing Operator CType(value As Long?) As StructWithValuePropertyAndCastOperators
            Return New StructWithValuePropertyAndCastOperators()
        End Operator
    End Structure
End Namespace

Class OutAndRefParams
    Private field As Integer

    Private Sub OutParams(ByVal iParam As Integer?)
        iParam = Nothing
        ModifyOutParam(iParam)
        Dim x = iParam.Value
        Dim iLocal As Integer?
        ModifyOutParam(iLocal)
        x = iLocal.Value
        iLocal = Nothing
        ModifyOutParam(iLocal)
        x = iLocal.Value
    End Sub

    Private Shared Sub ModifyOutParam(<Out> ByRef i As Integer?)
        i = Nothing
    End Sub

    Private Sub RefParams(ByVal iParam As Integer?)
        iParam = Nothing
        ModifyRefParam(iParam)
        Dim x = iParam.Value
        Dim iLocal As Integer? = Nothing
        ModifyRefParam(iLocal)
        x = iLocal.Value
    End Sub

    Private Shared Sub ModifyRefParam(ByRef i As Integer?)
        i = Nothing
    End Sub
End Class
