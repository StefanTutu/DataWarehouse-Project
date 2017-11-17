Imports Microsoft.AnalysisServices.AdomdClient

Public Class OLAP

    Function ConnectToMeasures() As Array
        Dim m1 As Array = Nothing
        Try
            'setam stringul de conectare'
            Dim conn As New AdomdConnection("Provider=SQLNCLI11.1;Data Source=STEFAN\STEFAN;" & _
                                   "Integrated Security=SSPI;Initial Catalog=Cubul de date")
            conn.Open()
            Dim cmd As New AdomdCommand("select {[Measures].AllMembers} on 0 " & _
                "from [Cubul Reseller]")
            cmd.Connection = conn
            'apelam functia'
            m1 = connect(cmd, 1)
            conn.Close()
        Catch ex As Exception
            Console.WriteLine("Eroare ")
            MessageBox.Show("Eroare connect to measures")
        End Try
        Return m1
    End Function

    Function connectToData(measure As String) As Array
        Dim m2 As Array = Nothing
        Try
            'setam stringul de conectare'
            Dim conn As New AdomdConnection("Provider=SQLNCLI11.1;Data Source=STEFAN\STEFAN;" & _
                                   "Integrated Security=SSPI;Initial Catalog=Cubul de date")
            conn.Open()
            'MessageBox.Show("Conexiune deschisa")
            Dim cmd As New AdomdCommand("select non empty [Order Date].[All].Children ON rows," & _
                "non empty [Dim Product].[All].children on columns from [Cubul Reseller]" & _
                "where " & measure)
            cmd.Connection = conn
            'apelam functia'
            m2 = connect(cmd, 2)
            conn.Close()
        Catch ex As Exception
            MessageBox.Show("Eroare connect to data")
        End Try
        Return m2
    End Function

    Function connect(cmd As AdomdCommand, AxesNo As Integer) As Array
        'declaram numarul de linii'
        Dim noLines As Integer
        'declaram numarul de coloane'
        Dim noCols As Integer
        Dim matrix(0, 0) As String
        Dim cs As CellSet
        'setam cell set'
        cs = cmd.ExecuteCellSet
        'setam marimea pentru numerele de coloane'
        noCols = cs.Axes(0).Positions.Count
        'daca axa este egala cu 1 incrementam numarul de linii cu 1, altfel numarum numarul de linii disponibile in axa Y'
        If AxesNo = 1 Then
            noLines = 1
        Else
            noLines = cs.Axes(1).Positions.Count
        End If
        Dim axis As Axis
        If AxesNo > 1 Then
            'redimensionam matricea'
            ReDim matrix(noCols, noLines)
            'este gol pentru ca nu avem valoare in coltul din stanga sus'
            matrix(0, 0) = ""
            'ne uitam prin coloane'
            For i = 0 To noCols - 1
                'obtinem caption pe coloane'
                matrix(0, i + 1) = cs.Axes(0).Positions(i).Members(0).Caption
                axis = cs.Axes(1)
                For j = 0 To noLines - 1
                    'obtinem caption pe randuri'
                    matrix(j + 1, 0) = axis.Positions(j).Members(0).Caption
                    'rotunjim valoarea'
                    matrix(j + 1, i + 1) = Math.Round(cs(i, j).Value, 2, MidpointRounding.AwayFromZero)
                Next

            Next
        Else
            'redimensionam matricea'
            ReDim matrix(noLines - 1, noCols)
            'este gol pentru ca nu avem valoare in coltul din stanga sus'
            matrix(0, 0) = ""
            For i = 0 To noCols - 1
                matrix(0, i + 1) = cs.Axes(0).Positions(i).Members(0).Caption
            Next
        End If
        Return matrix
    End Function

    Function CreateDataView(matrix As Array) As ICollection
        Dim noLines As Integer
        Dim noCols As Integer
        'extragem lungimea coloanei 1'
        noCols = matrix.GetLength(1) - 1
        'extragem lungimea randului 0'
        noLines = matrix.GetLength(0) - 1
        'instantiem o nou data table'
        Dim dt As New DataTable()
        'un nou dataRow'
        Dim dr As DataRow
        'adaugam valorile in vizualizarea de date pentru crearea unei surse care va popula mai tarziu data gridul'
        If matrix.Length > 1 Then
            dt.Columns.Add(New DataColumn("OY caption", GetType(String)))
            Dim k, l As Integer
            For k = 0 To noCols - 1
                dt.Columns.Add(New DataColumn(matrix(0, k + 1), GetType(String)))
            Next
            For l = 1 To noLines
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

    Private Sub buildChar(matrix As Array)
        Chart1.Series.Clear()
        Chart1.ResetAutoValues()
        Dim noLines As Integer
        Dim noCols As Integer
        'extragem numarul de coloane'
        noCols = matrix.GetLength(1) - 1
        'extragem numarul de randuri'
        noLines = matrix.GetLength(0) - 1
        For k = 2 To noCols
            Chart1.Series.Add(matrix(0, k))

            'MessageBox.Show("Valoare Matrix este " & matrix(0, k))
        Next
        For l = 1 To noLines
            For k = 2 To noCols
                'evitam prima valoare pentru ca este caption(text) header '
                Chart1.Series(matrix(0, k)).Points.AddXY(matrix(l, 0), matrix(l, k))
            Next
        Next
    End Sub

    'Private Sub Button2_Click(sender As Object, e As EventArgs)
    'Dim measure As String = "[Measures].[" & ComboBox1.SelectedItem.ToString() & "]"
    '   buildChar(connectToData(measure))
    'End Sub

    Private Sub OLAP_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim m1 As Array = Nothing
        Dim m2 As Array = Nothing
        m1 = ConnectToMeasures()
        For i = 1 To m1.GetLength(1) - 1
            ComboBox1.Items.Add(m1(0, i))
        Next
        ComboBox1.SelectedIndex = 3
        Dim measure As String = "[Measures].[Sales Amount]"
        m2 = connectToData(measure)
        DataGridView1.DataSource = CreateDataView(m2)
        buildChar(m2)
    End Sub

    Private Sub ComboBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBox1.SelectedIndexChanged
        DataGridView1.DataSource = Nothing
        Dim measure As String = "[Measures].[" & ComboBox1.SelectedItem.ToString() & "]"
        DataGridView1.DataSource = CreateDataView(connectToData(measure))
        buildChar(connectToData(measure))
    End Sub
End Class
