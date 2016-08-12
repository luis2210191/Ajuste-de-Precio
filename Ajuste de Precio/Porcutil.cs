using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ajuste_de_Precio
{
    public partial class Porcutil : Form
    {
        public Porcutil()
        {
            InitializeComponent();
        }

        private void Porcutil_Load(object sender, EventArgs e)
        {
            
            this.AcceptButton = button1;
            this.CancelButton = button2;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var f = Owner as Form1;
            if (f == null) return;
            f.Porcutil = new double[4]
            {Convert.ToDouble(textBox1.Text),
            Convert.ToDouble(textBox2.Text),
            Convert.ToDouble(textBox3.Text),
            Convert.ToDouble(textBox4.Text)
        };
            DialogResult = DialogResult.OK;

            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            button1.DialogResult = DialogResult.Cancel;
        }
    }
}
