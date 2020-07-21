using ArcGIS.Desktop.Framework.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoginTest02
{
    class ProjectComboBoxItem : ComboBoxItem
    {
        public String connectionProperties;
        public ProjectComboBoxItem(String name, String toolTip, String connectionProps) : base(name, null, toolTip)
        {
            this.connectionProperties = connectionProps;
        }
    }
}
