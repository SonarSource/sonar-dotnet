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
'Class originally developed by blackducksoftware and highly modified for Ember Media Manager

Imports System.IO

Public Class DVD

    #Region "Fields"

    Private Const ifo_CellInfoSize As Short = 24
    Private Const ifo_SECTOR_SIZE As Short = 2048

    'Variables
    Dim mAudioModes As New Hashtable
    Dim mVideoCodingMode As New Hashtable
    Dim mVideoResolution()() As String = {New String() {"720x480", "704x480", "352x480", "352x240"}, New String() {"720x576", "704x576", "352x576", "352x288"}}
    Private oEnc As System.Text.Encoding = System.Text.ASCIIEncoding.GetEncoding(1252)
    Private ParsedIFOFile As struct_IFO_VST_Parse

    #End Region 'Fields

    #Region "Constructors"

    Public Sub New()
        MyBase.New()
        'Audio Format
        mAudioModes.Add("0", "ac3")
        mAudioModes.Add("1", String.Empty)
        mAudioModes.Add("2", "mp1")
        mAudioModes.Add("3", "mp2")
        mAudioModes.Add("4", "wav")
        mAudioModes.Add("5", String.Empty)
        mAudioModes.Add("6", "dca")
        mAudioModes.Add("7", String.Empty)

        mVideoCodingMode.Add("0", "mpeg1")
        mVideoCodingMode.Add("1", "mpeg2")
    End Sub

    #End Region 'Constructors

    #Region "Properties"

    Public ReadOnly Property GetIFOAudio(ByVal bytAudioIndex As Integer) As String()
        Get
            Dim ReturnArray(2) As String
            Try
                If bytAudioIndex <= ParsedIFOFile.NumAudioStreams_VTS_VOBS AndAlso bytAudioIndex > 0 Then
                    bytAudioIndex -= 1
                    If mAudioModes.ContainsKey(ParsedIFOFile.AudioAtt_VTS_VOBS(bytAudioIndex).CodingMode.ToString) Then
                        ReturnArray(0) = mAudioModes.Item(ParsedIFOFile.AudioAtt_VTS_VOBS(bytAudioIndex).CodingMode.ToString).ToString
                    Else
                        'assume ac3
                        ReturnArray(0) = "ac3"
                    End If
                    ReturnArray(1) = Localization.ISOGetLangByCode2(ParsedIFOFile.AudioAtt_VTS_VOBS(bytAudioIndex).LanguageCode)
                    ReturnArray(2) = ParsedIFOFile.AudioAtt_VTS_VOBS(bytAudioIndex).NumberOfChannels.ToString
                End If
            Catch ex As Exception
                Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
            End Try
            Return ReturnArray
        End Get
    End Property

    Public ReadOnly Property GetIFOAudioNumberOfTracks() As Integer
        Get
            Return ParsedIFOFile.NumAudioStreams_VTS_VOBS
        End Get
    End Property

    Public ReadOnly Property GetIFOSubPic(ByVal bytSubPicIndex As Integer) As String
        Get
            Try
                If bytSubPicIndex <= ParsedIFOFile.NumSubPictureStreams_VTS_VOBS AndAlso bytSubPicIndex > 0 Then
                    bytSubPicIndex -= 1
                    Return Localization.ISOGetLangByCode2(ParsedIFOFile.SubPictureAtt_VTS_VOBS(bytSubPicIndex).LanguageCode)
                End If
            Catch ex As Exception
                Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
            End Try
            Return String.Empty
        End Get
    End Property

    Public ReadOnly Property GetIFOSubPicNumberOf() As Integer
        Get
            Return ParsedIFOFile.NumSubPictureStreams_VTS_VOBS
        End Get
    End Property

    Public ReadOnly Property GetIFOVideo() As String()
        Get
            Dim ReturnArray(2) As String
            Try
                If mVideoCodingMode.ContainsKey(ParsedIFOFile.VideoAtt_VTS_VOBS.Coding_Mode.ToString) Then
                    ReturnArray(0) = mVideoCodingMode.Item(ParsedIFOFile.VideoAtt_VTS_VOBS.Coding_Mode.ToString).ToString
                Else
                    'assume mpeg2
                    ReturnArray(0) = "mpeg2"
                End If
                ReturnArray(1) = mVideoResolution(ParsedIFOFile.VideoAtt_VTS_VOBS.Video_Standard)(ParsedIFOFile.VideoAtt_VTS_VOBS.Resolution)
                If ParsedIFOFile.VideoAtt_VTS_VOBS.Aspect_Ratio = 3 AndAlso ParsedIFOFile.VideoAtt_VTS_VOBS.LetterBoxed Then
                    ReturnArray(2) = "1.85"
                ElseIf ParsedIFOFile.VideoAtt_VTS_VOBS.Aspect_Ratio = 3 OrElse ParsedIFOFile.VideoAtt_VTS_VOBS.LetterBoxed Then
                    ReturnArray(2) = "1.78"
                ElseIf ReturnArray(1).Contains("x") Then
                    Dim strAspect() As String = ReturnArray(1).Split(Convert.ToChar("x"))
                    Dim strReturn As String = FormatNumber(NumUtils.ConvertToSingle(strAspect(0)) / NumUtils.ConvertToSingle(strAspect(1)), 2, TriState.False).Replace(",", ".")
                    If strReturn.EndsWith("0") Then
                        ReturnArray(2) = strReturn.Substring(0, strReturn.Length - 1)
                    Else
                        ReturnArray(2) = strReturn
                    End If
                Else
                    ReturnArray(2) = String.Empty
                End If
            Catch ex As Exception
                Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
            End Try
            Return ReturnArray
        End Get
    End Property

    Public ReadOnly Property GetNumberProgramChains() As Integer
        Get
            Return Convert.ToInt32(ParsedIFOFile.NumberOfProgramChains)
        End Get
    End Property

    Public ReadOnly Property GetProgramChainPlayBackTime(ByVal bytProChainIndex As Byte, Optional ByVal MinsOnly As Boolean = False) As String
        Get
            Try
                If bytProChainIndex <= ParsedIFOFile.NumberOfProgramChains Then
                    bytProChainIndex = Convert.ToByte(bytProChainIndex - 1)

                    Return fctPlayBackTimeToString(ParsedIFOFile.ProgramChainInformation(bytProChainIndex).PlayBackTime, MinsOnly)
                End If
            Catch ex As Exception
                Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
            End Try
            Return String.Empty
        End Get
    End Property

    #End Region 'Properties

    #Region "Methods"

    Public Sub Close()
        Me.Finalize()
    End Sub

    Public Function CovertByteToHex(ByVal BytConvert() As Byte) As String
        Dim hexStr As String = String.Empty
        Try

            Dim i As Integer
            For i = 0 To BytConvert.Length - 1
                hexStr = hexStr + (BytConvert(i)).ToString("X")
            Next i
            hexStr = hexStr.PadLeft(16, Convert.ToChar("0"))
            hexStr = hexStr.Insert(0, "0x")

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
        Return hexStr
    End Function

    Public Function fctOpenIFOFile(ByVal strPath As String) As Boolean
        Dim IFOFiles As New List(Of String)
        Dim tIFOFile As New struct_IFO_VST_Parse
        Dim currLongest As Integer = 0
        Dim currDuration As Integer = 0

        Try
            Try
                IFOFiles.AddRange(Directory.GetFiles(Directory.GetParent(strPath).FullName, "VTS*.IFO"))
            Catch
            End Try

            If IFOFiles.Count > 1 Then
                'find the one with the longest duration
                For Each fFile As String In IFOFiles
                    Try
                        ParsedIFOFile = fctParseIFO_VSTFile(fFile)
                        Dim test As String = GetProgramChainPlayBackTime(1, True)
                        currDuration = Convert.ToInt32(GetProgramChainPlayBackTime(1, True))
                        If currDuration > currLongest Then
                            currLongest = currDuration
                            tIFOFile = ParsedIFOFile
                        End If
                    Catch ex As Exception
                    End Try
                Next

                ParsedIFOFile = tIFOFile
                Return True
            ElseIf IFOFiles.Count = 1 Then
                ParsedIFOFile = fctParseIFO_VSTFile(IFOFiles(0).ToString)
                Return True
            ElseIf Path.GetExtension(strPath).ToLower = ".ifo" AndAlso Not Path.GetFileName(strPath).ToLower = "video_ts.ifo" Then
                ParsedIFOFile = fctParseIFO_VSTFile(strPath)
                Return True
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
        Return False
    End Function

    Protected Overrides Sub Finalize()
        mAudioModes = Nothing
        mVideoCodingMode = Nothing
        mVideoResolution = Nothing
        oEnc = Nothing

        MyBase.Finalize()
        GC.Collect()
    End Sub

    'Fill in the Audio Header Information
    Private Function fctAudioAttVTSM_VTS(ByVal strAudioInfo As String) As struct_AudioAttributes_VTSM_VTS
        Dim byteInfo(8) As Byte
        Dim i As Integer
        Dim bytTempValue As Byte
        Dim tVTSM As New struct_AudioAttributes_VTSM_VTS
        Try
            'Setup Byte info
            For i = 0 To 7
                byteInfo(i) = Convert.ToByte(oEnc.GetBytes(((strAudioInfo).Substring(i, 1)).Chars(0))(0))
            Next

            If byteInfo(2) <> 0 AndAlso byteInfo(3) <> 0 Then
                tVTSM.LanguageCode = Convert.ToChar(byteInfo(2)) & Convert.ToChar(byteInfo(3)) ' & Convert.ToChar(byteInfo(4))
            Else
                tVTSM.LanguageCode = String.Empty
            End If

            'Using Logic AND's to check if bits are set dec 176 -> bin 10110000
            bytTempValue = 0
            If (byteInfo(0) And 32) = 32 Then bytTempValue = 1
            If (byteInfo(0) And 64) = 64 Then bytTempValue = Convert.ToByte(bytTempValue + 2)
            If (byteInfo(0) And 128) = 128 Then bytTempValue = Convert.ToByte(bytTempValue + 4)
            tVTSM.CodingMode = bytTempValue

            If (byteInfo(1) And 1) = 1 Then bytTempValue = 1
            If (byteInfo(1) And 2) = 2 Then bytTempValue = Convert.ToByte(bytTempValue + 2)
            If (byteInfo(1) And 4) = 4 Then bytTempValue = Convert.ToByte(bytTempValue + 4)
            tVTSM.NumberOfChannels = Convert.ToByte(bytTempValue + 1)
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
        Return tVTSM
    End Function

    Private Function fctHexOffset(ByVal strHexString As String) As Integer
        Return Convert.ToInt32(Val(String.Concat("&H", (strHexString).ToUpper)))
    End Function

    Private Function fctHexTimeToDecTime(ByVal bytAmountHex As Byte) As Byte
        Return Convert.ToByte(bytAmountHex.ToString("X2"))
    End Function

    'Open an IFO file and return the Parsed Variable
    Private Function fctParseIFO_VSTFile(ByVal strFileName As String) As struct_IFO_VST_Parse
        Dim strTmpIFOFileIn As String
        Dim tmpIFO As New struct_IFO_VST_Parse

        Dim intFileLength As Integer

        Try
            'Read in the Info file name
            Dim objFS As FileStream
            objFS = File.Open(strFileName, FileMode.Open, FileAccess.Read)
            Dim objBR As New BinaryReader(objFS)
            strTmpIFOFileIn = System.Text.Encoding.Default.GetString(objBR.ReadBytes(Convert.ToInt32(objFS.Length)))
            intFileLength = strTmpIFOFileIn.Length
            objBR.Close()
            objBR = Nothing
            objFS.Close()
            objFS = Nothing

            If intFileLength > 0 Then
                'Save the Ifo File name
                tmpIFO.IFO_FileName = strFileName

                'See MPUcoder (http://www.mpucoder.com/DVD/ifo.html)
                'Parse the File in tmpIFOFileIn
                tmpIFO.FileType_Header = (strTmpIFOFileIn).Substring(fctHexOffset("0"), 12)
                tmpIFO.LastSectorOfTitle = fctStrByteToHex((strTmpIFOFileIn).Substring(fctHexOffset("C"), 4))
                tmpIFO.LastSectorOfIFO = fctStrByteToHex((strTmpIFOFileIn).Substring(fctHexOffset("1C"), 4))
                tmpIFO.VersionNumber = fctStrByteToHex((strTmpIFOFileIn).Substring(fctHexOffset("20"), 2))
                tmpIFO.VSTCategory = fctStrByteToHex((strTmpIFOFileIn).Substring(fctHexOffset("22"), 4))

                tmpIFO.EndByteAddress_VTS_MAT = fctStrByteToHex((strTmpIFOFileIn).Substring(fctHexOffset("80"), 4))

                tmpIFO.StartSector_MenuVOB = fctStrByteToHex((strTmpIFOFileIn).Substring(fctHexOffset("C0"), 4))
                tmpIFO.StartSector_TitleVOB = fctStrByteToHex((strTmpIFOFileIn).Substring(fctHexOffset("C4"), 4))

                tmpIFO.SectorPointer_VTS_PTT_SRPT = fctStrByteToHex((strTmpIFOFileIn).Substring(fctHexOffset("C8"), 4))
                tmpIFO.SectorPointer_VTS_PGCI = fctStrByteToHex((strTmpIFOFileIn).Substring(fctHexOffset("CC"), 4))
                tmpIFO.SectorPointer_VTSM_PGCI_UT = fctStrByteToHex((strTmpIFOFileIn).Substring(fctHexOffset("D0"), 4))
                tmpIFO.SectorPointer_VTS_TMAPTI = fctStrByteToHex((strTmpIFOFileIn).Substring(fctHexOffset("D4"), 4))
                tmpIFO.SectorPointer_VSTM_C_ADT = fctStrByteToHex((strTmpIFOFileIn).Substring(fctHexOffset("D8"), 4))
                tmpIFO.SectorPointer_VSTM_VOBU_ADMAP = fctStrByteToHex((strTmpIFOFileIn).Substring(fctHexOffset("DC"), 4))
                tmpIFO.SectorPointer_VST_C_ADT = fctStrByteToHex((strTmpIFOFileIn).Substring(fctHexOffset("E0"), 4))
                tmpIFO.SectorPointer_VTS_VOBU_ADMAP = fctStrByteToHex((strTmpIFOFileIn).Substring(fctHexOffset("E4"), 4))

                'VTSM
                'Get Video of Main Information
                tmpIFO.VideoAtt_VSTM_VOBS = fctVideoAtt_VTS_VOBS((strTmpIFOFileIn).Substring(fctHexOffset("100"), 2))
                tmpIFO.NumOfAudioStreamsIn_VTSM_VOBS = fctStrByteToHex((strTmpIFOFileIn).Substring(fctHexOffset("102"), 2))
                'Read in all Audio Track Information
                For intFileLength = 0 To tmpIFO.NumOfAudioStreamsIn_VTSM_VOBS - 1
                    ReDim Preserve tmpIFO.AudioAtt_VTSM_VOBS(intFileLength)
                    tmpIFO.AudioAtt_VTSM_VOBS(intFileLength) = fctAudioAttVTSM_VTS((strTmpIFOFileIn).Substring(fctHexOffset("104") + (intFileLength * 8), 8))
                Next

                'Get SubPicture Main info
                tmpIFO.NumSubPictureStreams_VTSM_VOBS = fctStrByteToHex((strTmpIFOFileIn).Substring(fctHexOffset("154"), 2))
                tmpIFO.SubPictureAtt_VTSM_VOBS = fctSubPictureAttVTSM_VTS((strTmpIFOFileIn).Substring(fctHexOffset("156"), 6))

                'VTS
                'Get Video Information
                tmpIFO.VideoAtt_VTS_VOBS = fctVideoAtt_VTS_VOBS((strTmpIFOFileIn).Substring(fctHexOffset("200"), 2))

                'Get Audio Information
                tmpIFO.NumAudioStreams_VTS_VOBS = fctStrByteToHex((strTmpIFOFileIn).Substring(fctHexOffset("203"), 1))
                For intFileLength = 0 To tmpIFO.NumAudioStreams_VTS_VOBS - 1
                    ReDim Preserve tmpIFO.AudioAtt_VTS_VOBS(intFileLength)
                    tmpIFO.AudioAtt_VTS_VOBS(intFileLength) = fctAudioAttVTSM_VTS((strTmpIFOFileIn).Substring(fctHexOffset("204") + (intFileLength * 8), 8))
                Next

                'Get SubPicture info
                tmpIFO.NumSubPictureStreams_VTS_VOBS = fctStrByteToHex((strTmpIFOFileIn).Substring(fctHexOffset("255"), 1))
                For intFileLength = 0 To tmpIFO.NumSubPictureStreams_VTS_VOBS - 1
                    ReDim Preserve tmpIFO.SubPictureAtt_VTS_VOBS(intFileLength)
                    tmpIFO.SubPictureAtt_VTS_VOBS(intFileLength) = fctSubPictureAttVTSM_VTS((strTmpIFOFileIn).Substring(fctHexOffset("256") + (intFileLength * 6), 6))
                Next

                'Get Program Chain Information
                tmpIFO.NumberOfProgramChains = fctStrByteToHex((strTmpIFOFileIn).Substring(Convert.ToInt32(tmpIFO.SectorPointer_VTS_PGCI * ifo_SECTOR_SIZE), 2))
                For intFileLength = 0 To Convert.ToInt32(tmpIFO.NumberOfProgramChains - 1)
                    ReDim Preserve tmpIFO.ProgramChainInformation(intFileLength)
                    tmpIFO.ProgramChainInformation(intFileLength) = fctProgramChainInformation(Convert.ToInt16(intFileLength), strTmpIFOFileIn, tmpIFO)
                Next
            End If

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
        Return tmpIFO
    End Function

    Private Function fctPlayBackTimeToString(ByRef PlayBack As DVD_Time_Type, Optional ByVal MinsOnly As Boolean = False) As String
        Try
            If MinsOnly Then
                Return ((PlayBack.hours * 60) + PlayBack.minutes).ToString
            Else
                Return String.Concat((PlayBack.hours).ToString("00"), "h ", (PlayBack.minutes).ToString("00"), "mn ", (PlayBack.seconds).ToString("00"), "s")
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
        Return String.Empty
    End Function

    Private Function fctProgramChainInformation(ByVal shoProgramChainNumber As Short, ByRef strIFOFileBuffer As String, ByRef tmpIFO As struct_IFO_VST_Parse) As struct_Program_Chain_Type
        Dim ChainLoc As Integer
        Dim PCT As New struct_Program_Chain_Type
        Try
            'Setup the Start loc for the File
            ChainLoc = Convert.ToInt32((tmpIFO.SectorPointer_VTS_PGCI * ifo_SECTOR_SIZE) + fctStrByteToHex((strIFOFileBuffer).Substring(Convert.ToInt32(tmpIFO.SectorPointer_VTS_PGCI * ifo_SECTOR_SIZE + 12 + (shoProgramChainNumber) * 8), 4)))

            'The Program Number
            PCT.NumberOfPrograms = Convert.ToByte(fctStrByteToHex((strIFOFileBuffer).Substring(ChainLoc + 2, 1)))

            'Number of Cells in Program Chain
            PCT.NumberOfCells = Convert.ToByte(fctStrByteToHex((strIFOFileBuffer).Substring(ChainLoc + 3, 1)))

            'Get DVD Chain Type Info
            PCT.PlayBackTime.hours = fctHexTimeToDecTime(Convert.ToByte(((strIFOFileBuffer).Substring(ChainLoc + 4, 1)).Chars(0)))
            PCT.PlayBackTime.minutes = fctHexTimeToDecTime(Convert.ToByte(((strIFOFileBuffer).Substring(ChainLoc + 5, 1)).Chars(0)))
            PCT.PlayBackTime.seconds = fctHexTimeToDecTime(Convert.ToByte(((strIFOFileBuffer).Substring(ChainLoc + 6, 1)).Chars(0)))
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
        Return PCT
    End Function

    'Creates the Video info from a string of 2 bytes
    Private Function fctSRPT(ByVal VideoInfo As String) As struct_SRPT
        Dim byte1 As Byte
        Dim byte2 As Byte
        Dim bytTmpValue As Byte
        Dim tSRPT As New struct_SRPT

        Try
            'Split String for logic AND checks
            byte1 = Convert.ToByte(oEnc.GetBytes(((VideoInfo).Substring(0, 1)).Chars(0))(0))
            byte2 = Convert.ToByte(oEnc.GetBytes(((VideoInfo).Substring(1, 1)).Chars(0))(0))

            bytTmpValue = 0
            If (byte1 And 4) = 4 Then bytTmpValue = 1
            If (byte1 And 8) = 8 Then bytTmpValue = Convert.ToByte(bytTmpValue + 2)
            fctSRPT.Aspect_Ratio = bytTmpValue

            bytTmpValue = 0
            If (byte1 And 64) = 64 Then bytTmpValue = 1
            If (byte1 And 128) = 128 Then bytTmpValue = Convert.ToByte(bytTmpValue + 2)
            tSRPT.Coding_Mode = bytTmpValue

            fctSRPT.LetterBoxed = Convert.ToBoolean(byte2 And 2)

            bytTmpValue = 0
            If (byte2 And 4) = 4 Then bytTmpValue = 1
            If (byte2 And 8) = 8 Then bytTmpValue = Convert.ToByte(bytTmpValue + 2)
            tSRPT.Resolution = bytTmpValue
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
        Return tSRPT
    End Function

    'Convert a string of Bytes (0x00-0xFF) into a complete number
    Private Function fctStrByteToHex(ByVal strHexString As String) As Integer
        Dim i As Long
        Dim HexTotal As Double = 0
        Dim HexMod As Double
        Dim CharNum As Long

        Try
            For i = 0 To (strHexString).Length - 1
                CharNum = Convert.ToInt32(oEnc.GetBytes(strHexString.Substring(Convert.ToInt32(i), 1).Chars(0))(0))
                If i <> (strHexString).Length Then
                    HexMod = 256 ^ (((strHexString).Length - 1) - i)
                    HexTotal = HexTotal + CharNum * HexMod
                Else
                    HexTotal = HexTotal + CharNum
                End If
            Next
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
        Return Convert.ToInt32(HexTotal)
    End Function

    Private Function fctSubPictureAttVTSM_VTS(ByVal strSubPictureInfo As String) As SubPictureAtt_VTSM_VTS_Type
        Dim SubPicATT As New SubPictureAtt_VTSM_VTS_Type
        Try
            SubPicATT.LanguageCode = (strSubPictureInfo).Substring(2, 1) & (strSubPictureInfo).Substring(3, 1)
            SubPicATT.CodeExtention = Convert.ToByte(oEnc.GetBytes((((strSubPictureInfo).Substring(5, 1)).Chars(0)))(0))
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
        Return SubPicATT
    End Function

    'Creates the Video info from a string of 2 bytes
    Private Function fctVideoAtt_VTS_VOBS(ByVal VideoInfo As String) As struct_VideoAttributes_VTS_VOBS
        Dim byte1 As Byte
        Dim byte2 As Byte
        Dim bytTmpValue As Byte
        Dim tVTSVOB As New struct_VideoAttributes_VTS_VOBS
        Try
            'Split String for logic AND checks
            byte1 = Convert.ToByte(oEnc.GetBytes(((VideoInfo).Substring(0, 1)).Chars(0))(0))
            byte2 = Convert.ToByte(oEnc.GetBytes(((VideoInfo).Substring(1, 1)).Chars(0))(0))

            bytTmpValue = 0
            If (byte1 And 4) = 4 Then bytTmpValue = 1
            If (byte1 And 8) = 8 Then bytTmpValue = Convert.ToByte(bytTmpValue + 2)
            fctVideoAtt_VTS_VOBS.Aspect_Ratio = bytTmpValue

            bytTmpValue = 0
            If (byte1 And 16) = 16 Then bytTmpValue = 1
            If (byte1 And 32) = 32 Then bytTmpValue = Convert.ToByte(bytTmpValue + 2)
            tVTSVOB.Video_Standard = bytTmpValue

            bytTmpValue = 0
            If (byte1 And 64) = 64 Then bytTmpValue = 1
            If (byte1 And 128) = 128 Then bytTmpValue = Convert.ToByte(bytTmpValue + 2)
            tVTSVOB.Coding_Mode = bytTmpValue

            fctVideoAtt_VTS_VOBS.LetterBoxed = Convert.ToBoolean(byte2 And 2)

            bytTmpValue = 0
            If (byte2 And 4) = 4 Then bytTmpValue = 1
            If (byte2 And 8) = 8 Then bytTmpValue = Convert.ToByte(bytTmpValue + 2)
            tVTSVOB.Resolution = bytTmpValue
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
        Return tVTSVOB
    End Function

    #End Region 'Methods

    #Region "Nested Types"

    'Program time information
    Private Structure DVD_Time_Type

        #Region "Fields"

        Dim frame As Byte
        Dim hours As Byte
        Dim minutes As Byte
        Dim seconds As Byte

        #End Region 'Fields

    End Structure

    'Individual Cell information
    Private Structure PGC_Cell_Info_Type

        #Region "Fields"

        Dim CellPlayBackTime As DVD_Time_Type

        #End Region 'Fields

    End Structure

    'Audio Type
    Private Structure struct_AudioAttributes_VTSM_VTS

        #Region "Fields"

        Dim CodingMode As Byte
        Dim LanguageCode As String
        Dim NumberOfChannels As Byte

        #End Region 'Fields

    End Structure

    'IFO VST information
    Private Structure struct_IFO_VST_Parse

        #Region "Fields"

        Dim AudioAtt_VTSM_VOBS() As struct_AudioAttributes_VTSM_VTS
        Dim AudioAtt_VTS_VOBS() As struct_AudioAttributes_VTSM_VTS
        Dim EndByteAddress_VTS_MAT As Long
        Dim FileType_Header As String
        Dim IFO_FileName As String
        Dim LastSectorOfIFO As Long
        Dim LastSectorOfTitle As Long
        Dim NumAudioStreams_VTS_VOBS As Integer
        Dim NumberOfProgramChains As Long
        Dim NumOfAudioStreamsIn_VTSM_VOBS As Integer
        Dim NumSubPictureStreams_VTSM_VOBS As Integer
        Dim NumSubPictureStreams_VTS_VOBS As Integer
        Dim ProgramChainInformation() As struct_Program_Chain_Type
        Dim SectorPointer_VSTM_C_ADT As Long
        Dim SectorPointer_VSTM_VOBU_ADMAP As Long
        Dim SectorPointer_VST_C_ADT As Long
        Dim SectorPointer_VTSM_PGCI_UT As Long
        Dim SectorPointer_VTS_PGCI As Long
        Dim SectorPointer_VTS_PTT_SRPT As Long
        Dim SectorPointer_VTS_TMAPTI As Long
        Dim SectorPointer_VTS_VOBU_ADMAP As Long
        Dim StartSector_MenuVOB As Long
        Dim StartSector_TitleVOB As Long
        Dim SubPictureAtt_VTSM_VOBS As SubPictureAtt_VTSM_VTS_Type
        Dim SubPictureAtt_VTS_VOBS() As SubPictureAtt_VTSM_VTS_Type
        Dim VersionNumber As Long
        Dim VideoAtt_VSTM_VOBS As struct_VideoAttributes_VTS_VOBS
        Dim VideoAtt_VTS_VOBS As struct_VideoAttributes_VTS_VOBS
        Dim VSTCategory As Long

        #End Region 'Fields

    End Structure

    'IFO VST Program Chain Information
    Private Structure struct_Program_Chain_Type

        #Region "Fields"

        Dim NumberOfCells As Byte
        Dim NumberOfPrograms As Byte
        Dim PlayBackTime As DVD_Time_Type

        #End Region 'Fields

        #Region "Other"

        'Currently only implamenting basic useful information

        #End Region 'Other

    End Structure

    Private Structure struct_SRPT

        #Region "Fields"

        Dim Aspect_Ratio As Byte
        Dim Coding_Mode As Byte
        Dim LetterBoxed As Boolean
        Dim Resolution As Byte

        #End Region 'Fields

    End Structure

    Private Structure struct_VideoAttributes_VTS_VOBS

        #Region "Fields"

        Dim Aspect_Ratio As Byte
        Dim Coding_Mode As Byte
        Dim LetterBoxed As Boolean
        Dim Resolution As Byte
        Dim Video_Standard As Byte

        #End Region 'Fields

    End Structure

    'SubPicture Type
    Private Structure SubPictureAtt_VTSM_VTS_Type

        #Region "Fields"

        Dim CodeExtention As Byte
        Dim CodingMode As Byte
        Dim LanguageCode As String

        #End Region 'Fields

    End Structure

    'All these types are used for the IFO Parsing
    Private Structure VTS_PTT_SRPT

        #Region "Fields"

        Dim EndAddress_VST_PTT As Integer
        Dim NumberOfTitles As Integer
        Dim OffsetTo_PPT As Integer
        Dim PackedString As String

        #End Region 'Fields

    End Structure

    #End Region 'Nested Types

End Class