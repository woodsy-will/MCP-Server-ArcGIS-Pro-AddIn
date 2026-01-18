using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
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

namespace APBridgeAddIn
{
    internal class Module1 : Module
    {
        private static Module1 _this = null;
        private ProBridgeService _service;

        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static Module1 Current => _this ??= (Module1)FrameworkApplication.FindModule("APBridgeAddIn_Module");

        /// <summary>
        /// Start the Named Pipe bridge service
        /// </summary>
        public void StartBridgeService()
        {
            if (_service == null)
            {
                Logger.Info("Initializing bridge service");
                _service = new ProBridgeService("ArcGisProBridgePipe");
                _service.Start();
                Logger.Info("Bridge service started successfully");
            }
            else
            {
                Logger.Warning("Bridge service already running");
            }
        }

        #region Overrides
        /// <summary>
        /// Called by Framework when ArcGIS Pro starts and the module is loaded
        /// </summary>
        /// <returns>True if initialization is successful</returns>
        protected override bool Initialize()
        {
            try
            {
                Logger.Info("APBridgeAddIn module initializing");
                // Auto-start the bridge service when ArcGIS Pro loads
                StartBridgeService();
                Logger.Info("APBridgeAddIn module initialized successfully");
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to auto-start bridge service", ex);
                MessageBox.Show($"Failed to auto-start bridge service:\n\n{ex.Message}\n\nCheck logs at: {Logger.GetLogPath()}", "Bridge Service Error");
            }
            return base.Initialize();
        }

        /// <summary>
        /// Called by Framework when ArcGIS Pro is closing
        /// </summary>
        /// <returns>False to prevent Pro from closing, otherwise True</returns>
        protected override bool CanUnload()
        {
            Logger.Info("APBridgeAddIn module unloading");
            _service?.Dispose();
            Logger.Info("Bridge service disposed");
            return true;
        }

        #endregion Overrides

    }
}
