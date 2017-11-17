using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.AnalysisServices.AdomdClient;
using System.Collections;

namespace Predicition
{
    public partial class Form1 : Form
    {
        string connectionS = "Provider=SQLNCLI11.1;Data Source = STEFAN\\STEFAN;Integrated Security=SSPI;Initial Catalog=";
        public Form1()
        {
            InitializeComponent();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //eventul cand utilizatorul schimba catalogul
            using (var con = new AdomdConnection(connectionS + comboBox1.SelectedItem.ToString()))
            {
                con.Open();
                var cmd = new AdomdCommand("select distinct service_name from $system.dmschema_mining_models", con);
                var adr = cmd.ExecuteReader();
                int i = 0;
                comboBox2.Items.Clear();//cand se schimba cataogul valaore trebuei resetata
                while (adr.Read())
                {
                    comboBox2.Items.Add(adr.GetValue(0));
                    i += 1;
                }
                adr.Close();
                if (i == 0)//poate exista un catalog, dar sa nu fie o schema mining pe el
                {
                    comboBox2.SelectedIndex = -1;
                    comboBox2.Text = "";
                    comboBox3.SelectedIndex = -1;
                    comboBox3.Text = "";
                    comboBox3.Items.Clear();
                }
                else { comboBox2.SelectedIndex = 0; }
                con.Close();
                cmd.Dispose();
            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            //evenimentul cand utilizatorul schimba schema mining
            if (comboBox2.SelectedIndex >= 0)
            {
                using (var con = new AdomdConnection(connectionS + comboBox1.SelectedItem.ToString()))
                {
                    con.Open();
                    var cmd = new AdomdCommand("select model_name from $system.dmschema_mining_models where service_name='" +
                        comboBox2.SelectedItem.ToString() + "'", con);
                    var adr = cmd.ExecuteReader();
                    int i = 0;
                    comboBox3.Items.Clear();
                    while (adr.Read())
                    {
                        comboBox3.Items.Add(adr.GetValue(0));
                        i += 1;
                    }
                    adr.Close();
                    if (i == 0)
                    {
                        comboBox3.SelectedIndex = -1;
                        comboBox3.Text = "";
                    }
                    else { comboBox3.SelectedIndex = 0; }
                    con.Clone();
                    cmd.Dispose();
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (comboBox3.SelectedIndex >= 0)
            {
                using (var con = new AdomdConnection(connectionS + comboBox1.SelectedItem.ToString()))
                {
                    con.Open();
                    string q = comboBox3.SelectedItem.ToString();
                    var cmd = new AdomdCommand("select node_description, node_probability from [" + q + "].CONTENT order by node_probability desc", con);
                    string[,] matr = new string[get_adr_no_lines(cmd) + 1, 2];//cu grija la alocarea memoriei desupra cititorului, se vor intersecta si vor aparea erori
                    var adr = cmd.ExecuteReader();
                    int i = 0;
                    while (adr.Read())
                    {
                        matr[i, 0] = adr.GetValue(0).ToString();
                        matr[i, 1] = Math.Round(Convert.ToDouble(adr.GetValue(1)), 2, MidpointRounding.AwayFromZero).ToString();
                        i += 1;
                    }
                    adr.Close();
                    cmd.Dispose();
                    buildChart(matr);
                    dataGridView1.DataSource = CreateDataView(matr);
                    con.Close();
                    dataGridView2.Visible = false;
                    chart1.Visible = true;
                }
            }
        }

        void buildChart(string[,] matr)
        {
            chart1.Series.Clear();
            int noLines = matr.GetLength(0) - 1, noCols = matr.GetLength(1);
            Console.WriteLine(noLines + noCols);
            for (int k = 1; k < noCols; k++)
                chart1.Series.Add(matr[0, k]);
            for (int l = 1; l < noLines; l++)
                for (int k = 1; k < noCols; k++)
                    chart1.Series[matr[0, k]].Points.AddXY(matr[l, 0], matr[l, k]);
        }
        private ICollection CreateDataView(string[,] matr)
        {
            int noLines = matr.GetLength(0), noCols = matr.GetLength(1);
            var dt = new DataTable();
            DataRow dr;
            if (matr.Length > 1)
            {
                dt.Columns.Add(new DataColumn("Descriere", typeof(string)));
                dt.Columns.Add(new DataColumn("Probabilitatea", typeof(string)));
                for (int l = 0; l < noLines; l++)
                {
                    dr = dt.NewRow();
                    for (int k = 0; k < noCols; k++)
                    {
                        dr[k] = matr[l, k];
                    }
                    dt.Rows.Add(dr);
                }
            }
            var dv = new DataView(dt);
            return dv;

        }

        int get_adr_no_lines(AdomdCommand cmd)
        {
            int i = 0;
            var adr = cmd.ExecuteReader();
            while (adr.Read())
            {
                i += 1;
            }
            adr.Close();
            return i;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //pnetru cluster numai
            string getVal = comboBox2.SelectedItem.ToString();//cam fortat, dar merge
            if (comboBox3.SelectedIndex >= 0 && getVal.Equals("Microsoft_Clustering"))
            {
                using (var con = new AdomdConnection(connectionS + comboBox1.SelectedItem.ToString()))
                {
                    con.Open();
                    string q = comboBox3.SelectedItem.ToString();
                    var cmd = new AdomdCommand("select node_name, node_caption,node_support, node_description from [" + q + "].CONTENT where node_type = 5 and node_support > 100", con);
                    string[,] matr = new string[get_adr_no_lines(cmd) + 1, 2];//cu grija la alocarea memoriei desupra cititorului, se vor intersecta si vor aparea erori
                    var adr = cmd.ExecuteReader();
                    int i = 0;
                    while (adr.Read())
                    {
                        MessageBox.Show("Numele: " + adr.GetValue(0).ToString() + "\n" +
                          "Titlu: " + adr.GetValue(1).ToString() + "\n" +
                          "Numarul Nodului: " + adr.GetValue(2).ToString() + "\n" +
                          "Descrierea: " + adr.GetValue(3).ToString() + "\n", "Inforamtii despre Cluster", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        matr[i, 0] = adr.GetValue(1).ToString();
                        matr[i, 1] = adr.GetValue(2).ToString();
                        i += 1;
                    }
                    buildChart(matr);
                    adr.Close();
                    cmd.Dispose();
                    buildChart(matr);
                    dataGridView1.DataSource = CreateDataView(matr);
                    con.Close();
                    dataGridView2.Visible = false;
                    chart1.Visible = true;
                }
            }
            else
            {
                MessageBox.Show("Acest buton este doar pentru cluster", "Buna");
            }
        }

   
        private void Form1_Load(object sender, EventArgs e)
        {
            populareCuCatalog();
            dataGridView2.Visible = false;
        }

        void populareCuCatalog()
        {
            string chooseCatalog = "DataMining";
            using (var con = new AdomdConnection(connectionS + chooseCatalog))
            {
                con.Open();
                var cmd = new AdomdCommand("select * from $SYSTEM.DBSCHEMA_Catalogs", con);
                var adr = cmd.ExecuteReader();
                string m; int i = 0;
                while (adr.Read())
                {
                    m = adr.GetValue(0).ToString();
                    comboBox1.Items.Add(m);
                    i += 1;
                }
                comboBox1.SelectedIndex = 0;
                adr.Close();
                con.Close();
                cmd.Dispose();
            }
        }

        public static string query = "";
        int inc = 0;
        Dictionary<int, string> dict = new Dictionary<int, string>();
       
        void executare()
        {
            chart1.Visible = false;
            dataGridView1.Visible = true;
            this.Cursor = Cursors.WaitCursor;
            DateTime t0 = DateTime.Now;
            run_query(query);
            DateTime t1 = DateTime.Now;
            inc++;
            dict.Add(inc, query);
            dataGridView2.Rows.Add(new String[] {(inc).ToString(), t0.ToShortDateString()+"/"+t1.ToShortTimeString(),
            dataGridView2.Rows.Count.ToString(),(t1-t0).TotalSeconds.ToString(),query,exception});
            dataGridView2.AutoResizeColumn(0, DataGridViewAutoSizeColumnMode.DisplayedCells);
            dataGridView2.AutoResizeColumn(1, DataGridViewAutoSizeColumnMode.DisplayedCells);
            dataGridView2.AutoResizeColumn(2, DataGridViewAutoSizeColumnMode.DisplayedCells);
            dataGridView2.AutoResizeColumn(3, DataGridViewAutoSizeColumnMode.DisplayedCells);
            this.Cursor = Cursors.Default;
        }

        string exception;
        void run_query(string q)
        {
            try
            {
                using (var con = new AdomdConnection(connectionS + comboBox1.SelectedItem.ToString()))
                {
                    con.Open();
                    var cmd = new AdomdCommand(q, con);
                    var a = new AdomdDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    a.Fill(dt);
                    dataGridView1.DataSource = dt;
                    dataGridView1.AutoResizeColumns();
                    dataGridView1.AutoResizeRows();
                }
            }
            catch (AdomdException ex)
            {
                Console.WriteLine(ex);
                exception = ex.ToString();
                MessageBox.Show("Interogarea gresita", "Reincercati", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Form2 f2 = new Form2();
            f2.ShowDialog();
            executare();
            dataGridView2.Visible = true;   
        }

        private void dataGridView2_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            int index = dataGridView2.SelectedRows[0].Index + 1;
            foreach (var pair in dict)
            {
                if (pair.Key == index)
                    run_query(pair.Value);
            }
        }

    }
}
