using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Mapping;
using Npgsql;

namespace LoginTest02
{

    /// <summary>
    /// Interaction logic for LoginDialogWPF.xaml
    /// </summary>
    public partial class LoginDialogWPF : Window
	{
        NpgsqlConnection conn = new NpgsqlConnection("Server=127.0.0.1;User Id=douglas; " +
                   "Password=password;Database=geomapmaker2;");

        public LoginDialogWPF()
		{
			InitializeComponent();

            //NpgsqlConnection conn = new NpgsqlConnection("Server=127.0.0.1;User Id=douglas; " +
            //   "Password=password;Database=geomapmaker;");
            conn.Open();
            NpgsqlCommand command = new NpgsqlCommand("SELECT id, name, notes FROM geomapmaker2.users order by name asc", conn);
            NpgsqlDataReader dr = command.ExecuteReader();

            DataTable dT = new DataTable();
            dT.Load(dr);
            
            foreach (DataRow row in dT.Rows)
            {
                Debug.Write("Hi there \n");
                //Debug.Write("{0} \n", row["name"].ToString());
                Debug.WriteLine(row["name"].ToString());
            }


            UserCombo.ItemsSource = dT.DefaultView;
            //UserCombo.DisplayMemberPath = "name";
            UserCombo.SelectedIndex = -1;
            UserCombo.IsTextSearchEnabled = true;
            UserCombo.IsEditable = true;
            //UserCombo.Style = Com ComboBoxStyle.DropDown;
            //UserCombo. AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            //UserCombo.AutoCompleteSource = AutoCompleteSource.ListItems;
            UserCombo.SelectionChanged  += nameSelected;

            // Output rows
            //while (dr.Read())
            //    Console.Write("{0} \n", dr[0]);

            //NpgsqlCommand command = new NpgsqlCommand("SELECT name FROM public.users", conn);

            // Execute the query and obtain a result set
            //DbDataReader dr = command.ExecuteDbDataReader(SingleResult);


            conn.Close();
        }

        private void nameSelected(object sender,
        System.EventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;

            System.Data.DataRowView selectedUser = (System.Data.DataRowView)comboBox.SelectedItem;
            Debug.WriteLine("selected user = " + selectedUser);
            if (selectedUser != null)
            {
                NotesTextBox.Text = selectedUser.Row.Field<String>("notes");
            }
        }

        private void loginButton_Click(object sender, EventArgs e)
        {
            Debug.WriteLine("selected index = " + UserCombo.SelectedIndex);

            DataHelper.UserLogin();

            if (UserCombo.SelectedIndex == -1)
            {
                Debug.WriteLine("new name");
                //NpgsqlConnection conn = new NpgsqlConnection("Server=127.0.0.1;User Id=douglas; " +
                //   "Password=password;Database=geomapmaker;");
                conn.Open();
                NpgsqlCommand command = new NpgsqlCommand("insert into geomapmaker2.users (name, notes) values ($c$" + UserCombo.Text + "$c$, $n$" + NotesTextBox.Text + "$n$) returning id", conn);
                // int rowCount = command.ExecuteNonQuery();
                //this.parentModule.userID = (int)command.ExecuteScalar();
                DataHelper.userID = (int)command.ExecuteScalar();
                //Debug.WriteLine("user id = " + id);
                conn.Close();
            }
            else
            {
                Debug.WriteLine("old name");
                //NpgsqlConnection conn = new NpgsqlConnection("Server=127.0.0.1;User Id=douglas; " +
                //   "Password=password;Database=geomapmaker;");
                conn.Open();
                NpgsqlCommand command = new NpgsqlCommand("update geomapmaker2.users set notes = $n$" + NotesTextBox.Text + "$n$ where name = $c$" + UserCombo.Text + "$c$ returning id", conn);
                //int rowCount = command.ExecuteNonQuery();
                //this.parentModule.userID = (int)command.ExecuteScalar();
                DataHelper.userID = (int)command.ExecuteScalar();
                conn.Close();
            }
            DataHelper.userName = UserCombo.Text;
            Debug.WriteLine("selected text = " + UserCombo.Text);
            Debug.WriteLine("LoginDialog, userID = " + DataHelper.userID);
            // UserSelectionFinishedEventArgs userSelectionFinishedEventArgs = new UserSelectionFinishedEventArgs(this.parentModule.userID); //TODO: using module context for now

            //FrameworkApplication.EventAggregator.GetEvent<UserSelectionFinishedEvent>().Publish(
            //UserSelectionFinishedEvent.Publish(userSelectionFinishedEventArgs);
             FrameworkApplication.State.Activate("user_logged_in");
            //addFeatureLayer();
            this.DialogResult = true;// DialogResult.OK;
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = false;  //DialogResult.Cancel;
        }

        private async Task openDatabase()
        {
            await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
            {
                //Get Layers that are NOT Group layers and are unchecked
                //var layers = MapView.Active.Map.Layers.ToList();
                //MapView.Active.Map.RemoveLayers(layers);

                // Opening a Non-Versioned SQL Server instance.
                ArcGIS.Core.Data.DatabaseConnectionProperties connectionProperties = new DatabaseConnectionProperties(EnterpriseDatabaseType.PostgreSQL)
                {
                    AuthenticationMode = AuthenticationMode.DBMS,

                    // Where testMachine is the machine where the instance is running and testInstance is the name of the SqlServer instance.
                    Instance = @"127.0.0.1",

                    // Provided that a database called LocalGovernment has been created on the testInstance and geodatabase has been enabled on the database.
                    Database = "geomapmaker2",

                    // Provided that a login called gdb has been created and corresponding schema has been created with the required permissions.
                    User = "douglas",
                    Password = "password",
                    //Version = "dbo.DEFAULT"
                };

                using (Geodatabase geodatabase = new Geodatabase(connectionProperties))
                {
                    // Use the geodatabase
                    CIMSqlQueryDataConnection sqldc = new CIMSqlQueryDataConnection()
                    {
                        WorkspaceConnectionString = geodatabase.GetConnectionString(),
                        GeometryType = esriGeometryType.esriGeometryPoint,
                        OIDFields = "OBJECTID",
                        Srid = "4326",
                        SqlQuery = "select * from geomapmaker2.features where id = " + DataHelper.userID + " and ST_GeometryType(geom)='ST_Point'",
                        Dataset = "features"
                    };
                    FeatureLayer flyr = (FeatureLayer)LayerFactory.Instance.CreateLayer(sqldc, MapView.Active.Map, layerName: DataHelper.userName + "'s points");

                    CIMSqlQueryDataConnection sqldcLines = new CIMSqlQueryDataConnection()
                    {
                        WorkspaceConnectionString = geodatabase.GetConnectionString(),
                        GeometryType = esriGeometryType.esriGeometryPolyline,
                        OIDFields = "OBJECTID",
                        Srid = "4326",
                        SqlQuery = "select * from geomapmaker2.features where id = " + DataHelper.userID + " and ST_GeometryType(geom)='ST_MultiLineString'",
                        Dataset = "features"
                    };
                    FeatureLayer flyrLines = (FeatureLayer)LayerFactory.Instance.CreateLayer(sqldc, MapView.Active.Map, layerName: DataHelper.userName + "'s lines");

                }
            });
        }

    }
}
