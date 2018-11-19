' ################################################################################
' #                             EMBER MEDIA MANAGER                              #
' ################################################################################
' ################################################################################
' # This file is part of Ember Media Manager.                                    #
' #                                                                              #
' # Ember Media Manager is free software: you can redistribute it and/or modify  #
' # it under the terms of the GNU General Public License as published by         #
' # the Free Software Foundation, either version 3 of the License, or            #
' # (at your option) any later version.                                          #
' #                                                                              #
' # Ember Media Manager is distributed in the hope that it will be useful,       #
' # but WITHOUT ANY WARRANTY; without even the implied warranty of               #
' # MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the                #
' # GNU General Public License for more details.                                 #
' #                                                                              #
' # You should have received a copy of the GNU General Public License            #
' # along with Ember Media Manager.  If not, see <http://www.gnu.org/licenses/>. #
' ################################################################################

Imports System.Globalization

Public Class NumUtils

    #Region "Methods"

    ''' <summary>
    ''' Convert a numerical string to single (internationally friendly method)
    ''' </summary>
    ''' <param name="sNumber">Number (as string) to convert</param>
    ''' <returns>Number as single</returns>
    Public Shared Function ConvertToSingle(ByVal sNumber As String) As Single
        Try
            If String.IsNullOrEmpty(sNumber) OrElse sNumber = "0" Then Return 0
            Dim numFormat As NumberFormatInfo = New NumberFormatInfo()
            numFormat.NumberDecimalSeparator = "."
            Return Single.Parse(sNumber.Replace(",", "."), NumberStyles.AllowDecimalPoint, numFormat)
        Catch
        End Try
        Return 0
    End Function

    #End Region 'Methods

End Class