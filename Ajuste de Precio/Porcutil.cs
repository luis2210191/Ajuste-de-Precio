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
        //Evento de carga de la ventana Porc_util
        private void Porcutil_Load(object sender, EventArgs e)
        {
            
            this.AcceptButton = button1;
            this.CancelButton = button2;
        }
        //Boton aceptar
        private void button1_Click(object sender, EventArgs e)
        {
            //Declaracion de dueño de la ventana (ventana ajustes)
            var f = Owner as Form1;
            if (f == null) return;
            //Asignacion de los porcentajes de utilidad al arreglo Porcutil
            f.Porcutil = new double[4]
            {Convert.ToDouble(textBox1.Text),
            Convert.ToDouble(textBox2.Text),
            Convert.ToDouble(textBox3.Text),
            Convert.ToDouble(textBox4.Text)
        };
            DialogResult = DialogResult.OK;
            //Cierre de ventana
            Close();
        }
        //Boton cancelar
        private void button2_Click(object sender, EventArgs e)
        {
            button1.DialogResult = DialogResult.Cancel;
        }
    }
}
