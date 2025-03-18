using FirebirdSql.Data.FirebirdClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace libreriaConsultasBD
{
    public class ConexionFirebird : ConexionBase
    {
        private FbConnection conexion;

        public string DatabasePath { get; set; }

        public ConexionFirebird(string server, string user, string password, string databasePath) : base("Firebird")
        {
            this.Servidor = server;
            this.Usuario = user;
            this.Password = password;
            this.DatabasePath = databasePath;
        }

        // Implementación para abrir la conexión a Firebird
        public override bool AbrirConexion()
        {
            try
            {
                string connectionString = $"User={Usuario};Password={Password};Database={DatabasePath};DataSource={Servidor};Port=3050;Dialect=3;Charset=UTF8;";
                conexion = new FbConnection(connectionString);
                conexion.Open();
                EstaAbierta = true;
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al abrir la conexión Firebird: {ex.Message}");
                EstaAbierta = false;
                return false;
            }
        }

        // Ejecutar consultas en Firebird
        public override DataTable EjecutarConsulta(string query)
        {
            DataTable dataTable = new DataTable();
            try
            {
                if (conexion == null || conexion.State != ConnectionState.Open)
                {
                    throw new InvalidOperationException("La conexión Firebird no está abierta.");
                }

                using (FbCommand command = new FbCommand(query, conexion))
                using (FbDataAdapter adapter = new FbDataAdapter(command))
                {
                    adapter.Fill(dataTable);
                }
            }
            catch (FbException fbEx)
            {
                Console.WriteLine($"Error de Firebird: {fbEx.Message}");
                Console.WriteLine($"Código de error: {fbEx.ErrorCode}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error general: {ex.Message}");
            }
            return dataTable;
        }

        // Cerrar la conexión a Firebird
        public override void Cerrar()
        {
            try
            {
                if (conexion != null && conexion.State == ConnectionState.Open)
                {
                    conexion.Close();
                }
                EstaAbierta = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cerrar la conexión Firebird: {ex.Message}");
            }
        }

        // Cargar las tablas de Firebird en el TreeView
        //public async Task CargarTablasFirebirdAsync(TreeView treeView)
        //{
        //    try
        //    {
        //        if (!await Task.Run(() => AbrirConexion())) // Ejecutamos la apertura de conexión de forma asíncrona
        //        {
        //            throw new InvalidOperationException("No se pudo abrir la conexión a la base de datos Firebird.");
        //        }

        //        TreeNode rootNode = new TreeNode("TABLAS Firebird");
        //        treeView.Nodes.Add(rootNode);

        //        string queryTables = "SELECT TRIM(RDB$RELATION_NAME) AS TABLE_NAME FROM RDB$RELATIONS WHERE RDB$SYSTEM_FLAG = 0";

        //        // Ejecutar la consulta de forma asíncrona
        //        DataTable tables = await Task.Run(() => EjecutarConsulta(queryTables));

        //        foreach (DataRow tableRow in tables.Rows)
        //        {
        //            string tableName = tableRow["TABLE_NAME"].ToString();
        //            TreeNode tableNode = new TreeNode(tableName);
        //            rootNode.Nodes.Add(tableNode);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Error al cargar tablas: {ex.Message}");
        //    }
        //    finally
        //    {
        //        // Es importante cerrar la conexión aunque haya habido un error
        //        await Task.Run(() => Cerrar());
        //    }
        //}

        public async Task CargarTablasFirebirdAsync(TreeView treeView)
        {
            try
            {
                if (!await Task.Run(() => AbrirConexion())) // Ejecutamos la apertura de conexión de forma asíncrona
                {
                    throw new InvalidOperationException("No se pudo abrir la conexión a la base de datos Firebird.");
                }

                TreeNode rootNode = new TreeNode("TABLAS Firebird");
                treeView.Nodes.Add(rootNode);

                string queryTables = "SELECT TRIM(RDB$RELATION_NAME) AS TABLE_NAME FROM RDB$RELATIONS WHERE RDB$SYSTEM_FLAG = 0";

                // Ejecutar la consulta de forma asíncrona
                DataTable tables = await Task.Run(() => EjecutarConsulta(queryTables));

                foreach (DataRow tableRow in tables.Rows)
                {
                    string tableName = tableRow["TABLE_NAME"].ToString();
                    TreeNode tableNode = new TreeNode(tableName);
                    rootNode.Nodes.Add(tableNode);

                    // Ahora obtenemos las columnas de la tabla
                    string queryColumns = @"
                SELECT
                    TRIM(RDB$FIELD_NAME) AS COLUMN_NAME
                FROM RDB$RELATION_FIELDS
                WHERE RDB$RELATION_NAME = @tableName
                ORDER BY RDB$FIELD_POSITION";

                    // Ejecutar la consulta de columnas de forma asíncrona
                    var parameters = new List<KeyValuePair<string, object>>()
            {
                new KeyValuePair<string, object>("@tableName", tableName)
            };
                    DataTable columns = await Task.Run(() => EjecutarConsultaConParametros(queryColumns, parameters));

                    foreach (DataRow columnRow in columns.Rows)
                    {
                        string columnName = columnRow["COLUMN_NAME"].ToString();
                        TreeNode columnNode = new TreeNode(columnName);
                        tableNode.Nodes.Add(columnNode);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar tablas y columnas: {ex.Message}");
            }
            finally
            {
                // Es importante cerrar la conexión aunque haya habido un error
                await Task.Run(() => Cerrar());
            }
        }

        public DataTable EjecutarConsultaConParametros(string query, List<KeyValuePair<string, object>> parameters)
        {
            DataTable result = new DataTable();

            try
            {
                if (conexion == null || conexion.State != ConnectionState.Open)
                {
                    throw new InvalidOperationException("La conexión Firebird no está abierta.");
                }

                using (var command = new FbCommand(query, conexion))
                {
                    // Agregar los parámetros a la consulta
                    foreach (var param in parameters)
                    {
                        command.Parameters.AddWithValue(param.Key, param.Value);
                    }

                    // Ejecutar la consulta y llenar el DataTable con los resultados
                    using (var adapter = new FbDataAdapter(command))
                    {
                        adapter.Fill(result);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al ejecutar la consulta con parámetros: {ex.Message}");
            }

            return result;
        }

        public async Task<List<string>> ObtenerBasesDeDatosAsyncFirebird()
        {
            List<string> basesDeDatos = new List<string>();

            try
            {
                AbrirConexion();

                string consulta = string.Empty;
                DataTable databases = new DataTable();

                // Determinar el tipo de base de datos (en este caso, si es Firebird)
                switch ("firebird")
                {
                    case "firebird":
                        // Consulta para obtener el nombre de la base de datos activa
                        consulta = "SELECT MON$DATABASE_NAME FROM MON$DATABASE";
                        break;

                        // Si tienes otro tipo de base de datos o más casos, puedes agregarlos aquí
                        // case "otraBaseDeDatos":
                        //     consulta = "SELECT ...";
                        //     break;
                }

                // Ejecutar la consulta según el caso
                using (FbCommand cmd = new FbCommand(consulta, (FbConnection)conexion))
                using (FbDataAdapter adapter = new FbDataAdapter(cmd))
                {
                    adapter.Fill(databases);
                }

                // Procesar el resultado
                if (databases != null && databases.Rows.Count > 0)
                {
                    // Extraer el nombre de la base de datos de la primera fila
                    string rutaBaseDeDatos = databases.Rows[0][0].ToString();
                    Console.WriteLine($"Ruta de la base de datos: {rutaBaseDeDatos}");

                    // Extraer solo el nombre de la base de datos (recortando la ruta)
                    string nombreBaseDeDatos = Path.GetFileName(rutaBaseDeDatos);
                    Console.WriteLine($"Nombre de la base de datos: {nombreBaseDeDatos}");

                    // Agregar el nombre de la base de datos a la lista
                    basesDeDatos.Add(nombreBaseDeDatos);
                }
                else
                {
                    Console.WriteLine("No se encontraron bases de datos o no hay resultados.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener la base de datos: {ex.Message}");
            }

            return basesDeDatos;
        }


        public async Task<DataTable> EjecutarConsultaEnBaseSeleccionadaFirebird(string query)
        {
            DataTable dataTable = new DataTable();

            try
            {
                // Verificar la consulta antes de ejecutarla
                Console.WriteLine($"Ejecutando consulta: {query}");

                // Abrir la conexión utilizando el método AbrirConexion()
                if (!AbrirConexion())  // Llamamos al método que ya maneja la apertura de conexión
                {
                    throw new InvalidOperationException("No se pudo abrir la conexión con la base de datos.");
                }

                // Ejecutar la consulta
                using (FbCommand command = new FbCommand(query, conexion))
                using (FbDataAdapter adapter = new FbDataAdapter(command))
                {
                    await Task.Run(() => adapter.Fill(dataTable));
                }
            }
            catch (FbException fbEx)
            {
                Console.WriteLine($"Error de Firebird: {fbEx.Message}");
                Console.WriteLine($"Código de error: {fbEx.ErrorCode}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error general: {ex.Message}");
            }
            finally
            {
                Cerrar();  // Cerrar la conexión después de ejecutar la consulta
            }

            return dataTable;
        }

    }
}
