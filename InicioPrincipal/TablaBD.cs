using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using libreriaConsultasBD;

namespace InicioPrincipal
{
    public partial class TablaBD : Form
    {
        ConexionSQLServer conexionSQLServer;
        ConexionMySQL ConexionMySQL;
        private string servidor;
        private string modoAuth;
        private string usuario;
        private string password;
        private string sgbdSeleccionado;
        private string BaseDatos;
        GestorConexiones gestorConexiones = new GestorConexiones();
        private ConexionBase conexion;

        public TablaBD(string sgbd, string servidor, string modoAuth, string usuario, string password, string BaseDatos, GestorConexiones gestorConexiones)
        {
            InitializeComponent();
            this.sgbdSeleccionado = sgbd;
            this.servidor = servidor;
            this.modoAuth = modoAuth;
            this.usuario = usuario;
            this.password = password;
            this.BaseDatos = BaseDatos;
            this.gestorConexiones = gestorConexiones;
        }

        private async void TablaBD_Load(object sender, EventArgs e)
        {
            switch (sgbdSeleccionado.ToLower())
            {
                case string s when s.Contains("sql server"):

                    if (gestorConexiones == null)
                    {
                        MessageBox.Show("NULL" + gestorConexiones);
                    }
                    else
                    {
                        ConexionSQLServer conexionSQLServer = new ConexionSQLServer(servidor, usuario, password, modoAuth);
                        await conexionSQLServer.CargarBasesDeDatosYTablasAsync(treeView1);
                        gestorConexiones.AgregarConexion(conexionSQLServer);
                    }
                    break;
                case string s when s.Contains("mysql"):
                    ConexionMySQL conexionMy = new ConexionMySQL(servidor, usuario, password);
                    await conexionMy.CargarBasesDeDatosYTablasMYSQL(treeView1);
                    gestorConexiones.AgregarConexion(conexionMy);
                    break;
                case string s when s.Contains("oracle"):
                    ConexionOracle conexionOracle = new ConexionOracle(servidor, usuario, password);
                    await conexionOracle.CargarBasesDeDatosYTablas(treeView1);
                    gestorConexiones.AgregarConexion(conexionOracle);
                    break;
                case string s when s.Contains("firebird"):
                    ConexionFirebird conexionFirebird = new ConexionFirebird(servidor, usuario, password, BaseDatos);
                    await conexionFirebird.CargarTablasFirebirdAsync(treeView1);
                    gestorConexiones.AgregarConexion(conexionFirebird);
                    break;

                case string s when s.Contains("postgresql"):
                    ConexionPostgreSQL conexionPostgreSQL = new ConexionPostgreSQL(servidor, usuario, password, BaseDatos);
                    await conexionPostgreSQL.CargarBasesDeDatosYTablasAsync(treeView1);
                    gestorConexiones.AgregarConexion(conexionPostgreSQL);
                    break;


            }

        }
        GestorConexiones GestorConexiones = new GestorConexiones();

        public void AgregarNuevaConexion(SGBDConexion nuevaConexion)
        {

            // Ahora que el identificador está correctamente asignado
            TreeNode nodoConexion = new TreeNode(nuevaConexion.Conexion.Identificador);
            nodoConexion.Tag = nuevaConexion;  // Puedes agregar más información si lo deseas
            treeView1.Nodes.Add(nodoConexion);


            if (nuevaConexion == null) return;

            // Agregar la conexión al gestor
            gestorConexiones.AgregarConexion(nuevaConexion.Conexion);

            // Cargar bases de datos y tablas según el tipo de conexión
            switch (nuevaConexion.Conexion)
            {
                case ConexionSQLServer sql:
                    sql.CargarBasesDeDatosYTablasAsync(treeView1);  // Pasar el TreeView como parámetro
                    break;
                case ConexionMySQL mysql:
                    mysql.CargarBasesDeDatosYTablasMYSQL(treeView1);  // Pasar el TreeView como parámetro
                    break;
                case ConexionOracle oracle:
                    oracle.CargarBasesDeDatosYTablas(treeView1);  // Pasar el TreeView como parámetro
                    break;
                case ConexionPostgreSQL pos:
                    pos.CargarBasesDeDatosYTablasAsync(treeView1);
                    break;
                case ConexionFirebird conexionFirebird:
                    conexionFirebird.CargarTablasFirebirdAsync(treeView1);
                    break;
                default:
                    MessageBox.Show("Conexión no soportada.");
                    break;
            }
        }


        private void pictureBox2_Click(object sender, EventArgs e)
        {
            GestorConexiones nuevoGestorConexiones = new GestorConexiones(); // Crear un nuevo gestor
            NuevoInstancia nuevoInstancia = new NuevoInstancia(gestorConexiones);
            nuevoInstancia.ConexionAgregada += AgregarNuevaConexion;
            nuevoInstancia.Show();
        }

        private async void treeView1_AfterSelect_1(object sender, TreeViewEventArgs e)
        {
            textBox1.Clear();
            string nodeText = e.Node.Text;
            if (string.IsNullOrEmpty(nodeText))
            {
                MessageBox.Show("No se seleccionó un nodo válido.");
                return;
            }


            string[] tiposSGBDValidos = { "SQL SERVER", "MYSQL", "ORACLE", "POSTGRESQL", "Firebird" };
            string tipoSGBD = tiposSGBDValidos.FirstOrDefault(t => nodeText.Contains(t));
            if (string.IsNullOrEmpty(tipoSGBD))
            {
                MessageBox.Show("No se pudo determinar el tipo de SGBD.");
                return;
            }

            // Mostrar mensaje con las conexiones activas
            //  MessageBox.Show(gestorConexiones.ObtenerMensajeConConexionesActivas());

            // Limpiar el comboBox antes de llenarlo
            comboBox1.Items.Clear();

            // Bandera para verificar si las bases de datos ya se cargaron
            bool basesDeDatosCargadas = false;

            // Buscar la conexión correspondiente al tipo de SGBD seleccionado
            ConexionBase conexion = gestorConexiones.ObtenerConexionPorTipo(tipoSGBD);

            // Si no se encuentra la conexión, mostramos un mensaje y salimos


            // Cargar las bases de datos dependiendo del tipo de conexión
            if (conexion is ConexionSQLServer sql)
            {
                List<string> basesDeDatos = await Task.Run(() => sql.ObtenerBasesDeDatosAsync());
                foreach (string baseDeDatos in basesDeDatos)
                {
                    comboBox1.Items.Add(baseDeDatos);
                }
                basesDeDatosCargadas = true;
            }
            else if (conexion is ConexionMySQL mysql)
            {
                List<string> basesDeDatos = await Task.Run(() => mysql.ObtenerBasesDeDatosAsyncmysql());
                foreach (string baseDeDatos in basesDeDatos)
                {
                    comboBox1.Items.Add(baseDeDatos);
                }
                basesDeDatosCargadas = true;
            }
            else if (conexion is ConexionPostgreSQL postgreSQL)
            {
                List<string> basesDeDatos = await Task.Run(() => postgreSQL.ObtenerBasesDeDatosAsync());
                foreach (string baseDeDatos in basesDeDatos)
                {
                    comboBox1.Items.Add(baseDeDatos);
                }
                basesDeDatosCargadas = true;
            }
            else if (conexion is ConexionFirebird firebird)
            {
                List<string> basesDeDatos = await Task.Run(() => firebird.ObtenerBasesDeDatosAsyncFirebird());
                foreach (string baseDeDatos in basesDeDatos)
                {
                    comboBox1.Items.Add(baseDeDatos);
                }
                basesDeDatosCargadas = true;
                MessageBox.Show($"Bases de datos cargadas: {basesDeDatos.Count}");

            }
            else if (conexion is ConexionOracle oracle)
            {
                List<string> basesDeDatos = await Task.Run(() => oracle.ObtenerBasesDeDatosOracleAsync());
                foreach (string baseDeDatos in basesDeDatos)
                {
                    comboBox1.Items.Add(baseDeDatos);
                }
                basesDeDatosCargadas = true;

            }
            else
            {
                MessageBox.Show("Tipo de conexión no soportado.");
            }

            // Si se encontraron bases de datos, seleccionar la primera por defecto
            if (comboBox1.Items.Count > 0)
            {
                comboBox1.SelectedIndex = 0; // Selecciona el primer ítem
            }
            else
            {
                MessageBox.Show("No se encontraron bases de datos.");
            }
        }

        private async void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private async void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true; // Evita que se agregue una nueva línea en el TextBox

                
                // Verificar que se haya ingresado una consulta SQL
                if (string.IsNullOrWhiteSpace(textBox1.Text))
                {
                    MessageBox.Show("Ingrese una consulta SQL.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string baseSeleccionada = comboBox1.SelectedItem?.ToString();
                if (string.IsNullOrEmpty(baseSeleccionada))
                {
                    MessageBox.Show("Por favor, seleccione una base de datos.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string query = textBox1.Text;

                // Obtener el TreeNode seleccionado en el TreeView
                TreeNode nodoSeleccionado = treeView1.SelectedNode;

                // Verificar si hay un nodo seleccionado en el TreeView
                if (nodoSeleccionado == null)
                {
                    MessageBox.Show("Seleccione un nodo en el TreeView.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Obtener el tipo de SGBD según el nodo seleccionado
                string tipoSGBD = ObtenerTipoSGBD(nodoSeleccionado);

                // Obtener la conexión según el tipo de SGBD
                ConexionBase conexion = gestorConexiones.ObtenerConexionPorTipo(tipoSGBD);

                // Verificar que la conexión no sea nula
                if (conexion == null)
                {
                    MessageBox.Show("No se ha encontrado una conexión válida para el SGBD seleccionado.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                try
                {
                    // Ejecutar la consulta según el tipo de conexión
                    if (conexion is ConexionSQLServer conexionSQLServer)
                    {
                        DataTable resultadoSQL = await conexionSQLServer.EjecutarConsultaEnBaseSeleccionada(baseSeleccionada, query);
                        // Mostrar los resultados en el DataGridView
                         dataGridView1.DataSource = resultadoSQL;

                        // Limpiar y refrescar el TreeView para SQL Server
                        LimpiarNodosSGBD("SQL SERVER");
                        await conexionSQLServer.CargarBasesDeDatosYTablasAsync(treeView1);
                    }
                    else if (conexion is ConexionMySQL conexionMySQL)
                    {
                        DataTable resultadoMySQL = await conexionMySQL.EjecutarConsultaEnBaseSeleccionadaMySQL(baseSeleccionada, query);
                        // Mostrar los resultados en el DataGridView
                         dataGridView1.DataSource = resultadoMySQL;

                        // Limpiar y refrescar el TreeView para MySQL
                        LimpiarNodosSGBD("MYSQL");
                        await conexionMySQL.CargarBasesDeDatosYTablasMYSQL(treeView1);
                    }
                    else if (conexion is ConexionOracle conexionOracle)
                    {
                        DataTable resultadoOracle = await conexionOracle.EjecutarConsultaEnBaseSeleccionadaOracle(baseSeleccionada, query);
                        // Mostrar los resultados en el DataGridView
                         dataGridView1.DataSource = resultadoOracle;

                        // Limpiar y refrescar el TreeView para Oracle
                        LimpiarNodosSGBD("ORACLE");
                        await conexionOracle.CargarBasesDeDatosYTablas(treeView1);
                    }
                    else if (conexion is ConexionFirebird conexionFirebird)
                    {
                         DataTable resultadoFirebird = await conexionFirebird.EjecutarConsultaEnBaseSeleccionadaFirebird( query);
                        // Mostrar los resultados en el DataGridView
                         dataGridView1.DataSource = resultadoFirebird;

                        // Limpiar y refrescar el TreeView para Firebird
                        LimpiarNodosSGBD("Firebird");
                        await conexionFirebird.CargarTablasFirebirdAsync(treeView1);
                    }
                    else if (conexion is ConexionPostgreSQL conexionPostgreSQL)
                    {
                         DataTable resultadoPostgreSQL = await conexionPostgreSQL.EjecutarConsultaEnBaseSeleccionadaPostgreSQL(baseSeleccionada, query);
                        // Mostrar los resultados en el DataGridView
                         dataGridView1.DataSource = resultadoPostgreSQL;

                        // Limpiar y refrescar el TreeView para PostgreSQL
                        LimpiarNodosSGBD("POSTGRESQL");
                        await conexionPostgreSQL.CargarBasesDeDatosYTablasAsync(treeView1);
                    }
                    else
                    {
                        MessageBox.Show("Tipo de base de datos no soportado.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al ejecutar la consulta o cargar las bases de datos: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }
        }

        private string ObtenerTipoSGBD(TreeNode nodoSeleccionado)
        {
            // Obtener el texto del nodo seleccionado en el TreeView
            string nodeText = nodoSeleccionado.Text;

            // Definir los tipos de SGBD válidos
            string[] tiposSGBDValidos = { "SQL SERVER", "MYSQL", "ORACLE", "POSTGRESQL", "Firebird" };

            // Buscar el tipo de SGBD en el texto del nodo
            string tipoSGBD = tiposSGBDValidos.FirstOrDefault(t => nodeText.Contains(t));

            if (string.IsNullOrEmpty(tipoSGBD))
            {
                MessageBox.Show("No se pudo determinar el tipo de SGBD.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return tipoSGBD;
        }


        private void LimpiarNodosSGBD(string tipoSGBD)
        {
            // Crear una lista para almacenar los nodos a eliminar
            List<TreeNode> nodosAEliminar = new List<TreeNode>();

            // Iterar sobre los nodos del TreeView
            foreach (TreeNode node in treeView1.Nodes)
            {
                // Si el nodo contiene el tipo de SGBD, agregarlo a la lista de nodos a eliminar
                if (node.Text.Contains(tipoSGBD))
                {
                    nodosAEliminar.Add(node);
                }
            }

            // Eliminar los nodos encontrados
            foreach (TreeNode node in nodosAEliminar)
            {
                treeView1.Nodes.Remove(node);
            }
        }

    }
}
