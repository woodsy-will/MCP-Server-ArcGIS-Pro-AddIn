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
using ArcGIS.Desktop.KnowledgeGraph;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APBridgeAddIn
{
    internal class Button1 : Button
    {
        protected override void OnClick()
        {
            try
            {
                Module1.Current.StartBridgeService();
                MessageBox.Show("Bridge service started!\n\nNamed Pipe: ArcGisProBridgePipe\n\nThe MCP server can now connect.", "Success");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to start bridge service:\n\n{ex.Message}\n\n{ex.StackTrace}", "Error");
            }
        }
    }
}
