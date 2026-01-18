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
                Logger.Info("Manual bridge service start requested via button");
                Module1.Current.StartBridgeService();
                MessageBox.Show($"Bridge service started!\n\nNamed Pipe: ArcGisProBridgePipe\n\nThe MCP server can now connect.\n\nLogs: {Logger.GetLogPath()}", "Success");
            }
            catch (Exception ex)
            {
                Logger.Error("Manual bridge service start failed", ex);
                MessageBox.Show($"Failed to start bridge service:\n\n{ex.Message}\n\nCheck logs at: {Logger.GetLogPath()}", "Error");
            }
        }
    }
}
