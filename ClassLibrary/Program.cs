﻿using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Diagnostics;

namespace ClassLibrary;

internal class Program
{
    private static async Task Main(string[] args)
    {
#if DEBUG
        Console.WriteLine($"Process ID: {Process.GetCurrentProcess().Id}");
        Console.WriteLine("Waiting for debugger to attach... Press ENTER to continue");
        Console.ReadLine();
        Console.WriteLine("Continuing ..");
#endif

        string connType;
        string connServer;
        string connDb;
        string connUser;
        string connPass;
        var cmd = new RootCommand
        {
            new Option<string>("--conn-type", "This accepts win or sql") { IsRequired = true },
            new Option<string>("--conn-server", "The SQL Server Name") { IsRequired = true },
            new Option<string>("--conn-db", "The SQL Database Name") { IsRequired = true },
            new Option<string>("--conn-user", description: "The SQL Auth User Name", getDefaultValue: () => "sa"),
            new Option<string>("--conn-pass", "The SQL Auth User Pass"),
            new Option<string>("--sqlite-path",
                    "The path for the sqlite database to be created. Note this must be fully qualified. or it will create it where the bin is")
                { IsRequired = true }
        };

        cmd.Handler =
            CommandHandler.Create<string, string, string, string?, string?, string>((connType, connServer, connDb,
                connUser, connPass, sqlitePath) =>
            {
                // Expand ~ to home directory (shell doesn't do this automatically in .NET)
                if (sqlitePath.StartsWith("~/") || sqlitePath == "~")
                {
                    var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                    sqlitePath = sqlitePath == "~" ? home : Path.Combine(home, sqlitePath.Substring(2));
                }
                
                // Make the path absolute
                sqlitePath = Path.GetFullPath(sqlitePath);
                
                var sqlConnString = "";
                var dirPath = Path.GetDirectoryName(sqlitePath);
                var fileName = Path.GetFileName(sqlitePath);
                
                Console.WriteLine($"Creating SQLite database at: {sqlitePath}");
                
                var di = Directory.CreateDirectory(dirPath);
                if (di == null)
                {
                    Console.WriteLine("Sorry but can't create your file in that dir");
                    return;
                }

                if (connType == "win")
                {
                    sqlConnString = GetSqlServerConnectionString(connServer, connDb);
                }
                else
                {
                    if (connUser == null || connPass == null)
                    {
                        Console.WriteLine("You must provide user and pass if you are not going to use windows auth");
                        return;
                    }

                    sqlConnString = GetSqlServerConnectionString(connServer, connDb, connUser, connPass);
                }

                // change true to argument for Triggers
                // change false to argument for Views
                // change true to argument for GuiDAs String
                SqlServerToSQLite.ConvertSqlServerToSQLiteDatabase(sqlConnString, sqlitePath, null, null,
                    null, null, true, false, true, true);

                // Verify the file was created and show its size
                Console.WriteLine();
                if (File.Exists(sqlitePath))
                {
                    var fileInfo = new FileInfo(sqlitePath);
                    var sizeInBytes = fileInfo.Length;
                    string sizeDisplay;
                    
                    if (sizeInBytes >= 1024 * 1024 * 1024) // GB
                    {
                        sizeDisplay = $"{sizeInBytes / (1024.0 * 1024.0 * 1024.0):F2} GB";
                    }
                    else if (sizeInBytes >= 1024 * 1024) // MB
                    {
                        sizeDisplay = $"{sizeInBytes / (1024.0 * 1024.0):F2} MB";
                    }
                    else if (sizeInBytes >= 1024) // KB
                    {
                        sizeDisplay = $"{sizeInBytes / 1024.0:F2} KB";
                    }
                    else
                    {
                        sizeDisplay = $"{sizeInBytes} bytes";
                    }
                    
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("═══════════════════════════════════════════════════════════════");
                    Console.WriteLine($"✓ SUCCESS! SQLite database created");
                    Console.WriteLine("═══════════════════════════════════════════════════════════════");
                    Console.WriteLine($"Location: {sqlitePath}");
                    Console.WriteLine($"File Size: {sizeDisplay} ({sizeInBytes:N0} bytes)");
                    Console.WriteLine("═══════════════════════════════════════════════════════════════");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("═══════════════════════════════════════════════════════════════");
                    Console.WriteLine("✗ WARNING: SQLite file was not created!");
                    Console.WriteLine("═══════════════════════════════════════════════════════════════");
                    Console.WriteLine($"Expected location: {sqlitePath}");
                    Console.WriteLine("Please check for errors above.");
                    Console.WriteLine("═══════════════════════════════════════════════════════════════");
                    Console.ResetColor();
                }

                //string sqlitePath = txtSQLitePath.Text.Trim();
                //    SqlConversionHandler handler = new SqlConversionHandler(delegate (bool done,
                //        bool success, int percent, string msg)
                //    {
                //        Invoke(new MethodInvoker(delegate ()
                //        {
                //            UpdateSensitivity();
                //            lblMessage.Text = msg;
                //            pbrProgress.Value = percent;

                //            if (done)
                //            {
                //                btnStart.Enabled = true;
                //                this.Cursor = Cursors.Default;
                //                UpdateSensitivity();

                //                if (success)
                //                {
                //                    MessageBox.Show(this,
                //                        msg,
                //                        "Conversion Finished",
                //                        MessageBoxButtons.OK,
                //                        MessageBoxIcon.Information);
                //                    pbrProgress.Value = 0;
                //                    lblMessage.Text = string.Empty;
                //                }
                //                else
                //                {
                //                    if (!_shouldExit)
                //                    {
                //                        MessageBox.Show(this,
                //                            msg,
                //                            "Conversion Failed",
                //                            MessageBoxButtons.OK,
                //                            MessageBoxIcon.Error);
                //                        pbrProgress.Value = 0;
                //                        lblMessage.Text = string.Empty;
                //                    }
                //                    else
                //                        Application.Exit();
                //                }
                //            }
                //        }));
                //    });
                //    SqlTableSelectionHandler selectionHandler = new SqlTableSelectionHandler(delegate (List<TableSchema> schema)
                //    {
                //        List<TableSchema> updated = null;
                //        Invoke(new MethodInvoker(delegate
                //        {
                //                // Allow the user to select which tables to include by showing him the 
                //                // table selection dialog.
                //            TableSelectionDialog dlg = new TableSelectionDialog();
                //            DialogResult res = dlg.ShowTables(schema, this);
                //            if (res == DialogResult.OK)
                //                updated = dlg.IncludedTables;
                //        }));
                //        return updated;
                //    });

                //    FailedViewDefinitionHandler viewFailureHandler = new FailedViewDefinitionHandler(delegate (ViewSchema vs)
                //    {
                //        string updated = null;
                //        Invoke(new MethodInvoker(delegate
                //        {
                //            ViewFailureDialog dlg = new ViewFailureDialog();
                //            dlg.View = vs;
                //            DialogResult res = dlg.ShowDialog(this);
                //            if (res == DialogResult.OK)
                //                updated = dlg.ViewSQL;
                //            else
                //                updated = null;
                //        }));

                //        return updated;
                //    });

                //    string password = txtPassword.Text.Trim();
                //    if (!cbxEncrypt.Checked)
                //        password = null;
                //    SqlServerToSQLite.ConvertSqlServerToSQLiteDatabase(sqlConnString, sqlitePath, password, handler,
                //        selectionHandler, viewFailureHandler, cbxTriggers.Checked, createViews, chkBox_treatGuidAsString.Checked);
                //}
            });

        cmd.Invoke(args);
    }

    private static string GetSqlServerConnectionString(string address, string db)
    {
        var res = @"Data Source=" + address.Trim() +
                  ";Initial Catalog=" + db.Trim() + ";Integrated Security=SSPI;ApplicationIntent=ReadOnly";
        return res;
    }

    private static string GetSqlServerConnectionString(string address, string db, string user, string pass)
    {
        var res = @"Data Source=" + address.Trim() +
                  ";Initial Catalog=" + db.Trim() + ";User ID=" + user.Trim() + ";Password=" + pass.Trim() +
                  ";ApplicationIntent=ReadOnly";
        return res;
    }

    //private static string GetSqlServerConnectionString(string address, string db)
    //{
    //    string res = @"Data Source=" + address.Trim() +
    //            ";Initial Catalog=" + db.Trim() + ";Integrated Security=SSPI;";
    //    return res;
    //}
    //private static string GetSqlServerConnectionString(string address, string db, string user, string pass)
    //{
    //    string res = @"Data Source=" + address.Trim() +
    //        ";Initial Catalog=" + db.Trim() + ";User ID=" + user.Trim() + ";Password=" + pass.Trim();
    //    return res;
    //}
}