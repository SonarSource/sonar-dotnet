# Generating Nunit Test Results

## valid_nunit3.xml

These results are generated from [this project](https://github.com/alex-meseldzija-sonarsource/Playground/tree/main/Nunit) by running the `dotnet test --logger:nunit` command

This will generate a valid .xml file in Playground/Nunit/TestResults.

An extra assembly was also added from the [nunit-3 sample report](https://nunit.org/files/testresult_30.txt).

## valid_nunit2.xml

These results are [the Nunit 2 sample report](https://nunit.org/files/testresult_25.txt)

This is a valid xml file that was generated from the above Nunit project but with a single cs file consisting of:

```csharp
namespace DataDrivenWithNunit.Test
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

This is a valid .xml file that consists of a single unit test that has had the duration tag removed.

## valid_comma_in_double.xml

This is a valid .xml file that consists of a single unit test that has had the duration tag modified to have a double with a comma in the value.

```xml
<test-case duration="1,041" />
```

## invalid_test_outcome.xml

This is a valid .xml file that has been modified to have an outcome that doesn't exist.

```xml
<test-case result="InvalidOutcome"/>
```

## test_name_not_mapped.xml

This is a valid .xml file consisting of a single unit test extracted from valid.xml that is then not mapped correctly in the Hashmap provided to the NunitTestResultParser.

## invalid_root.xml

This is a valid .xml file but not a valid Nunit report, it has had its root tag replace with

```xml
<foo/>
```
