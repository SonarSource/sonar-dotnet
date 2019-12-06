# Coding Style

## General

When contributing to the project, and if otherwise not mentioned in this document, our coding conventions
follow the Microsoft [C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/inside-a-program/coding-conventions)
and standard [Naming Guidelines](https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/naming-guidelines).

## Class Members

Members and types should always have the lowest possible visibility.

Ordering of class members should be the following:

1. Constants
1. Fields
1. Abstract members
1. Constructors
1. Properties
1. Methods 
1. Private nested classes

Furthermore, each of these categories should be ordered from higher to lower accessibility level (public, internal, protected, private).

## FIXME and ToDo

* `FIXME` should only be used in the code during development as a temporary reminder that there is still work to
be done here. A `FIXME` should never appear in a PR or be merged into master.

* `ToDo` can be used to mark part of the code that will need to be updated at a later time. It can be used to
track updates that should be done at some point, but that either cannot be done at that moment, or can be fixed later.
Ideally, a `ToDo` comment should be followed by an issue number (what needs to be done should be in the github issues).

## Regions

Generally, as we do not want to have classes that are too long, regions are not necessary and should be avoided.
It can still be used when and where it makes sense. For instance, when a class having a specific purpose is
implementing generic interfaces (such as `IComparable`, `IDisposable`), it can make sense to have regions 
for the implementation of these interfaces.

## VB.NET Specifics

Empty lines should be used between blocks, `Namespace`/`End Namespace` statements, `Class`/`End Class` statements
and regions to improve readability.
