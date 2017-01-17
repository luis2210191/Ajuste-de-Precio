using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ajuste_de_Precio
{
    public partial class Form1 : Form
    {
        //Declaracion de variables 
        string connectionstring;
        string sql;
        double valorIni;
        public double[] Porcutil { get; set; }
        public bool status { get; set; }
        public int utilidad = 0;
        public static bool documento = false;
        //Declaracion de dataTables
        DataTable inventario = new DataTable();
        DataTable Codigo1 = new DataTable();
        DataSet DtSet = new DataSet(); 
        DataTable Codigo2 = new DataTable();
        //Declaracion de objetos de conexion
        System.Data.OleDb.OleDbConnection Myconnetion;
        System.Data.OleDb.OleDbDataAdapter MyCommand;
        public Form1()
        {
            Login fLogin = new Login();
            if (fLogin.ShowDialog() == DialogResult.Cancel)
            {
                Environment.Exit(-1);
            }
            InitializeComponent();
        }
        //Evento al cargar la ventana
        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                DataTable categoria = new DataTable();
                //Conexion a la base de datos
                connectionstring = @"Host=" + Globals.Host + ";port=" + Globals.port + ";Database=" + Globals.DB + ";User ID=" + Globals.usuario + ";Password=" + Globals.pass + ";";
                NpgsqlConnection conn = new NpgsqlConnection(connectionstring);
                conn.Open();
                //Consulta para cargar las categorias
                sql = "select cat_hijo,descri from admin.inv_cat";
                NpgsqlCommand com = new NpgsqlCommand(sql, conn);
                NpgsqlDataAdapter ad = new NpgsqlDataAdapter(com);
                ad.Fill(categoria);
                //Consulta de inventario en base a la categoria seleccionada
                sql = @"SELECT a.codigo, a.descri FROM admin.inv_art AS a JOIN admin.inv_cat_art as b on a.codigo=b.cod_articulo where b.cat_hijo='" + comboBox1.SelectedValue + "' ";
                com = new NpgsqlCommand(sql, conn);
                ad = new NpgsqlDataAdapter(com);
                ad.Fill(Codigo1);
                ad.Fill(Codigo2);
                //Cierre de conexion a la base de datos
                conn.Close();
                DataRow workRow = categoria.NewRow();
                workRow["descri"] = "Ninguna";
                workRow["cat_hijo"] = "Ninguna";
                categoria.Rows.Add(workRow);
                comboBox1.DataSource = categoria;
                comboBox1.DisplayMember = "descri";
                comboBox1.ValueMember = "cat_hijo";
                //Asignando origen de datos a combo box para filtro desde
                comboBox2.DataSource = Codigo1;
                comboBox2.DisplayMember = "codigo";
                comboBox2.ValueMember = "descri";

                //Asignando origen de datos a combo box para filtro hasta
                comboBox3.DataSource = Codigo2;
                comboBox3.DisplayMember = "codigo";
                comboBox3.ValueMember = "descri";

                //Obteniendo organizacion de tabla de cfg_org
                sql = @"SELECT org_hijo from admin.cfg_org";
                conn = new NpgsqlConnection(connectionstring);
                NpgsqlCommand dbcmd = new NpgsqlCommand(sql, conn);
                conn.Open();
                Globals.org = dbcmd.ExecuteScalar().ToString();

                //Obteniendo tipo de calculo de utilidad de tabla cfg_preferencia
                sql = "select tipo_porc_utilidad from admin.cfg_preferencia";
                dbcmd = new NpgsqlCommand(sql, conn);
                Globals.pref = (int)dbcmd.ExecuteScalar();
                conn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        //Boton para realizar la busqueda 
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                documento = false;
                string condicion = "";
                //Conexion con la BD
                NpgsqlConnection conn = new NpgsqlConnection(connectionstring);
                conn.Open();
                //Si la busqueda es por categoria
                if (radioButton1.Checked == true)
                {
                    if (checkBox1.Checked == false && comboBox1.Text != "Ninguna")
                    {
                        condicion = @"AND b.cat_hijo ='" + comboBox1.SelectedValue + "'";
                    }else if (checkBox1.Checked == false && comboBox1.Text == "Ninguna")
                    {
                        condicion = "";
                    }
                    else if (checkBox1.Checked == true && comboBox1.Text == "Ninguna")
                    {
                        condicion = @"AND a.codigo BETWEEN '" + comboBox2.SelectedValue + "' AND '" + comboBox3.SelectedValue + "'";
                    }
                    else
                    {
                        condicion = @"AND b.cat_hijo ='" + comboBox1.SelectedValue + "' AND a.codigo BETWEEN '" + comboBox2.Text + "' AND '" + comboBox3.Text + "'";
                    }

                }

                //Si la busqueda es por codigo
                else if (radioButton2.Checked == true)
                {
                    condicion = @"AND a.codigo ='" + textBox1.Text + "'";
                }

                //Obteniendo la lista de los productos en base a la condicion establecida (filtro por categoria, codigo o todos)
                sql = @"select distinct a.codigo,a.cod_interno,a.costo_pro, a.costo, a.descri, d.utilidad1 as porc_util1,
                        d.utilidad2 as porc_util2, d.utilidad3 as porc_util3, 
                        d.utilidad4 as porc_util4,e.utilidad1, e.utilidad2, e.utilidad3, 
                        e.utilidad4, c.precio1, c.precio2, c.precio3, c.precio4 
                        from admin.inv_art as a LEFT Join admin.inv_cat_art as b on a.org_hijo = b.org_hijo 
                        and a.codigo = b.cod_articulo JOIN admin.tvinv003_p as c on a.codigo = c.codigo_art
                        JOIN admin.tvinv003_u as d on a.codigo = d.codigo_art
                        JOIN admin.tvinv003_um as e on a.codigo = e.codigo_art WHERE tipo_art='11.1' " + condicion +"ORDER BY a.cod_interno";

                NpgsqlCommand com = new NpgsqlCommand(sql, conn);
                NpgsqlDataAdapter ad = new NpgsqlDataAdapter(com);
                //Limpieza de dataTbale de inventario 
                inventario.Clear();
                //Asignacion de resultado de la consulta al dataTable de inventario
                ad.Fill(inventario);
                dataGridView1.AutoGenerateColumns = false;
                dataGridView1.DataSource = null;
                dataGridView1.Rows.Clear();
                //Asignacion de origen de datos de dataGridView1
                dataGridView1.DataSource = inventario;
                conn.Close();
                //Recorrido del datagridview
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {

                    foreach (DataGridViewCell cell in row.Cells)
                    {
                        //Cambio den propiedad solo lectura a columnas a exepcion de codigo, descripcion, costo, restaurar, modificado
                        if (cell.OwningColumn.Name != "codigo" && cell.OwningColumn.Name != "descripcion" && cell.OwningColumn.Name != "costo" && cell.OwningColumn.Name != "Restaurar" && cell.OwningColumn.Name != "Modificado")
                        {
                            cell.ReadOnly = false;
                        }
                    }
                }
            }
            catch (Exception )
            {
                //Mensaje de error durante el proceso de carga de informacion
                MessageBox.Show("Hubo un problema al cargar la informacion", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //Evento de cambio de valor del radioButton de "categoria"
        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked == true)
            {
                comboBox1.Enabled = true;
                textBox1.Enabled = false;
                checkBox1.Enabled = true;
            }
        }

        //Evento de cambio de valor del radioButton de "codigo"
        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked == true)
            {
                comboBox2.Enabled = false;
                comboBox3.Enabled = false;
                comboBox1.Enabled = false;
                textBox1.Enabled = true;
                checkBox1.Enabled = false;
            }
        }
        //Evento de cambio de valor del radioButton de "todos"
        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {

            if (radioButton3.Checked == true)
            {
                comboBox2.Enabled = false;
                comboBox3.Enabled = false;
                comboBox1.Enabled = false;
                textBox1.Enabled = false;
                checkBox1.Enabled = false;
            }
        }

        //Evento de cambio de valor del checkBox de "Por rango"
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            //si el checkBox de categoria y de codigo ambos estan marcados
            if (checkBox1.Checked == true && radioButton1.Checked == true)
            {
                comboBox2.Enabled = true;
                comboBox3.Enabled = true;
            }
            else
            {
                comboBox2.Enabled = false;
                comboBox3.Enabled = false;
            }
        }

        //Evento de cambio de seleccion en el comboBox
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Si la categoria seleccionada es Ninguna la consulta solo verifica el codigo del articulo, de haber seleccionado una categoria en la consulta se toma en cuenta tanto el codigo del articulo como la categoria seleccionada
            if (comboBox1.SelectedValue.ToString() == "Ninguna")
            {
                sql = @"SELECT a.codigo, a.descri FROM admin.inv_art AS a JOIN admin.inv_cat_art as b on a.codigo=b.cod_articulo";

            }
            else
            {
                sql = @"SELECT a.codigo, a.descri FROM admin.inv_art AS a JOIN admin.inv_cat_art as b on a.codigo=b.cod_articulo where b.cat_hijo='" + comboBox1.SelectedValue + "' ";

            }
            //Conexion con la base de datos
            NpgsqlConnection conn = new NpgsqlConnection(connectionstring);
            conn.Open();
            Codigo1.Clear();
            Codigo2.Clear();
            NpgsqlCommand com = new NpgsqlCommand(sql, conn);
            NpgsqlDataAdapter ads = new NpgsqlDataAdapter(com);
            ads.Fill(Codigo1);
            ads.Fill(Codigo2);
            conn.Close();

            comboBox2.DataSource = Codigo1;
            comboBox2.DisplayMember = "codigo";
            comboBox2.ValueMember = "descri";

            comboBox3.DataSource = Codigo2;
            comboBox3.DisplayMember = "codigo";
            comboBox3.ValueMember = "descri";
        }
        //Evento al finalizar edicion de celda dentro del grid
        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                //Si el valor inicial de la celda es distinto del valor actual
                if (valorIni != Convert.ToDouble(dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value))
                {
                    int index = e.ColumnIndex;

                    string columnName = dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].OwningColumn.Name;

                    int size = columnName.Length;

                    string lastChar = columnName.Substring(size - 1);
                    //Si el cambio se hizo en la columna de porc_util se recalculan precio y utilidad
                    if (dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].OwningColumn.Name.Contains("porc_util"))
                    {
                        dataGridView1.Rows[e.RowIndex].Cells["tipoAjuste"].Value = 1;
                        dataGridView1.Rows[e.RowIndex].Cells["precio" + lastChar + ""].Value = calculoPrecio(Globals.pref, Convert.ToDecimal(dataGridView1.Rows[e.RowIndex].Cells["costo_pro"].Value), Convert.ToDecimal(dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value));
                        dataGridView1.Rows[e.RowIndex].Cells["utilidad" + lastChar + ""].Value = calculoUtilidad(Convert.ToDecimal(dataGridView1.Rows[e.RowIndex].Cells["costo_pro"].Value), Convert.ToDecimal(dataGridView1.Rows[e.RowIndex].Cells["precio" + lastChar + ""].Value));

                    }
                    //Si el cambio se hizo en la columna de utilidad se recalcula precio y porc_util
                    else if (dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].OwningColumn.Name.Contains("utilidad"))
                    {
                        dataGridView1.Rows[e.RowIndex].Cells["tipoAjuste"].Value = 1;
                        dataGridView1.Rows[e.RowIndex].Cells["precio" + lastChar + ""].Value = Convert.ToDouble(dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value) + Convert.ToDouble(dataGridView1.Rows[e.RowIndex].Cells["costo_pro"].Value);
                        dataGridView1.Rows[e.RowIndex].Cells["porc_util" + lastChar + ""].Value = calculoPorcentaje(Globals.pref, Convert.ToDecimal(dataGridView1.Rows[e.RowIndex].Cells["precio" + lastChar + ""].Value), Convert.ToDecimal(dataGridView1.Rows[e.RowIndex].Cells["costo_pro"].Value));
                    }
                    //Si se modifica precio se modifica porc_util u utilidad
                    else
                    {
                        dataGridView1.Rows[e.RowIndex].Cells["tipoAjuste"].Value = 0;
                        dataGridView1.Rows[e.RowIndex].Cells["porc_util" + lastChar + ""].Value = calculoPorcentaje(Globals.pref, Convert.ToDecimal(dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value), Convert.ToDecimal(dataGridView1.Rows[e.RowIndex].Cells["costo_pro"].Value));
                        dataGridView1.Rows[e.RowIndex].Cells["utilidad" + lastChar + ""].Value = calculoUtilidad(Convert.ToDecimal(dataGridView1.Rows[e.RowIndex].Cells["costo_pro"].Value), Convert.ToDecimal(dataGridView1.Rows[e.RowIndex].Cells["precio" + lastChar + ""].Value));
                    }

                    //Recorrido
                    foreach (DataGridViewCell cell in dataGridView1.Rows[e.RowIndex].Cells)
                    {
                        if (cell.OwningColumn.Name != "codigo" && cell.OwningColumn.Name != "descripcion" && cell.OwningColumn.Name != "costo" && cell.OwningColumn.Name != "Restaurar" && cell.OwningColumn.Name != "Modificado")
                        {
                            //Al modificar una de las 3 columnas (porc_util, precio o utilidad) las otras dos se vuelven de solo lectura y se colocan en gris para hacer referencia al hecho que ya no pueden ser editadas
                            if (!cell.ReadOnly && cell.OwningColumn.Name.Contains(lastChar) && cell.ColumnIndex != index)
                            {
                                cell.ReadOnly = true;
                                cell.Style.BackColor = Color.Gray;
                            }
                        }
                    }
                    //Se cambia el valor del campo modificado a verdadero para que sea aparente que la data en esa fila se modifico
                    dataGridView1.Rows[e.RowIndex].Cells[0].Value = true;
                }
            }catch(Exception x)
            {
                MessageBox.Show(x.Message);
            }
            
        }

        //Evento del inicio de modificacion de edicion de una columna para capturar valor inicial
        private void dataGridView1_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            valorIni = Convert.ToDouble(dataGridView1.SelectedCells[0].Value);
        }
        //Click del boton seleccionar todos para aplicar un porcentaje de utilidad a todos los registros simultaneamente
        private void button4_Click(object sender, EventArgs e)
        {
            using (var f = new Porcutil() { Owner = this })
            {
                f.ShowDialog();
                if (f.DialogResult == DialogResult.OK)
                {
                    //Recorrido del dataGridView
                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        foreach (DataGridViewCell cell in row.Cells)
                        {
                            //Verificando cual porcentaje de utilidad se modificara
                            switch (cell.OwningColumn.Name)
                            {
                                case "porc_util1":
                                    {
                                        cell.Value = Porcutil[0];
                                        row.Cells["utilidad1"].ReadOnly = true;
                                        row.Cells["utilidad1"].Style.BackColor = Color.Gray;
                                        row.Cells["precio1"].ReadOnly = true;
                                        row.Cells["precio1"].Style.BackColor = Color.Gray;
                                        break;
                                    }
                                case "porc_util2":
                                    {
                                        cell.Value = Porcutil[1];
                                        row.Cells["utilidad2"].ReadOnly = true;
                                        row.Cells["utilidad2"].Style.BackColor = Color.Gray;
                                        row.Cells["precio2"].ReadOnly = true;
                                        row.Cells["precio2"].Style.BackColor = Color.Gray;
                                        break;
                                    }
                                case "porc_util3":
                                    {
                                        cell.Value = Porcutil[2];
                                        row.Cells["utilidad3"].ReadOnly = true;
                                        row.Cells["utilidad3"].Style.BackColor = Color.Gray;
                                        row.Cells["precio3"].ReadOnly = true;
                                        row.Cells["precio3"].Style.BackColor = Color.Gray;
                                        break;
                                    }
                                case "porc_util4":
                                    {
                                        cell.Value = Porcutil[3];
                                        row.Cells["utilidad4"].ReadOnly = true;
                                        row.Cells["utilidad4"].Style.BackColor = Color.Gray;
                                        row.Cells["precio4"].ReadOnly = true;
                                        row.Cells["precio4"].Style.BackColor = Color.Gray;
                                        break;
                                    }
                            }

                        }
                        row.Cells["Modificado"].Value = true;                       
                    }
                    

                }
            }
        }
        //Boton para realizar el ajuste
        private void button5_Click(object sender, EventArgs e)
        {
            string codigos;
            int COUNT = 1;
            double TOTALP = 0;
            int cantidad = 0;
            bool STATUS = false;
            //Conexion a la base de datos
            NpgsqlConnection conn = new NpgsqlConnection(connectionstring);
            conn.Open();
            try
            {
                //Inicio de la transaccion
                NpgsqlTransaction t = conn.BeginTransaction();
                NpgsqlCommand com = new NpgsqlCommand();
                //Si no es cargado directamente de un documento
                if (!documento)
                {
                    //Recorrido del datagridview
                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        //Si la fila fue modificada se actualiza la data
                        if (Convert.ToBoolean(row.Cells[0].Value) == true)
                        {
                            STATUS = true;
                            //Acumulador de costo total del ajuste
                            TOTALP += Convert.ToDouble(row.Cells["Costo"].Value);
                            codigos = row.Cells[3].Value.ToString();
                            //Repeticion de la consulta por cada uno de los porcentajes de utilidad
                            for (COUNT = 1; COUNT <= 4; COUNT++)
                            {
                                sql = @"UPDATE admin.inv_art_precio SET porc_utilidad=@porcutilidad" + COUNT + ",utilidad=@utilidad" + COUNT + ",precio =@precio" + COUNT + " WHERE cod_articulo = @codigo AND cod_precio='0" + COUNT + "'";
                                com = new NpgsqlCommand(sql, conn);
                                com.Parameters.Add(new NpgsqlParameter("@porcutilidad" + COUNT + "", NpgsqlDbType.Double));
                                com.Parameters.Add(new NpgsqlParameter("@utilidad" + COUNT + "", NpgsqlDbType.Double));
                                com.Parameters.Add(new NpgsqlParameter("@precio" + COUNT + "", NpgsqlDbType.Double));
                                com.Parameters.Add(new NpgsqlParameter("@codigo", NpgsqlDbType.Varchar));
                                com.Prepare();
                                com.Parameters[0].Value = row.Cells["porc_util" + COUNT + ""].Value.ToString().Replace(".", ",");
                                com.Parameters[1].Value = row.Cells["utilidad" + COUNT + ""].Value.ToString().Replace(".", ",");
                                com.Parameters[2].Value = row.Cells["precio" + COUNT + ""].Value.ToString().Replace(".", ",");
                                com.Parameters[3].Value = row.Cells["codigo"].Value.ToString().Replace(" ", string.Empty);
                                com.ExecuteNonQuery();
                            }

                            cantidad++;
                        }
                    }
                    if (STATUS == true)
                    {
                        //Ajuste de precio
                        sql = @"INSERT INTO admin.int_ajuste_precio(org_hijo,descri,total_precio,total_utilidad, 
                    reg_usu_cc, reg_estatus, nro_items,tipo_opera) VALUES(@org_hijo,@descri,@total_precio,@total_utilidad, 
                    @reg_usu_cc, @reg_estatus, @nro_items, @topera)";
                        com = new NpgsqlCommand(sql, conn);
                        com.Parameters.Add(new NpgsqlParameter("@org_hijo", NpgsqlDbType.Varchar));
                        com.Parameters.Add(new NpgsqlParameter("@descri", NpgsqlDbType.Varchar));
                        com.Parameters.Add(new NpgsqlParameter("@total_precio", NpgsqlDbType.Double));
                        com.Parameters.Add(new NpgsqlParameter("@total_utilidad", NpgsqlDbType.Double));
                        com.Parameters.Add(new NpgsqlParameter("@reg_usu_cc", NpgsqlDbType.Varchar));
                        com.Parameters.Add(new NpgsqlParameter("@reg_estatus", NpgsqlDbType.Integer));
                        com.Parameters.Add(new NpgsqlParameter("@nro_items", NpgsqlDbType.Integer));
                        com.Parameters.Add(new NpgsqlParameter("@topera", NpgsqlDbType.Integer));

                        com.Prepare();

                        com.Parameters[0].Value = Globals.org;
                        com.Parameters[1].Value = "AJUSTE DE PRECIOS";
                        com.Parameters[2].Value = TOTALP;
                        com.Parameters[3].Value = "0";
                        com.Parameters[4].Value = "INNOVA";
                        com.Parameters[5].Value = 1;
                        com.Parameters[6].Value = cantidad;
                        com.Parameters[7].Value = 36;

                        com.ExecuteNonQuery();

                        //Obtencion del numero del ajuste de precio para aplicar al detalle
                        sql = @"SELECT doc from admin.int_ajuste_precio order by fecha_reg desc";
                        com = new NpgsqlCommand(sql, conn);
                        string reader = com.ExecuteScalar().ToString();

                        //Insercion del detalle del ajuste
                        int Item = 1;

                        //Segundo recorrido del dataGridView para realizacion del detalle del ajuste de precio
                        foreach (DataGridViewRow ROW2 in dataGridView1.Rows)
                        {
                            if (Convert.ToBoolean(ROW2.Cells[0].Value) == true)
                            {
                                //Consulta para realizacion del detalle del ajuste de precio
                                sql = @"INSERT INTO admin.int_ajuste_precio_det(org_hijo,doc,cod_alterno,cod_articulo,
                        costo,costo_promedio,fecha,tipo_ajuste,item) VALUES(@org_hijo,@doc,
                        @cod_alterno,@cod_articulo,@costo,@costo_promedio,@fecha,@tipo_ajuste,@item)";
                                com = new NpgsqlCommand(sql, conn);
                                com.Parameters.Add(new NpgsqlParameter("@org_hijo", NpgsqlDbType.Varchar));
                                com.Parameters.Add(new NpgsqlParameter("@doc", NpgsqlDbType.Bigint));
                                com.Parameters.Add(new NpgsqlParameter("@cod_alterno", NpgsqlDbType.Varchar));
                                com.Parameters.Add(new NpgsqlParameter("@cod_articulo", NpgsqlDbType.Varchar));
                                com.Parameters.Add(new NpgsqlParameter("@costo", NpgsqlDbType.Double));
                                com.Parameters.Add(new NpgsqlParameter("@costo_promedio", NpgsqlDbType.Double));
                                com.Parameters.Add(new NpgsqlParameter("@fecha", NpgsqlDbType.Date));
                                com.Parameters.Add(new NpgsqlParameter("@tipo_ajuste", NpgsqlDbType.Integer));
                                com.Parameters.Add(new NpgsqlParameter("@item", NpgsqlDbType.Integer));

                                com.Prepare();

                                com.Parameters[0].Value = Globals.org;
                                com.Parameters[1].Value = Convert.ToInt64(reader);
                                com.Parameters[2].Value = ROW2.Cells["codigo"].Value.ToString().Replace(" ", string.Empty);
                                com.Parameters[3].Value = ROW2.Cells["codigo"].Value.ToString().Replace(" ", string.Empty);
                                com.Parameters[4].Value = ROW2.Cells["costo"].Value;
                                com.Parameters[5].Value = ROW2.Cells["costo"].Value;
                                com.Parameters[6].Value = DateTime.Now;
                                com.Parameters[7].Value = ROW2.Cells["tipoAjuste"].Value;
                                com.Parameters[8].Value = Item;
                                com.ExecuteNonQuery();
                                Item++;
                            }
                        }
                    }
                }
                //Si la informacion se cargo por documento
                else
                {
                    //Recorrido del dataGridView
                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {

                        
                        STATUS = true;
                        TOTALP += Convert.ToDouble(row.Cells["costo_pro"].Value);
                        //Repeticion de la consulta por cada uno de los porcentajes de utilidad
                        for (COUNT = 1; COUNT <= 4; COUNT++)
                        {
                            row.Cells[0].Value = true;
                               
                            sql = @"UPDATE admin.inv_art_precio SET porc_utilidad=@porcutilidad" + COUNT + ",utilidad=@utilidad" + COUNT + ",precio =@precio" + COUNT + " WHERE cod_articulo = @codigo AND cod_precio='0" + COUNT + "'";
                            com = new NpgsqlCommand(sql, conn);
                            com.Parameters.Add(new NpgsqlParameter("@porcutilidad" + COUNT + "", NpgsqlDbType.Double));
                            com.Parameters.Add(new NpgsqlParameter("@utilidad" + COUNT + "", NpgsqlDbType.Double));
                            com.Parameters.Add(new NpgsqlParameter("@precio" + COUNT + "", NpgsqlDbType.Double));
                            com.Parameters.Add(new NpgsqlParameter("@codigo", NpgsqlDbType.Varchar));

                            com.Prepare();
                                
                            com.Parameters[0].Value = row.Cells["porc_util" + COUNT].Value.ToString().Replace(".", ",");
                            com.Parameters[1].Value = row.Cells["utilidad" + COUNT].Value.ToString().Replace(".", ",");
                            com.Parameters[2].Value = row.Cells["precio" + COUNT + ""].Value.ToString().Replace(".", ",");
                            com.Parameters[3].Value = row.Cells["codigo"].Value.ToString().Replace(" ", string.Empty);
                                com.ExecuteNonQuery();
                                
                        }
                        cantidad++;
                    }
                    if (STATUS == true)
                    {
                        //Ajuste de precio de los productos
                        sql = @"INSERT INTO admin.int_ajuste_precio(org_hijo,descri,total_precio,total_utilidad, 
                    reg_usu_cc, reg_estatus, nro_items,tipo_opera) VALUES(@org_hijo,@descri,@total_precio,@total_utilidad, 
                    @reg_usu_cc, @reg_estatus, @nro_items, @topera)";
                        com = new NpgsqlCommand(sql, conn);
                        com.Parameters.Add(new NpgsqlParameter("@org_hijo", NpgsqlDbType.Varchar));
                        com.Parameters.Add(new NpgsqlParameter("@descri", NpgsqlDbType.Varchar));
                        com.Parameters.Add(new NpgsqlParameter("@total_precio", NpgsqlDbType.Double));
                        com.Parameters.Add(new NpgsqlParameter("@total_utilidad", NpgsqlDbType.Double));
                        com.Parameters.Add(new NpgsqlParameter("@reg_usu_cc", NpgsqlDbType.Varchar));
                        com.Parameters.Add(new NpgsqlParameter("@reg_estatus", NpgsqlDbType.Integer));
                        com.Parameters.Add(new NpgsqlParameter("@nro_items", NpgsqlDbType.Integer));
                        com.Parameters.Add(new NpgsqlParameter("@topera", NpgsqlDbType.Integer));

                        com.Prepare();

                        com.Parameters[0].Value = Globals.org;
                        com.Parameters[1].Value = "AJUSTE DE PRECIOS";
                        com.Parameters[2].Value = TOTALP;
                        com.Parameters[3].Value = "0";
                        com.Parameters[4].Value = "INNOVA";
                        com.Parameters[5].Value = 1;
                        com.Parameters[6].Value = cantidad;
                        com.Parameters[7].Value = 36;

                        com.ExecuteNonQuery();

                        sql = @"SELECT doc from admin.int_ajuste_precio order by fecha_reg desc";
                        com = new NpgsqlCommand(sql, conn);
                        string reader = com.ExecuteScalar().ToString();
                        //Insercion del detalle del ajuste
                        int Item = 1;
                        //Segundo recorrido del datagridview para realizar el detalle del ajuste de precio
                        foreach (DataGridViewRow ROW2 in dataGridView1.Rows)
                        {
                            if (Convert.ToBoolean(ROW2.Cells[0].Value) == true)
                            {

                                sql = @"INSERT INTO admin.int_ajuste_precio_det(org_hijo,doc,cod_alterno,cod_articulo,
                        costo,costo_promedio,fecha,tipo_ajuste,item) VALUES(@org_hijo,@doc,
                        @cod_alterno,@cod_articulo,@costo,@costo_promedio,@fecha,@tipo_ajuste,@item)";
                                com = new NpgsqlCommand(sql, conn);
                                com.Parameters.Add(new NpgsqlParameter("@org_hijo", NpgsqlDbType.Varchar));
                                com.Parameters.Add(new NpgsqlParameter("@doc", NpgsqlDbType.Bigint));
                                com.Parameters.Add(new NpgsqlParameter("@cod_alterno", NpgsqlDbType.Varchar));
                                com.Parameters.Add(new NpgsqlParameter("@cod_articulo", NpgsqlDbType.Varchar));
                                com.Parameters.Add(new NpgsqlParameter("@costo", NpgsqlDbType.Double));
                                com.Parameters.Add(new NpgsqlParameter("@costo_promedio", NpgsqlDbType.Double));
                                com.Parameters.Add(new NpgsqlParameter("@fecha", NpgsqlDbType.Date));
                                com.Parameters.Add(new NpgsqlParameter("@tipo_ajuste", NpgsqlDbType.Integer));
                                com.Parameters.Add(new NpgsqlParameter("@item", NpgsqlDbType.Integer));

                                com.Prepare();

                                com.Parameters[0].Value = Globals.org;
                                com.Parameters[1].Value = Convert.ToInt64(reader);
                                com.Parameters[2].Value = ROW2.Cells["codigo"].Value.ToString().Replace(" ", string.Empty);
                                com.Parameters[3].Value = ROW2.Cells["codigo"].Value.ToString().Replace(" ", string.Empty);
                                com.Parameters[4].Value = ROW2.Cells["costo"].Value;
                                com.Parameters[5].Value = ROW2.Cells["costo_pro"].Value;
                                com.Parameters[6].Value = DateTime.Now;
                                com.Parameters[7].Value = utilidad;
                                com.Parameters[8].Value = Item;
                                com.ExecuteNonQuery();
                                Item++;
                            }
                        }
                    }
                }
                //Finalizacion de la transaccion a la base de datos
                t.Commit();
                MessageBox.Show("Ajuste realizado con exito");
            }
            catch (Exception EX)
            {
                MessageBox.Show("Excepcion : " + EX.Message);
            }
            conn.Close();
        }
        //Click en alguna celda dentro del gridview
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                var senderGrid = (DataGridView)sender;
                DataTable art = new DataTable();
                //Verificar que se haya hecho click en el boton restaurar
                if (senderGrid.Columns[e.ColumnIndex] is DataGridViewButtonColumn &&
                    e.RowIndex >= 0)
                {
                    string codigo;
                    NpgsqlConnection conn = new NpgsqlConnection(connectionstring);
                    conn.Open();
                    //Captura del codigo del producto a restaurar
                    codigo = dataGridView1.Rows[e.RowIndex].Cells[3].Value.ToString();
                    //Consulta de toda la informacion original del producto
                    sql = @"select distinct a.codigo, a.costo, a.descri, d.utilidad1 as porc_util1,
                        d.utilidad2 as porc_util2, d.utilidad3 as porc_util3, 
                        d.utilidad4 as porc_util4,e.utilidad1, e.utilidad2, e.utilidad3, 
                        e.utilidad4, c.precio1, c.precio2, c.precio3, c.precio4 
                        from admin.inv_art as a Join admin.inv_cat_art as b on a.org_hijo = b.org_hijo 
                        and a.codigo = b.cod_articulo JOIN admin.tvinv003_p as c on a.codigo = c.codigo_art
                        JOIN admin.tvinv003_u as d on a.codigo = d.codigo_art
                        JOIN admin.tvinv003_um as e on a.codigo = e.codigo_art WHERE a.codigo='" + codigo + "'";
                    NpgsqlCommand com = new NpgsqlCommand(sql, conn);
                    NpgsqlDataAdapter ads = new NpgsqlDataAdapter(com);
                    ads.Fill(art);

                    //Asignar valores originales de la fila
                    dataGridView1.Rows[e.RowIndex].Cells[5].Value = art.Rows[0][1];
                    dataGridView1.Rows[e.RowIndex].Cells[6].Value = art.Rows[0][3];
                    dataGridView1.Rows[e.RowIndex].Cells[7].Value = art.Rows[0][7];
                    dataGridView1.Rows[e.RowIndex].Cells[8].Value = art.Rows[0][11];
                    dataGridView1.Rows[e.RowIndex].Cells[9].Value = art.Rows[0][4];
                    dataGridView1.Rows[e.RowIndex].Cells[10].Value = art.Rows[0][8];
                    dataGridView1.Rows[e.RowIndex].Cells[11].Value = art.Rows[0][12];
                    dataGridView1.Rows[e.RowIndex].Cells[12].Value = art.Rows[0][5];
                    dataGridView1.Rows[e.RowIndex].Cells[13].Value = art.Rows[0][9];
                    dataGridView1.Rows[e.RowIndex].Cells[14].Value = art.Rows[0][13];
                    dataGridView1.Rows[e.RowIndex].Cells[15].Value = art.Rows[0][6];
                    dataGridView1.Rows[e.RowIndex].Cells[16].Value = art.Rows[0][10];
                    dataGridView1.Rows[e.RowIndex].Cells[17].Value = art.Rows[0][14];
                    dataGridView1.Rows[e.RowIndex].Cells[0].Value = false;
                    //Volver los campos recien restaurados editables
                    foreach (DataGridViewCell cell in dataGridView1.Rows[e.RowIndex].Cells)
                    {
                        if (cell.ColumnIndex >= 5)
                        {
                            cell.ReadOnly = false;
                            cell.Style.BackColor = Color.White;
                        }
                    }

                }
            }
            catch (Exception x)
            {
                MessageBox.Show("Se produjo una excepcion del tipo : " + x);
            }
        }
        //Metodo para el calculo de porcentaje en base al tipo de calculo, el precio y costo
        private decimal calculoPorcentaje(int tipo, decimal precio, decimal costo)
        {
            decimal result = 0;
            if (tipo == 1)
            {
                result = (precio - costo) * 100 / costo;
            }
            else
            {
                result = (precio - costo) * 100 / precio;
            }
            return result;
        }
        //Calculo del precio en base al tipo de calculo el costo y el porcentaje de utilidad
        private decimal calculoPrecio(int tipo, decimal costo, decimal porc)
        {
            decimal result = 0;
            if (tipo == 1)
            {
                result = costo + (costo * porc / 100);//lineal
            }
            else
            {
                result = costo / ((100 - porc) / 100);//Financiero
            }

            return result;
        }
        //Calculo de la utilidad en base al costo y el precio
        private decimal calculoUtilidad(decimal costo, decimal precio)
        {
            decimal result = precio - costo;
            return result;
        }
        //Boton para cargar ajuste desde documento
        private void button6_Click(object sender, EventArgs e)
        {
            try
            {
                //seleccion del archivo a cargar
                DialogResult dr = openFileDialog1.ShowDialog();
                if (dr == DialogResult.OK)
                {
                    string file = openFileDialog1.FileName;
                    try
                    {
                        //inicio de conexion al archivo .xls
                        Myconnetion = new System.Data.OleDb.OleDbConnection("provider=Microsoft.Jet.OLEDB.4.0;Data Source =" + file + "; Extended Properties=\"Excel 8.0;HDR=YES;IMEX=1;\"");
                        Myconnetion.Open();
                        //carga de informacion del .xls

                        // obtener nombre de la hoja de excel
                        System.Data.DataTable dbSchema = Myconnetion.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                        if (dbSchema == null || dbSchema.Rows.Count < 1)
                        {
                            throw new Exception("Error: No se pudo determinar el nombre de la Hoja de Trabajo.");
                        }
                        string firstSheetName = dbSchema.Rows[0]["TABLE_NAME"].ToString();
                        //Consulta al formato en excel
                        MyCommand = new System.Data.OleDb.OleDbDataAdapter("select * from [" + firstSheetName + "]", Myconnetion);
                        MyCommand.TableMappings.Add("Table", "TestTable");

                        //Vaciado del DataSet
                        DtSet.Reset();
                        //Llenado del DataSet con resultado de comando ejecutado al .xls
                        MyCommand.Fill(DtSet);
                        //Asignacion del DataSet como origen de datos del DataGridView 
                        dataGridView1.DataSource = DtSet.Tables[0];
                        //Cerrar conexion
                        Myconnetion.Close();
                        documento = true;
                    }
                    catch (Exception ex)
                    {
                        //Captura de excepcion durante las acciones del button1_click
                        MessageBox.Show("Se produjo un error al cargar la informacion. Error: " + ex.Message.ToString());
                    }
                }
                MessageBox.Show("¿Desea mantener el precio?", "Atencion", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if (DialogResult == DialogResult.Yes) utilidad = 0;
                else utilidad = 1;
            }
            catch(Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
            
        }
    }
}
