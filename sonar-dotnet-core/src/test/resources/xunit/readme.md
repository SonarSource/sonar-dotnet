# Generating XUnit Test Results

## valid.xml

These results are generated from [this project](https://github.com/alex-meseldzija-sonarsource/Playground/tree/main/XUnit) by running the `dotnet test --logger:xunit` command

This will generate a valid .xml file in Playground/XUnit/TestResults.

An extra assembly was also added.

```xml
  <assembly name="C:\dev\Playground\XUnit\bin\Debug\net9.0\XUnitTestProj2.dll" run-date="2024-11-20" run-time="09:45:53" total="1" passed="1" failed="0" skipped="0" time="0.006" errors="0">
    <errors />
    <collection total="1" passed="0" failed="0" skipped="1" name="Test collection for XUnitTestProject2.UnitTest2" time="0.006">
      <test name="XUnitTestProject2.UnitTest2.XUnitTestNotRun" type="XUnitTestProject2.UnitTest2" method="XUnitTestNotRun" time="0.0061234" result="NotRun">
        <traits />
      </test>
    </collection>
  </assembly>
```

## valid_data_attribute.xml

This is a valid xml file that was generated from the above xunit project but with a single cs file consisting of:

```csharp
namespace DataDrivenWithXUnit.Test
{
    public class CalculatorTestWithClassData
    {
        [Theory]
        [ClassData(typeof(TestClassDataGenerator))]
        public void Add_ShouldReturnCorrectSum(int a, int b, int expected)
        {
            // Act
            int result = Hello.AddNumber(a, b);

            // Assert
            Assert.Equal(expected, result);
        }


    }

    public class TestClassDataGenerator : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[] { 2, 3, 5 }; // Test case 1
            yield return new object[] { -1, 1, 0 }; // Test case 2
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
```

## valid_no_execution_time.xml

This is a valid .xml file that consists of a single unit test that has had the execution time removed.

## invalid_test_outcome.xml

This is a valid .xml file that has been modified to have an outcome that doesn't exist.

```xml
<test result="SomeOtherResult"/>
```

## test_name_not_mapped.xml

This is a valid .xml file consisting of a single unit test extracted from valid.xml that is then not mapped correctly in the Hashmap provided to the XUnitTestResultParser.

## invalid_root.xml

This is a valid .xml file but not a valid XUnit report, it has had its root tag replace with 

```xml
<foo/>
```
