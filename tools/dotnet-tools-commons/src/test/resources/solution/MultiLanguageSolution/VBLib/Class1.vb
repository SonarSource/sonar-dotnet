Imports System.Data.SqlClient

Public Class Class1
    'database connection 
    Dim connectionString As String = "Data Source=(local);AttachDbFilename=|DataDirectory|\epc.mdf;Trusted_Connection=Yes;"

    'executes the procedure with the param and values - returns the dataset 
    Public Function getDB(ByVal sp As String, ByVal spp As String, ByVal sppv As String) As DataSet

        Dim dataSet As New DataSet()
        Using conn As New SqlConnection(connectionString) 'System.Configuration.ConfigurationManager.ConnectionStrings(connectionString).ConnectionString) 
            Try
                Dim adapter As New SqlDataAdapter()
                Using cmd As New SqlCommand(sp)
                    cmd.CommandType = CommandType.StoredProcedure
                    'check parameter 
                    Dim prmVal() As String = sppv.Split("|")
                    Dim i As Integer = 0
                    For Each prmItem As String In spp.Split("|")
                        If prmVal(i) <> "" Then
                            cmd.Parameters.Add(New SqlParameter("@" & prmItem, prmVal(i)))
                            i += 1
                        End If
                    Next

                    conn.Open()
                    cmd.Connection = conn
                    adapter.SelectCommand = cmd
                    adapter.InsertCommand = cmd
                    adapter.Fill(dataSet)
                    conn.Close()

                End Using

            Catch ex As Exception

            End Try
        End Using
        Return dataSet
        Try

        Catch ex As Exception

        End Try
    End Function



    'executes the procedure with the param and values- returns the state 
    Public Function exeDB(ByVal sp As String, ByVal spp As String, ByVal sppv As String) As String
        '   Return sp & " " & spp & " " & sppv 
        Dim myElems As String = ""

        Using conn As New SqlConnection(connectionString)
            Try
                Dim adapter As New SqlDataAdapter()
                Using cmd As New SqlCommand(sp)
                    cmd.CommandType = CommandType.StoredProcedure
                    'check parameter 
                    Dim prmVal() As String = sppv.Split("|")
                    Dim i As Integer = 0
                    For Each prmItem As String In spp.Split("|")
                        If prmVal(i) <> "" Then
                            myElems &= " '" & prmItem & " " & prmVal(i) & "'"
                            cmd.Parameters.Add(New SqlParameter("@" & prmItem, prmVal(i)))
                            i += 1
                        End If
                    Next

                    conn.Open()
                    cmd.Connection = conn
                    cmd.ExecuteNonQuery()
                    conn.Close()
                End Using

            Catch ex As Exception
                Return ex.Message.ToString
                Exit Function
            End Try
        End Using

        Return True

    End Function

    'handles parameters 
    Function cmdGetParametersFromSP(ByVal _SP As String) As String
        Try
            Dim myParams As String = ""
            Dim cmd As New SqlCommand()
            Dim conn As New SqlConnection(connectionString)
            cmd.CommandType = CommandType.StoredProcedure
            cmd.CommandText = _SP
            cmd.Connection = conn

            conn.Open()
            SqlCommandBuilder.DeriveParameters(cmd)
            conn.Close()
            Dim collection As SqlParameterCollection = cmd.Parameters

            ' Populate the Input Parameters With Values Provided         
            For Each parameter As SqlParameter In collection
                If parameter.Direction = ParameterDirection.Input Then ' Or parameter.Direction = ParameterDirection.InputOutput Then ' 
                    myParams &= parameter.ParameterName.ToString.Replace("@", "") & "|"

                End If
            Next
            Return myParams
        Catch ex As Exception
            Return ex.Message.ToString
        End Try

    End Function


End Class
