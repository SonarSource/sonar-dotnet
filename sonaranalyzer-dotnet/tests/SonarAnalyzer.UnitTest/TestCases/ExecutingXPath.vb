Imports System
Imports System.Xml
Imports System.Xml.XPath

Namespace Tests.TestCases
    Public Class Foo

        Private Const ClassConstant As String = "fixed path"

        Public Sub RspecExample(user As String, pass As String) ' https://jira.sonarsource.com/browse/RSPEC-4876
            Dim xpathNavigator = New XPathDocument("http://uri").CreateNavigator()

            Dim expression = "/users/user[@name='" + user + "' and @pass='" + pass + "']"
            xpathNavigator.Evaluate(expression)     ' Noncompliant {{Make sure that executing this XPATH expression is safe.}}
'           ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        End Sub

        Public Sub XPathExpressionMembers(nsResolver As IXmlNamespaceResolver)

            Dim path = "variable"

            Dim pathExpression = XPathExpression.Compile(path)            ' Noncompliant
'                                ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            pathExpression = XPathExpression.Compile(path, nsResolver)    ' Noncompliant
'                            ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

            pathExpression = XPathExpression.Compile("fixed path")
            pathExpression = XPathExpression.Compile("fixed path", nsResolver)

            Dim returnType = pathExpression.ReturnType
            pathExpression.AddSort(Nothing, Nothing)
        End Sub

        Public Sub XPathNavigatorMembers(nav As XPathNavigator, nsResolver As IXmlNamespaceResolver, pathExpression As XPathExpression)

            Const constantPath = "/my/xpath/expression"
            Dim path As String = "variable path"

            ' Should not raise for hard-coded paths
            nav.Compile(constantPath)
            nav.Evaluate("xpath-expression")
            nav.Select(constantPath)
            nav.Matches("another path")

            ' Should raise for variable paths
            nav.Compile(path)                       ' Noncompliant

            nav.Evaluate(path)                      ' Noncompliant
            nav.Evaluate(path, nsResolver)          ' Noncompliant
            nav.Evaluate(pathExpression)            ' Compliant - using path expression objects is ok
            nav.Evaluate(pathExpression, Nothing)

            nav.Matches(path)                       ' Noncompliant
            nav.Matches(pathExpression)

            ' XPathNavigator selection methods
            nav.Select(path)                        ' Noncompliant
            nav.Select(path, nsResolver)            ' Noncompliant
            nav.Select(pathExpression)

            nav.SelectSingleNode(path)              ' Noncompliant
            nav.SelectSingleNode(path, nsResolver)  ' Noncompliant
            nav.SelectSingleNode(pathExpression)

            nav.SelectAncestors("name", "uri", False)
            nav.SelectChildren("name", "uri")
            nav.SelectDescendants("name", "uri", False)

            nav.AppendChild("newChild")
            nav.AppendChildElement("prefix", "localName", "uri", "value")
            nav.CheckValidity(Nothing, Nothing)

            Dim nav2 = nav.CreateNavigator()
            nav2.DeleteRange(nav)

        End Sub

        Public Sub XmlNodeMembers(node As XmlNode, nsMgr As XmlNamespaceManager, node2 As XmlNode)

            Dim path As String = "variable path"

            node.SelectNodes(path)                      ' Noncompliant
            node.SelectNodes(path, nsMgr)               ' Noncompliant

            node.SelectSingleNode(path)                 ' Noncompliant
            node.SelectSingleNode(path, nsMgr)          ' Noncompliant

            ' Fixed paths are ok
            Const fixedPath = "fixed path"
            node.SelectNodes(fixedPath)
            node.SelectNodes(ClassConstant, nsMgr)
            node.SelectSingleNode(ClassConstant)
            node.SelectSingleNode(fixedPath, nsMgr)

            node.ReplaceChild(node2, node2)
            node.RemoveAll()
            node.WriteContentTo(Nothing)

        End Sub

        Friend Sub XPathNavigatorDerivedType(nav As MyXmlPathNavigator, nsResolver As IXmlNamespaceResolver,
            pathExpression As XPathExpression, path As String)

            ' Constant paths are ok
            nav.Compile(Nothing)
            nav.Evaluate("xpath-expression")

            nav.Compile(path)                              ' Noncompliant
            nav.Evaluate(path)                             ' Noncompliant

            nav.Matches(path)                              ' Noncompliant
            nav.Matches(pathExpression)

            nav.Select(path)                               ' Noncompliant
            nav.Select(pathExpression)

            nav.SelectSingleNode(path)                     ' Noncompliant
            nav.SelectSingleNode(path, nsResolver)         ' Noncompliant

            nav.SelectAncestors("name", "uri", False)
        End Sub

        Public Sub XmlNodeDerivedTypes(attNode As XmlAttribute, nsMgr As XmlNamespaceManager, node2 As XmlNode, path As String)
            ' Fixed paths are ok
            attNode.SelectNodes("fixed")
            attNode.SelectSingleNode(FixedPath) ' Error [BC30451]

            attNode.SelectNodes(path)                      ' Noncompliant
            attNode.SelectNodes(path, nsMgr)               ' Noncompliant

            attNode.SelectSingleNode(path)                 ' Noncompliant
            attNode.SelectSingleNode(path, nsMgr)          ' Noncompliant

            attNode.ReplaceChild(node2, node2)
            attNode.RemoveAll()
            attNode.WriteContentTo(Nothing)
        End Sub

    End Class

    Friend Class MyXmlPathNavigator
        Inherits XPathNavigator

        Public Overrides ReadOnly Property NameTable As XmlNameTable
            Get
                Throw New NotImplementedException()
            End Get
        End Property

        Public Overrides ReadOnly Property NodeType As XPathNodeType
            Get
                Throw New NotImplementedException()
            End Get
        End Property

        Public Overrides ReadOnly Property LocalName As String
            Get
                Throw New NotImplementedException()
            End Get
        End Property

        Public Overrides ReadOnly Property Name As String
            Get
                Throw New NotImplementedException()
            End Get
        End Property

        Public Overrides ReadOnly Property NamespaceURI As String
            Get
                Throw New NotImplementedException()
            End Get
        End Property

        Public Overrides ReadOnly Property Prefix As String
            Get
                Throw New NotImplementedException()
            End Get
        End Property

        Public Overrides ReadOnly Property BaseURI As String
            Get
                Throw New NotImplementedException()
            End Get
        End Property

        Public Overrides ReadOnly Property IsEmptyElement As Boolean
            Get
                Throw New NotImplementedException()
            End Get
        End Property

        Public Overrides ReadOnly Property Value As String
            Get
                Throw New NotImplementedException()
            End Get
        End Property

        Public Overrides Function Clone() As XPathNavigator
            Throw New NotImplementedException()
        End Function

        Public Overrides Function MoveToFirstAttribute() As Boolean
            Throw New NotImplementedException()
        End Function

        Public Overrides Function MoveToNextAttribute() As Boolean
            Throw New NotImplementedException()
        End Function

        Public Overrides Function MoveToFirstNamespace(namespaceScope As XPathNamespaceScope) As Boolean
            Throw New NotImplementedException()
        End Function

        Public Overrides Function MoveToNextNamespace(namespaceScope As XPathNamespaceScope) As Boolean
            Throw New NotImplementedException()
        End Function

        Public Overrides Function MoveToNext() As Boolean
            Throw New NotImplementedException()
        End Function

        Public Overrides Function MoveToPrevious() As Boolean
            Throw New NotImplementedException()
        End Function

        Public Overrides Function MoveToFirstChild() As Boolean
            Throw New NotImplementedException()
        End Function

        Public Overrides Function MoveToParent() As Boolean
            Throw New NotImplementedException()
        End Function

        Public Overrides Function MoveTo(other As XPathNavigator) As Boolean
            Throw New NotImplementedException()
        End Function

        Public Overrides Function MoveToId(id As String) As Boolean
            Throw New NotImplementedException()
        End Function

        Public Overrides Function IsSamePosition(other As XPathNavigator) As Boolean
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace
