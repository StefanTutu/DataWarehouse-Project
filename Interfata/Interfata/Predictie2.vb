Imports Microsoft.AnalysisServices.AdomdClient
Public Class Predictie2

    Dim conn As AdomdConnection
    Dim connstr As String = "Provider=SQLNCLI11.1;Data Source=STEFAN\STEFAN;Integrated Security=SSPI;Initial Catalog=DataMining"
    Dim q_part1 As String = "SELECT FLATTENED (SELECT $Time,PredictVariance([Quantity]) AS[VARIANCE],[Quantity] as [PREDICTION] FROM PredictTimeSeries([Quantity],"
    Dim q_Part2 As String = ") AS t) AS t FROM [v Time Series] WHERE[Model Region] = 'M200 Europe'"
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim k As Integer
        For k = 1 To 30
            ComboBox1.Items.Add(k.ToString)
        Next
        ComboBox1.SelectedIndex = 0
        conn = New AdomdConnection(connstr)
        conn.Open()
        Dim cmd As AdomdCommand
        cmd = New AdomdCommand(q_part1 & ComboBox1.SelectedItem.ToString & q_Part2)
        cmd.Connection = conn
        Dim matr(get_adr_no_of_lines(cmd) - 1, 1)
        Dim adr As AdomdDataReader
        adr = cmd.ExecuteReader()
        Dim i As Integer = 0
        While (adr.Read)
            matr(i, 0) = Convert.ToString(adr.GetValue(0))
            Dim spliafter As Integer
            spliafter = 4
            Dim s1 As String
            Dim s2 As String
            s1 = Convert.ToString(adr.GetValue(0)).Substring(0, spliafter)
            s2 = Convert.ToString(adr.GetValue(0)).Substring(spliafter)
            Convert.ToInt32(s1.ToString)
            Convert.ToInt32(s2.ToString)
            If s2 >= 13 Then
                s1 = s1 + 1
                s2 = 1
            End If
            MessageBox.Show(s1)
            MessageBox.Show(s2)

            'ToDateTime(adr.GetValue(0)).ToShortDateString()
            matr(i, 1) = Math.Round(adr.GetValue(1), 2, MidpointRounding.AwayFromZero).ToString
            i = i + 1
        End While
        DataGridView1.DataSource = CreateDataView(matr)
        adr.Close()
        cmd.Dispose()
        conn.Close()
    End Sub

    Private Sub ComboBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBox1.SelectedIndexChanged
        conn = New AdomdConnection(connstr)
        conn.Open()
        Dim cmd As AdomdCommand
        cmd = New AdomdCommand(q_part1 & ComboBox1.SelectedItem.ToString & q_Part2)
        cmd.Connection = conn
        Dim matr(get_adr_no_of_lines(cmd) - 1, 1)
        Dim adr As AdomdDataReader
        adr = cmd.ExecuteReader()
        Dim i As Integer = 0
        While (adr.Read)
            matr(i, 0) = adr.GetValue(0)
            matr(i, 1) = Math.Round(adr.GetValue(1), 2, MidpointRounding.AwayFromZero).ToString
            i = i + 1
        End While
        adr.Close()
        cmd.Dispose()
        DataGridView1.DataSource = Nothing
        DataGridView1.DataSource = CreateDataView(matr)
        conn.Close()
    End Sub

    Function CreateDataView(matrix As Array) As ICollection
        Dim noLines As Integer
        Dim noCols As Integer
        noCols = matrix.GetLength(1) - 1
        noLines = matrix.GetLength(0) - 1
        Dim dt As New DataTable()
        Dim dr As DataRow
        If matrix.Length > 1 Then
            dt.Columns.Add(New DataColumn("TimeStamp", GetType(String)))
            dt.Columns.Add(New DataColumn("Predicted Average Sales", GetType(String)))
            Dim k, l As Integer
            For l = 0 To noLines
                dr = dt.NewRow()
                For k = 0 To noCols
                    dr(k) = matrix(l, k)
                Next
                dt.Rows.Add(dr)
            Next
        End If
        Dim dv As New DataView(dt)
        Return dv
    End Function

    Function get_adr_no_of_lines(cmd As AdomdCommand) As Integer
        Dim adr As AdomdDataReader
        adr = cmd.ExecuteReader()
        Dim i As Integer = 0
        While (adr.Read)
            i = i + 1
        End While
        adr.Close()
        Return i
    End Function
End Class