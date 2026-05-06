//
// Copyright (C) 2013-2025 getMaNGOS <https://www.getmangos.eu>
//
// This program is free software. You can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation. either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY. Without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program. If not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//

using System;
using System.ComponentModel;
using System.Data;
using System.Threading;
using MySqlConnector;

namespace Mangos.MySql;

// Legacy synchronous database abstraction for MySQL connections.
// Provides backward compatibility while supporting modern async operations.
// Note: Consider migrating to Dapper-based queries in new code.
public class SQL : IDisposable
{
    private MySqlConnection MySQLConn = null!;
    private readonly SemaphoreSlim _connectionLock = new(1, 1);
    private volatile bool _disposedValue;

    public enum EMessages
    {
        ID_Error = 0,
        ID_Message = 1
    }

    public event SQLMessageEventHandler SQLMessage = null!;

    public delegate void SQLMessageEventHandler(EMessages MessageID, string OutBuf);

    public enum DB_Type
    {
        MySQL = 0,
        MariaDB = 1
    }

    public enum ReturnState
    {
        Success = 0,
        MinorError = 1,
        FatalError = 2
    }

    private DB_Type _sqlType;
    private string _sqlHost = "localhost";
    private string _sqlPort = "3306";
    private string _sqlUser = string.Empty;
    private string _sqlPass = string.Empty;
    private string _sqlDBName = string.Empty;

    // Gets or sets the SQL server type.
    [Description("SQL Server selection.")]
    public DB_Type SQLTypeServer { get => _sqlType; set => _sqlType = value; }

    // Gets or sets the SQL host name.
    [Description("SQL Host name.")]
    public string SQLHost { get => _sqlHost; set => _sqlHost = value ?? "localhost"; }

    // Gets or sets the SQL host port.
    [Description("SQL Host port.")]
    public string SQLPort { get => _sqlPort; set => _sqlPort = value ?? "3306"; }

    // Gets or sets the SQL user name.
    [Description("SQL User name.")]
    public string SQLUser { get => _sqlUser; set => _sqlUser = value ?? string.Empty; }

    // Gets or sets the SQL password.
    [Description("SQL Password.")]
    public string SQLPass { get => _sqlPass; set => _sqlPass = value ?? string.Empty; }

    // Gets or sets the SQL database name.
    [Description("SQL Database name.")]
    public string SQLDBName { get => _sqlDBName; set => _sqlDBName = value ?? string.Empty; }

    // Establishes a connection to the SQL server.
    [Description("Start up the SQL connection.")]
    public int Connect()
    {
        try
        {
            // Validate required settings
            if (string.IsNullOrWhiteSpace(SQLHost))
            {
                SQLMessage?.Invoke(EMessages.ID_Error, "SQLHost cannot be empty");
                return (int)ReturnState.FatalError;
            }

            if (string.IsNullOrWhiteSpace(SQLPort))
            {
                SQLMessage?.Invoke(EMessages.ID_Error, "SQLPort cannot be empty");
                return (int)ReturnState.FatalError;
            }

            if (string.IsNullOrWhiteSpace(SQLUser))
            {
                SQLMessage?.Invoke(EMessages.ID_Error, "SQLUser cannot be empty");
                return (int)ReturnState.FatalError;
            }

            if (string.IsNullOrWhiteSpace(SQLPass))
            {
                SQLMessage?.Invoke(EMessages.ID_Error, "SQLPassword cannot be empty");
                return (int)ReturnState.FatalError;
            }

            if (string.IsNullOrWhiteSpace(SQLDBName))
            {
                SQLMessage?.Invoke(EMessages.ID_Error, "SQLDatabaseName cannot be empty");
                return (int)ReturnState.FatalError;
            }

            switch (_sqlType)
            {
                case DB_Type.MySQL:
                case DB_Type.MariaDB:
                {
                    MySQLConn = new MySqlConnection(
                        $"Server={SQLHost};Port={SQLPort};User ID={SQLUser};Password={SQLPass};Database={SQLDBName};Compress=false;Connection Timeout=1;");
                    MySQLConn.Open();
                    
                    // Test the connection
                    if (!TestConnection())
                    {
                        return (int)ReturnState.FatalError;
                    }
                    
                    SQLMessage?.Invoke(EMessages.ID_Message, $"{_sqlType} Connection Opened Successfully [{SQLUser}@{SQLHost}]");
                    break;
                }

                default:
                    SQLMessage?.Invoke(EMessages.ID_Error, "Unsupported SQL server type.");
                    return (int)ReturnState.FatalError;
            }
        }
        catch (MySqlException ex)
        {
            SQLMessage?.Invoke(EMessages.ID_Error, $"{_sqlType} Connection Error [{ex.Message}]");
            return (int)ReturnState.FatalError;
        }

        return (int)ReturnState.Success;
    }

    // Restarts the SQL connection.
    [Description("Restart the SQL connection.")]
    public void Restart()
    {
        try
        {
            _connectionLock.Wait();
            try
            {
                switch (_sqlType)
                {
                    case DB_Type.MySQL:
                    case DB_Type.MariaDB:
                    {
                        MySQLConn?.Close();
                        MySQLConn?.Dispose();
                        MySQLConn = new MySqlConnection(
                            $"Server={SQLHost};Port={SQLPort};User ID={SQLUser};Password={SQLPass};Database={SQLDBName};Compress=false;Connection Timeout=1;");
                        MySQLConn.Open();
                        if (MySQLConn.State == ConnectionState.Open)
                        {
                            // Test the restarted connection
                            if (!TestConnection())
                            {
                                SQLMessage?.Invoke(EMessages.ID_Error, $"{_sqlType} Connection restart failed: test failed");
                                return;
                            }
                            
                            SQLMessage?.Invoke(EMessages.ID_Message, $"{_sqlType} Connection restarted!");
                        }
                        else
                        {
                            SQLMessage?.Invoke(EMessages.ID_Error, $"Unable to restart {_sqlType} connection.");
                        }

                        break;
                    }

                    default:
                        SQLMessage?.Invoke(EMessages.ID_Error, "Unsupported SQL server type.");
                        break;
                }
            }
            finally
            {
                _connectionLock.Release();
            }
        }
        catch (MySqlException ex)
        {
            SQLMessage?.Invoke(EMessages.ID_Error, $"{_sqlType} Connection Error [{ex.Message}]");
        }
    }

    // Disposes resources used by this SQL connection.
    [Description("Close connection and dispose resources.")]
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _connectionLock?.Dispose();
            }

            try
            {
                switch (_sqlType)
                {
                    case DB_Type.MySQL:
                    case DB_Type.MariaDB:
                    {
                        MySQLConn?.Close();
                        MySQLConn?.Dispose();
                        break;
                    }

                    default:
                        break;
                }
            }
            catch
            {
                // Suppress exceptions during cleanup
            }
        }

        _disposedValue = true;
    }

    // Disposes the SQL connection and releases resources.
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private DataTable _result = null!;

    // Executes a SELECT query and stores the result internally.
    [Description("SQLQuery. EG.: (SELECT * FROM db_accounts WHERE account = 'name';')")]
    [Obsolete("Legacy method. Consider using async query methods or Dapper-based queries for new code.")]
    public bool QuerySQL(string query)
    {
        var queryText = query ?? throw new ArgumentNullException(nameof(query));
        var result = new DataTable();
        Query(queryText, ref result);
        _result = result;
        return _result.Rows.Count > 0;
    }

    // Gets a value from the last query result.
    [Description("SQLGet. Used after the query to get a section value")]
    [Obsolete("Legacy method. Consider using typed query results for new code.")]
    public string GetSQL(string field)
    {
        if (_result is null || _result.Rows.Count == 0)
        {
            return string.Empty;
        }

        if (!_result.Columns.Contains(field))
        {
            return string.Empty;
        }

        var value = _result.Rows[0][field];
        return value is null or DBNull ? string.Empty : Convert.ToString(value) ?? string.Empty;
    }

    // Gets the last query result as a DataTable.
    [Obsolete("Legacy method. Consider using typed query results for new code.")]
    public DataTable GetDataTableSQL() => _result;

    // Executes an INSERT query.
    [Description("SQLInsert. EG.: (INSERT INTO db_textpage (pageid, text, nextpageid, wdbversion, checksum) VALUES ('pageid DWORD', 'pagetext STRING', 'nextpage DWORD', 'version DWORD', 'checksum DWORD'))")]
    [Obsolete("Legacy method. Consider using async insert methods or Dapper for new code.")]
    public void InsertSQL(string query)
    {
        Insert(query ?? throw new ArgumentNullException(nameof(query)));
    }

    // Executes an UPDATE query.
    [Description("SQLUpdate. EG.: (UPDATE db_textpage SET pagetext='pagetextstring' WHERE pageid = 'pageiddword';")]
    [Obsolete("Legacy method. Consider using async update methods or Dapper for new code.")]
    public void UpdateSQL(string query)
    {
        Update(query ?? throw new ArgumentNullException(nameof(query)));
    }

    // Executes a SELECT query synchronously and returns the result.
    public (int status, DataTable result) QueryResult(string sqlquery)
    {
        if (string.IsNullOrWhiteSpace(sqlquery))
        {
            throw new ArgumentException("Query cannot be empty.", nameof(sqlquery));
        }

        var result = new DataTable();
        
        try
        {
            EnsureConnectionOpen();

            _connectionLock.Wait();
            try
            {
                using var command = new MySqlCommand(sqlquery, MySQLConn)
                {
                    CommandTimeout = 30
                };

                using var adapter = new MySqlDataAdapter(command);
                adapter.Fill(result);
            }
            finally
            {
                _connectionLock.Release();
            }

            return ((int)ReturnState.Success, result);
        }
        catch (MySqlException ex)
        {
            SQLMessage?.Invoke(EMessages.ID_Error, $"Error executing query: {ex.Message}");
            return ((int)ReturnState.FatalError, result);
        }
        catch (Exception ex)
        {
            SQLMessage?.Invoke(EMessages.ID_Error, $"Unexpected error: {ex.Message}");
            return ((int)ReturnState.FatalError, result);
        }
    }

    // Executes a SELECT query synchronously.
    [Obsolete("Legacy method. Use QueryResult instead to avoid ref parameters.")]
    public int Query(string sqlquery, ref DataTable result)
    {
        var (status, data) = QueryResult(sqlquery);
        if (status == (int)ReturnState.Success)
        {
            result = data;
        }

        return status;
    }

    // Executes a SELECT query asynchronously and returns the result.
    public async Task<(int status, DataTable result)> QueryResultAsync(string sqlquery)
    {
        if (string.IsNullOrWhiteSpace(sqlquery))
        {
            throw new ArgumentException("Query cannot be empty.", nameof(sqlquery));
        }

        var result = new DataTable();

        try
        {
            await EnsureConnectionOpenAsync();
            await _connectionLock.WaitAsync();
            try
            {
                using var command = new MySqlCommand(sqlquery, MySQLConn)
                {
                    CommandTimeout = 30
                };

                using var adapter = new MySqlDataAdapter(command);
                await Task.Run(() => adapter.Fill(result));
            }
            finally
            {
                _connectionLock.Release();
            }

            return ((int)ReturnState.Success, result);
        }
        catch (MySqlException ex)
        {
            SQLMessage?.Invoke(EMessages.ID_Error, $"Error executing query: {ex.Message}");
            return ((int)ReturnState.FatalError, result);
        }
    }

    // Executes a SELECT query asynchronously.
    [Obsolete("Legacy method. Use QueryResultAsync instead to avoid parameter mutation.")]
    public async Task<int> QueryAsync(string sqlquery, DataTable result)
    {
        if (string.IsNullOrWhiteSpace(sqlquery))
        {
            throw new ArgumentException("Query cannot be empty.", nameof(sqlquery));
        }

        try
        {
            await EnsureConnectionOpenAsync();
            await _connectionLock.WaitAsync();
            try
            {
                using var command = new MySqlCommand(sqlquery, MySQLConn);
                using var adapter = new MySqlDataAdapter(command);
                result.Clear();
                await Task.Run(() => adapter.Fill(result));
            }
            finally
            {
                _connectionLock.Release();
            }

            return (int)ReturnState.Success;
        }
        catch (MySqlException ex)
        {
            SQLMessage?.Invoke(EMessages.ID_Error, $"Error executing query: {ex.Message}");
            return (int)ReturnState.FatalError;
        }
    }

    // Executes an INSERT query with transaction support synchronously.
    public void Insert(string sqlquery)
    {
        if (string.IsNullOrWhiteSpace(sqlquery))
        {
            throw new ArgumentException("Query cannot be empty.", nameof(sqlquery));
        }

        try
        {
            EnsureConnectionOpen();
            _connectionLock.Wait();
            try
            {
                using var transaction = MySQLConn.BeginTransaction();
                using var command = new MySqlCommand(sqlquery, MySQLConn, transaction)
                {
                    CommandTimeout = 30
                };

                command.ExecuteNonQuery();
                transaction.Commit();
            }
            finally
            {
                _connectionLock.Release();
            }
        }
        catch (MySqlException ex)
        {
            SQLMessage?.Invoke(EMessages.ID_Error, $"Error executing insert: {ex.Message}");
            throw;
        }
    }

    // Executes an INSERT query with transaction support asynchronously.
    public async Task InsertAsync(string sqlquery)
    {
        if (string.IsNullOrWhiteSpace(sqlquery))
        {
            throw new ArgumentException("Query cannot be empty.", nameof(sqlquery));
        }

        try
        {
            await EnsureConnectionOpenAsync();
            await _connectionLock.WaitAsync();
            try
            {
                using var transaction = await MySQLConn.BeginTransactionAsync();
                using var command = new MySqlCommand(sqlquery, MySQLConn, transaction);
                await command.ExecuteNonQueryAsync();
                await transaction.CommitAsync();
            }
            finally
            {
                _connectionLock.Release();
            }
        }
        catch (MySqlException ex)
        {
            SQLMessage?.Invoke(EMessages.ID_Error, $"Error executing insert: {ex.Message}");
            throw;
        }
    }

    // Executes a generic INSERT query into a table (legacy method, deprecated).
    [Obsolete("Use Insert or InsertAsync methods with parameterized queries instead.")]
    public int TableInsert(string tablename, string dbField1, string dbField1Value, string dbField2, int dbField2Value)
    {
        if (string.IsNullOrWhiteSpace(tablename) || string.IsNullOrWhiteSpace(dbField1) || string.IsNullOrWhiteSpace(dbField2))
        {
            throw new ArgumentException("Table name and field names cannot be empty.");
        }

        try
        {
            EnsureConnectionOpen();
            using var command = new MySqlCommand($"INSERT INTO `{tablename}` (`{dbField1}`, `{dbField2}`) VALUES (@field1value, @field2value)", MySQLConn)
            {
                CommandTimeout = 30
            };

            command.Parameters.AddWithValue("@field1value", dbField1Value);
            command.Parameters.AddWithValue("@field2value", dbField2Value);
            command.ExecuteNonQuery();
            return 0;
        }
        catch (Exception ex)
        {
            SQLMessage?.Invoke(EMessages.ID_Error, $"Table insert error: {ex.Message}");
            return -1;
        }
    }

    // Executes a generic SELECT query from a table (legacy method, deprecated).
    [Obsolete("Use Query or QueryAsync methods with parameterized queries instead.")]
    public DataSet TableSelect(string tablename, string returnfields, string dbField1, string dbField1Value)
    {
        var dataset = new DataSet();
        if (string.IsNullOrWhiteSpace(tablename) || string.IsNullOrWhiteSpace(returnfields) || string.IsNullOrWhiteSpace(dbField1))
        {
            return dataset;
        }

        try
        {
            EnsureConnectionOpen();
            using var command = new MySqlCommand($"SELECT {returnfields} FROM `{tablename}` WHERE `{dbField1}` = @dbField1value;", MySQLConn)
            {
                CommandTimeout = 30
            };

            command.Parameters.AddWithValue("@dbField1value", dbField1Value);
            using var adapter = new MySqlDataAdapter(command);
            adapter.Fill(dataset);
            return dataset;
        }
        catch (Exception ex)
        {
            SQLMessage?.Invoke(EMessages.ID_Error, $"Table select error: {ex.Message}");
            return dataset;
        }
    }

    // Executes an UPDATE/DELETE query synchronously.
    public void Update(string sqlquery)
    {
        if (string.IsNullOrWhiteSpace(sqlquery))
        {
            throw new ArgumentException("Query cannot be empty.", nameof(sqlquery));
        }

        try
        {
            EnsureConnectionOpen();
            _connectionLock.Wait();
            try
            {
                using var command = new MySqlCommand(sqlquery, MySQLConn)
                {
                    CommandTimeout = 30
                };

                command.ExecuteNonQuery();
            }
            finally
            {
                _connectionLock.Release();
            }
        }
        catch (MySqlException ex)
        {
            SQLMessage?.Invoke(EMessages.ID_Error, $"Error executing update: {ex.Message}");
            throw;
        }
    }

    // Executes an UPDATE/DELETE query asynchronously.
    public async Task UpdateAsync(string sqlquery)
    {
        if (string.IsNullOrWhiteSpace(sqlquery))
        {
            throw new ArgumentException("Query cannot be empty.", nameof(sqlquery));
        }

        try
        {
            await EnsureConnectionOpenAsync();
            await _connectionLock.WaitAsync();
            try
            {
                using var command = new MySqlCommand(sqlquery, MySQLConn)
                {
                    CommandTimeout = 30
                };

                await command.ExecuteNonQueryAsync();
            }
            finally
            {
                _connectionLock.Release();
            }
        }
        catch (MySqlException ex)
        {
            SQLMessage?.Invoke(EMessages.ID_Error, $"Error executing update: {ex.Message}");
            throw;
        }
    }

    // Ensures the database connection is open, restarting if necessary.
    private void EnsureConnectionOpen()
    {
        if (MySQLConn?.State != ConnectionState.Open)
        {
            Restart();
            if (MySQLConn?.State != ConnectionState.Open)
            {
                throw new InvalidOperationException("Failed to establish database connection.");
            }
        }
    }

    // Ensures the database connection is open asynchronously.
    private async Task EnsureConnectionOpenAsync()
    {
        if (MySQLConn?.State != ConnectionState.Open)
        {
            Restart();
            if (MySQLConn?.State != ConnectionState.Open)
            {
                throw new InvalidOperationException("Failed to establish database connection.");
            }
        }

        await Task.CompletedTask;
    }

    // Tests the database connection with a simple query.
    private bool TestConnection()
    {
        try
        {
            if (MySQLConn == null || MySQLConn.State != ConnectionState.Open)
            {
                SQLMessage?.Invoke(EMessages.ID_Error, "Cannot test connection: connection is not open");
                return false;
            }

            using var command = new MySqlCommand("SELECT 1", MySQLConn);
            var result = command.ExecuteScalar();
            if (result == null || Convert.ToInt32(result) != 1)
            {
                SQLMessage?.Invoke(EMessages.ID_Error, "Connection test failed: unexpected result from test query");
                return false;
            }
            
            SQLMessage?.Invoke(EMessages.ID_Message, "Database connection test passed");
            return true;
        }
        catch (Exception ex)
        {
            SQLMessage?.Invoke(EMessages.ID_Error, $"Database connection test failed: {ex.Message}");
            return false;
        }
    }
}
