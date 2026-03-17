using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.UserSecrets;
using System;
using System.Windows.Forms;
using System.Diagnostics;
using System.Reflection;

namespace EmployeeManagementSyst
{
    public class ServerConnection
    {
        private static string? _connectionString;

        /// <summary>
        /// Initialize the server connection from an external configuration provider (Config).
        /// This avoids reading user-secrets from inside this class.
        /// </summary>
        public static void Initialize(Config config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            _connectionString = config.AppConn ?? throw new Exception("AppConn missing in configuration.");
        }

        public static string GetConnectionString()
        {
            if (string.IsNullOrEmpty(_connectionString)) throw new InvalidOperationException("ServerConnection not initialized. Call ServerConnection.Initialize(config) before using.");
            return _connectionString!;
        }

        public static SqlConnection GetOpenConnection()
        {
            try
            {
                var serverCon = new SqlConnection(GetConnectionString());

                serverCon.Open();
                return serverCon;
            }
            catch (Exception e)
            {
                MessageBox.Show("Error Initiating Connection: " + e.Message);
                Debug.WriteLine(e.Message);
                throw;
            }
        }

        /// <summary>
        /// Close a connection or perform a safe no-op if none is provided.
        /// </summary>
        public static void CloseConnection(SqlConnection? conn = null)
        {
            try
            {
                if (conn != null)
                {
                    conn.Close();
                }
                else
                {
                    // no-op when no active connection is tracked
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Error closing connection: " + e.Message);
                Debug.WriteLine(e.Message);
                throw;
            }
        }
    }
}
