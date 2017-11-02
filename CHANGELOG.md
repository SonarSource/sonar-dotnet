# Change Log (Release Notes)

All _notable_ changes to this project will be documented in this file (`CHANGELOG.md`).

Contributors to this file, please follow the guidelines on [keepachangelog.com](http://keepachangelog.com/).

For reference, the possible headings are:

* **New Feature** for new features.
* **Improvement** for changes in existing functionality.
* **Bug** for any bug fixes.
* **External Contributors** to list contributors outside of SonarSource SA.
* **Notes**


## Unreleased

### Notes
* [All commits](https://github.com/SonarSource/sonar-csharp/compare/6.5.0.3766...HEAD)


## [6.5](https://github.com/SonarSource/sonar-csharp/releases/tag/6.5.0.3766)

### Bug
* [#792](https://github.com/SonarSource/sonar-csharp/issues/792) - Fix S3928: Rule throws NullReferenceException

### False Positive
* [#708](https://github.com/SonarSource/sonar-csharp/issues/708) - Fix S4056: Rule should not recommend to use obsolete methods
* [#761](https://github.com/SonarSource/sonar-csharp/issues/761) - Fix S1168: Rule should not report when returning null within a lambda

### New Feature
* [#740](https://github.com/SonarSource/sonar-csharp/issues/740) - Rule S4225: Extension methods should not extend "object"
* [#741](https://github.com/SonarSource/sonar-csharp/issues/741) - Rule S4226: Extensions should be in separate namespaces
* [#742](https://github.com/SonarSource/sonar-csharp/issues/742) - Rule S4220: Events should have proper arguments
* [#746](https://github.com/SonarSource/sonar-csharp/issues/746) - Rule S4214: "P/Invoke" methods should not be visible
* [#779](https://github.com/SonarSource/sonar-csharp/issues/779) - Rule S4260: "ConstructorArgument" parameters should exist

### Notes
* [All commits](https://github.com/SonarSource/sonar-csharp/compare/6.4.1.3596...6.5.0.3766)


## [6.4.1](https://github.com/SonarSource/sonar-csharp/releases/tag/6.4.1.3596)

### Bug
* [#791](https://github.com/SonarSource/sonar-csharp/issues/791) - Fix SonarC#: Parameterized rules are not read properly

### Notes
* [All commits](https://github.com/SonarSource/sonar-csharp/compare/6.4.0.3347...6.4.1.3596)


## [6.4](https://github.com/SonarSource/sonar-csharp/releases/tag/6.4.0.3347)

### Bug
* [#631](https://github.com/SonarSource/sonar-csharp/issues/631) - Fix S2325: Should raise issues for methods and properties with SuppressMessage attribute
* [#671](https://github.com/SonarSource/sonar-csharp/issues/671) - Update of plugin to v6.3(build 2862) doesn't report any issues in code.
* [#690](https://github.com/SonarSource/sonar-csharp/issues/690) - Fix S3881: Rule should not fail with 'Syntax node is not within syntax tree'
* [#721](https://github.com/SonarSource/sonar-csharp/issues/721) - Fix link to GitHub page on sonarlint-website
* [#725](https://github.com/SonarSource/sonar-csharp/issues/725) - Fix S1121: NullReferenceException when while loop with assignment expression is within a for loop with no condition
* [#726](https://github.com/SonarSource/sonar-csharp/issues/726) - Fix S2372: do not throw ArgumentNullException on nlog
* [#727](https://github.com/SonarSource/sonar-csharp/issues/727) - Fix CognitiveComplexity to not throw an exception on Lucene.Net
* [#728](https://github.com/SonarSource/sonar-csharp/issues/728) - Fix S3877: do not throw ArgumentNullException on ravendb

### False Positive
* [#538](https://github.com/SonarSource/sonar-csharp/issues/538) - Fix S2259: "Null pointer dereference" false positive when fields qualified with 'this' are assigned and then read
* [#596](https://github.com/SonarSource/sonar-csharp/issues/596) - Fix S3242: Rule should not trigger on interface methods
* [#655](https://github.com/SonarSource/sonar-csharp/issues/655) - Update S3881: Correct documentation and add more unit tests
* [#674](https://github.com/SonarSource/sonar-csharp/issues/674) - Rules S3897 and S4035 might be in contradiction
* [#682](https://github.com/SonarSource/sonar-csharp/issues/682) - Fix S1751: Rule shouldn't raise on exceptions within while loops
* [#692](https://github.com/SonarSource/sonar-csharp/issues/692) - Fix S3263: Rule should not raise when constant field is used in initialization
* [#694](https://github.com/SonarSource/sonar-csharp/issues/694) - Fix S2372: Allow some exceptions being raised in properties as per CA1065
* [#701](https://github.com/SonarSource/sonar-csharp/issues/701) - Fix S3897: Rule should not suggest to implement IEquatable<T> when Equals is not public

### Improvement
* [#601](https://github.com/SonarSource/sonar-csharp/issues/601) - Update S110: Update the rule behavior to not count classes defined into another root namespace
* [#623](https://github.com/SonarSource/sonar-csharp/issues/623) - Update S2068: Credentials should not be hard-coded
* [#684](https://github.com/SonarSource/sonar-csharp/issues/684) - Update SonarLint website to mention GitHub repository as a place to file issues

### New Feature
* [#335](https://github.com/SonarSource/sonar-csharp/issues/335) - Update plugin to support NUnit 3 test results
* [#581](https://github.com/SonarSource/sonar-csharp/issues/581) - Rule S2183: Ints and longs should not be shifted by zero or more than their number of bits-1
* [#582](https://github.com/SonarSource/sonar-csharp/issues/582) - Rule S1168: Empty arrays and collections should be returned instead of null
* [#610](https://github.com/SonarSource/sonar-csharp/issues/610) - Rule S4144: Methods should not have identical implementations
* [#611](https://github.com/SonarSource/sonar-csharp/issues/611) - Rule S4142: Duplicate values should not be passed as arguments
* [#626](https://github.com/SonarSource/sonar-csharp/issues/626) - Rule S3433: Test methods should have correct signature
* [#636](https://github.com/SonarSource/sonar-csharp/issues/636) - Rule S4158: Empty collections should not be accessed or iterated
* [#662](https://github.com/SonarSource/sonar-csharp/issues/662) - Rule S1607: Tests should not be ignored
* [#663](https://github.com/SonarSource/sonar-csharp/issues/663) - Rule S3415: Assertion arguments should be passed in the correct order
* [#664](https://github.com/SonarSource/sonar-csharp/issues/664) - Rule S2701: Literal boolean values should not be used in assertions
* [#665](https://github.com/SonarSource/sonar-csharp/issues/665) - Rule S2699: Tests should include assertions
* [#666](https://github.com/SonarSource/sonar-csharp/issues/666) - Rule S3431: 'ExpectedExceptionAttribute' should not be used
* [#667](https://github.com/SonarSource/sonar-csharp/issues/667) - Rule S2187: Test class should contain test methods

### Notes
* [All commits](https://github.com/SonarSource/sonar-csharp/compare/6.3.0.2862...6.4.0.3347)


## [6.3](https://github.com/SonarSource/sonar-csharp/releases/tag/6.3.0.2862)

### Bug
* [#416](https://github.com/SonarSource/sonar-csharp/issues/416) - Fix S3881: Rule should not fail on partial classes
* [#526](https://github.com/SonarSource/sonar-csharp/issues/526) - Fix S101: "Class Name" infinite loop when class name contains non-Latin Characters
* [#576](https://github.com/SonarSource/sonar-csharp/issues/576) - Fix S100: infinite loop when class name contains non-Latin Characters
* [#587](https://github.com/SonarSource/sonar-csharp/issues/587) - Fix S3966: Rule should not throw cast exception
* [#589](https://github.com/SonarSource/sonar-csharp/issues/589) - Fix S3925: Call to 'base.GetObjectData' is not properly detected

### False Positive
* [#174](https://github.com/SonarSource/sonar-csharp/issues/174) - Update S101: A special case should be made for two-letter acronyms in which both letters are capitalized
* [#547](https://github.com/SonarSource/sonar-csharp/issues/547) - Fix S3346: Rule raises FP on peach
* [#571](https://github.com/SonarSource/sonar-csharp/issues/571) - Fix S2275: Rule should not trigger on space before alignment
* [#590](https://github.com/SonarSource/sonar-csharp/issues/590) - Fix S1751: Rule should not raise on "retry on exception" pattern

### Improvement
* [#320](https://github.com/SonarSource/sonar-csharp/issues/320) - Update S100: Support custom dictionaries for adding names that will not raise issues
* [#565](https://github.com/SonarSource/sonar-csharp/issues/565) - POC for analyzer running on Linux
* [#566](https://github.com/SonarSource/sonar-csharp/issues/566) - Automate website release process
* [#591](https://github.com/SonarSource/sonar-csharp/issues/591) - Update S3966: Rule should detect multiple call of Dispose on this
* [#600](https://github.com/SonarSource/sonar-csharp/issues/600) - Update S2275: Rule should detect too big values for ArgumentIndex and Alignment
* [#607](https://github.com/SonarSource/sonar-csharp/issues/607) - Update S3962: Ignore public static readonly fields
* [#608](https://github.com/SonarSource/sonar-csharp/issues/608) - Update S112: Report if exceptions are actually thrown, not just created

### New Feature
* [#254](https://github.com/SonarSource/sonar-csharp/issues/254) - Rule S3985: Unused private classes should be removed
* [#541](https://github.com/SonarSource/sonar-csharp/issues/541) - Rule S2114: Collections should not be passed as arguments to their own methods
* [#570](https://github.com/SonarSource/sonar-csharp/issues/570) - Symbolic Execution Engine supports multiple constraints per Symbolic Value
* [#583](https://github.com/SonarSource/sonar-csharp/issues/583) - Rule S1696: 'NullReferenceException' should not be caught
* [#584](https://github.com/SonarSource/sonar-csharp/issues/584) - Rule S3693: Exception constructors should not throw exceptions
* [#585](https://github.com/SonarSource/sonar-csharp/issues/585) - Rule S3717: Track use of "NotImplementedException"

### Notes
* [All commits](https://github.com/SonarSource/sonar-csharp/compare/6.2.0.2536...6.3.0.2862)


## [6.2](https://github.com/SonarSource/sonar-csharp/releases/tag/6.2.0.2536)

### False Positive
* [#504](https://github.com/SonarSource/sonar-csharp/issues/504) - Fix S2386: Rule should not report when readonly field is initialized with null
* [#530](https://github.com/SonarSource/sonar-csharp/issues/530) - Fix S3881: "Implement IDisposable correctly" should allow calling GC.SuppressFinalize(this) even when there is no destructor
* [#550](https://github.com/SonarSource/sonar-csharp/issues/550) - Fix S3908: Rule should not report issues on classes that implement interfaces

### Improvement
* [#516](https://github.com/SonarSource/sonar-csharp/issues/516) - Update S927: C#: parameter names should match base declaration and other partial definitions
* [#519](https://github.com/SonarSource/sonar-csharp/issues/519) - Rule S110: "filteredClasses" documentation should describe which delimiter to use or provide specific examples

### New Feature
* [#164](https://github.com/SonarSource/sonar-csharp/issues/164) - Rule S3900: Arguments of public methods should be validated against null
* [#209](https://github.com/SonarSource/sonar-csharp/issues/209) - Rule S3966: Objects should not be disposed more than once
* [#497](https://github.com/SonarSource/sonar-csharp/issues/497) - Rule S4070: Non-flags enums should not be marked with "FlagsAttribute"
* [#498](https://github.com/SonarSource/sonar-csharp/issues/498) - Rule S4049: Properties should be preferred
* [#499](https://github.com/SonarSource/sonar-csharp/issues/499) - Rule S4050: Operators should be overloaded consistently
* [#500](https://github.com/SonarSource/sonar-csharp/issues/500) - Rule S4052: Types should not extend outdated base types
* [#501](https://github.com/SonarSource/sonar-csharp/issues/501) - Rule S4060: Non-abstract attributes should be sealed
* [#502](https://github.com/SonarSource/sonar-csharp/issues/502) - Rule S4061: "params" should be use instead of "varargs"
* [#503](https://github.com/SonarSource/sonar-csharp/issues/503) - Rule S4047: Generics should be used when appropriate
* [#506](https://github.com/SonarSource/sonar-csharp/issues/506) - Rule S3956: "Generic.List" instances should not be part of public APIs
* [#507](https://github.com/SonarSource/sonar-csharp/issues/507) - Rule S1075: URIs should not be hardcoded
* [#508](https://github.com/SonarSource/sonar-csharp/issues/508) - Rule S4069: Operator overloads should have named alternatives
* [#509](https://github.com/SonarSource/sonar-csharp/issues/509) - Rule S4058: Overloads with a "StringComparison" parameter should be used
* [#510](https://github.com/SonarSource/sonar-csharp/issues/510) - Rule S4059: Property names should not match get methods
* [#511](https://github.com/SonarSource/sonar-csharp/issues/511) - Rule S4057: Locales should be set for data types
* [#512](https://github.com/SonarSource/sonar-csharp/issues/512) - Rule S4055: Literals should not be passed as localized parameters
* [#513](https://github.com/SonarSource/sonar-csharp/issues/513) - Rule S4056: Overloads with a "CultureInfo" or an "IFormatProvider" parameter should be used
* [#514](https://github.com/SonarSource/sonar-csharp/issues/514) - Rule S3242: Method parameters should be declared with base types
* [#515](https://github.com/SonarSource/sonar-csharp/issues/515) - Rule S1200: Classes should not be coupled to too many other classes (Single Responsibility Principle)
* [#517](https://github.com/SonarSource/sonar-csharp/issues/517) - Rule S3649: User-provided values should be sanitized before use in SQL statements

### Notes
* [All commits](https://github.com/SonarSource/sonar-csharp/compare/6.1.0.2359...6.2.0.2536)


## [6.1](https://github.com/SonarSource/sonar-csharp/releases/tag/6.1.0.2359)

### Bug
* [#298](https://github.com/SonarSource/sonar-csharp/issues/298) - Potential problem in CFG with continue statement in and do-while loop
* [#406](https://github.com/SonarSource/sonar-csharp/issues/406) - SonarC# fails when parsing xUnit test results with empty <assembly> tags
* [#462](https://github.com/SonarSource/sonar-csharp/issues/462) - Fix S3897: Classes that provide "Equals(<T>)" should implement "IEquatable<T>"
* [#489](https://github.com/SonarSource/sonar-csharp/issues/489) - Provide better error message when using old version of Scanner for MSBuild

### False Positive
* [#152](https://github.com/SonarSource/sonar-csharp/issues/152) - Fix S2583: Rule should consider Nullable<bool> values
* [#376](https://github.com/SonarSource/sonar-csharp/issues/376) - Fix S3897: Correctly recognize IEquatable<T> implementations from base classes
* [#424](https://github.com/SonarSource/sonar-csharp/issues/424) - Update S3427: "Method overloads" should not raise when methods differ by generic type arguments
* [#430](https://github.com/SonarSource/sonar-csharp/issues/430) - Update S3237: Add exception for empty properties from interfaces
* [#431](https://github.com/SonarSource/sonar-csharp/issues/431) - Fix S1944: don't raise issue on "as"
* [#472](https://github.com/SonarSource/sonar-csharp/issues/472) - Fix S2386: Do not raise if the readonly field is initialized with a known immutable collection
* [#473](https://github.com/SonarSource/sonar-csharp/issues/473) - Fix S4015: Rule should not raise on 'new' or 'override' members
* [#493](https://github.com/SonarSource/sonar-csharp/issues/493) - Fix S2259: "Null pointer dereference" raises FP when a variable is initialized in catch(Exception) block

### Improvement
* [#154](https://github.com/SonarSource/sonar-csharp/issues/154) - Support many coverage reports
* [#235](https://github.com/SonarSource/sonar-csharp/issues/235) - Update S1764: Comparing the same object with Object.Equals() method
* [#322](https://github.com/SonarSource/sonar-csharp/issues/322) - Fail fast ITs if not on Windows
* [#380](https://github.com/SonarSource/sonar-csharp/issues/380) - [Technical] Improve build scripts to allow building SonarC# on a dev box
* [#388](https://github.com/SonarSource/sonar-csharp/issues/388) - Update S1479: "Switch with too many cases" should ignore empty, fall-through cases
* [#420](https://github.com/SonarSource/sonar-csharp/issues/420) - S3897 is an unsafe suggestion in my opinion
* [#422](https://github.com/SonarSource/sonar-csharp/issues/422) - default() should have a null or not null constraint on it
* [#455](https://github.com/SonarSource/sonar-csharp/issues/455) - Add blocks for try-finally statements into CFG, ignoring catch clauses (happy path)
* [#467](https://github.com/SonarSource/sonar-csharp/issues/467) - Add blocks in CFG for catch clauses and connect try-finally
* [#468](https://github.com/SonarSource/sonar-csharp/issues/468) - Support nested try-catch blocks
* [#486](https://github.com/SonarSource/sonar-csharp/issues/486) - Fix spelling errors

### New Feature
* [#215](https://github.com/SonarSource/sonar-csharp/issues/215) - Rule S3972: Conditionals should start on new lines
* [#433](https://github.com/SonarSource/sonar-csharp/issues/433) - Rule S2221: "Exception" should not be caught when not required by called methods
* [#434](https://github.com/SonarSource/sonar-csharp/issues/434) - Rule S4039: Interface methods should be callable by derived types
* [#436](https://github.com/SonarSource/sonar-csharp/issues/436) - Rule S4041: Type names should not match namespaces
* [#437](https://github.com/SonarSource/sonar-csharp/issues/437) - Rule S3927: Serialization event handlers should be implemented correctly
* [#461](https://github.com/SonarSource/sonar-csharp/issues/461) - Rule S4035: Classes implementing "IEquatable<T>" should be sealed

### Notes
* [All commits](https://github.com/SonarSource/sonar-csharp/compare/6.0.0.2033...6.1.0.2359)


## [6.0](https://github.com/SonarSource/sonar-csharp/releases/tag/6.0.0.2033)

### Bug
* [#386](https://github.com/SonarSource/sonar-csharp/issues/386) - Fix S1939: Extends and implements list entries should not be redundant
* [#423](https://github.com/SonarSource/sonar-csharp/issues/423) - Fix Symbolic Execution: Invalid nameof() should be properly handled

### False Negative
* [#317](https://github.com/SonarSource/sonar-csharp/issues/317) - Update S2234: Add support for constructors

### False Positive
* [#153](https://github.com/SonarSource/sonar-csharp/issues/153) - Fix S1871: "Branches with same implementation" should ignore single line blocks
* [#297](https://github.com/SonarSource/sonar-csharp/issues/297) - Fix S1940: "Inverted boolean checks" should not suggest inversion for Nullable<T>
* [#343](https://github.com/SonarSource/sonar-csharp/issues/343) - Fix S2681: Fix FP on try-catch block
* [#345](https://github.com/SonarSource/sonar-csharp/issues/345) - Fix S1751: "Unconditional jump statements" should not raise if a conditional continue is detected
* [#347](https://github.com/SonarSource/sonar-csharp/issues/347) - Fix S2589: "Gratuitous expressions" should not raise if expression is the condition of a loop and break is detected
* [#366](https://github.com/SonarSource/sonar-csharp/issues/366) - Fix S3963: Should not raise issue when the static constructor does not assign any fields

### Improvement
* [#220](https://github.com/SonarSource/sonar-csharp/issues/220) - Process coverage reports only once for a solution
* [#346](https://github.com/SonarSource/sonar-csharp/issues/346) - Update S2328: "GetHashCode should not reference mutable fields" should report once per method
* [#348](https://github.com/SonarSource/sonar-csharp/issues/348) - Update S3904: "Assemblies should have version" should be code smell
* [#370](https://github.com/SonarSource/sonar-csharp/issues/370) - Update S2387, Rule S4025: "Child class fields shadowing parent fields" should ignore "static" fields, field names that differ by case are handled by S4025
* [#374](https://github.com/SonarSource/sonar-csharp/issues/374) - Drop MSBuild 12 support
* [#394](https://github.com/SonarSource/sonar-csharp/issues/394) - Decrease plugin size
* [#438](https://github.com/SonarSource/sonar-csharp/issues/438) - Update S1210: "Override Equals when implementing IComparable" should be code smell
* [#439](https://github.com/SonarSource/sonar-csharp/issues/439) - Update S2743: "Static fields in generic types" should be code smell
* [#440](https://github.com/SonarSource/sonar-csharp/issues/440) - Update S3220: "Method calls should not resolve ambiguously" should be code smell
* [#441](https://github.com/SonarSource/sonar-csharp/issues/441) - Update S3881: "IDisposable should be implemented correctly" should be code smell
* [#442](https://github.com/SonarSource/sonar-csharp/issues/442) - Update S3885: "Assembly.Load should be used" should be code smell
* [#443](https://github.com/SonarSource/sonar-csharp/issues/443) - Update S3904: "Assemblies should have version information" should be code smell
* [#444](https://github.com/SonarSource/sonar-csharp/issues/444) - Update S3925: "ISerializable should be implemented correctly" should be code smell

### New Feature
* [#333](https://github.com/SonarSource/sonar-csharp/issues/333) - Rule S2068: Credentials should not be hard-coded
* [#350](https://github.com/SonarSource/sonar-csharp/issues/350) - Rule S4015: Inherited member visibility should not be decreased
* [#351](https://github.com/SonarSource/sonar-csharp/issues/351) - Rule S4016: Enumeration members should not be named "Reserved"
* [#352](https://github.com/SonarSource/sonar-csharp/issues/352) - Rule S4017: Method signatures should not contain nested generic types
* [#353](https://github.com/SonarSource/sonar-csharp/issues/353) - Rule S4018: Generic methods should provide type parameters
* [#354](https://github.com/SonarSource/sonar-csharp/issues/354) - Rule S4019: Base class methods should not be hidden
* [#356](https://github.com/SonarSource/sonar-csharp/issues/356) - Rule S4022: Enums storage should be Int32
* [#357](https://github.com/SonarSource/sonar-csharp/issues/357) - Rule S4023: Interfaces should not be empty
* [#359](https://github.com/SonarSource/sonar-csharp/issues/359) - Rule S3906: Declare event handlers correctly
* [#360](https://github.com/SonarSource/sonar-csharp/issues/360) - Rule S3908: Generic event handlers should be used
* [#361](https://github.com/SonarSource/sonar-csharp/issues/361) - Rule S3909: Collections should implement the generic interface
* [#365](https://github.com/SonarSource/sonar-csharp/issues/365) - Rule S1123: "Obsolete" attributes should include explanations
* [#383](https://github.com/SonarSource/sonar-csharp/issues/383) - Rule S4026: Assemblies should be marked with NeutralResourcesLanguageAttribute
* [#384](https://github.com/SonarSource/sonar-csharp/issues/384) - Rule S4027: Exceptions should provide standard constructor

### Notes
* [All commits](https://github.com/SonarSource/sonar-csharp/compare/5.11.0.1761...6.0.0.2033)


## [5.11](https://github.com/SonarSource/sonar-csharp/releases/tag/5.11.0.1761)

### Bug
* [#302](https://github.com/SonarSource/sonar-csharp/issues/302) - Fix S2275: InvalidCastExteption when analyzing single argument method
* [#362](https://github.com/SonarSource/sonar-csharp/issues/362) - Fix Sensors: Unit/Integration test results are skipped when module doesn't contain cs file

### False Positive
* [#167](https://github.com/SonarSource/sonar-csharp/issues/167) - Fix S1121: Assignments should not be made from within sub-expressions' should ignore chained assignments

### Improvement
* [#277](https://github.com/SonarSource/sonar-csharp/issues/277) - Update S3881: should require a call to base.Dispose(bool)
* [#296](https://github.com/SonarSource/sonar-csharp/issues/296) - Update S3449: replace XML-encoded characters in description
* [#309](https://github.com/SonarSource/sonar-csharp/issues/309) - Update S3776: Disable by default because it is a parametrized rule
* [#325](https://github.com/SonarSource/sonar-csharp/issues/325) - Update RSPEC metadata before releasing 5.11
* [#328](https://github.com/SonarSource/sonar-csharp/issues/328) - Deprecate S1145
* [#330](https://github.com/SonarSource/sonar-csharp/issues/330) - Update S2589 and S2583: raise issues for "true" and "false" literals
* [#336](https://github.com/SonarSource/sonar-csharp/issues/336) - Use the Category property from RSPEC to determine the rule type
* [#338](https://github.com/SonarSource/sonar-csharp/issues/338) - Update S112: "Do not throw general exceptions" should be Code Smell
* [#339](https://github.com/SonarSource/sonar-csharp/issues/339) - Update S1854: Dead Stores should be code smell
* [#340](https://github.com/SonarSource/sonar-csharp/issues/340) - Update S2372: "Do not throw exceptions from property getters" should be Code Smell

### New Feature
* [#173](https://github.com/SonarSource/sonar-csharp/issues/173) - Rule S3346: Expressions used in "Debug.Assert" should not produce side effects
* [#250](https://github.com/SonarSource/sonar-csharp/issues/250) - Rule S3981: Collection sizes and array lengths should not be tested against ">= 0"
* [#251](https://github.com/SonarSource/sonar-csharp/issues/251) - Rule S3984: Exception should not be created without being thrown
* [#265](https://github.com/SonarSource/sonar-csharp/issues/265) - Rule S3990: Assemblies should be marked as CLS compliant
* [#266](https://github.com/SonarSource/sonar-csharp/issues/266) - Rule S3992: Assemblies should explicitly specify COM visibility
* [#267](https://github.com/SonarSource/sonar-csharp/issues/267) - Rule S3993: Custom attributes should be marked with "System.AttributeUsageAttribute"
* [#268](https://github.com/SonarSource/sonar-csharp/issues/268) - Rule S3994: URI Parameters should not be strings
* [#269](https://github.com/SonarSource/sonar-csharp/issues/269) - Rule S3995: URI return values should not be strings
* [#270](https://github.com/SonarSource/sonar-csharp/issues/270) - Rule S3996: URI properties should not be strings
* [#271](https://github.com/SonarSource/sonar-csharp/issues/271) - Rule S3997: String URI overloads should call "System.Uri" overloads
* [#274](https://github.com/SonarSource/sonar-csharp/issues/274) - Rule S3998: Threads should not lock on objects with weak identity
* [#275](https://github.com/SonarSource/sonar-csharp/issues/275) - Rule S4000: Pointers to unmanaged memory should not be visible
* [#279](https://github.com/SonarSource/sonar-csharp/issues/279) - Rule S4002: Disposable types should declare finalizer
* [#280](https://github.com/SonarSource/sonar-csharp/issues/280) - Rule S4004: Collection properties should be readonly
* [#281](https://github.com/SonarSource/sonar-csharp/issues/281) - Rule S4005: "System.Uri" argument should be passed instead of string

### Notes
* [All commits](https://github.com/SonarSource/sonar-csharp/compare/5.10.1.1411...5.11.0.1761)
