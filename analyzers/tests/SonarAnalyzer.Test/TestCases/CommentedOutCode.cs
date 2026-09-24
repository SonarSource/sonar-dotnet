// Copyright © 2011 - Present RealDimensions Software, LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// Noncompliant@+1 {{Remove this commented out code.}}
// ;
using System;
using System.Collections.Generic;

// Noncompliant@+1
// ;
using System;
// Noncompliant@+1
// {
using System;
// Noncompliant@+1
// }
using System;
// Noncompliant@+1
// ;}
using System;
// Noncompliant@+1
// ; }
using System;
// foo ; {} bar
using System;
// ; {} foo
using System;
// Noncompliant: ++
using System;
// Noncompliant@+1
// for    ( .. i != 5
using System;
// Noncompliant@+1
// if ( 1==2
using System;
// Noncompliant@+1
// while( i > 5
using System;
// Noncompliant@+1
// catch(
using System;
// Noncompliant@+1
// switch(
using System;
// Noncompliant@+1
// try{
using System;
// Noncompliant@+1
// else{
using System;
// &&
// ||
// && &&
// && ||
using System;
// Noncompliant: && && &&
using System;
// Noncompliant: || || ||
using System;
// Noncompliant: || && ||
using System;
/*

    hello

    // Noncompliant: || && ||

    ; world

    ;

*/

// Noncompliant: Console.WriteLine("Hello, world!");
// Console.WriteLine("Hello, world!");
// Console.WriteLine("Hello, world!");

// Console.WriteLine("Hello, world!");

/*

    // Noncompliant: Console.WriteLine();

    Console.WriteLine();
    */

namespace Tests.Diagnostics
{

    /// <summary>
    /// ...
    /// </summary>
    /// <code>
    /// Console.WriteLine("Hello, world!");
    /// </code>
    public class CommentedOutCode
    {
        // https://sonarsource.atlassian.net/browse/NET-3164
        void SentenceWithSemicolon()
        {
            _ = "separator";
            // * read the configuration file;

            _ = "separator";
            // - validate the input parameters;

            _ = "separator";
            // -> process the remaining items;

            _ = "separator";
            // => transform the data accordingly;

            _ = "separator";
            // => the computed value or null;

            _ = "separator";
            // 1. process the remaining items;

            _ = "separator"; // Noncompliant@+1
            // => null;

            _ = "separator";
            // if the listener fails to start, it gets disposed;

            _ = "separator";
            // for all intents and purposes, this will be removed;

            _ = "separator";
            // while the application is running, monitor the logs;

            _ = "separator";
            // return to the previous state if necessary;

            _ = "separator";
            // throw an exception if validation fails;

            _ = "separator";
            //Automatically initialized to false;

            _ = "separator";
            //Only act upon the failure, if it comes from a currently known child;

            _ = "separator";
            //todo: bounded priority mailbox;

            _ = "separator";
            //TODO: return deploy;

            _ = "separator"; // Noncompliant@+1
            // return result;

            _ = "separator"; // Noncompliant@+1
            // int x;

            _ = "separator"; // Noncompliant@+1
            // throw ex;

            _ = "separator"; // Noncompliant@+1
            // throw new NotImplementedException;

            _ = "separator"; // Noncompliant@+1
            // using static System;

            _ = "separator"; // Noncompliant@+1
            // yield return value;

            /* foo */
            SentenceWithSemicolon();
            SentenceWithSemicolon(); /* foo */

            // Noncompliant: Console.WriteLine("Hello, world!");
            // Console.WriteLine("Hello, world!");
            // Console.WriteLine("Hello, world!");

            // Console.WriteLine("Hello, world!"); //this is compliant, as there is code above and newline above

            SentenceWithSemicolon();
            /// Console.WriteLine("Hello, world!");
            ///
            ///

            /// The C++ access level for a member function, e.g. private
            ///

            SentenceWithSemicolon();
            _ = "separator"; // Noncompliant@+1
            // Debug.Assert(this.MemberTypeName != null == storage.HasFlag(StorageClass.Member));
            //
            //if (storage.HasFlag(StorageClass.Member))
            //{
            // output |= this.MemberTypeName.DisplayOn(builder, s);
            // builder.Append(CppNameBuilder.NameSeparator);
            // // Not trailing space wanted
            // output = false;
            //}

            _ = "separator";
            // process items;

            _ = "separator";
            // return null if the value is not found;

            _ = "separator"; // Noncompliant@+1
            // extern alias MyAlias;
        }

        void WhitespaceInExclusions()
        {
            _ = "separator";
            // Li cense: x = 1;

            _ = "separator";
            // C + +: x = 1;

            _ = "separator";
            // Li	cense: x = 1;

            _ = "separator";
            // c	+	+: x = 1;

            // Non-breaking space (U+00A0).
            _ = "separator";
            // Li cense: x = 1;

            // Em spaces (U+2003).
            _ = "separator";
            // C + +: x = 1;

            // Task tags still use the original word boundaries.
            _ = "separator"; // Noncompliant@+1
            // TO DO: x = 1;
        }

        void TaskCommentsAreExcluded()
        {
            _ = "separator";
            // TODO: x = 1;

            _ = "separator";
            //	FIXME: x = 1;

            _ = "separator";
            /*
             * HACK: x = 1;
             */

            _ = "separator";
            /* // XXX: x = 1; */

            _ = "separator";
            // - TODO: x = 1;

            _ = "separator";
            // [alice] TODO: x = 1;

            _ = "separator";
            // NET-123 fixMe: x = 1;

            _ = "separator";
            // Temporary workaround - HACK: x = 1;

            _ = "separator";
            /* Note: XXX x = 1; */

            _ = "separator";
            // 2026-09-17 Victor - TODO: x = 1;

            _ = "separator";
            // [a.contributor.with.a.long.name] 2026-09-17 - TODO: x = 1;

            _ = "separator";
            // TODOHandler TODO: x = 1;
        }

        void TaskTagsInsideCodeAreDetected()
        {
            _ = "separator"; // Noncompliant@+1
            // var message = "TODO: retry";

            _ = "separator"; // Noncompliant@+1
            // name = "xxx";

            _ = "separator"; // Noncompliant@+1
            // hack = ComputeHack();

            _ = "separator"; // Noncompliant@+1
            // HACK += ComputeHack();

            _ = "separator"; // Noncompliant@+1
            // TODO();

            _ = "separator"; // Noncompliant@+1
            // FIXME.Run();

            _ = "separator"; // Noncompliant@+1
            // if (ready) { TODO

            _ = "separator"; // Noncompliant@+1
            /* var message = "TODO: retry"; */

            _ = "separator"; // Noncompliant@+1
            /* hack = ComputeHack(); */

            // FN: the trailing tag hides the semicolon from code recognition.
            _ = "separator";
            // x = 1; TODO
        }

        void TaskTagsInsideIdentifiersAreDetected()
        {
            _ = "separator"; // Noncompliant@+1
            // TODOHandler = 1;

            _ = "separator"; // Noncompliant@+1
            // myTODO = 1;

            _ = "separator"; // Noncompliant@+1
            // _TODO = 1;

            _ = "separator"; // Noncompliant@+1
            // TODO_ = 1;

            _ = "separator"; // Noncompliant@+1
            // TODO2 = 1;

            _ = "separator"; // Noncompliant@+1
            // TODOé = 1;
        }

        void TaskTagsOnlyExcludeTheirOwnLine()
        {
            _ = "separator"; // Noncompliant@+2
            // [alice] TODO: x = 1;
            // x = 1;

            _ = "separator"; // Noncompliant@+2
            /* [alice] FIXME: x = 1;
             * x = 1;
             */
        }

        int a; // Noncompliant: Console.WriteLine();
        int b; // Noncompliant: Console.WriteLine();

        // this should be compliant:
        // does *not* overwrite file if (still) exists

        //  https://github.com/SonarSource/sonar-dotnet/issues/2772
        int c;
        // It's just a URL and it is not an interpolated string
        // http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}

        int rx;
        // regex{2,5}
        int d;
        // {this, is, a ,set}
        int e;
        // Noncompliant@+1
        // {this, is, a ,set; }
        int f;
        // Noncompliant@+1
        // {Command();}
        int g;
        // Noncompliant@+1
        // {Command(); }
        int h;
        // Noncompliant@+1
        // int Method() { }
        int i;
        // Noncompliant@+1
        // int Method() {}
        int j;
        // Compliant, not a C# code, but a data sample
        // { "json": "fragment" }
    }

    /**
        <summary>
        ...
        </summary>
        <code>
        Console.WriteLine("Hello, world!");
        </code>
    */
    public class CommentedOutCode2
    {
    }

    public class CommentedOutCodeXmlDoc
    {
        /// <summary>
        /// See <see cref="Method(int, string)"/> and <seealso cref="Dictionary{TKey, TValue}"/>.
        /// </summary>
        /// <param name="value">See <paramref name="value"/> for details.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is negative.</exception>
        /// <returns>See <see cref="List{T}.Add(T)"/>.</returns>
        /// <code>
        /// var result = Method(42, "test");
        /// if (result > 0) { return result; }
        /// </code>
        public int Method(int value, string text) => value;

        //// Four slashes are not documentation either, but are still excluded: if (x > 0) { return x; }
        public int Field;
    }

    // Some C++ reference
    class X { }
    // Some c++ reference
    class Y { }
    // Somec++ reference
    class Z { }
}

// https://github.com/SonarSource/sonar-dotnet/issues/8819
class Repro_8819
{
    void LineTerminators()
    {
        // Remark: separators are required to consider comments as independent sentences

        _ = "separator";
        // Natural language sentence with semicolon at the end;

        _ = "separator";
        // Natural language sentence with open-brace at the end{

        _ = "separator";
        // Natural language sentence preceding sentence with semicolon at the end
        // Natural language sentence with semicolon at the end;

        _ = "separator";
        // Natural language sentence with escaped semicolon at the end;
        // Natural language sentence following sentence with escaped semicolon at the end

        _ = "separator"; // Compliant: Natural language sentence with semicolon and escaped space at the end; \u0020
        _ = "separator"; // Compliant: Natural language sentence with semicolon and multiple escaped spaces at the end; \u0020\u0020
        _ = "separator"; // Compliant: Natural language sentence with escaped semicolon at the end\u003B
        _ = "separator"; // Compliant: Natural language sentence with escaped open-brace at the end\u007B
        _ = "separator"; // Compliant: Natural language sentence with escaped close-brace at the end\u007D
        _ = "separator"; // Compliant: Natural language sentence with semicolon; in the middle
        _ = "separator"; // Compliant: Natural language sentence with colon at the end:
        _ = "separator"; // Compliant: Natural language sentence with comma at the end,
        _ = "separator"; // Compliant: Natural language sentence with period at the end.
        _ = "separator"; // Compliant: Natural language sentence with close-brace at the end}
        _ = "separator"; // Compliant: Natural language sentence with closing parenthesis at the end)
        _ = "separator"; // Compliant: Natural language sentence with an opening parenthesis (in the middle

        _ = "separator";
        // The empty set is indicated either with ∅ or with {}
    }

    // Natural-language comments that used to be misclassified as code. https://sonarsource.atlassian.net/browse/NET-3964
    // Remark: separators are required to consider comments as independent sentences.
    void NaturalLanguageMisclassifiedAsCode()
    {
        _ = "separator";
        // Natural language sentence with a hyphenated-word ending in semicolon;

        _ = "separator";
        // Natural language sentence with an interruption — an aside — ending in semicolon;

        _ = "separator";
        // Natural language sentence with the module's possessive noun ending in semicolon;

        _ = "separator";
        // Natural language sentence with a Namespace.Member dotted token ending in semicolon;

        _ = "separator";
        // Natural language sentence with an abbreviation e.g. this one ending in semicolon;

        _ = "separator";
        // Natural language sentence with a (parenthetical aside) ending in semicolon;

        _ = "separator";
        // Natural language sentence mentioning the ++ operator in passing

        _ = "separator";
        // Natural language sentence describing a catch (Exception) clause

        _ = "separator";
        // Natural language sentence describing a try { } block

        _ = "separator";
        // Natural language sentence quoting an if (value != null) guard

        // Double quotes around a quoted code token do not turn prose into code.
        _ = "separator";
        // Natural language sentence that quotes a "catch (Exception)" block in prose
    }

    void KeywordsFromTheCompilerTableAreDetected()
    {
        _ = "separator"; // Noncompliant@+1
        // ushort x;

        _ = "separator"; // Noncompliant@+1
        // value from nameof lookup;

        _ = "separator"; // Noncompliant@+1
        // nint offset;

        _ = "separator"; // Noncompliant@+1
        // nuint value;

        _ = "separator"; // Noncompliant@+1
        // generic constraint notnull required;
    }

    void TerseStatementsAndDeclarationsAreDetected()
    {
        _ = "separator"; // Noncompliant@+1
        // x = 1;

        _ = "separator"; // Noncompliant@+1
        // flag = true;

        _ = "separator"; // Noncompliant@+1
        // result = null;

        _ = "separator"; // Noncompliant@+1
        // object x;

        _ = "separator"; // Noncompliant@+1
        //object val = null;

        _ = "separator"; // Noncompliant@+1
        // string name;

        _ = "separator"; // Noncompliant@+1
        // new byte[10];

        _ = "separator"; // Noncompliant@+1
        // int value = 42;

        _ = "separator"; // Noncompliant@+1
        // value = other ?? fallback;

        _ = "separator"; // Noncompliant@+1
        // var result = Compute(input);

        _ = "separator"; // Noncompliant@+1
        // await Task.Delay(100);

        _ = "separator"; // Noncompliant@+1
        // collection.Add(key, value);

        _ = "separator"; // Noncompliant@+1
        // message = "Please provide a value.";

        _ = "separator"; // Noncompliant@+1
        // throw new InvalidOperationException("Cannot create SomeType: configuration not found");

        _ = "separator"; // Noncompliant@+1
        // public static readonly string SomeConstant = "value";

        // A field declaration whose call and English-looking string literal both sit on one line.
        _ = "separator"; // Noncompliant@+1
        // private static readonly Logger DefaultLogger = LoggerFactory.Create<Program>("startup message text");

        _ = "separator"; // Noncompliant@+1
        // base.dispose(disposing);

        _ = "separator"; // Noncompliant@+1
        // return;

        _ = "separator"; // Noncompliant@+1
        // break;
    }

    void StringLiteralContentsDoNotCountAsProse()
    {
        _ = "separator"; // Noncompliant@+1
        // Method("a sentence containing several ordinary words separated by spaces");

        _ = "separator";
        // "first" natural language sentence containing several ordinary words "second";

        // Unpaired quotes leave the whole line available for prose detection.
        _ = "separator";
        // Method("a sentence containing several ordinary words separated by spaces", ");
    }

    void AssignmentsWithoutSurroundingWhitespaceAreDetected()
    {
        _ = "separator"; // Noncompliant@+1
        // buffer[position&mask]=current=lookup[index];

        _ = "separator"; // Noncompliant@+1
        // _fieldName=owner.fieldName.EnumValue;
    }

    void CompoundAssignmentsAreDetected()
    {
        _ = "separator"; // Noncompliant@+1
        // total += count;

        _ = "separator"; // Noncompliant@+1
        //buffer.Changed -= OnChanged;

        _ = "separator"; // Noncompliant@+1
        // items.Count += Bar.Count;

        // Fixed FP (NET-4429): a negated regex character class excluding '=' is not an XOR-assignment. Written
        // as a trailing comment because the verifier reads a caret in an own-line comment as a location marker.
        _ = "carrier"; // (?<key>([^=\s\p{Cc}]|\s+[^=\s\p{Cc}]|\s+==|==)+)
    }

    void TernariesAndWrappedContinuationsAreDetected()
    {
        _ = "separator"; // Noncompliant@+1
        // var result = current != null ? current.Value : fallback.Value;

        _ = "separator"; // Noncompliant@+1
        // Label: (current != null && other != null) ? current.Value : fallback.Value);

        _ = "separator"; // Noncompliant@+1
        // does the equivalent of: var items = CreateArray(4);
    }

    void ControlFlowInStatementPositionIsDetected()
    {
        _ = "separator"; // Noncompliant@+1
        // if (x)

        _ = "separator"; // Noncompliant@+1
        // if (index < count && IsValid(items[index])) {

        _ = "separator"; // Noncompliant@+1
        // } else if (someCondition && otherCondition) {

        // Comment decoration counts as punctuation.
        _ = "separator"; // Noncompliant@+1
        // * if (x) {

        _ = "separator"; // Noncompliant@+1
        // try { if (value != null) { Process(); } }

        _ = "separator"; // Noncompliant@+1
        // for (Node current = node.Parent; current != null; current = current.Parent)

        _ = "separator"; // Noncompliant@+1
        // case SomeEnum.Double : result.value = (SomeCastType)left.value / (SomeCastType)right.value; break;

        _ = "separator"; // Noncompliant@+1
        // if (!((kind == SomeKind.First || kind == SomeKind.Second || kind == SomeKind.Third) && scope is SomeScope))

        _ = "separator"; // Noncompliant@+1
        // if (value.Length < threshold) { throw "Value exceeds the allowed length" }
    }

    void ProseQuotingCodeStaysCompliant()
    {
        _ = "separator";
        // Natural language sentence describing a generic call Foo<T>()

        _ = "separator";
        // Natural language sentence describing a generic call Foo<T>();

        _ = "separator";
        // Natural language sentence noting the ratio is greater than (the threshold) allows;

        _ = "separator";
        // Natural language sentence noting the ratio is greater than(the threshold) allows;

        _ = "separator";
        // Generates the code for an if(condition)-then-else branch.

        _ = "separator";
        // This follows the same approach as is used in SomeType.SomeMethod(int, int, out int)

        // A keyword used as an ordinary English word, alongside a real call shape.
        _ = "separator";
        // Natural language sentence noting that in case the value is null, the callback HandleMessage() is invoked

        // Pseudo-code notation in a documentation comment, with an '=' and two '||'.
        _ = "separator";
        // Combine the parts: result = Merge(partOne || partTwo || partThree)

        _ = "separator";
        // returning if(x) to the caller whenever the flag is set

        _ = "separator";
        // treat the input as "trusted and carry on;

        _ = "separator";
        // lookup constants: k=2, n=5, m=32 : 0b_0000_0111_1100_0100_1010_1100_1101_1101u
    }

    void TrailingDocumentationCommentStaysCompliant()
    {
        _ = "separator";
        int value = 42; // This is TheComputedValue;
    }

    void TrailingCodeShapedCommentsAreKnownFalsePositives()
    {
        _ = "separator"; // Noncompliant@+1
        _ = "carrier"; // while (true)

        _ = "separator"; // Noncompliant@+1
        _ = "carrier"; // if (!ready)

        _ = "separator"; // Noncompliant@+1
        _ = "carrier"; // value == null || count != limit || offset >= length
    }

    void DecorativeMarkupAndBannersStayCompliant()
    {
        _ = "separator";
        // in case this happens; **

        _ = "separator";
        // **Natural language text with emphasis**

        _ = "separator";
        // _Natural language text with emphasis_

        _ = "separator";
        //===============================================================================

        _ = "separator";
        // ========================================================

        _ = "separator";
        // ===================== SECTION TITLE =====================

        _ = "separator";
        // ------------------------------

        _ = "separator";
        // ******************************

        _ = "separator";
        // <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<

        _ = "separator";
        // >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>

        _ = "separator";
        // &&&&&&&&&&&&&&&&&&&&&&&&&&&&&&

        _ = "separator";
        // ||||||||||||||||||||||||||||||

        _ = "separator";
        // +---+---+---|---+---+---+---|

        _ = "separator";
        // +----------------+----------------+

        _ = "separator";
        // =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-
    }

    void ForeignNotationIsJudgedOnCodeShapeAlone()
    {
        _ = "separator"; // Noncompliant@+1
        // System.out.println("value: " + result);

        _ = "separator";
        // assert matches(value, offset, length, out result);

        _ = "separator";
        // PATTERN: [AnnotateSomething] outer:(Something input:*) => (AddPattern outer {Category}) AND (AddArgument outer {Key} input) AND { }
    }

    void MathematicalAndTabularNotationAreKnownFalsePositives()
    {
        // 0.9536 against a threshold of 0.9105: Assignment@0.60 + Call@0.884 (5 matches); language score 0.10.
        _ = "separator"; // Noncompliant@+1
        // f(x) = g(x) + h(x) = k(x)*m(x)

        _ = "separator"; // Noncompliant@+1
        // q(a) = f(b) * g(c) + h(d) - k(e)

        // 0.9585 against a threshold of 0.9022: Comparison@0.45 + Call@0.9246 (6 matches); language score 0.4347.
        _ = "separator"; // Noncompliant@+1
        // log(f(x)) <= exp(g(x)) * sqrt(h(x))

        _ = "separator"; // Noncompliant@+1
        // && || == != > < >= <=

        _ = "separator"; // Noncompliant@+1
        // allowed types: bool, byte, char, double, float, int, long, sbyte, short, ushort, uint

        _ = "separator"; // Noncompliant@+1
        // store(T, load(items), load(offset), Op(load(items), load(offset)), Next(T))
    }

    // Ordinary prose that happens to end in ';' and to use words that are also C# keywords.
    void ProseUsingKeywordsStaysCompliant()
    {
        _ = "separator";
        // Otherwise, do nothing;

        _ = "separator";
        // See the notes below;

        _ = "separator";
        // Returns true on success;

        _ = "separator";
        // Set to true to enable;

        _ = "separator";
        // the value is null if not set;

        _ = "separator";
        // true for success, false for failure;

        _ = "separator";
        // do this in case the lock is held;

        _ = "separator";
        // returns the object as a string;

        _ = "separator";
        // this is the default for new events;

        _ = "separator";
        // out of range if the index is negative;
    }

    void ShortProseEndingInSemicolonStaysCompliant()
    {
        _ = "separator";
        // Use this as needed;

        _ = "separator";
        // in case of failure;

        _ = "separator";
        // for each item in turn;

        _ = "separator";
        // Reset to default if set;

        _ = "separator";
        // true if enabled, false otherwise;

        _ = "separator";
        // is null or empty;
    }

    void AcceptedGapsAndKnownFalsePositives()
    {
        _ = "separator";
        // Node item;

        _ = "separator";
        // SomeExpressionType localExpr;

        _ = "separator"; // Noncompliant@+1
        // default is null;

        _ = "separator";
        // collection.Add (key, value);

        _ = "separator";
        // counter++; // NOTE: kept for backward compatibility

        _ = "separator";
        // throw;

        _ = "separator";
        // remainingCharacters++;

        _ = "separator";
        // remainingCapacity--;

        _ = "separator";
        // && value < items.Length && Contains(items, ++i, sep)

        // Bare argument-list continuation: 0.9000 against a threshold of 0.9123.
        _ = "separator";
        // value, offset, count);

        // Arithmetic expression fragment: 0.9000 against a threshold of 0.9142.
        _ = "separator";
        // total + count - offset;

        // Member-access chain statement: 0.9000 against a threshold of 0.9061.
        _ = "separator";
        // currentHandler.ActiveSelection.Content;

        // Long call statement: 0.9350 against a threshold of 0.9549.
        _ = "separator";
        // Handler.Process(value, offset, count, buffer, flag, mode, extra);

        // Long assignment-plus-call statement: 0.9740 against a threshold of 0.9790.
        _ = "separator";
        // totalValue = ComputeResult(firstItem, secondItem, thirdItem, fourthItem, fifthItem, sixthItem, seventhItem, eighthItem);

        _ = "separator"; // Noncompliant@+1
        // equivalent to accumulator /= baseValue;

        // Exceeds its threshold by only 0.0040, the tightest margin in this file: if it flips, the payload is the
        // thing to leave alone.
        _ = "separator"; // Noncompliant@+1
        // rank is SubTreeSize(Node.Left)+1, we do +1 here to offset the +1 done in rank. index -= rank;
    }

    void CommentedOutCommentsSeparatedByEmptyLine()
    {
        // Noncompliant: var x = 42;

        // FN: var y = 3;
    }

    void CommentedOutCommentsSeparatedByMultiLineComment()
    {
        // Noncompliant: var x = 42;
        /* A multiline comment  between the two commented-out blocks */
        // Noncompliant: var y = 42;
    }

    void CommentedOutCommentsSeparatedByBlock()
    {
        // Noncompliant: var x = 42;
        { }
        // Noncompliant: var y = 42;
    }

}
