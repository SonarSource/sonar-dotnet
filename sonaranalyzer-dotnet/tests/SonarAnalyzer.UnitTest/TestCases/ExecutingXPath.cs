namespace Tests.TestCases
{
    using System;
    using System.Xml;
    using System.Xml.XPath;

    public class Foo
    {
        public const string FixedPath = "fixed path";

        public void RspecExample(string user, string pass) // https://jira.sonarsource.com/browse/RSPEC-4876
        {
            var xpathNavigator = new XPathDocument("http://uri").CreateNavigator();

            var expression = "/users/user[@name='" + user + "' and @pass='" + pass + "']";
            xpathNavigator.Evaluate(expression);             // Noncompliant {{Make sure that executing this XPATH expression is safe.}}
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        }

        public void XPathExpressionMembers(IXmlNamespaceResolver nsResolver)
        {
            string path = "variable";

            var pathExpression = XPathExpression.Compile(path);           // Noncompliant
//                               ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            pathExpression = XPathExpression.Compile(path, nsResolver);   // Noncompliant
//                           ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

            pathExpression = XPathExpression.Compile("fixed path");
            pathExpression = XPathExpression.Compile("fixed path", nsResolver);

            var returnType = pathExpression.ReturnType;
            pathExpression.AddSort(null, null);
        }

        public void XPathNavigatorMembers(XPathNavigator nav, IXmlNamespaceResolver nsResolver, XPathExpression pathExpression)
        {
            const string constantPath = "/my/xpath/expression";
            string path = "variable path";

            // Should not raise for hard-coded paths
            nav.Compile(constantPath);
            nav.Evaluate("xpath-expression");
            nav.Select(constantPath);
            nav.Matches(FixedPath);

            // Should raise for variable paths
            nav.Compile(path);                              // Noncompliant

            nav.Evaluate(path);                             // Noncompliant
            nav.Evaluate(path, nsResolver);                 // Noncompliant
            nav.Evaluate(pathExpression);                   // Compliant - using path expression objects is ok
            nav.Evaluate(pathExpression, null);

            nav.Matches(path);                              // Noncompliant
            nav.Matches(pathExpression);

            // XPathNavigator selection methods
            nav.Select(path);                               // Noncompliant
            nav.Select(path, nsResolver);                   // Noncompliant
            nav.Select(pathExpression);

            nav.SelectSingleNode(path);                     // Noncompliant
            nav.SelectSingleNode(path, nsResolver);         // Noncompliant
            nav.SelectSingleNode(pathExpression);

            nav.SelectAncestors("name", "uri", false);
            nav.SelectChildren("name", "uri");
            nav.SelectDescendants("name", "uri", false);

            nav.AppendChild("newChild");
            nav.AppendChildElement("prefix", "localName", "uri", "value");
            nav.CheckValidity(null, null);

            var nav2 = nav.CreateNavigator();
            nav2.DeleteRange(nav);
        }

        public void XmlNodeMembers(XmlNode node, XmlNamespaceManager nsMgr, XmlNode node2, string path)
        {
            node.SelectNodes(path);                     // Noncompliant
            node.SelectNodes(path, nsMgr);              // Noncompliant

            node.SelectSingleNode(path);                // Noncompliant
            node.SelectSingleNode(path, nsMgr);         // Noncompliant

            const string fixedPath = "fixed path";
            node.SelectNodes(fixedPath);
            node.SelectNodes(fixedPath, nsMgr);
            node.SelectSingleNode(fixedPath);
            node.SelectSingleNode(fixedPath, nsMgr);

            node.ReplaceChild(node2, node2);
            node.RemoveAll();
            node.WriteContentTo(null);
        }

        internal void XPathNavigatorDerivedType(MyXmlPathNavigator nav, IXmlNamespaceResolver nsResolver,
            XPathExpression pathExpression, string path)
        {
            // Constant paths are ok
            nav.Compile(null);
            nav.Evaluate("xpath-expression");

            nav.Compile(path);                              // Noncompliant
            nav.Evaluate(path);                             // Noncompliant

            nav.Matches(path);                              // Noncompliant
            nav.Matches(pathExpression);

            nav.Select(path);                               // Noncompliant
            nav.Select(pathExpression);

            nav.SelectSingleNode(path);                     // Noncompliant
            nav.SelectSingleNode(path, nsResolver);         // Noncompliant

            nav.SelectAncestors("name", "uri", false);
        }

        public void XmlNodeDerivedTypes(XmlAttribute attNode, XmlNamespaceManager nsMgr, XmlNode node2, string path)
        {
            // Fixed paths are ok
            attNode.SelectNodes("fixed");
            attNode.SelectSingleNode(FixedPath);

            attNode.SelectNodes(path);                      // Noncompliant
            attNode.SelectNodes(path, nsMgr);               // Noncompliant

            attNode.SelectSingleNode(path);                 // Noncompliant
            attNode.SelectSingleNode(path, nsMgr);          // Noncompliant

            attNode.ReplaceChild(node2, node2);
            attNode.RemoveAll();
            attNode.WriteContentTo(null);
        }

    }

    internal class MyXmlPathNavigator : XPathNavigator
    {
        public override XmlNameTable NameTable => throw new NotImplementedException();
        public override XPathNodeType NodeType => throw new NotImplementedException();
        public override string LocalName => throw new NotImplementedException();
        public override string Name => throw new NotImplementedException();
        public override string NamespaceURI => throw new NotImplementedException();
        public override string Prefix => throw new NotImplementedException();
        public override string BaseURI => throw new NotImplementedException();
        public override bool IsEmptyElement => throw new NotImplementedException();
        public override string Value => throw new NotImplementedException();
        public override XPathNavigator Clone() => throw new NotImplementedException();
        public override bool IsSamePosition(XPathNavigator other) => throw new NotImplementedException();
        public override bool MoveTo(XPathNavigator other) => throw new NotImplementedException();
        public override bool MoveToFirstAttribute() => throw new NotImplementedException();
        public override bool MoveToFirstChild() => throw new NotImplementedException();
        public override bool MoveToFirstNamespace(XPathNamespaceScope namespaceScope) => throw new NotImplementedException();
        public override bool MoveToId(string id) => throw new NotImplementedException();
        public override bool MoveToNext() => throw new NotImplementedException();
        public override bool MoveToNextAttribute() => throw new NotImplementedException();
        public override bool MoveToNextNamespace(XPathNamespaceScope namespaceScope) => throw new NotImplementedException();
        public override bool MoveToParent() => throw new NotImplementedException();
        public override bool MoveToPrevious() => throw new NotImplementedException();
    }
}
