using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ArcGIS.Desktop.Framework;
using Npgsql;

namespace LoginTest02
{
    public partial class LoginDialog : Form
    {
        //private DataHelper helper;

        public LoginDialog(/*DataHelper helper*/)
        {
            //this.helper = helper;

            InitializeComponent();

            NpgsqlConnection conn = new NpgsqlConnection("Server=127.0.0.1;User Id=postgres; " +
               "Password=postgres;Database=geomapmaker;");
            conn.Open();
            NpgsqlCommand command = new NpgsqlCommand("SELECT id, name, notes FROM public.users order by name asc", conn);
            NpgsqlDataReader dr = command.ExecuteReader();

            DataTable dT = new DataTable();
            dT.Load(dr);
            /*
            foreach (DataRow row in dT.Rows)
            {
                Debug.Write("Hi there \n");
                Debug.Write("{0} \n", row["name"].ToString());
            }
            */

            comboBox1.DataSource = dT;
            comboBox1.DisplayMember = "name";
            comboBox1.SelectedIndex = -1;
            comboBox1.DropDownStyle = ComboBoxStyle.DropDown;
            comboBox1.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            comboBox1.AutoCompleteSource = AutoCompleteSource.ListItems;
            comboBox1.SelectedIndexChanged += nameSelected;

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
            //Debug.WriteLine("selected user = " + selectedUser);
            notesBox.Text = selectedUser.Row.Field<String>("notes");
        }

        private void loginButton_Click(object sender, EventArgs e)
        {
            Debug.WriteLine("selected index = " + comboBox1.SelectedIndex);
            if (comboBox1.SelectedIndex == -1)
            {
                Debug.WriteLine("new name");
                NpgsqlConnection conn = new NpgsqlConnection("Server=127.0.0.1;User Id=postgres; " +
                   "Password=postgres;Database=geomapmaker;");
                conn.Open();
                NpgsqlCommand command = new NpgsqlCommand("insert into public.users (name, notes) values ($c$" + comboBox1.Text + "$c$, $n$" + notesBox.Text + "$n$) returning id", conn);
                // int rowCount = command.ExecuteNonQuery();
                //this.parentModule.userID = (int)command.ExecuteScalar();
                DataHelper.userID = (int)command.ExecuteScalar();
                //Debug.WriteLine("user id = " + id);
                conn.Close();
            } else
            {
                Debug.WriteLine("old name");
                NpgsqlConnection conn = new NpgsqlConnection("Server=127.0.0.1;User Id=postgres; " +
                   "Password=postgres;Database=geomapmaker;");
                conn.Open();
                NpgsqlCommand command = new NpgsqlCommand("update public.users set notes = $n$" + notesBox.Text + "$n$ where name = $c$" + comboBox1.Text + "$c$ returning id", conn);
                //int rowCount = command.ExecuteNonQuery();
                //this.parentModule.userID = (int)command.ExecuteScalar();
                DataHelper.userID = (int)command.ExecuteScalar();
                conn.Close();
            }
            Debug.WriteLine("selected text = " + comboBox1.Text);
            Debug.WriteLine("LoginDialog, userID = " + DataHelper.userID);
            // UserSelectionFinishedEventArgs userSelectionFinishedEventArgs = new UserSelectionFinishedEventArgs(this.parentModule.userID); //TODO: using module context for now

            //FrameworkApplication.EventAggregator.GetEvent<UserSelectionFinishedEvent>().Publish(
            //UserSelectionFinishedEvent.Publish(userSelectionFinishedEventArgs);
            FrameworkApplication.State.Activate("user_logged_in");
            this.DialogResult = DialogResult.OK;
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }
    }
}
