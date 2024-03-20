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

Imports System
Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Security.Cryptography
Imports System.Text
Imports System.Text.Encoding

Public Class HashFile

    #Region "Methods"

    Public Shared Function ComputeMovieHash(ByVal filename As String) As Byte()
        Dim result As Byte()
        Using input As Stream = File.OpenRead(filename)
            result = ComputeMovieHash(input)
        End Using
        Return result
    End Function

    Public Shared Function ComputeMovieHash(ByVal input As Stream) As Byte()
        Dim lhash As System.Int64, streamsize As Long
        streamsize = input.Length
        lhash = streamsize

        Dim i As Long = 0
        Dim buffer As Byte() = New Byte(Marshal.SizeOf(GetType(Long)) - 1) {}
        While i < 65536 / Marshal.SizeOf(GetType(Long)) AndAlso (input.Read(buffer, 0, Marshal.SizeOf(GetType(Long))) > 0)
            i += 1

            lhash += BitConverter.ToInt64(buffer, 0)
        End While

        input.Position = Math.Max(0, streamsize - 65536)
        i = 0
        While i < 65536 / Marshal.SizeOf(GetType(Long)) AndAlso (input.Read(buffer, 0, Marshal.SizeOf(GetType(Long))) > 0)
            i += 1
            lhash += BitConverter.ToInt64(buffer, 0)
        End While
        input.Close()
        Dim result As Byte() = BitConverter.GetBytes(lhash)
        Array.Reverse(result)
        Return result
    End Function

    Public Shared Function CurrentETHashes(ByVal sPath As String) As List(Of String)
        Dim ETHashes As New List(Of String)
        Dim tPath As String = String.Empty

        If Master.eSettings.VideoTSParent AndAlso FileUtils.Common.isVideoTS(sPath) Then
            tPath = Path.Combine(Directory.GetParent(Directory.GetParent(sPath).FullName).FullName, "extrathumbs")
        ElseIf Master.eSettings.VideoTSParent AndAlso FileUtils.Common.isBDRip(sPath) Then
            tPath = Path.Combine(Directory.GetParent(Directory.GetParent(Directory.GetParent(sPath).FullName).FullName).FullName, "extrathumbs")
        Else
            tPath = Path.Combine(Directory.GetParent(sPath).FullName, "extrathumbs")
        End If

        If Directory.Exists(tPath) Then
            Dim fThumbs As New List(Of String)

            Try
                fThumbs.AddRange(Directory.GetFiles(tPath, "thumb*.jpg"))
            Catch
            End Try

            For Each fThumb As String In fThumbs
                ETHashes.Add(HashCalcFile(fThumb))
            Next
        End If

        Return ETHashes
    End Function

    Public Shared Function HashCalcFile(ByVal filepath As String) As String
        Using reader As New FileStream(filepath, FileMode.Open, FileAccess.Read)
            Dim KeyValue As Byte() = (New System.Text.UnicodeEncoding).GetBytes("HashingKey")
            Using HMA As New HMACSHA1(KeyValue, True)

                Dim hash() As Byte = HMA.ComputeHash(reader)

                Return ByteArrayToString(hash)

            End Using
        End Using
    End Function

    Public Shared Function ToHexadecimal(ByVal bytes As Byte()) As String
        Dim hexBuilder As New StringBuilder()
        For i As Integer = 0 To bytes.Length - 1
            hexBuilder.Append(bytes(i).ToString("x2"))
        Next
        Return hexBuilder.ToString()
    End Function

    Private Shared Function ByteArrayToString(ByVal arrInput() As Byte) As String
        Dim sb As New StringBuilder(arrInput.Length * 2)

        For i As Integer = 0 To arrInput.Length - 1
            sb.Append(arrInput(i).ToString("X2"))
        Next

        Return sb.ToString().ToLower
    End Function

    #End Region 'Methods

End Class