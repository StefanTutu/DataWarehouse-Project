Imports Microsoft.AnalysisServices.AdomdClient

Public Class Predictie

    Dim conn As AdomdConnection
    Dim fixstr As String = "Provider=SQLNCLI11.1;Data Source=STEFAN\STEFAN; Integrated Security=SSPI;Initial Catalog="

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim initcast As String
        initcast = "DataMining"
        conn = New AdomdConnection(fixstr & initcast)
        conn.Open()
        Dim cmd As AdomdCommand
        cmd = New AdomdCommand("SELECT * FROM $SYSTEM.DBSCHEMA_Catalogs")
        cmd.Connection = conn
        Dim adr As AdomdDataReader
        adr = cmd.ExecuteReader()
        Dim m
        Dim i
        i = 0
        While (adr.Read)
            m = adr.GetValue(0)
            ComboBox1.Items.Add(m)
            i = i + 1
        End While
        ComboBox1.SelectedIndex = 0
        conn.Close()
        cmd.Dispose()
    End Sub

    Private Sub ComboBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBox1.SelectedIndexChanged
        conn = New AdomdConnection(fixstr & ComboBox1.SelectedItem.ToString)
        conn.Open()
        Dim cmd As AdomdCommand
        cmd = New AdomdCommand("SELECT DISTINCT SERVICE_NAME FROM $SYSTEM.DMSCHEMA_mining_models")
        cmd.Connection = conn
        Dim adr As AdomdDataReader
        adr = cmd.ExecuteReader()
        Dim i As Integer = 0
        ComboBox2.Items.Clear()
        While (adr.Read)
            ComboBox2.Items.Add(adr.GetValue(0))
            i = i + 1
        End While
        If (i = 0) Then
            MessageBox.Show("Nici un model!!")
            ComboBox2.SelectedIndex = -1
            ComboBox2.Text = " "
            ComboBox3.SelectedIndex = -1
            ComboBox3.Text = " "
            ComboBox3.Items.Clear()
        Else
            ComboBox2.SelectedIndex = 0
        End If
        cmd.Dispose()
        conn.Close()
    End Sub

    Private Sub ComboBox2_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBox2.SelectedIndexChanged
        If (ComboBox2.SelectedIndex >= 0) Then
            conn = New AdomdConnection(fixstr & ComboBox1.SelectedItem.ToString)
            conn.Open()
            Dim cmd As AdomdCommand
            cmd = New AdomdCommand("SELECT MODEL_NAME FROM $SYSTEM.DMSCHEMA_MINING_MODELS WHERE SERVICE_NAME='" & ComboBox2.SelectedItem.ToString & "'")
            cmd.Connection = conn
            Dim adr As AdomdDataReader
            adr = cmd.ExecuteReader()
            Dim i As Integer = 0
            ComboBox3.Items.Clear()
            While (adr.Read)
                ComboBox3.Items.Add(adr.GetValue(0))
                i = i + 1
            End While
            If (i = 0) Then
                ComboBox3.SelectedIndex = -1
                ComboBox3.Text = ""
            Else
                ComboBox3.SelectedIndex = 0
            End If
            conn.Close()
            cmd.Dispose()
        End If
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        If (ComboBox3.SelectedIndex >= 0) Then
            conn = New AdomdConnection(fixstr & ComboBox1.SelectedItem.ToString())
            conn.Open()
            Dim cmd As AdomdCommand
            cmd = New AdomdCommand("SELECT MODE_DESCRIPTION, NODE_PROBABILITY FROM [" & ComboBox3.SelectedItem.ToString & "].CONTENT ORDER BY NODE_PROBABILITY DESC")
            cmd.Connection = conn
            Dim matr(get_adr_no_of_lines(cmd) + 1, 2) 'be careful to allocate memory above the reader, they'll intersect and occur error
            Dim adr As AdomdDataReader
            adr = cmd.ExecuteReader()
            Dim i As Integer = 0
            While (adr.Read)
                matr(i, 0) = adr.GetValue(0).ToString()
                matr(i, 1) = Math.Round(Convert.ToDouble(adr.GetValue(1)), 2, MidpointRounding.AwayFromZero).ToString()
                i = i + 1
            End While
            adr.Close()
            cmd.Dispose()
            buildChart(matr)
            DataGridView1.DataSource = Nothing
            DataGridView1.DataSource = CreateDataView(matr)
            conn.Close()
        End If
    End Sub

    Private Sub buildChart(matrix As Array)
        Chart1.Series.Clear()
        Dim noLines As Integer
        Dim noCols As Integer
        noCols = matrix.GetLength(1)
        noLines = matrix.GetLength(0) - 1
        MessageBox.Show(noCols & noLines)
        For k = 1 To noCols - 1
            Chart1.Series.Add(matrix(0, k))
        Next
        For i = 1 To noLines - 1
            For k = 1 To noCols - 1
                Chart1.Series(matrix(0, k)).Points.AddXY(matrix(i, 0), matrix(i, k))
            Next
        Next
    End Sub

    Function CreateDataView(matrix As Array) As ICollection
        Dim noLines As Integer
        Dim noCols As Integer
        noCols = matrix.GetLength(1)
        noLines = matrix.GetLength(0)
        Dim dt As New DataTable()
        Dim dr As DataRow

        If matrix.Length > 1 Then
            dt.Columns.Add(New DataColumn("Descpription", GetType(String)))
            dt.Columns.Add(New DataColumn("Probability", GetType(String)))
            Dim k, l As Integer
            For l = 0 To noLines - 1
                dr = dt.NewRow()
                For k = 0 To noCols - 1
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

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Dim getVal As String
        getVal = ComboBox2.SelectedItem.ToString()
        If ComboBox3.SelectedIndex >= 0 & getVal.Equals("Microsoft Clustering") Then
            conn = New AdomdConnection(fixstr & ComboBox1.SelectedItem.ToString())
            conn.Open()
            Dim q = ComboBox3.SelectedItem.ToString
            Dim cmd As AdomdCommand
            cmd = New AdomdCommand("select node_name, node_caption,node_support, node_description from [" & q & "].CONTENT where node_type = 5 and node_support > 100")
            cmd.Connection = conn
            Dim matr(get_adr_no_of_lines(cmd) + 1, 2)
            Dim adr As AdomdDataReader
            adr = cmd.ExecuteReader()
            Dim i As Integer = 0
            While (adr.Read)
                MessageBox.Show("Nume: " & adr.GetValue(0).ToString & "\n" &
                                 "Titlu: " & adr.GetValue(1).ToString & "\n" &
                          "Nodes no: " & adr.GetValue(2).ToString & "\n" &
                          "Description: " & adr.GetValue(3).ToString & "\n", "Informatii despre Cluster", MessageBoxButtons.OK, MessageBoxIcon.Information)
                matr(i, 0) = adr.GetValue(1).ToString()
                matr(i, 1) = adr.GetValue(2).ToString()
                i = i + 1
            End While
            buildChart(matr)
            adr.Close()
            cmd.Dispose()
            buildChart(matr)
            DataGridView1.DataSource = Nothing
            DataGridView1.DataSource = CreateDataView(matr)
            conn.Close()
        Else
            MessageBox.Show("Acest buton este doar pentru clsuter", "Buna")
        End If
    End Sub
End Class
