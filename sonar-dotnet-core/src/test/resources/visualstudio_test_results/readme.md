# Generating VS Test Results

## Valid.trx

These results are generated from [this project](https://github.com/alex-meseldzija-sonarsource/Playground/tree/main/Playground.Test) by running the `dotnet test --logger:trx` command

This will generate a valid .trx file in Playground/Playground.Test/TestResults.

These results do not cover the full possibilities of results possible from .trx files.
There is no documentation on how the outcome value is determined so many are added manually.

Under the `<Results>` tag these tests are added to hit all possible outcomes

```xml
    <UnitTestResult executionId="b93d94d0-7d71-492e-9d2e-acfb902c888d" testId="2386b338-1542-4ec7-a8b2-cdfbaa53ba67" testName="TestMethod5 (1,2)" computerName="PC-L0109" duration="00:00:00.0001001" startTime="2024-11-25T15:38:27.3972727+01:00" endTime="2024-11-25T15:38:27.4000768+01:00" testType="13cdc9d9-ddb5-4fa4-a97d-d965ccfc6d4b" outcome="Warning" testListId="8c84fa94-04c1-424b-9868-57a2d4851a1d" relativeResultsDirectory="abe9481a-f298-4416-ae61-4c893c7c801d" />
    <UnitTestResult executionId="8cf735d6-85fb-4f31-8258-7ff19fc56bbe" testId="35fa7c77-814c-4fae-913d-282d27c9d317" testName="TestMethod5 (2,1)" computerName="PC-L0109" duration="00:00:00.0001001" startTime="2024-11-25T15:38:27.3972727+01:00" endTime="2024-11-25T15:38:27.4000768+01:00" testType="13cdc9d9-ddb5-4fa4-a97d-d965ccfc6d4b" outcome="Error" testListId="8c84fa94-04c1-424b-9868-57a2d4851a1d" relativeResultsDirectory="abe9481a-f298-4416-ae61-4c893c7c801d" />
    <UnitTestResult executionId="7cd0835a-a270-49be-bf79-ecae420940e8" testId="ebb027a0-c08c-4edb-b618-97dc337b39fe" testName="TestMethod5 (2,2)" computerName="PC-L0109" duration="00:00:00.0001001" startTime="2024-11-25T15:38:27.3972727+01:00" endTime="2024-11-25T15:38:27.4000768+01:00" testType="13cdc9d9-ddb5-4fa4-a97d-d965ccfc6d4b" outcome="PassedButRunAborted" testListId="8c84fa94-04c1-424b-9868-57a2d4851a1d" relativeResultsDirectory="abe9481a-f298-4416-ae61-4c893c7c801d" />
    <UnitTestResult executionId="5ecd7703-31f9-49f3-97bc-fccd97bb97c4" testId="358b3d3f-0c53-478e-9943-28ad836e4539" testName="TestMethod5 (1,3)" computerName="PC-L0109" duration="00:00:00.0001001" startTime="2024-11-25T15:38:27.3972727+01:00" endTime="2024-11-25T15:38:27.4000768+01:00" testType="13cdc9d9-ddb5-4fa4-a97d-d965ccfc6d4b" outcome="NotExecuted" testListId="8c84fa94-04c1-424b-9868-57a2d4851a1d" relativeResultsDirectory="abe9481a-f298-4416-ae61-4c893c7c801d" />
    <UnitTestResult executionId="ef149fac-e774-44e2-ae72-ac52efa688be" testId="98d81cac-eddf-41ef-98b6-a008cc35182b" testName="TestMethod5 (3,1)" computerName="PC-L0109" duration="00:00:00.0001001" startTime="2024-11-25T15:38:27.3972727+01:00" endTime="2024-11-25T15:38:27.4000768+01:00" testType="13cdc9d9-ddb5-4fa4-a97d-d965ccfc6d4b" outcome="Inconclusive" testListId="8c84fa94-04c1-424b-9868-57a2d4851a1d" relativeResultsDirectory="abe9481a-f298-4416-ae61-4c893c7c801d" />
    <UnitTestResult executionId="bbd331db-a908-4b7b-b4ed-d0bff07ab6b3" testId="0628e8f4-6604-423d-a520-a7011d0637eb" testName="TestMethod5 (2,3)" computerName="PC-L0109" duration="00:00:00.0001001" startTime="2024-11-25T15:38:27.3972727+01:00" endTime="2024-11-25T15:38:27.4000768+01:00" testType="13cdc9d9-ddb5-4fa4-a97d-d965ccfc6d4b" outcome="Completed" testListId="8c84fa94-04c1-424b-9868-57a2d4851a1d" relativeResultsDirectory="abe9481a-f298-4416-ae61-4c893c7c801d" />
    <UnitTestResult executionId="5c71e89b-7d7c-46f8-851e-a9a152398281" testId="ddfa5c76-cbe1-4372-8a85-a236697ddf5b" testName="TestMethod5 (3,2)" computerName="PC-L0109" duration="00:00:00.0001001" startTime="2024-11-25T15:38:27.3972727+01:00" endTime="2024-11-25T15:38:27.4000768+01:00" testType="13cdc9d9-ddb5-4fa4-a97d-d965ccfc6d4b" outcome="Timeout" testListId="8c84fa94-04c1-424b-9868-57a2d4851a1d" relativeResultsDirectory="abe9481a-f298-4416-ae61-4c893c7c801d" />
    <UnitTestResult executionId="61a90e58-33a5-48c7-9d1f-ecfcf82a5625" testId="0683bb92-89c8-4e66-a1f8-522e2199ef53" testName="TestMethod5 (3,3)" computerName="PC-L0109" duration="00:00:00.0001001" startTime="2024-11-25T15:38:27.3972727+01:00" endTime="2024-11-25T15:38:27.4000768+01:00" testType="13cdc9d9-ddb5-4fa4-a97d-d965ccfc6d4b" outcome="Aborted" testListId="8c84fa94-04c1-424b-9868-57a2d4851a1d" relativeResultsDirectory="abe9481a-f298-4416-ae61-4c893c7c801d" />
    <UnitTestResult executionId="ba64eb5c-6ab2-4fc0-83f5-f29d0bea1ab1" testId="5553b0bd-1390-49c8-bfff-70b4c94a4321" testName="TestMethod5 (4,1)" computerName="PC-L0109" duration="00:00:00.0001001" startTime="2024-11-25T15:38:27.3972727+01:00" endTime="2024-11-25T15:38:27.4000768+01:00" testType="13cdc9d9-ddb5-4fa4-a97d-d965ccfc6d4b" outcome="Blocked" testListId="8c84fa94-04c1-424b-9868-57a2d4851a1d" relativeResultsDirectory="abe9481a-f298-4416-ae61-4c893c7c801d" />
    <UnitTestResult executionId="abe9481a-f298-4416-ae61-4c893c7c801d" testId="5733759c-ad55-9784-8372-5dfbf9179fbc" testName="TestMethod5 (1,4)" computerName="PC-L0109" duration="00:00:00.0001001" startTime="2024-11-25T15:38:27.3972727+01:00" endTime="2024-11-25T15:38:27.4000768+01:00" testType="13cdc9d9-ddb5-4fa4-a97d-d965ccfc6d4b" outcome="NotRunnable" testListId="8c84fa94-04c1-424b-9868-57a2d4851a1d" relativeResultsDirectory="abe9481a-f298-4416-ae61-4c893c7c801d" />
```

Then Under the `<TestDefinitions>` tag these are added. Please note that the `<UnitTest id>`value matches exactly to a `<UnitTestResult testId>` value.

```xml
    <UnitTest name="TestMethod5 (1,2)" storage="c:\dev\playground\playground.test\bin\debug\net9.0\playground.test.dll" id="2386b338-1542-4ec7-a8b2-cdfbaa53ba67">
      <Execution id="abe9481a-f298-4416-ae61-4c893c7c801d" />
      <TestMethod codeBase="C:\dev\Playground\Playground.Test\bin\Debug\net9.0\Playground.Test.dll" adapterTypeName="executor://mstestadapter/v2" className="TestProject1.UnitTest2" name="TestMethod5" />
    </UnitTest>
    <UnitTest name="TestMethod5 (2,1)" storage="c:\dev\playground\playground.test\bin\debug\net9.0\playground.test.dll" id="35fa7c77-814c-4fae-913d-282d27c9d317">
      <Execution id="abe9481a-f298-4416-ae61-4c893c7c801d" />
      <TestMethod codeBase="C:\dev\Playground\Playground.Test\bin\Debug\net9.0\Playground.Test.dll" adapterTypeName="executor://mstestadapter/v2" className="TestProject1.UnitTest2" name="TestMethod5" />
    </UnitTest>
    <UnitTest name="TestMethod5 (2,2)" storage="c:\dev\playground\playground.test\bin\debug\net9.0\playground.test.dll" id="ebb027a0-c08c-4edb-b618-97dc337b39fe">
      <Execution id="abe9481a-f298-4416-ae61-4c893c7c801d" />
      <TestMethod codeBase="C:\dev\Playground\Playground.Test\bin\Debug\net9.0\Playground.Test.dll" adapterTypeName="executor://mstestadapter/v2" className="TestProject1.UnitTest2" name="TestMethod5" />
    </UnitTest>
    <UnitTest name="TestMethod5 (1,3)" storage="c:\dev\playground\playground.test\bin\debug\net9.0\playground.test.dll" id="358b3d3f-0c53-478e-9943-28ad836e4539">
      <Execution id="abe9481a-f298-4416-ae61-4c893c7c801d" />
      <TestMethod codeBase="C:\dev\Playground\Playground.Test\bin\Debug\net9.0\Playground.Test.dll" adapterTypeName="executor://mstestadapter/v2" className="TestProject1.UnitTest2" name="TestMethod5" />
    </UnitTest>
    <UnitTest name="TestMethod5 (3,1)" storage="c:\dev\playground\playground.test\bin\debug\net9.0\playground.test.dll" id="98d81cac-eddf-41ef-98b6-a008cc35182b">
      <Execution id="abe9481a-f298-4416-ae61-4c893c7c801d" />
      <TestMethod codeBase="C:\dev\Playground\Playground.Test\bin\Debug\net9.0\Playground.Test.dll" adapterTypeName="executor://mstestadapter/v2" className="TestProject1.UnitTest2" name="TestMethod5" />
    </UnitTest>
    <UnitTest name="TestMethod5 (2,3)" storage="c:\dev\playground\playground.test\bin\debug\net9.0\playground.test.dll" id="ddfa5c76-cbe1-4372-8a85-a236697ddf5b">
      <Execution id="abe9481a-f298-4416-ae61-4c893c7c801d" />
      <TestMethod codeBase="C:\dev\Playground\Playground.Test\bin\Debug\net9.0\Playground.Test.dll" adapterTypeName="executor://mstestadapter/v2" className="TestProject1.UnitTest2" name="TestMethod5" />
    </UnitTest>
    <UnitTest name="TestMethod5 (3,2)" storage="c:\dev\playground\playground.test\bin\debug\net9.0\playground.test.dll" id="0683bb92-89c8-4e66-a1f8-522e2199ef53">
      <Execution id="abe9481a-f298-4416-ae61-4c893c7c801d" />
      <TestMethod codeBase="C:\dev\Playground\Playground.Test\bin\Debug\net9.0\Playground.Test.dll" adapterTypeName="executor://mstestadapter/v2" className="TestProject1.UnitTest2" name="TestMethod5" />
    </UnitTest>
    <UnitTest name="TestMethod5 (3,3)" storage="c:\dev\playground\playground.test\bin\debug\net9.0\playground.test.dll" id="5553b0bd-1390-49c8-bfff-70b4c94a4321">
      <Execution id="abe9481a-f298-4416-ae61-4c893c7c801d" />
      <TestMethod codeBase="C:\dev\Playground\Playground.Test\bin\Debug\net9.0\Playground.Test.dll" adapterTypeName="executor://mstestadapter/v2" className="TestProject1.UnitTest2" name="TestMethod5" />
    </UnitTest>
    <UnitTest name="TestMethod5 (4,1)" storage="c:\dev\playground\playground.test\bin\debug\net9.0\playground.test.dll" id="5733759c-ad55-9784-8372-5dfbf9179fbc">
      <Execution id="abe9481a-f298-4416-ae61-4c893c7c801d" />
      <TestMethod codeBase="C:\dev\Playground\Playground.Test\bin\Debug\net9.0\Playground.Test.dll" adapterTypeName="executor://mstestadapter/v2" className="TestProject1.UnitTest2" name="TestMethod5" />
    </UnitTest>
    <UnitTest name="TestMethod5 (1,4)" storage="c:\dev\playground\playground.test\bin\debug\net9.0\playground.test.dll" id="9c66db0f-1976-ce08-cc4c-71201b65b30a">
      <Execution id="abe9481a-f298-4416-ae61-4c893c7c801d" />
      <TestMethod codeBase="C:\dev\Playground\Playground.Test\bin\Debug\net9.0\Playground.Test.dll" adapterTypeName="executor://mstestadapter/v2" className="TestProject1.UnitTest2" name="TestMethod5" />
    </UnitTest>
```

## invalid_character.trx

This is a valid .trx file that has been modified to be an invalid .trx and invalid .xml.
There can be only 1 UnitTestResult tag and no Test Definition tags, otherwise the XMLParserHelper will not throw an exception.

```xml
    <UnitTestResult executionId="eff25556-c0df-4a48-b88d-2286e542af4f" testId="d7744238-9adf-b364-3d70-ae38261a8cd8" testName="TestShouldFail" computerName="PC-L0109" duration="00:00:00.0183996" startTime="2024-11-25T15:38:27.3755778+01:00" endTime="2024-11-25T15:38:27.3951390+01:00" testType="13cdc9d9-ddb5-4fa4-a97d-d965ccfc6d4b" outcome="Failed" testListId="8c84fa94-04c1-424b-9868-57a2d4851a1d" relativeResultsDirectory="eff25556-c0df-4a48-b88d-2286e542af4f">
    <Output>
      <StdOut><</StdOut>
    </Output>
    </UnitTestResult>
```

## invalid_dates.trx

This is a valid .trx file that has been modified to have an invalid datetime format.

```xml
<UnitTestResult startTime="2016-xx-14T17:04:31.1+01:00" endTime="2016-xx-14T17:04:31.9162137+01:00" />
```

## invalid_test_outcome.trx

This is a valid .trx file that has been modified to have an outcome that doesn't exist.

```xml
<UnitTestResult outcome="ThisDoesntMatch"/>
```

## test_name_not mapped.trx

This is a valid .trx file consisting of a single unit test extracted from valid.trx that is then not mapped correctly in the Hashmap provided to the VisualStudioTestResultParser.

## test_result_no_test_method.trx

This is a valid .trx file consisting of a a single unit test extracted from valid.trx that has had the methodName tag removed from the UnitTestResult tag.

## multiple_runs_same_test.trx

Generated by analyzing the [TestReport project](projects/TestReport) with the `dotnet test --logger:trx` command.