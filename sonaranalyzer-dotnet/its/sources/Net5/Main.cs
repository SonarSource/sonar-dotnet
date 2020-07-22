using System;
using Net5;

Console.WriteLine("Hello World!");

new AttributesOnLocalFunctions().Method();
// new CallIndirectAttribute();
// new CovariantReturn();
new ExtensionGetEnumerator().Method();
new FunctionPointers().Method();
new InitOnlyProperties { FirstName = "not null" };
new LambdaDiscardParameters().Method();
new NativeInts().Method();
// ParameterValidation();
new Records().TestRecords();

new SkipLocalsInit().Method();
new TargetTypedNew().Method();


LocalMethodInMain();

void LocalMethodInMain() { }
