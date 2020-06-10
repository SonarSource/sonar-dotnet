using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters.Soap;
using System.Web.UI;

namespace Tests.Diagnostics
{
    internal class Serializer
    {
        internal void BinaryFormatterDeserialize(MemoryStream memoryStream)
        {
            new BinaryFormatter().Deserialize(memoryStream); // Noncompliant {{Restrict types of objects allowed to be deserialized.}}
        }

        internal void NetDataContractSerializerDeserialize()
        {
            new NetDataContractSerializer().Deserialize(new MemoryStream()); // Noncompliant {{Restrict types of objects allowed to be deserialized.}}
        }

        internal void SoapFormatterDeserialize()
        {
            new SoapFormatter().Deserialize(new MemoryStream()); // Noncompliant {{Restrict types of objects allowed to be deserialized.}}
        }

        internal void ObjectStateFormatterDeserialize()
        {
            new ObjectStateFormatter().Deserialize(new MemoryStream()); // Noncompliant {{Restrict types of objects allowed to be deserialized.}}
        }

        internal void BinderAsVariable(Stream stream, bool condition)
        {
            var safeBinder = new SafeBinderStatementWithReturnNull();
            var unsafeBinder = new UnsafeBinder();
            SerializationBinder nullBinder = null;

            var formatter1 = new BinaryFormatter();
            formatter1.Binder = safeBinder;
            formatter1.Deserialize(stream); // Compliant: safe binder was used

            var formatter2 = new BinaryFormatter();
            formatter2.Binder = unsafeBinder;
            formatter2.Deserialize(stream); // Noncompliant: unsafe binder used

            var formatter3 = new BinaryFormatter();
            formatter3.Binder = nullBinder;
            formatter3.Deserialize(stream); // Noncompliant: the binder is null

            var possibleNullBinder = condition ? null : new SafeBinderStatementWithReturnNull();
            var formatter4 = new BinaryFormatter();
            formatter4.Binder = possibleNullBinder;
            formatter4.Deserialize(stream); // Noncompliant: the binder can be null

            var formatter5 = new BinaryFormatter();
            if (condition)
            {
                formatter5.Binder = new SafeBinderStatementWithReturnNull();
            }
            formatter5.Deserialize(stream); // Noncompliant: the binder can be null

            var formatter6 = new BinaryFormatter();
            formatter6.Binder = new SafeBinderExpressionWithNull();
            formatter6.Binder = new UnsafeBinder();
            formatter6.Deserialize(stream); // Noncompliant: the last binder set is unsafe

            var formatter7 = new BinaryFormatter {Binder = new SafeBinderExpressionWithNull()};
            formatter7.Binder = new UnsafeBinder();
            formatter7.Deserialize(stream); // Noncompliant: the last binder set is unsafe

            var formatter8 = new BinaryFormatter();
            formatter8.Binder = new UnsafeBinder();
            formatter8.Binder = new SafeBinderExpressionWithNull();
            formatter8.Deserialize(stream); // Compliant: the last binder set is safe

            var formatter9 = new BinaryFormatter {Binder = new UnsafeBinder()};
            formatter9.Binder = new SafeBinderExpressionWithNull();
            formatter9.Deserialize(stream); // Compliant: the last binder set is safe

            var formatter10 = new BinaryFormatter {Binder = new UnsafeBinder()};
            formatter10.Deserialize(stream); // Noncompliant: the safe binder was set after deserialize call
            formatter10.Binder = new SafeBinderExpressionWithNull();

            var formatter11 = new BinaryFormatter();
            formatter11.Binder ??= new SafeBinderExpressionWithNull();
            formatter11.Deserialize(stream); // Compliant: safe binder using null coalescence

            var formatter12 = new BinaryFormatter();
            formatter12.Binder ??= new UnsafeBinder();
            formatter12.Deserialize(stream); // Noncompliant: unsafe binder
        }

        internal void DeserializeOnExpression(MemoryStream memoryStream, bool condition)
        {
            new BinaryFormatter().Deserialize(memoryStream); // Noncompliant - Unsafe by default
            new BinaryFormatter {Binder = null}.Deserialize(memoryStream); // Noncompliant - Unsafe when the binder is null
            (condition ? new BinaryFormatter() : null).Deserialize(memoryStream); // Noncompliant - Unsafe in ternary operator
            BinaryFormatter bin = null;
            (bin ??= new BinaryFormatter()).Deserialize(memoryStream); // Noncompliant - unsafe in null coalescence
            new BinaryFormatter {Binder = new SafeBinderStatementWithReturnNull()}.Deserialize(memoryStream); // safe binder set in initializer
            new BinaryFormatter {Binder = new UnsafeBinder()}.Deserialize(memoryStream); // Noncompliant - unsafe binder set in initializer
            (condition
                ? new BinaryFormatter {Binder = new SafeBinderStatementWithReturnNull()}
                : new BinaryFormatter {Binder = new SafeBinderWithThrowStatement()}).Deserialize(memoryStream); // Safe in ternary operator
        }

        internal void BinderCases(MemoryStream memoryStream)
        {
            var formatter = new BinaryFormatter();

            formatter.Binder = new SafeBinderStatementWithReturnNull();
            formatter.Deserialize(memoryStream); // Compliant: a safe binder was used

            formatter.Binder = new SafeBinderExpressionWithNull();
            formatter.Deserialize(memoryStream); // Compliant: a safe binder was used

            formatter.Binder = new SafeBinderWithThrowStatement();
            formatter.Deserialize(memoryStream); // Compliant: a safe binder was used

            formatter.Binder = new SafeBinderWithThrowExpression();
            formatter.Deserialize(memoryStream); // Compliant: a safe binder was used

            formatter.Binder = new UnsafeBinder();
            formatter.Deserialize(memoryStream); // Noncompliant: the used binder does not validate the deserialized types

            formatter.Binder = new UnsafeBinderExpressionBody();
            formatter.Deserialize(memoryStream); // Noncompliant: the used binder does not validate the deserialized types
        }

        internal void UnknownBindersAreSafe(SerializationBinder binder, bool condition)
        {
            new BinaryFormatter {Binder = binder}.Deserialize(new MemoryStream()); // Compliant: Unknown binders are considered safe

            var formatter = new BinaryFormatter {Binder = binder};
            formatter.Deserialize(new MemoryStream()); // Compliant: Unknown binders are considered safe

            new BinaryFormatter()
            {
                Binder = condition ? (SerializationBinder) new SafeBinderExpressionWithNull() : new UnsafeBinder()
            }.Deserialize(new MemoryStream()); // Compliant - FN: common type is SerializationBinder
        }
    }
}
