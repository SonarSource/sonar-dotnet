Imports System
Imports Config = System.Configuration.Assemblies
Imports System.Globalization
Imports System.Reflection
Imports System.Security
Imports System.Security.Policy

Namespace Test

    Public Class TestReflection

        Public Shared Sub Run(typeName As String, methodName As String, fieldName As String, propertyName As String, moduleName As String,
            data As Byte(), name As String, path As String, assemblyName As AssemblyName,
            evidence As Evidence, contextSource As SecurityContextSource)

            'Assembly.Load(...) ' Questionable
            Assembly.Load(assemblyName)
'           ^^^^^^^^^^^^^^^^^^^^^^^^^^^     {{Make sure that this dynamic injection or execution of code is safe.}}
            Assembly.Load(data)                        ' Noncompliant
            Assembly.Load(assemblyName, evidence)      ' Noncompliant
            Assembly.Load(data, data)                  ' Noncompliant
            Assembly.Load(name, evidence)              ' Noncompliant
            Assembly.Load(data, data, evidence)        ' Noncompliant
            Assembly.Load(data, data, contextSource)   ' Noncompliant

            'Assembly.LoadFile(...) ' Questionable
            Assembly.LoadFile(path)                ' Noncompliant
            Assembly.LoadFile(path, evidence)      ' Noncompliant

            'Assembly.LoadFrom(...) ' Questionable
            Assembly.LoadFrom(path)                ' Noncompliant
            Assembly.LoadFrom(path, evidence)      ' Noncompliant
            Assembly.LoadFrom(path, data, System.Configuration.Assemblies.AssemblyHashAlgorithm.MD5)           ' Noncompliant
            Assembly.LoadFrom(path, evidence, data, System.Configuration.Assemblies.AssemblyHashAlgorithm.None) ' Noncompliant

            'Assembly.LoadWithPartialName(...) ' Questionable + deprecated
            Assembly.LoadWithPartialName(name)             ' Noncompliant
            Assembly.LoadWithPartialName(name, evidence)   ' Noncompliant

            'Assembly.ReflectionOnlyLoad(...)  ' This is OK as the resulting type is not executable.
            Assembly.ReflectionOnlyLoad(data)
            Assembly.ReflectionOnlyLoad(name)

            Dim asm = GetType(TestReflection).Assembly

            ' Review this code to make sure that the module, type, method and field are safe
            Dim type = assembly.GetType(typeName)
'                      ^^^^^^^^^^^^^^^^^^^^^^^^^^     {{Make sure that this dynamic injection or execution of code is safe.}}


            type = asm.GetType(typeName, False)                   ' Noncompliant
            type = asm.GetType(typeName, False, False)            ' Noncompliant
            Dim [module] As [Module] = asm.GetModule(moduleName)      ' Noncompliant

            type = Type.GetType(typeName)                              ' Noncompliant
            type = Type.GetType(typeName, True)                        ' Noncompliant
            type = Type.GetType(typeName, True, True)                  ' Noncompliant

            type = type.GetNestedType(typeName)                        ' Noncompliant
            type = type.GetNestedType(typeName, BindingFlags.Instance) ' Noncompliant

            type = type.GetInterface(typeName)                         ' Noncompliant
            type = type.GetInterface(typeName, False)                  ' Noncompliant

            Dim method = type.GetMethod(methodName)                    ' Noncompliant
            method = type.GetMethod(methodName, BindingFlags.IgnoreReturn) ' Noncompliant
            method = type.GetMethod(methodName, New Type() {})         ' Noncompliant

            Dim field = type.GetField(fieldName)                       ' Noncompliant
            field = type.GetField(fieldName, BindingFlags.SetField)    ' Noncompliant

            Dim prop = type.GetProperty(propertyName)              ' Noncompliant
            prop = type.GetProperty(propertyName, BindingFlags.SetProperty)    ' Noncompliant


            ' Review this code to make sure that the modules, types, methods and fields are used safely
            Dim modules = asm.GetModules()                ' Noncompliant
            modules = asm.GetLoadedModules()              ' Noncompliant
            modules = asm.GetLoadedModules(False)         ' Noncompliant

            Dim types = asm.GetTypes()                    ' Noncompliant
            types = asm.GetExportedTypes()                ' Noncompliant

            ' Only available in NET Core 2.1+
            'types = assembly.GetForwardedTypes() ' Questionable

            types = type.GetNestedTypes()                      ' Noncompliant
            types = type.GetNestedTypes(BindingFlags.Public)   ' Noncompliant

            Dim methods = type.GetMethods()           ' Noncompliant
            Dim fields = type.GetFields()              ' Noncompliant
            Dim properties = type.GetProperties()   ' Noncompliant
            Dim members = type.GetMembers()           ' Noncompliant
            members = type.GetMember(methodName)               ' Noncompliant
            members = type.GetDefaultMembers()                 ' Noncompliant

            'type.InvokeMember(...) ' Questionable
            type.InvokeMember(methodName, BindingFlags.Public, Nothing, Nothing, Nothing) ' Noncompliant
            type.InvokeMember(methodName, BindingFlags.Public, Nothing, Nothing, Nothing, CultureInfo.CurrentCulture) ' Noncompliant
            type.InvokeMember(methodName, BindingFlags.Public, Nothing, Nothing, Nothing, Nothing, CultureInfo.CurrentCulture, Nothing) ' Noncompliant

            asm.CreateInstance(typeName)                  ' Noncompliant
            asm.CreateInstance(typeName, False)           ' Noncompliant
            asm.CreateInstance(typeName, False, BindingFlags.Public, Nothing, Nothing, CultureInfo.CurrentCulture, Nothing) ' Noncompliant

            type = Type.ReflectionOnlyGetType(typeName, True, True) ' This is OK as the resulting type is not executable.
        End Sub

        Public Sub ActivatorTests(name As String, activationContext As System.ActivationContext)
            Const fixedType As String = "fixedType"
            Activator.CreateComInstanceFrom(name, fixedType)       ' Noncompliant
            Activator.CreateComInstanceFrom(name, fixedType, New Byte() {}, Config.AssemblyHashAlgorithm.MD5) ' Noncompliant

            Dim t = GetType(Exception)
            Activator.CreateInstance(t) ' Don't report - constructed from type
            Activator.CreateInstance(t, Nothing, Nothing) ' Don't report - constructed from type
            Activator.CreateInstance(activationContext)        ' Noncompliant
            Activator.CreateInstance(name, fixedType)          ' Noncompliant
            Activator.CreateInstance(name, fixedType, Nothing) ' Noncompliant

            Activator.CreateInstanceFrom(name, fixedType)      ' Noncompliant
            Activator.CreateInstanceFrom(AppDomain.CurrentDomain, name, fixedType) ' Noncompliant
            Activator.CreateInstance(activationContext)        ' Noncompliant

            Activator.CreateInstance(Of Exception)() ' OK - known type
        End Sub


        Public Sub AdditionalTests(evidence As Evidence)

            Const constantName As String = "fixedName"

            ' Hard-coded names for Assembly.Load are not ok...
            Dim asm = Assembly.Load(constantName)          ' Noncompliant
            asm = Assembly.Load(constantName, evidence)    ' Noncompliant
            Assembly.LoadFile(constantName)                ' Noncompliant
            Assembly.LoadFrom(constantName, evidence)      ' Noncompliant
            Assembly.LoadWithPartialName(constantName)     ' Noncompliant

            ' ...but hard-coded names for other methods are ok
            Dim type = asm.GetType(constantName)
            type = asm.GetType(constantName, False)

            type = Type.GetType(constantName)

            type.GetNestedType(constantName)
            type.GetInterface(constantName)
            type.GetMethod(constantName)
            type.GetField(constantName)
            type.GetProperty(constantName)
            asm.GetModule(constantName)
            type.GetMember(constantName)

            type.InvokeMember(constantName, BindingFlags.NonPublic, Nothing, Nothing, Nothing)

            type = type.GetType()                   ' OK - method on Object
            Dim obj1 = Me.GetType()                 ' OK - this / Me
            Dim obj2 = GetType(Object).GetMethods() ' OK - TypeOf/ GetType

        End Sub
    End Class
End Namespace
