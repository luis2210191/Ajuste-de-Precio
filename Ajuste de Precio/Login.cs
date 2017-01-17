using Npgsql;
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
    public partial class Login : Form
    {
        public Login()
        {
            
            InitializeComponent();
        }
        //Boton para iniciar la sesion
        private void button1_Click(object sender, EventArgs e)
        {
            //Validacion que el campo de usuario y password no esten vacios
            if (textBox1.Text != "" && textBox2.Text != "")
            {
                try
                {
                    //String de conexion a la base de datos
                    string connectionString = @"Host=" + Globals.Host + ";port=" + Globals.port + ";Database=" + Globals.DB + ";User ID=" + Globals.usuario + ";Password=" + Globals.pass + ";";
                    string sql;
                    string userId = textBox1.Text;
                    string psw = textBox2.Text;
                    object count = null;
                    //Encriptacion de la contraseña
                    String txtxor = xorMsg(psw);
                    psw = Base64Encode(txtxor);
                    //Instancia de la conexion
                    NpgsqlConnection conn = new NpgsqlConnection(connectionString);

                    conn.Open();
                    //Consulta para verificar existencia del usuario
                    sql = @"SELECT COUNT(*) FROM admin.cfg_usu WHERE codigo='" + userId + "' AND pwd ='" + psw + "'";

                    NpgsqlCommand dbcmd = new NpgsqlCommand(sql, conn);
                    //Ejecutar consulta
                    count = dbcmd.ExecuteScalar();
                    //Si el usuario existe
                    if (count.ToString() == "1")
                    {
                     
                        Globals.userid = userId;
                        DialogResult = DialogResult.OK;
                    }
                    else
                    {
                        //Mensaje de error en caso que el usuario no exista
                        MessageBox.Show("Combinacion de Usuario y contraseña no es la correcta", "Atencion");
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("No hay conexion valida a la base de datos");
                }

            }
        }
        //Encriptacion usada para el login
        private static string xorMsg(string Msg)
        {
            try
            {
                string Key = "Inn0v4RlZ";
                char[] keys = Key.ToCharArray();
                char[] msg = Msg.ToCharArray();

                int ml = msg.Length;
                int kl = keys.Length;

                char[] newmsg = new char[ml];
                for (int i = 0; i < ml; i++)
                {
                    newmsg[i] = (char)(msg[i] ^ keys[i % kl]);
                }
                msg = null; keys = null;
                return new String(newmsg);
            }
            catch (Exception )
            {
                return null;
            }
        }
        //Encripcion base 64
        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
        //Desencripcion base 64
        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }
        //Boton de base de datos
        private void button2_Click(object sender, EventArgs e)
        {
            Dbconexion conn = new Dbconexion();
            conn.ShowDialog();
        }
    }
}
