using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Npgsql;

namespace LoginTest02
{
    /// <summary>
    /// Represents the ComboBox
    /// </summary>
    internal class ProjectsComboBox : ComboBox
    {
        NpgsqlConnection conn = new NpgsqlConnection("Server=127.0.0.1;User Id=geomapmaker; " +
                   "Password=password;Database=geomapmaker;");


        private bool _isInitialized;

        /// <summary>
        /// Combo Box constructor
        /// </summary>
        public ProjectsComboBox()
        {
            //UpdateCombo();
            DataHelper.UserLoginHandler += onUserLogin;
        }

        void onUserLogin()
        {
            _isInitialized = false;
            UpdateCombo();
        }

    /// <summary>
    /// Updates the combo box with all the items.
    /// </summary>

    private void UpdateCombo()
        {

            if (_isInitialized)
                SelectedItem = null; // ItemCollection.FirstOrDefault(); //set the default item in the comboBox

            if (!_isInitialized)
            {
                Clear();
                conn.Open();
                NpgsqlCommand command = new NpgsqlCommand("SELECT * FROM geomapmaker.projects where id in (select project_id from user_project_links where user_id = " + DataHelper.userID + ") order by name asc", conn);
                NpgsqlDataReader dr = command.ExecuteReader();

                DataTable dT = new DataTable();
                dT.Load(dr);

                Add(new ComboBoxItem("<choose>")); //TODO: This is only here because I'm not sure how to have a combobox without an initial selection
                foreach (DataRow row in dT.Rows)
                {
                    //Debug.Write("Hi there \n");
                    //Debug.Write("{0} \n", row["name"].ToString());
                    Debug.WriteLine(row["name"].ToString());
                    Add(new ProjectComboBoxItem(row["name"].ToString(), row["notes"].ToString(), row["connection_properties"].ToString()));
                }

                conn.Close();
            }

            Enabled = true; //enables the ComboBox
            SelectedItem = ItemCollection.FirstOrDefault(); //set the default item in the comboBox

        }


        /// <summary>
        /// The on comboBox selection change event. 
        /// </summary>
        /// <param name="item">The newly selected combo box item</param>
        protected override void OnSelectionChange(ComboBoxItem item)
        {
            //Debug.WriteLine("item type = " + item.GetType());
            if (item == null)
                return;

            if (string.IsNullOrEmpty(item.Text))
                return;

            // TODO  Code behavior when selection changes.
            if (item is ProjectComboBoxItem) //TODO: This is only here because I'm not sure how to have a combobox without an initial selection
            {
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show($"connection properties: " + ((ProjectComboBoxItem)item).connectionProperties);
            }
        }

    }
}
