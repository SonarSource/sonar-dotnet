using System;

const int a = 1;
int[] array = [2, 3];

_ = new MyClass(1, [1, 2, 3]);     // Noncompliant
_ = new MyClass(1, []);            // Noncompliant

// repro for https://github.com/SonarSource/sonar-dotnet/issues/8510
_ = new MyClass(1, [a, .. array]);    // Noncompliant FP

_ = new MyClass2([1], [1, 2, 3]);  // Noncompliant
_ = new MyClass2([1, 2, 3], 1);
_ = new MyClass3([1, 2, 3], [4, 5, 6]); // Noncompliant FP
_ = new MyClass3([[1, 2, 3], [4, 5, 6]]); // Noncompliant
_ = new MyClass3([[1, 2, 3], [4, 5, 6]]); // Noncompliant

class MyClass(int a, params int[] args);
class MyClass2(int[] a, params int[] args);
class MyClass3(params int[][] args);
