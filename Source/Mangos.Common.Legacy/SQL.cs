//
// Copyright (C) 2013-2022 getMaNGOS <https://getmangos.eu>
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

using MySql.Data.MySqlClient;
using System;
using System.ComponentModel;
using System.Data;
using System.Threading;

namespace Mangos.Common.Legacy;

public class SQL : IDisposable
{
    private MySqlConnection MySQLConn;

    public enum EMessages
    {
        ID_Error = 0,
        ID_Message = 1
    }

    public event SQLMessageEventHandler SQLMessage;

    public delegate void SQLMessageEventHandler(EMessages MessageID, string OutBuf);

    // <Description("Class info version/last date updated.")> _
    // Public ReadOnly Property Class_Version_Info() As String
    // Get
    // Return "Version: " + VInfo + ", Updated at: " + rvDate
    // End Get
    // End Property
    // #End Region

    private string v_SQLHost = "localhost";
    private string v_SQLPort = "3306";
    private string v_SQLUser = "";
    private string v_SQLPass = "";
    private string v_SQLDBName = "";

    public enum DB_Type
    {
        MySQL = 0
    }

    public enum ReturnState
    {
        Success = 0,
        MinorError = 1,
        FatalError = 2
    }

    private DB_Type v_SQLType;

    [Description("SQL Server selection.")]
    public DB_Type SQLTypeServer
    {
        get
        {
            var SQLTypeServerRet = v_SQLType;
            return SQLTypeServerRet;
        }

        set => v_SQLType = value;
    }

    [Description("SQL Host name.")]
    public string SQLHost
    {
        get
        {
            var SQLHostRet = v_SQLHost;
            return SQLHostRet;
        }

        set => v_SQLHost = value;
    }

    [Description("SQL Host port.")]
    public string SQLPort
    {
        get
        {
            var SQLPortRet = v_SQLPort;
            return SQLPortRet;
        }

        set => v_SQLPort = value;
    }

    [Description("SQL User name.")]
    public string SQLUser
    {
        get
        {
            var SQLUserRet = v_SQLUser;
            return SQLUserRet;
        }

        set => v_SQLUser = value;
    }

    [Description("SQL Password.")]
    public string SQLPass
    {
        get
        {
            var SQLPassRet = v_SQLPass;
            return SQLPassRet;
        }

        set => v_SQLPass = value;
    }

    [Description("SQL Database name.")]
    public string SQLDBName
    {
        get
        {
            var SQLDBNameRet = v_SQLDBName;
            return SQLDBNameRet;
        }

        set => v_SQLDBName = value;
    }

    [Description("Start up the SQL connection.")]
    public int Connect()
    {
        try
        {
            if (SQLHost.Length < 1)
            {
                SQLMessage?.Invoke(EMessages.ID_Error, "You have to set the SQLHost cannot be empty");
                return (int)ReturnState.FatalError;
            }

            if (SQLPort.Length < 1)
            {
                SQLMessage?.Invoke(EMessages.ID_Error, "You have to set the SQLPort cannot be empty");
                return (int)ReturnState.FatalError;
            }

            if (SQLUser.Length < 1)
            {
                SQLMessage?.Invoke(EMessages.ID_Error, "You have to set the SQLUser cannot be empty");
                return (int)ReturnState.FatalError;
            }

            if (SQLPass.Length < 1)
            {
                SQLMessage?.Invoke(EMessages.ID_Error, "You have to set the SQLPassword cannot be empty");
                return (int)ReturnState.FatalError;
            }

            if (SQLDBName.Length < 1)
            {
                SQLMessage?.Invoke(EMessages.ID_Error, "You have to set the SQLDatabaseName cannot be empty");
                return (int)ReturnState.FatalError;
            }

            switch (v_SQLType)
            {
                case DB_Type.MySQL:
                    {
                        MySQLConn = new MySqlConnection(string.Format("Server={0};Port={4};User ID={1};Password={2};Database={3};Compress=false;Connection Timeout=1;", SQLHost, SQLUser, SQLPass, SQLDBName, SQLPort));
                        MySQLConn.Open();
                        SQLMessage?.Invoke(EMessages.ID_Message, "MySQL Connection Opened Successfully [" + SQLUser + "@" + SQLHost + "]");
                        break;
                    }
            }
        }
        catch (MySqlException e)
        {
            SQLMessage?.Invoke(EMessages.ID_Error, "MySQL Connection Error [" + e.Message + "]");
            return (int)ReturnState.FatalError;
        }

        return (int)ReturnState.Success;
    }

    [Description("Restart the SQL connection.")]
    public void Restart()
    {
        try
        {
            switch (v_SQLType)
            {
                case DB_Type.MySQL:
                    {
                        MySQLConn.Close();
                        MySQLConn.Dispose();
                        MySQLConn = new MySqlConnection(string.Format("Server={0};Port={4};User ID={1};Password={2};Database={3};Compress=false;Connection Timeout=1;", SQLHost, SQLUser, SQLPass, SQLDBName, SQLPort));
                        MySQLConn.Open();
                        if (MySQLConn.State == ConnectionState.Open)
                        {
                            SQLMessage?.Invoke(EMessages.ID_Message, "MySQL Connection restarted!");
                        }
                        else
                        {
                            SQLMessage?.Invoke(EMessages.ID_Error, "Unable to restart MySQL connection.");
                        }

                        break;
                    }
            }
        }
        catch (MySqlException e)
        {
            SQLMessage?.Invoke(EMessages.ID_Error, "MySQL Connection Error [" + e.Message + "]");
        }
    }

    private bool _disposedValue; // To detect redundant calls

    // IDisposable
    [Description("Close file and dispose the wdb reader.")]
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            // TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
            // TODO: set large fields to null.
            switch (v_SQLType)
            {
                case DB_Type.MySQL:
                    {
                        MySQLConn.Close();
                        MySQLConn.Dispose();
                        break;
                    }
            }
        }

        _disposedValue = true;
    }

    // This code added by Visual Basic to correctly implement the disposable pattern.
    public void Dispose()
    {
        // Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private string mQuery = "";
    private DataTable mResult;

    [Description("SQLQuery. EG.: (SELECT * FROM db_accounts WHERE account = 'name';')")]
    public bool QuerySQL(string SQLQuery)
    {
        mQuery = SQLQuery;
        Query(mQuery, ref mResult);
        if (mResult.Rows.Count > 0)
        {
            // Table gathered
            return true;
        }

        // Table dosent exist
        return false;
    }

    [Description("SQLGet. Used after the query to get a section value")]
    public string GetSQL(string TableSection)
    {
        return mResult.Rows[0][TableSection].ToString();
    }

    public DataTable GetDataTableSQL()
    {
        return mResult;
    }

    [Description("SQLInsert. EG.: (INSERT INTO db_textpage (pageid, text, nextpageid, wdbversion, checksum) VALUES ('pageid DWORD', 'pagetext STRING', 'nextpage DWORD', 'version DWORD', 'checksum DWORD')")]
    public void InsertSQL(string SQLInsertionQuery)
    {
        Insert(SQLInsertionQuery);
    }

    [Description("SQLUpdate. EG.: (UPDATE db_textpage SET pagetext='pagetextstring' WHERE pageid = 'pageiddword';")]
    public void UpdateSQL(string SQLUpdateQuery)
    {
        Update(SQLUpdateQuery);
    }

    public int Query(string sqlquery, ref DataTable Result)
    {
        switch (v_SQLType)
        {
            case DB_Type.MySQL:
                {
                    if (MySQLConn.State != ConnectionState.Open)
                    {
                        Restart();
                        if (MySQLConn.State != ConnectionState.Open)
                        {
                            SQLMessage?.Invoke(EMessages.ID_Error, "MySQL Database Request Failed!");
                            return (int)ReturnState.MinorError;
                        }
                    }

                    break;
                }
        }

        var ExitCode = (int)ReturnState.Success;
        try
        {
            switch (v_SQLType)
            {
                case DB_Type.MySQL:
                    {
                        Monitor.Enter(MySQLConn);
                        MySqlCommand MySQLCommand = new(sqlquery, MySQLConn);
                        MySqlDataAdapter MySQLAdapter = new(MySQLCommand);
                        if (Result is null)
                        {
                            Result = new DataTable();
                        }
                        else
                        {
                            Result.Clear();
                        }

                        MySQLAdapter.Fill(Result);
                        break;
                    }
            }
        }
        catch (MySqlException e)
        {
            SQLMessage?.Invoke(EMessages.ID_Error, "Error Reading From MySQL Database " + e.Message);
            SQLMessage?.Invoke(EMessages.ID_Error, "Query string was: " + sqlquery);
            ExitCode = (int)ReturnState.FatalError;
        }
        finally
        {
            switch (v_SQLType)
            {
                case DB_Type.MySQL:
                    {
                        Monitor.Exit(MySQLConn);
                        break;
                    }
            }
        }

        return ExitCode;
    }

    public void Insert(string sqlquery)
    {
        switch (v_SQLType)
        {
            case DB_Type.MySQL:
                {
                    if (MySQLConn.State != ConnectionState.Open)
                    {
                        Restart();
                        if (MySQLConn.State != ConnectionState.Open)
                        {
                            SQLMessage?.Invoke(EMessages.ID_Error, "MySQL Database Request Failed!");
                            return;
                        }
                    }

                    break;
                }
        }

        try
        {
            switch (v_SQLType)
            {
                case DB_Type.MySQL:
                    {
                        Monitor.Enter(MySQLConn);
                        var MySQLTransaction = MySQLConn.BeginTransaction();
                        MySqlCommand MySQLCommand = new(sqlquery, MySQLConn, MySQLTransaction);
                        MySQLCommand.ExecuteNonQuery();
                        MySQLTransaction.Commit();
                        Console.WriteLine("transaction completed");
                        break;
                    }
            }
        }
        catch (MySqlException e)
        {
            SQLMessage?.Invoke(EMessages.ID_Error, "Error Reading From MySQL Database " + e.Message);
            SQLMessage?.Invoke(EMessages.ID_Error, "Insert string was: " + sqlquery);
        }
        finally
        {
            switch (v_SQLType)
            {
                case DB_Type.MySQL:
                    {
                        Monitor.Exit(MySQLConn);
                        break;
                    }
            }
        }
    }

    // TODO: Apply proper implementation as needed
    public int TableInsert(string tablename, string dbField1, string dbField1Value, string dbField2, int dbField2Value)
    {
        MySqlCommand cmd = new("", MySQLConn);
        cmd.Connection.Open();
        cmd.CommandText = "insert into `" + tablename + "`(`" + dbField1 + "`,`" + dbField2 + "`) " + "VALUES (@field1value, @field2value)";
        cmd.Parameters.AddWithValue("@field1value", dbField1Value);
        cmd.Parameters.AddWithValue("@field2value", dbField2Value);
        try
        {
            cmd.ExecuteScalar();
            cmd.Connection.Close();
            return 0;
        }
        catch (Exception)
        {
            cmd.Connection.Close();
            return -1;
        }
    }

    // TODO: Apply proper implementation as needed
    public DataSet TableSelect(string tablename, string returnfields, string dbField1, string dbField1Value)
    {
        MySqlCommand cmd = new("", MySQLConn);
        cmd.Connection.Open();
        cmd.CommandText = "select " + returnfields + " FROM `" + tablename + "` WHERE `" + dbField1 + "` = '@dbField1value';";
        cmd.Parameters.AddWithValue("@dbfield1value", dbField1Value);
        try
        {
            MySqlDataAdapter adapter = new();
            DataSet myDataset = new();
            adapter.SelectCommand = cmd;
            adapter.Fill(myDataset);
            cmd.ExecuteScalar();
            cmd.Connection.Close();
            return myDataset;
        }
        catch (Exception)
        {
            cmd.Connection.Close();
            return null;
        }
    }

    public void Update(string sqlquery)
    {
        switch (v_SQLType)
        {
            case DB_Type.MySQL:
                {
                    if (MySQLConn.State != ConnectionState.Open)
                    {
                        Restart();
                        if (MySQLConn.State != ConnectionState.Open)
                        {
                            SQLMessage?.Invoke(EMessages.ID_Error, "MySQL Database Request Failed!");
                            return;
                        }
                    }

                    break;
                }
        }

        try
        {
            switch (v_SQLType)
            {
                case DB_Type.MySQL:
                    {
                        Monitor.Enter(MySQLConn);
                        MySqlCommand MySQLCommand = new(sqlquery, MySQLConn);
                        MySqlDataAdapter MySQLAdapter = new(MySQLCommand);
                        DataTable result = new();
                        MySQLAdapter.Fill(result);
                        break;
                    }
            }
        }
        catch (MySqlException e)
        {
            SQLMessage?.Invoke(EMessages.ID_Error, "Error Reading From MySQL Database " + e.Message);
            SQLMessage?.Invoke(EMessages.ID_Error, "Update string was: " + sqlquery);
        }
        finally
        {
            switch (v_SQLType)
            {
                case DB_Type.MySQL:
                    {
                        Monitor.Exit(MySQLConn);
                        break;
                    }
            }
        }
    }
}
