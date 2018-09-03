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
* [All commits](https://github.com/SonarSource/sonar-csharp/compare/7.5.0.6605...HEAD)



## [7.5](https://github.com/SonarSource/sonar-csharp/releases/tag/7.5.0.6605)

### New Rules
* [1814](https://github.com/SonarSource/sonar-csharp/issues/1814) - Rule S106: Standard outputs should not be used directly to log anything

### Improvements
* [1812](https://github.com/SonarSource/sonar-csharp/issues/1812) - Deprecate S2228 in favor of S106
* [1798](https://github.com/SonarSource/sonar-csharp/issues/1798) - Update S1854: Dead stores should allow initialization with default()
* [1780](https://github.com/SonarSource/sonar-csharp/issues/1780) - Improve debug logging when importing code coverage and test coverage
* [1775](https://github.com/SonarSource/sonar-csharp/issues/1775) - Add support for switch statements pattern matching in CFG
* [1774](https://github.com/SonarSource/sonar-csharp/issues/1774) - Update S3253: Rule should handle ExpressionBody
* [1773](https://github.com/SonarSource/sonar-csharp/issues/1773) - Update S3626: Rule should handle ExpressionBody
* [1767](https://github.com/SonarSource/sonar-csharp/issues/1767) - Update S1172: Rule should handle ExpressionBody
* [1764](https://github.com/SonarSource/sonar-csharp/issues/1764) - Update S1185: Rule should handle ExpressionBody
* [1763](https://github.com/SonarSource/sonar-csharp/issues/1763) - Update S3604: Rule should handle ExpressionBody
* [1761](https://github.com/SonarSource/sonar-csharp/issues/1761) - Update S3052: Rule should handle ExpressionBody
* [1758](https://github.com/SonarSource/sonar-csharp/issues/1758) - Update S3963: Rule should handle ExpressionBody
* [1754](https://github.com/SonarSource/sonar-csharp/issues/1754) - Update S2326: Rule should handle ExpressionBody
* [1752](https://github.com/SonarSource/sonar-csharp/issues/1752) - Update S2292: Rule should handle ExpressionBody
* [1751](https://github.com/SonarSource/sonar-csharp/issues/1751) - Update metrics to handle ExpressionBody
* [1746](https://github.com/SonarSource/sonar-csharp/issues/1746) - Update S1144: Rule should handle ExpressionBody
* [1743](https://github.com/SonarSource/sonar-csharp/issues/1743) - Update Symbolic Execution Engine: Run rules on ExpressionBody
* [1739](https://github.com/SonarSource/sonar-csharp/issues/1739) - Update S2325: Rule should handle ExpressionBody
* [1737](https://github.com/SonarSource/sonar-csharp/issues/1737) - Update S3880: Rule should handle ExpressionBody
* [1734](https://github.com/SonarSource/sonar-csharp/issues/1734) - Update S2365: Rule should handle ExpressionBody
* [1733](https://github.com/SonarSource/sonar-csharp/issues/1733) - Update S138: Rule should handle ExpressionBody
* [1728](https://github.com/SonarSource/sonar-csharp/issues/1728) - Update S3881: Rule should handle ExpressionBody
* [1727](https://github.com/SonarSource/sonar-csharp/issues/1727) - Update S4005: Rule should handle ExpressionBody
* [1726](https://github.com/SonarSource/sonar-csharp/issues/1726) - Update S3997: Rule should handle ExpressionBody

### Bug Fixes
* [1824](https://github.com/SonarSource/sonar-csharp/issues/1824) - SonarC# NPE with SonarLint for VS connected mode
* [1801](https://github.com/SonarSource/sonar-csharp/issues/1801) - Create BrancBlock for "case null" sections to avoid exceptions in the exploded graph
* [1791](https://github.com/SonarSource/sonar-csharp/issues/1791) - Module and file level issues are not de-duplicated correctly
* [1789](https://github.com/SonarSource/sonar-csharp/issues/1789) - Module levels issues are not reported correctly
* [1799](https://github.com/SonarSource/sonar-csharp/issues/1799) - Fix S1854: False Positive when variable initialized with -1 or +1


## [7.4](https://github.com/SonarSource/sonar-csharp/releases/tag/7.4.0.6452)

### Improvements
*   [#1195](https://github.com/SonarSource/sonar-csharp/issues/1195) - Fix S1144:   Issues is raised while method is being used (DebuggerDisplayAttribute)
*   [#1225](https://github.com/SonarSource/sonar-csharp/issues/1225) - Fix S1144:   False Positive on Inner Classes
*   [#1398](https://github.com/SonarSource/sonar-csharp/issues/1398) - S1144   False positive for protected ctor
*   [#1434](https://github.com/SonarSource/sonar-csharp/issues/1434) - Rule   S4150: False positive on field used in switch block
*   [#1448](https://github.com/SonarSource/sonar-csharp/issues/1448) - S1450:   false negative for fields used in expression bodies
*   [#1449](https://github.com/SonarSource/sonar-csharp/issues/1449) - S1450 not   appearing in VS2015 IDE
*   [#1460](https://github.com/SonarSource/sonar-csharp/issues/1460) - Update   S3881: Rule should allow abstract IDisposable implementations
*   [#1486](https://github.com/SonarSource/sonar-csharp/issues/1486) - Fix S2187:   does not report for test classes with only assembly-related attributes
*   [#1491](https://github.com/SonarSource/sonar-csharp/issues/1491) - Fix S3887:   Rule should not report when field is readonly and initialized with immutable   type in ctor
*   [#1498](https://github.com/SonarSource/sonar-csharp/issues/1498) - Test   method detection code is not consistent across rules
*   [#1529](https://github.com/SonarSource/sonar-csharp/issues/1529) -   "Fields should not have public accessibility" should not run   against structs
*   [#1536](https://github.com/SonarSource/sonar-csharp/issues/1536) - S1450   "Private fields only used as local variables in methods should become   local variables" not triggered by rule sample
*   [#1537](https://github.com/SonarSource/sonar-csharp/issues/1537) - Fix S3242:   Rule should not suggest base type for virtual methods
*   [#1543](https://github.com/SonarSource/sonar-csharp/issues/1543) - S3400:   Don't raise issue for virtual methods
*   [#1553](https://github.com/SonarSource/sonar-csharp/issues/1553) - Fix S4226:   False positive for interfaces
*   [#1562](https://github.com/SonarSource/sonar-csharp/issues/1562) - Populate   Security Standards data for Security Hotspots and Vulnerabilities rules
*   [#1563](https://github.com/SonarSource/sonar-csharp/issues/1563) - Change   "Message" of Security Hotspot issues
*   [#158](https://github.com/SonarSource/sonar-csharp/issues/158) - Fix S1450:   Rule should not raise an issue when methods call each other
*   [#1586](https://github.com/SonarSource/sonar-csharp/issues/1586) - Fix S1075:   Rule should not report on virtual path for asp.net
*   [#1588](https://github.com/SonarSource/sonar-csharp/issues/1588) - Adjust the   "message" of S2245 because RSPEC-2245 is now a Security   Hotspot
*   [#159](https://github.com/SonarSource/sonar-csharp/issues/159) - Fix S1144:   Unused private members should not report false positives with Unity classes
*   [#1593](https://github.com/SonarSource/sonar-csharp/issues/1593) - Fix S4049:   GetEnumerator should be white-listed
*   [#1596](https://github.com/SonarSource/sonar-csharp/issues/1596) - Stop   feeding the comment_lines_data metric
*   [#1607](https://github.com/SonarSource/sonar-csharp/issues/1607) - Fix S1450:   Implement robust detection whether a local field could be converted to a   local variable
*   [#1608](https://github.com/SonarSource/sonar-csharp/issues/1608) - Update   S2551: rule should be enabled by default (Sonar way)
*   [#1609](https://github.com/SonarSource/sonar-csharp/issues/1609) - Update   S3963: rule should be enabled by default (Sonar way)
*   [#1610](https://github.com/SonarSource/sonar-csharp/issues/1610) - Fix S3242:   Rule should not suggest base type resulting in inconsistent accessibility   (bis)
*   [#1623](https://github.com/SonarSource/sonar-csharp/issues/1623) - Update   S1144: Develop robust mechanism to detect when a class member is unused
*   [#1638](https://github.com/SonarSource/sonar-csharp/issues/1638) - Add a   warning to notify user that no coverage report file was found for the given   pattern
*   [#1643](https://github.com/SonarSource/sonar-csharp/issues/1643) - Fix S4143:   False Positive when variable is reassigned
*   [#1644](https://github.com/SonarSource/sonar-csharp/issues/1644) - Fix S4261:   False positive on async Main
*   [#1649](https://github.com/SonarSource/sonar-csharp/issues/1649) - Fix the   executable lines of code count to ignore attributes
*   [#1658](https://github.com/SonarSource/sonar-csharp/issues/1658) - Update   S4261: Default severity should be Code Smell
*   [#1660](https://github.com/SonarSource/sonar-csharp/issues/1660) - Update   S4524: Rule should be in the default quality profile (SonarWay)
*   [#1661](https://github.com/SonarSource/sonar-csharp/issues/1661) - Update   S2255: Rule should be in the default quality profile (SonarWay)
*   [#1662](https://github.com/SonarSource/sonar-csharp/issues/1662) - Update   S2245: Rule should be in the default quality profile (SonarWay)
*   [#1667](https://github.com/SonarSource/sonar-csharp/issues/1667) - Update   S4524: metadata needs to be updated
*   [#1669](https://github.com/SonarSource/sonar-csharp/issues/1669) - Fix S1226:   rule doesn't detect correctly that param was read before being assigned
*   [#1670](https://github.com/SonarSource/sonar-csharp/issues/1670) - Deprecate   S2758 in favor of S3923
*   [#1673](https://github.com/SonarSource/sonar-csharp/issues/1673) - Update   S1764: update rule metadata
*   [#1675](https://github.com/SonarSource/sonar-csharp/issues/1675) - Update   S2259: documentation should include ability to use ValidatedNotNull   attribute
*   [#1686](https://github.com/SonarSource/sonar-csharp/issues/1686) - Legacy   Xunit test projects are not recognized as test projects
*   [#1687](https://github.com/SonarSource/sonar-csharp/issues/1687) - Fix S2699:   handle skipped XUnit Theory tests
*   [#1688](https://github.com/SonarSource/sonar-csharp/issues/1688) - Fix S2699:   handle all test method types for supported test frameworks
*   [#1691](https://github.com/SonarSource/sonar-csharp/issues/1691) - Fix S3433:   handle all test method types for supported test frameworks
*   [#1693](https://github.com/SonarSource/sonar-csharp/issues/1693) - Fix S2386:   Rule should handle effective accessiblity
*   [#1694](https://github.com/SonarSource/sonar-csharp/issues/1694) - Fix S3887:   Rule should handle effective accessiblity
*   [#1695](https://github.com/SonarSource/sonar-csharp/issues/1695) - Fix S3887:   Rule should not raise for uninitialized readonly fields
*   [#1705](https://github.com/SonarSource/sonar-csharp/issues/1705) - Fix S1607:   : handle all test method types for supported test frameworks
*   [#1710](https://github.com/SonarSource/sonar-csharp/issues/1710) - Fix S2699:   handle all test method types for supported test frameworks
*   [#1711](https://github.com/SonarSource/sonar-csharp/issues/1711) - Update   S2971: Rule should not only suggest to remove call to ToList or ToArray
*   [#182](https://github.com/SonarSource/sonar-csharp/issues/182) - Fix 1450:   False positive in VS2017 but not VS2015
*   [#505](https://github.com/SonarSource/sonar-csharp/issues/505) - Fix S2386:   Rule should not report when field is readonly and ...
*   [#904](https://github.com/SonarSource/sonar-csharp/issues/904) - Fix S1144:   rule should not report false positives with constants

### Bug Fixes
*   [#1446](https://github.com/SonarSource/sonar-csharp/issues/1446) - Exclusions   from executable lines of code are not handled correctly
* [#1636](https://github.com/SonarSource/sonar-csharp/issues/1636) - Fix S3963:   Rule should not raise a NullPointerException for expression body
* [#1657](https://github.com/SonarSource/sonar-csharp/issues/1657) - Compute   metrics only on source files with .cs and .csx file extensions
* [#1663](https://github.com/SonarSource/sonar-csharp/issues/1663) - Fix S4433:   Remediation cost should be constant


## [7.3.1](https://github.com/SonarSource/sonar-csharp/releases/tag/7.3.1.5982)

### Improvements
* [1540](https://github.com/SonarSource/sonar-csharp/issues/1540) - Rule S1313: do not report issue for loopback address

### Bug Fixes
* [1590](https://github.com/SonarSource/sonar-csharp/issues/1590) - SonarC# fails when parsing xUnit test results with no tests for a test assembly


## [7.3](https://github.com/SonarSource/sonar-csharp/releases/tag/7.3.0.5690)

### Bug Fixes
* [1438](https://github.com/SonarSource/sonar-csharp/issues/1438) - Fix string formats in the csharp plugin

### False Positive
* [1493](https://github.com/SonarSource/sonar-csharp/issues/1493) - Fix S107: Do not raise for P/Invoke methods (#1459)
* [1464](https://github.com/SonarSource/sonar-csharp/issues/1464) - S4055 should not raise issues for string literal used in the 'message' of Debug.Assert
* [1436](https://github.com/SonarSource/sonar-csharp/issues/1436) - Fix S4586: False positive when returning null from inside Task.Run
* [1419](https://github.com/SonarSource/sonar-csharp/issues/1419) - Fix S1192: False positive for [SuppressMessage()] attribute
* [1417](https://github.com/SonarSource/sonar-csharp/issues/1417) - Fix S2259: False positive on switch statement with conditional access operator
* [1411](https://github.com/SonarSource/sonar-csharp/issues/1411) - Fix S1118: Rule should not raise issue on abstract classes
* [1400](https://github.com/SonarSource/sonar-csharp/issues/1400) - Fix S2583: Rule should not report false positives when combined with async methods
* [1378](https://github.com/SonarSource/sonar-csharp/issues/1378) - Fix S4023: False positive on interfaces that derive from other non-empty interfaces
* [1366](https://github.com/SonarSource/sonar-csharp/issues/1366) - Update S4261: should not raise on async unit test methods
* [1325](https://github.com/SonarSource/sonar-csharp/issues/1325) - Fix S1125: FP with nullable and VS2015/MSBuild 14
* [1324](https://github.com/SonarSource/sonar-csharp/issues/1324) - Rule S2259: False positive on 'try/catch' with exception filter
* [1279](https://github.com/SonarSource/sonar-csharp/issues/1279) - Fix S2228: Rule should ignore calls within DEBUG preprocessor
* [1265](https://github.com/SonarSource/sonar-csharp/issues/1265) - Rule S3626: "Jump statements should not be redundant" issues false positive with try/catch/finally clause
* [1180](https://github.com/SonarSource/sonar-csharp/issues/1180) - Fix S1200: Should not count generic type parameters of extension methods
* [947](https://github.com/SonarSource/sonar-csharp/issues/947) - S2583: False-positive when Monitor.Wait is used
* [621](https://github.com/SonarSource/sonar-csharp/issues/621) - CFG does not correctly support try-catch blocks with unconditional return insude
* [496](https://github.com/SonarSource/sonar-csharp/issues/496) - Fix S2234: Check for parameter types before reporting it as bug

### False Negative
* [247](https://github.com/SonarSource/sonar-csharp/issues/247) - Update S3776: Cogntive Complexity should handle C# 7 inner methods

### New Rules
* [823](https://github.com/SonarSource/sonar-csharp/issues/823) - Rule S4275: Getters and setters should access the right field
* [634](https://github.com/SonarSource/sonar-csharp/issues/634) - Rule S4143: Dictionary values should not be replaced unconditionally
* [257](https://github.com/SonarSource/sonar-csharp/issues/257) - Rule S2327: "try" statements with identical "catch" and/or "finally" blocks should be merged
* [221](https://github.com/SonarSource/sonar-csharp/issues/221) - Rule S3973: A conditionally executed single line should be denoted by indentation


## [7.2](https://github.com/SonarSource/sonar-csharp/releases/tag/7.2.0.5463)

### Removed Rules
* [#1386](https://github.com/SonarSource/sonar-csharp/issues/1386) - Drop S3649: to be replaced by SonarQube functionality

### Improvements
* [#1387](https://github.com/SonarSource/sonar-csharp/issues/1387) - Update S4457: make message clearer

### False Positives
* [#1380](https://github.com/SonarSource/sonar-csharp/issues/1380) - Fix S4457 - false positive with local functions
* [#1404](https://github.com/SonarSource/sonar-csharp/issues/1404) - Update S4457: FP when func/action is async/await

### Bug fixes
* [#1389](https://github.com/SonarSource/sonar-csharp/issues/1389) - AD0001 - NullReferenceException - Object reference not set to an instance of an object


## [7.1](https://github.com/SonarSource/sonar-csharp/releases/tag/7.1.0.5212)

### New features
* [#1057](https://github.com/SonarSource/sonar-csharp/issues/1057) - Rule S4211: Members should not have conflicting transparency annotations
* [#1072](https://github.com/SonarSource/sonar-csharp/issues/1072) - Rule S3400: Methods should not return constants
* [#1079](https://github.com/SonarSource/sonar-csharp/issues/1079) - Rule S3923: All branches in a conditional structure should not have exactly the same implementation
* [#1151](https://github.com/SonarSource/sonar-csharp/issues/1151) - Rule S881: Increment (++) and decrement (--) operators should not be used in a method call or mixed with other operators in an expression
* [#1159](https://github.com/SonarSource/sonar-csharp/issues/1159) - Rule S4462: Calls to 'async' methods should not be blocking
* [#1164](https://github.com/SonarSource/sonar-csharp/issues/1164) - Rule S4457: Parameter validation in "async"/"await" methods should be wrapped
* [#1165](https://github.com/SonarSource/sonar-csharp/issues/1165) - Rule S4456: Parameter validation in yielding methods should be wrapped
* [#1166](https://github.com/SonarSource/sonar-csharp/issues/1166) - Rule S4428: "PartCreationPolicyAttribute" should be used with "ExportAttribute"
* [#1167](https://github.com/SonarSource/sonar-csharp/issues/1167) - Rule S4426: Cryptographic keys should not be too short
* [#1171](https://github.com/SonarSource/sonar-csharp/issues/1171) - Rule S1192: String literals should not be duplicated
* [#1267](https://github.com/SonarSource/sonar-csharp/issues/1267) - Rule S4524: Switch 'default' case should be first or last
* [#1287](https://github.com/SonarSource/sonar-csharp/issues/1287) - Rule S2255: Cookies should not be used to store sensitive information
* [#1290](https://github.com/SonarSource/sonar-csharp/issues/1290) - Rule S3330: "HttpOnly" should be set on cookies
* [#1292](https://github.com/SonarSource/sonar-csharp/issues/1292) - Rule S2092: Cookies should be "secure"
* [#1294](https://github.com/SonarSource/sonar-csharp/issues/1294) - Rule S2245: Pseudorandom number generators (PRNGs) should not be used in secure contexts
* [#1303](https://github.com/SonarSource/sonar-csharp/issues/1303) - Rule S4432: AES encryption algorithm should be used with secured mode
* [#1310](https://github.com/SonarSource/sonar-csharp/issues/1310) - Rule S4433: LDAP connections should be authenticated
* [#1313](https://github.com/SonarSource/sonar-csharp/issues/1313) - Rule S4564: ASP.NET HTTP request validation feature should not be disabled
* [#1329](https://github.com/SonarSource/sonar-csharp/issues/1329) - Rule S4581: "new Guid()" should not be used
* [#1344](https://github.com/SonarSource/sonar-csharp/issues/1344) - Rule S4586: Non-async "Task/Task<T>" method should not return null

### Improvements
* [#1087](https://github.com/SonarSource/sonar-csharp/issues/1087) - Rules should use analysis scope from RSPEC and not be hardcoded
* [#1222](https://github.com/SonarSource/sonar-csharp/issues/1222) - Deprecate rule S4142
* [#1236](https://github.com/SonarSource/sonar-csharp/issues/1236) - Update S4040: Rule should also cover char calls
* [#1256](https://github.com/SonarSource/sonar-csharp/issues/1256) - Update S1313: Rule should not be run against test sources
* [#1258](https://github.com/SonarSource/sonar-csharp/issues/1258) - Update S2228: Rule should not be run on test sources
* [#1260](https://github.com/SonarSource/sonar-csharp/issues/1260) - Update S1104: Rule should not be run on test sources
* [#1262](https://github.com/SonarSource/sonar-csharp/issues/1262) - Fix S3433: Rule should not report on non-public test methods for xUnit
* [#1263](https://github.com/SonarSource/sonar-csharp/issues/1263) - Adjust the behavior of S131 to not raise an issue when "default" is not at the correct position
* [#1282](https://github.com/SonarSource/sonar-csharp/issues/1282) - Update S4061: Rule should be part of SonarWay
* [#1288](https://github.com/SonarSource/sonar-csharp/issues/1288) - Update S1751: Rule should be a BUG
* [#1291](https://github.com/SonarSource/sonar-csharp/issues/1291) - Update S2486: Rule should be a VULNERABILITY
* [#1357](https://github.com/SonarSource/sonar-csharp/issues/1357) - Fix SonarC# category under SonarQube/SonarCloud general settings
* [#1368](https://github.com/SonarSource/sonar-csharp/issues/1368) - Remove deprecated TEST_SUCCESS_DENSITY

### False Positives
* [#825](https://github.com/SonarSource/sonar-csharp/issues/825) - Fix S1905: Rule should not report on Cast<T> over multi-dimensional arrays
* [#1037](https://github.com/SonarSource/sonar-csharp/issues/1037) - Fix S1168: "Return empty collection" should not raise when the method return type is XmlNode
* [#1083](https://github.com/SonarSource/sonar-csharp/issues/1083) - Fix S4056: False positives when calling Convert.ToInt32(double x)
* [#1123](https://github.com/SonarSource/sonar-csharp/issues/1123) - Fix S3264: Rule should not raise when 'BeginInvoke' or 'EndInvoke' are called
* [#1149](https://github.com/SonarSource/sonar-csharp/issues/1149) - Fix S2971: Do not trigger on ToList().AsEnumerable()
* [#1189](https://github.com/SonarSource/sonar-csharp/issues/1189) - Fix S4015: Incorrectly Identifies Descendent Overload as Member Hiding
* [#1210](https://github.com/SonarSource/sonar-csharp/issues/1210) - Fix S2933: issue should not be raised for types marked with [Serializable]
* [#1219](https://github.com/SonarSource/sonar-csharp/issues/1219) - Fix S3264: FP when using add/remove syntax
* [#1223](https://github.com/SonarSource/sonar-csharp/issues/1223) - Fix S2228: Should not raise any issue when in a console application
* [#1224](https://github.com/SonarSource/sonar-csharp/issues/1224) - Fix S1694 - message should suggest protected constructor
* [#1244](https://github.com/SonarSource/sonar-csharp/issues/1244) - Fix S109: Rule should not raise FP when using enums
* [#1283](https://github.com/SonarSource/sonar-csharp/issues/1283) - Fix S4061: Rule should not report when method is interop
* [#1284](https://github.com/SonarSource/sonar-csharp/issues/1284) - Fix S4061: Rule should not raise issue when overriding/implenting interface
* [#1296](https://github.com/SonarSource/sonar-csharp/issues/1296) - Fix S4159: Rule should handle generic interfaces correctly

### Bug fixes
* [#1274](https://github.com/SonarSource/sonar-csharp/issues/1274) - Parameterized rules should be disabled by default


## [7.0.1](https://github.com/SonarSource/sonar-csharp/releases/tag/7.0.1.4822)

### Bug fixes
* Fix default activation of rules


## [7.0](https://github.com/SonarSource/sonar-csharp/releases/tag/7.0.0.4787)

### New Features
* [#780](https://github.com/SonarSource/sonar-csharp/issues/780) - Rule S4261: Methods should be named according to their synchronicity
* [#996](https://github.com/SonarSource/sonar-csharp/issues/996) - Rule S109: Magic numbers should not be used
* [#1131](https://github.com/SonarSource/sonar-csharp/issues/1131) - Rule S1048: Destructors should not throw exceptions
* [#1170](https://github.com/SonarSource/sonar-csharp/issues/1170) - Rule S1151: "switch case" clauses should not have too many lines of code
* [#1172](https://github.com/SonarSource/sonar-csharp/issues/1172) - Rule S1264: A "while" loop should be used instead of a "for" loop
* [#1184](https://github.com/SonarSource/sonar-csharp/issues/1184) - Rule S1821: "switch" statements should not be nested
* [#1212](https://github.com/SonarSource/sonar-csharp/issues/1212) - [New SonarLint] Enable all rules by default

### Improvements
* [#1155](https://github.com/SonarSource/sonar-csharp/issues/1155) - Move analyzers to Roslyn 1.3.2 and drop compatibility with VS 2015 < Update 3

### False Positives
* [#1161](https://github.com/SonarSource/sonar-csharp/issues/1161) - Fix S3168: Rule should not be too strict on first parameter name
* [#1162](https://github.com/SonarSource/sonar-csharp/issues/1162) - Fix S2302: Do not raise issue when C# < 6.0
* [#1175](https://github.com/SonarSource/sonar-csharp/issues/1175) - Fix S1118 (Utility classes should not have public constructors) ignores its exceptions
* [#1196](https://github.com/SonarSource/sonar-csharp/issues/1196) - Fix S2187: Rule should not raise False Positives for class inheriting class with tests

### Notes
* **Breaking change:** [#1155](https://github.com/SonarSource/sonar-csharp/issues/1155) - Move analyzers to Roslyn 1.3.2 and drop compatibility with VS 2015 < Update 3
* **Breaking change:** [#1212](https://github.com/SonarSource/sonar-csharp/issues/1212) - [New SonarLint] Enable all rules by default


## [6.8.2](https://github.com/SonarSource/sonar-csharp/releases/tag/6.8.2.4717)

### Bug
* [#1174](https://github.com/SonarSource/sonar-csharp/issues/1174) - SonarC# should compare paths using the OS case-sensitivity


## [6.8.1](https://github.com/SonarSource/sonar-csharp/releases/tag/6.8.1.4648)

### New Features
* [#1013](https://github.com/SonarSource/sonar-csharp/issues/1013) - Rule S138: Functions should not have too many lines of code
* [#1056](https://github.com/SonarSource/sonar-csharp/issues/1056) - Rule S4277: "Shared" parts should not be created with "new"
* [#1058](https://github.com/SonarSource/sonar-csharp/issues/1058) - Rule S4210: Windows Forms entry points should be marked with STAThread
* [#1059](https://github.com/SonarSource/sonar-csharp/issues/1059) - Rule S4200: Native methods should be wrapped
* [#1060](https://github.com/SonarSource/sonar-csharp/issues/1060) - Rule S4159: Classes should implement their "ExportAttribute" interfaces
* [#1063](https://github.com/SonarSource/sonar-csharp/issues/1063) - Rule S3343: Caller information parameters should come at the end of the parameter list
* [#1064](https://github.com/SonarSource/sonar-csharp/issues/1064) - Rule S3464: Type inheritance should not be recursive
* [#1065](https://github.com/SonarSource/sonar-csharp/issues/1065) - Rule S3353: Unchanged local variables should be "const"
* [#1066](https://github.com/SonarSource/sonar-csharp/issues/1066) - Rule S3060: "is" should not be used with "this"
* [#1067](https://github.com/SonarSource/sonar-csharp/issues/1067) - Rule S3366: "this" should not be exposed from constructors
* [#1070](https://github.com/SonarSource/sonar-csharp/issues/1070) - Rule S4212: Serialization constructors should be secured
* [#1071](https://github.com/SonarSource/sonar-csharp/issues/1071) - Rule S1147: Exit methods should not be called
* [#1073](https://github.com/SonarSource/sonar-csharp/issues/1073) - Rule S3358: Ternary operators should not be nested
* [#1075](https://github.com/SonarSource/sonar-csharp/issues/1075) - Rule S2302: 'nameof' should be used

### Improvements
* [#998](https://github.com/SonarSource/sonar-csharp/issues/998) - Reduce size of jar
* [#1000](https://github.com/SonarSource/sonar-csharp/issues/1000) - Update S3440: "Variables should not be checked before assignment" should not raise on properties
* [#1104](https://github.com/SonarSource/sonar-csharp/issues/1104) - Update S104: Multiline tokens should count as multiple lines
* [#1106](https://github.com/SonarSource/sonar-csharp/issues/1106) - Update plugin properties to declare them as multi-values property
* [#1112](https://github.com/SonarSource/sonar-csharp/issues/1112) - Update S3776: "Cognitive Complexity" should provide values for remediation function and cost
* [#326](https://github.com/SonarSource/sonar-csharp/issues/326) - Fix S2930: Should report on all classes implementing IDisposable
* [#1124](https://github.com/SonarSource/sonar-csharp/issues/1124) - Fix S1607: Should be applied for test classes as well
* [#242](https://github.com/SonarSource/sonar-csharp/issues/242) - Fix S3459: False Positive with interop methods
* [#710](https://github.com/SonarSource/sonar-csharp/issues/710) - Fix S1144: False positive for partial methods/classes
* [#760](https://github.com/SonarSource/sonar-csharp/issues/760) - Fix S3881: "Implement IDisposable correctly" has inconsistent behaviour in partial classes
* [#767](https://github.com/SonarSource/sonar-csharp/issues/767) - Fix S4158: "Empty collections" raises false positive when analyzed method has too many branches
* [#777](https://github.com/SonarSource/sonar-csharp/issues/777) - Fix S2183: Add 2 exceptions to the rule to reduce the noise
* [#789](https://github.com/SonarSource/sonar-csharp/issues/789) - Fix S3254: False positive when used in Expression
* [#1038](https://github.com/SonarSource/sonar-csharp/issues/1038) - False positive in S3966: calls to arg.Dispose() are not handled correctly
* [#1076](https://github.com/SonarSource/sonar-csharp/issues/1076) - Fix S3925: Should not raise on explicit implementation of ISerializable
* [#1126](https://github.com/SonarSource/sonar-csharp/issues/1126) - Fix S1607: Should not raise issues for [Ignore] attribute with a parameter
* [#1138](https://github.com/SonarSource/sonar-csharp/issues/1138) - Fix S1643: check if expression is concatenation

### External Contributor

* [#1129](https://github.com/SonarSource/sonar-csharp/pull/1129) - Thanks @kvpt!


## [6.7.1](https://github.com/SonarSource/sonar-csharp/releases/tag/6.7.1.4347)

### Bug
* [#992](https://github.com/SonarSource/sonar-csharp/issues/992) - Fix S3532: default clauses with comments should not be reported
* [#1002](https://github.com/SonarSource/sonar-csharp/issues/1002) - Fix S4158: "Empty Collections Should Not Be Enumerated" should not throw InvalidOperationException
* [#1003](https://github.com/SonarSource/sonar-csharp/issues/1003) - Fix S3237: Rule should not throw NullReferenceException when using expression body accessor
* [#1006](https://github.com/SonarSource/sonar-csharp/issues/1006) - Fix S101: Rule should not throw IndexOutOfRangeException
* [#1009](https://github.com/SonarSource/sonar-csharp/issues/1009) - Fix S2933: Code Fix should handle regions

### False Positive
* [#888](https://github.com/SonarSource/sonar-csharp/issues/888) - Fix S1144: False positive on async Main
* [#1010](https://github.com/SonarSource/sonar-csharp/issues/1010) - Fix S2187: rule should not warn on MSTest when using DataTestMethodAttribute
* [#1015](https://github.com/SonarSource/sonar-csharp/issues/1015) - Fix S107: "Methods with too many parameters" should not raise on constructors calling base
* [#1024](https://github.com/SonarSource/sonar-csharp/issues/1024) - Fix S1172: Rule should also ignore the new Main syntax

### Improvement
* [#1004](https://github.com/SonarSource/sonar-csharp/issues/1004) - Support syntax highlighting for multi-targetting projects that use conditional compilation
* [#1014](https://github.com/SonarSource/sonar-csharp/issues/1014) - Support symbol highlighting for multi-targetting projects that use conditional compilation



## [6.7](https://github.com/SonarSource/sonar-csharp/releases/tag/6.7.0.4267)

### Bugs
* [#934](https://github.com/SonarSource/sonar-csharp/issues/934) - Don't fail to parse issues on file names with '['

### False Positives
* [#318](https://github.com/SonarSource/sonar-csharp/issues/318) - Fix S101: Rule should not report an issue on auto-generated members
* [#704](https://github.com/SonarSource/sonar-csharp/issues/704) - Fix S3168: Rule should not raise FP with UWP event args
* [#707](https://github.com/SonarSource/sonar-csharp/issues/707) - Fix S4056 and S4058: False positive for string comparisons with StringComparison parameter
* [#733](https://github.com/SonarSource/sonar-csharp/issues/733) - Update S2325: Rule should exempt MVC / Web Api controller public methods
* [#804](https://github.com/SonarSource/sonar-csharp/issues/804) - Update S2583: Should not raise issues for boolean constants and literals in conditions
* [#929](https://github.com/SonarSource/sonar-csharp/issues/929) - Fix S2187: Rule should not raise an issue when class is abstract

### Improvements
* [#396](https://github.com/SonarSource/sonar-csharp/issues/396) - Update S104: "Too many lines in a file" should only count lines of code
* [#656](https://github.com/SonarSource/sonar-csharp/issues/656) - Update sensors to support projects with linked (shared) files
* [#722](https://github.com/SonarSource/sonar-csharp/issues/722) - Update S3415: Support other test frameworks
* [#752](https://github.com/SonarSource/sonar-csharp/issues/752) - Rule S1110: Redundant parenthesis - move the implementation from S3235
* [#786](https://github.com/SonarSource/sonar-csharp/issues/786) - Update S2187: Rule should handle "TheoryAttribute"
* [#836](https://github.com/SonarSource/sonar-csharp/issues/836) - Update S2068: Make the rule parameterised
* [#910](https://github.com/SonarSource/sonar-csharp/issues/910) - Update S2436: Rule should provide another parameter for methods
* [#917](https://github.com/SonarSource/sonar-csharp/issues/917) - Remove support for deprecated 'sonar.cs.msbuild.testProjectPattern'
* [#921](https://github.com/SonarSource/sonar-csharp/issues/921) - Change the log level of the autogenerated file messages to DEBUG
* [#973](https://github.com/SonarSource/sonar-csharp/issues/973) - Update SonarC#: Unit Tests total and skipped numbers are not consistent
* [#979](https://github.com/SonarSource/sonar-csharp/issues/979) - Update S2187: Rule should handle TestCaseSourceAttribute
* [#981](https://github.com/SonarSource/sonar-csharp/issues/981) - Update S1607: Rule should handle 'TestCaseSourceAttribute'
* [#983](https://github.com/SonarSource/sonar-csharp/issues/983) - Update S2699: Rule should handle more test method attributes
* [#989](https://github.com/SonarSource/sonar-csharp/issues/989) - SonarC# should display a warning when coverage report doesn't cover any file imported in SonarQube
* [#991](https://github.com/SonarSource/sonar-csharp/issues/991) - Enable analysis on non-Windows OSes

### New Features
* [#955](https://github.com/SonarSource/sonar-csharp/issues/955) - Rule S113: Files should contain an empty newline at the end

### Notes
* [All commits](https://github.com/SonarSource/sonar-csharp/compare/6.6.0.3969...6.7.0.4267)


## [6.6](https://github.com/SonarSource/sonar-csharp/releases/tag/6.6.0.3969)

### Bug
* [#535](https://github.com/SonarSource/sonar-csharp/issues/535) - CSharp sensor should fail if OS is not windows.
* [#803](https://github.com/SonarSource/sonar-csharp/issues/803) - Fix S3242: Rule should not throw ArgumentException
* [#819](https://github.com/SonarSource/sonar-csharp/issues/819) - Fix S2325: 'Member should be static' throws NullReferenceException
* [#834](https://github.com/SonarSource/sonar-csharp/issues/834) - Fix S3242: Rule throws ArgumentException when 2+ params of method have the same name

### False Positive
* [#640](https://github.com/SonarSource/sonar-csharp/issues/640) - Fix S3242: Rule should not report when more general type doesn't have indexer
* [#680](https://github.com/SonarSource/sonar-csharp/issues/680) - Fix S3242: Rule should not suggest IEnumerable<T> when there are multiple iterations over the collection
* [#705](https://github.com/SonarSource/sonar-csharp/issues/705) - Fix S3242: Should not suggest base class for arguments when method has an event handler signature
* [#782](https://github.com/SonarSource/sonar-csharp/issues/782) - Fix S3242: Do not suggest ICollection<KVP<TKey, TValue>>
* [#795](https://github.com/SonarSource/sonar-csharp/issues/795) - Fix S4004: Rule should not raise a violation when [DataMember] is applied
* [#809](https://github.com/SonarSource/sonar-csharp/issues/809) - Fix S4070: False positive with combined values
* [#813](https://github.com/SonarSource/sonar-csharp/issues/813) - Fix S3242: Do not suggest IReadOnlyCollection<T> interface instead of IReadOnlyList<T> if collection items are accessed by index
* [#828](https://github.com/SonarSource/sonar-csharp/issues/828) - Fix S2758: false positive on conditional operator with interpolated string
* [#863](https://github.com/SonarSource/sonar-csharp/issues/863) - Fix S3242: Rule should not suggest base type resulting in inconsistent accessibility

### Improvement
* [#237](https://github.com/SonarSource/sonar-csharp/issues/237) - Do not import files detected as autogenerated in SonarQube
* [#364](https://github.com/SonarSource/sonar-csharp/issues/364) - Improve test sensors to execute them only when the related property is defined
* [#428](https://github.com/SonarSource/sonar-csharp/issues/428) - Do not skip source files that contains "ExcludeFromCodeCoverage*" attributes
* [#839](https://github.com/SonarSource/sonar-csharp/issues/839) - Update S1226: "Parameter values should not be ignored" should be a bug
* [#840](https://github.com/SonarSource/sonar-csharp/issues/840) - Update S1751: "Unconditional jump statements" should be a code smell
* [#841](https://github.com/SonarSource/sonar-csharp/issues/841) - Update S2234: "Parameters should be passed in correct order" should be a code smell
* [#842](https://github.com/SonarSource/sonar-csharp/issues/842) - Update S2681: "Multiline blocks should be in curly braces" should be a code smell
* [#843](https://github.com/SonarSource/sonar-csharp/issues/843) - Update S3010: "Static fields should not be updated in constructors" should be a code smell
* [#844](https://github.com/SonarSource/sonar-csharp/issues/844) - Update S4158: "Empty collections should not be iterated" should be a bug
* [#851](https://github.com/SonarSource/sonar-csharp/issues/851) - Fix Cognitive Complexity Metric to detect recursion correctly
* [#858](https://github.com/SonarSource/sonar-csharp/issues/858) - Update release notes link in vsix manifest and nuget spec
* [#862](https://github.com/SonarSource/sonar-csharp/issues/862) - Improve test assembly detection logic
* [#905](https://github.com/SonarSource/sonar-csharp/issues/905) - Improve generated code recognition

### New Feature
* [#155](https://github.com/SonarSource/sonar-csharp/issues/155) - Feed metric 'executable_lines_data' when SQ >= 6.2

### External Contributors
* [#814](https://github.com/SonarSource/sonar-csharp/pull/814) Thanks @Chelaris182

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
