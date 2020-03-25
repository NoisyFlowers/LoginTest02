using System;
using System.Collections.Generic;
using System.Data;
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
     internal class LoginButton : Button
    {
        protected override void OnClick()
        {
            //LoginDialog lD = new LoginDialog(((Module1)FrameworkApplication.FindModule("LoginTest02_Module")).helper);
            //LoginDialog lD = new LoginDialog();
            LoginDialogWPF lD = new LoginDialogWPF();
            lD.ShowDialog();
        }
    }
}
