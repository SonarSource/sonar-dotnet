interface I_Foo { }
interface I_I_Foo { }

interface i_foo { } // Noncompliant {{Rename interface 'i_foo' to match pascal case naming rules, consider using 'II_Foo'.}}

class Ab_Cd_Ef { }
class Foo_2 { }

class IFoo_2 { } // Noncompliant {{Rename class 'IFoo_2' to match pascal case naming rules, consider using 'Foo_2'.}}
class _Foo { } // Noncompliant {{Rename class '_Foo' to match pascal case naming rules, trim underscores from the name.}}
class Foo_ { } // Noncompliant {{Rename class 'Foo_' to match pascal case naming rules, trim underscores from the name.}}

class myClass_bar // Noncompliant {{Rename class 'myClass_bar' to match pascal case naming rules, consider using 'MyClass_Bar'.}}
{
}
