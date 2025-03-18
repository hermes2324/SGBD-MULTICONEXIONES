using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Npgsql;
using Oracle.ManagedDataAccess.Client;
using FirebirdSql.Data.Client;
using MySql.Data.MySqlClient;
using System.Data;
using System.Windows.Forms;
using FirebirdSql.Data.FirebirdClient;

namespace libreriaConsultasBD
{
    public class ConexionSQLServer : ConexionBase
    {
        private SqlConnection conexion;


        public ConexionSQLServer(string servidor, string usuario, string password, string modoAuth) : base("SQL SERVER")
        {
            this.Servidor = servidor;
            this.Usuario = usuario;
            this.Password = password;
            this.ModoAuth = modoAuth;
        }
        public override bool AbrirConexion()
        {
            try
            {
                if (string.IsNullOrEmpty(ModoAuth))
                    throw new ArgumentException("El modo de autenticación no puede estar vacío.");

                string connectionString;

                if (ModoAuth.Trim().Equals("Windows Authentication", StringComparison.OrdinalIgnoreCase))
                {
                    connectionString = $"Data Source={Servidor};Initial Catalog=master;Integrated Security=True";
                }
                else
                {
                    if (string.IsNullOrEmpty(Usuario) || string.IsNullOrEmpty(Password))
                        throw new ArgumentException("El usuario y la contraseña son obligatorios para la autenticación de SQL Server.");

                    connectionString = $"Data Source={Servidor};Initial Catalog=master;User Id={Usuario};Password={Password};";
                }

                conexion = new SqlConnection(connectionString);
                conexion.Open();
                return conexion.State == ConnectionState.Open;
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Error al conectar con SQL Server: {ex.Message}", "Error de conexión", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inesperado: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public override DataTable EjecutarConsulta(string query)
        {
            DataTable dataTable = new DataTable();

            try
            {
                using (SqlDataAdapter adapter = new SqlDataAdapter(query, conexion))
                {
                    adapter.Fill(dataTable);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al ejecutar consulta SQL Server: {ex.Message}");
            }

            return dataTable;
        }

        public override void Cerrar()
        {
            if (conexion != null && conexion.State == ConnectionState.Open)
            {
                conexion.Close();
            }
        }

        #region CargarBasesDeDatosYTablasAsync

        public async Task CargarBasesDeDatosYTablasAsync(TreeView treeView)
        {
            //treeView.Nodes.Clear();
            TreeNode rootNode = new TreeNode("BASES DE DATOS SQL SERVER");
            treeView.Nodes.Add(rootNode);

            try
            {
                if (AbrirConexion())
                {
                    string query = "SELECT name AS BaseDeDatos FROM sys.databases";
                    DataTable dbTable = await EjecutarConsultaAsync(query);

                    foreach (DataRow dbRow in dbTable.Rows)
                    {
                        string dbName = dbRow["BaseDeDatos"].ToString();
                        TreeNode dbNode = new TreeNode(dbName);

                        string tableQuery = $"USE [{dbName}]; SELECT name AS Tabla FROM sys.tables";
                        DataTable tableTable = await EjecutarConsultaAsync(tableQuery);

                        foreach (DataRow tableRow in tableTable.Rows)
                        {
                            string tableName = tableRow["Tabla"].ToString();
                            TreeNode tableNode = new TreeNode(tableName);

                            string columnQuery = $@"
                            USE [{dbName}];
                            SELECT COLUMN_NAME AS Columna, DATA_TYPE AS TipoDeDato,
                                   CHARACTER_MAXIMUM_LENGTH AS Longitud, IS_NULLABLE AS PermiteNull
                            FROM INFORMATION_SCHEMA.COLUMNS 
                            WHERE TABLE_NAME = '{tableName}';";

                            DataTable columnTable = await EjecutarConsultaAsync(columnQuery);

                            string pkQuery = $@"
                            USE [{dbName}];
                            SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
                            WHERE TABLE_NAME = '{tableName}';";

                            DataTable pkTable = await EjecutarConsultaAsync(pkQuery);

                            foreach (DataRow columnRow in columnTable.Rows)
                            {
                                string columnName = columnRow["Columna"].ToString();
                                string dataType = columnRow["TipoDeDato"].ToString();
                                string lengthStr = columnRow["Longitud"].ToString();
                                bool allowsNull = columnRow["PermiteNull"].ToString() == "YES";
                                bool isPK = pkTable.AsEnumerable().Any(pk => pk["COLUMN_NAME"].ToString() == columnName);

                                string columnDetails = $"Tipo: {dataType}, Longitud: {(!string.IsNullOrEmpty(lengthStr) ? lengthStr : "N/A")}, Null: {(allowsNull ? "Sí" : "No")}";
                                if (isPK)
                                {
                                    columnDetails += ", PK";
                                }

                                TreeNode columnNode = new TreeNode($"{columnName} ({columnDetails})");
                                tableNode.Nodes.Add(columnNode);
                            }

                            dbNode.Nodes.Add(tableNode);
                        }

                        rootNode.Nodes.Add(dbNode);
                    }
                }
                else
                {
                    MessageBox.Show("No se pudo establecer la conexión con el servidor.", "Error de conexión", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Error SQL ({ex.Number}): {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error general: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //private async Task<DataTable> EjecutarConsultaAsync(string query)
        //{

        //        DataTable dataTable = new DataTable();

        //        using (SqlDataAdapter adapter = new SqlDataAdapter(query, conexion))
        //        {
        //            await Task.Run(() => adapter.Fill(dataTable));
        //        }

        //        return dataTable;

        //}
        private async Task<DataTable> EjecutarConsultaAsync(string query)
        {
            DataTable dataTable = new DataTable();

            try
            {
                using (SqlDataAdapter adapter = new SqlDataAdapter(query, conexion))
                {
                    // Ejecutar la consulta en un hilo separado para evitar bloquear la UI
                    await Task.Run(() => adapter.Fill(dataTable));
                }
            }
            catch (SqlException sqlEx)
            {
                // Manejo de excepciones específicas de SQL
                MessageBox.Show($"Error al ejecutar la consulta SQL: {sqlEx.Message}", "Error SQL", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                // Manejo de cualquier otra excepción general
                MessageBox.Show($"Error inesperado: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return dataTable;
        }


        #endregion

        public async Task<List<string>> ObtenerBasesDeDatosAsync()
        {
            List<string> basesDeDatos = new List<string>();

            try
            {
                AbrirConexion();

                // Consulta para obtener las bases de datos
                string query = "SELECT name FROM sys.databases;";

                // Ejecutar la consulta
                using (SqlCommand command = new SqlCommand(query, conexion))
                using (SqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        string nombreBaseDeDatos = reader["name"].ToString(); // Corregido el nombre de la columna
                        basesDeDatos.Add(nombreBaseDeDatos);
                    }
                }
            }
            catch (SqlException ex)
            {
                // En WinForms usa Invoke para evitar problemas de UI
                MessageBox.Show($"Error SQL ({ex.Number}): {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error general: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return basesDeDatos;
        }

        public async Task<DataTable> EjecutarConsultaEnBaseSeleccionada(string baseDeDatos, string query)
        {
            DataTable dataTable = new DataTable();

            try
            {
                if (conexion == null || conexion.State != ConnectionState.Open)
                {
                    MessageBox.Show("La conexión con el servidor no está abierta.", "Error de conexión", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return dataTable;
                }

                // Cambiar la base de datos antes de ejecutar la consulta
                conexion.ChangeDatabase(baseDeDatos);

                using (SqlDataAdapter adapter = new SqlDataAdapter(query, conexion))
                {
                    await Task.Run(() => adapter.Fill(dataTable));
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Error al ejecutar consulta SQL Server: {ex.Message}", "Error SQL", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inesperado: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return dataTable;
        }


    }

}
