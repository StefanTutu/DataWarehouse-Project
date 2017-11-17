using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Predicition
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }
        Form1 f1 = new Form1();
        private void button1_Click(object sender, EventArgs e)
        {
            Form1.query = textBox1.Text;
            this.Hide();
        }
    }
}
