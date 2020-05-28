using System;
using System.Collections.Generic;
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
	internal class PointTool : MapTool
	{
		private FeatureLayer featureLayer = null;

		public PointTool()
		{
			IsSketchTool = true;
			SketchType = SketchGeometryType.Point;
			SketchOutputMode = SketchOutputMode.Map;

			DataHelper.UserLoginHandler += onUserLogin;

		}

		void onUserLogin()
		{
			QueuedTask.Run(() =>
			{
				if (featureLayer != null)
				{
					MapView.Active.Map.RemoveLayer(featureLayer);
					addFeatureLayer();
				}
			});
		}

		protected override Task OnToolDeactivateAsync(bool hasMapViewChanged)
		{
			return QueuedTask.Run(() =>
			{
				if (featureLayer != null)
				{
					MapView.Active.Map.RemoveLayer(featureLayer);
				}
			});
		}

		protected override Task OnToolActivateAsync(bool active)
		{
			return addFeatureLayer();
		}

		protected override /*async*/ Task<bool> OnSketchCompleteAsync(Geometry geometry)
		{
			Debug.WriteLine("OnSketchCompleteAsync, enter");

			// Create an edit operation
			var createOperation = new EditOperation();
			createOperation.Name = string.Format("Create {0}", "points");
			createOperation.SelectNewFeatures = true;

			// Queue feature creation
			//createOperation.Create(featureLayer, geometry);
			var attributes = new Dictionary<string, object>();
			attributes.Add("geom", geometry);
			attributes.Add("user_id", DataHelper.userID);

			createOperation.Create(featureLayer, attributes);

			// Execute the operation
			return createOperation.ExecuteAsync();
		}

        private Task addFeatureLayer()
        {
			Debug.WriteLine("addFeatureLayer, enter");

			return ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
            {
                ArcGIS.Core.Data.DatabaseConnectionProperties connectionProperties = new DatabaseConnectionProperties(EnterpriseDatabaseType.PostgreSQL)
                {
                    AuthenticationMode = AuthenticationMode.DBMS,
                    Instance = @"127.0.0.1",
                    Database = "geomapmaker2",
                    User = "douglas",
                    Password = "password",
                    //Version = "dbo.DEFAULT"
                };

                using (Geodatabase geodatabase = new Geodatabase(connectionProperties))
                {
					using (FeatureClass featureClass = geodatabase.OpenDataset<FeatureClass>("geomapmaker2.geomapmaker2.point_features"))
					{
						var layerParamsQueryDefn = new FeatureLayerCreationParams(featureClass)
						{
							IsVisible = true,
							DefinitionFilter = new CIMDefinitionFilter()
							{
								Name = "User",
								DefinitionExpression = "user_id = " + DataHelper.userID
							}
						};
						featureLayer = LayerFactory.Instance.CreateLayer<FeatureLayer>(layerParamsQueryDefn, MapView.Active.Map);
					}
				}
			});
        }
    }
}
