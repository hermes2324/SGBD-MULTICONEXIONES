using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace libreriaConsultasBD
{
    public class ConexionOracle : ConexionBase
    {
        private OracleConnection conexion3;

        public ConexionOracle(string servidor, string usuario, string password) : base("ORACLE")
        {
            this.Servidor = servidor;
            this.Usuario = usuario;
            this.Password = password;
        }

        public override bool AbrirConexion()
        {
            try
            {
                string connectionString = $"Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST={Servidor})(PORT=1521)))(CONNECT_DATA=(SID=XE)));User Id={Usuario};Password={Password};";
                conexion3 = new OracleConnection(connectionString);
                conexion3.Open();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al abrir la conexión Oracle: {ex.Message}");
                conexion3 = null;
                return false;
            }
        }

        public override void Cerrar()
        {
            try
            {
                if (conexion3 != null && conexion3.State == ConnectionState.Open)
                {
                    conexion3.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cerrar la conexión Oracle: {ex.Message}");
            }
        }

        public override DataTable EjecutarConsulta(string query)
        {
            DataTable dataTable = new DataTable();
            try
            {
                if (conexion3 == null)
                {
                    throw new InvalidOperationException("La conexión Oracle no ha sido inicializada.");
                }

                if (conexion3.State != ConnectionState.Open)
                {
                    conexion3.Open();
                }

                using (OracleCommand command = new OracleCommand(query, conexion3))
                using (OracleDataAdapter adapter = new OracleDataAdapter(command))
                {
                    adapter.Fill(dataTable);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al ejecutar la consulta: {ex.Message}");
            }
            return dataTable;
        }

        //public async Task CargarBasesDeDatosYTablas(TreeView treeView)
        //{
        //    try
        //    {
        //        if (!AbrirConexion())
        //        {
        //            throw new InvalidOperationException("No se pudo abrir la conexión a la base de datos.");
        //        }

        //        TreeNode rootNode = new TreeNode("BASES DE DATOS ORACLE");
        //        treeView.Nodes.Add(rootNode);

        //        string query = "SELECT USERNAME FROM ALL_USERS";
        //        DataTable dataTable = EjecutarConsulta(query);

        //        foreach (DataRow row in dataTable.Rows)
        //        {
        //            string dbName = row["USERNAME"].ToString();
        //            TreeNode dbNode = new TreeNode(dbName);
        //            rootNode.Nodes.Add(dbNode);

        //            string queryTables = $"SELECT TABLE_NAME FROM ALL_TABLES WHERE OWNER = '{dbName}'";
        //            DataTable tables = EjecutarConsulta(queryTables);

        //            foreach (DataRow tableRow in tables.Rows)
        //            {
        //                string tableName = tableRow["TABLE_NAME"].ToString();
        //                TreeNode tableNode = new TreeNode(tableName);
        //                dbNode.Nodes.Add(tableNode);

        //                string queryColumns = $"SELECT COLUMN_NAME, DATA_TYPE, DATA_LENGTH, NULLABLE FROM ALL_TAB_COLUMNS WHERE OWNER = '{dbName}' AND TABLE_NAME = '{tableName}'";
        //                DataTable columns = EjecutarConsulta(queryColumns);

        //                foreach (DataRow columnRow in columns.Rows)
        //                {
        //                    string columnName = columnRow["COLUMN_NAME"].ToString();
        //                    string dataType = columnRow["DATA_TYPE"].ToString();
        //                    string length = columnRow["DATA_LENGTH"].ToString();
        //                    string isNullable = columnRow["NULLABLE"].ToString();

        //                    string columnDetails = $"{columnName} ({dataType}, {length}, Null: {isNullable})";
        //                    tableNode.Nodes.Add(new TreeNode(columnDetails));
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Error al cargar bases de datos y tablas Oracle: {ex.Message}");
        //    }
        //    finally
        //    {
        //        Cerrar();
        //    }
        //}
        public async Task CargarBasesDeDatosYTablas(TreeView treeView)
        {
            try
            {
                if (!AbrirConexion())
                {
                    throw new InvalidOperationException("No se pudo abrir la conexión a la base de datos.");
                }

                TreeNode rootNode = new TreeNode("BASES DE DATOS ORACLE");  // Cambié BASES DE DATOS a SCHEMAS para mayor precisión
                treeView.Nodes.Add(rootNode);

                // Obtener solo usuarios que tienen tablas
                string query = @"
            SELECT DISTINCT OWNER 
            FROM ALL_TABLES 
            ORDER BY OWNER";

                DataTable dataTable = EjecutarConsulta(query);

                foreach (DataRow row in dataTable.Rows)
                {
                    string schemaName = row["OWNER"].ToString();
                    TreeNode schemaNode = new TreeNode(schemaName);
                    rootNode.Nodes.Add(schemaNode);

                    // Obtener las tablas del esquema
                    string queryTables = $"SELECT TABLE_NAME FROM ALL_TABLES WHERE OWNER = '{schemaName}'";
                    DataTable tables = EjecutarConsulta(queryTables);

                    foreach (DataRow tableRow in tables.Rows)
                    {
                        string tableName = tableRow["TABLE_NAME"].ToString();
                        TreeNode tableNode = new TreeNode(tableName);
                        schemaNode.Nodes.Add(tableNode);

                        // Obtener columnas de la tabla
                        string queryColumns = $@"
                    SELECT COLUMN_NAME, DATA_TYPE, DATA_LENGTH, NULLABLE 
                    FROM ALL_TAB_COLUMNS 
                    WHERE OWNER = '{schemaName}' 
                    AND TABLE_NAME = '{tableName}'";

                        DataTable columns = EjecutarConsulta(queryColumns);

                        foreach (DataRow columnRow in columns.Rows)
                        {
                            string columnName = columnRow["COLUMN_NAME"].ToString();
                            string dataType = columnRow["DATA_TYPE"].ToString();
                            string length = columnRow["DATA_LENGTH"].ToString();
                            string isNullable = columnRow["NULLABLE"].ToString();

                            string columnDetails = $"{columnName} ({dataType}, {length}, Null: {isNullable})";
                            tableNode.Nodes.Add(new TreeNode(columnDetails));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar esquemas y tablas en Oracle: {ex.Message}");
            }
            finally
            {
                Cerrar();
            }
        }



        public async Task<DataTable> EjecutarConsultaEnBaseSeleccionadaOracle(string baseDeDatos, string query)
        {
            DataTable dataTable = new DataTable();

            try
            {
                // Abrir la conexión a la base de datos Oracle
                AbrirConexion();

                //// Cambiar el esquema a la base de datos seleccionada (si es necesario)
                //string switchDatabaseQuery = $"ALTER SESSION SET CURRENT_SCHEMA = \"{baseDeDatos}\"";
                //using (OracleCommand switchCommand = new OracleCommand(switchDatabaseQuery, conexion3))
                //{
                //    await Task.Run(() => switchCommand.ExecuteNonQuery());  // Ejecutar cambio de esquema
                //}

                // Validar si la consulta es un SELECT (consulta de datos)
                if (query.TrimStart().ToUpper().StartsWith("SELECT"))
                {
                    // Ejecutar la consulta SELECT y llenar el DataTable con los resultados
                    using (OracleCommand command = new OracleCommand(query, conexion3))
                    using (OracleDataAdapter adapter = new OracleDataAdapter(command))
                    {
                        adapter.Fill(dataTable);  // Ejecutar consulta y llenar el DataTable
                    }
                }
                else if (query.TrimStart().ToUpper().StartsWith("INSERT") ||
                         query.TrimStart().ToUpper().StartsWith("UPDATE") ||
                         query.TrimStart().ToUpper().StartsWith("DELETE") ||
                         query.TrimStart().ToUpper().StartsWith("CREATE"))
                {
                    using (OracleCommand command = new OracleCommand(query, conexion3))
                    {
                        await Task.Run(() => command.ExecuteNonQuery());  // Ejecutar la consulta sin esperar resultados
                        using (OracleCommand commitCommand = new OracleCommand("COMMIT", conexion3))
                        {
                            await Task.Run(() => commitCommand.ExecuteNonQuery());  // Confirmar cambios
                        }
                    }
                }
                else
                {
                    throw new InvalidOperationException("La consulta SQL no es de tipo SELECT, INSERT, UPDATE o DELETE.");
                }
            }
            catch (OracleException ex)
            {
                MessageBox.Show($"Error al ejecutar consulta Oracle: {ex.Message}", "Error Oracle", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inesperado: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return dataTable;
        }

        public async Task<List<string>> ObtenerBasesDeDatosOracleAsync()
        {
            List<string> basesDeDatos = new List<string>();

            try
            {
                // Abrir la conexión utilizando el método AbrirConexion()
                if (!AbrirConexion())
                {
                    throw new InvalidOperationException("No se pudo abrir la conexión a la base de datos.");
                }

                // Consulta para obtener los esquemas (bases de datos) en Oracle
                string query = "SELECT USERNAME FROM ALL_USERS";

                // Ejecutar la consulta y obtener los resultados
                using (OracleCommand command = new OracleCommand(query, conexion3))
                using (OracleDataReader reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        // Agregar el nombre del esquema (base de datos) a la lista
                        string nombreBaseDeDatos = reader["USERNAME"].ToString();
                        basesDeDatos.Add(nombreBaseDeDatos);
                    }
                }
            }
            catch (OracleException ex)
            {
                // Lanzar una excepción personalizada o registrar el error
                throw new Exception($"Error Oracle ({ex.Number}): {ex.Message}");
            }
            catch (Exception ex)
            {
                // Lanzar una excepción personalizada o registrar el error
                throw new Exception($"Error general: {ex.Message}");
            }
            finally
            {
                Cerrar();  // Asegurarse de cerrar la conexión después de ejecutar la consulta
            }

            return basesDeDatos;
        }


    }

}
