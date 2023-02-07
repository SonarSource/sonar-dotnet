Public Class Testcases

    Public Sub Comment() '

        ' Noncompliant@-2 (inline comment)

        Dim x = 42 'Ipsem Lorum
        Dim y = 42 ' Ipsem Lorum
        Dim z = 42 '

        ' Noncompliant@-2

        Dim a = 42 '''Ipsem Lorum
        Dim b = 42 ''' Ipsem Lorum
        Dim c = 42 '''

        '

        ' Noncompliant@-2 (whitespace)


        '
        '
        '
        ' hey

        ' hey
        '
        '
        '


        '
        '
        '
        '

        ' Noncompliant@-5
        ' Secondary@-5
        ' Secondary@-5
        ' Secondary@-5

        ' Noncompliant@+2

        '

        '
        '
        '

        ' Noncompliant@-4
        ' Secondary@-4
        ' Secondary@-4

        ' *
        ' Ipsem Lorum
        'Ipsem Lorum
        '

        '

        ' Noncompliant @-2

        ' \r

        ' \n

        ' \r\n

        ' \t

        ' z̶̤͚̅̍a̷͈̤̪͌͛̈ļ̷̈͐͝g̸̰̈́͂̆o̴̓̏͜

        ' '''

        ''''

        ' '

        ''

        ''''''

        '
        '
        '
        ''' Ipsem Lorum
        ' Noncompliant@-4
        ' Secondary@-4
        ' Secondary@-4
    End Sub

    Public Sub DocumentationComment()
        '''
        ' Noncompliant@-1 (whitespace)

        ''' *
        ''' Ipsem Lorum
        '''Ipsem Lorum
        '''

        ''' Ipsem Lorum

        '''Ipsem Lorum

        '''
        ' Noncompliant @-1

        ' Noncompliant @+1
        '''
        '''
        '''

        ''' \r

        ''' \n

        ''' \r\n

        ''' \t

        ''' z̶̤͚̅̍a̷͈̤̪͌͛̈ļ̷̈͐͝g̸̰̈́͂̆o̴̓̏͜

        ''' '''

        ''' '
    End Sub

End Class
'
' hey
'
'
' there
'

'''
''' hey
'''
''' there
'''



'
'
'

' Noncompliant@-4
' Secondary@-4
' Secondary@-4

' Noncompliant@+1
'''
'''
