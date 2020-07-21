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
using ArcGIS.Desktop.Core.Geoprocessing;
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
	internal class PolyTool : MapTool
	{
		private FeatureLayer featureLayer = null;

		public PolyTool()
		{
			IsSketchTool = true;
			SketchType = SketchGeometryType.Polygon;
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
			//return addFeatureLayer();

			/*
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
				IReadOnlyList<string> createParams = Geoprocessing.MakeValueArray(new object[] { geodatabase, "fc01", null, null });
				return Geoprocessing.ExecuteToolAsync("management.CreateFeatureClass", createParams);
			}
			*/
			
			return QueuedTask.Run(async () =>
			{
				//This works, but creates an xml representation of the connect string. Not sure that's what the next call wants
				var cStr = "";
				IReadOnlyList<string> cParams = Geoprocessing.MakeValueArray(new object[] {
					"POSTGRESQL", 
					@"127.0.0.1",
					"DATABASE_AUTH ",
					"geomapmaker2",
					"password",
					"geomapmaker2"
				});
				IGPResult res = await Geoprocessing.ExecuteToolAsync("management.CreateDatabaseConnectionString", cParams);
				if (!res.IsFailed)
				{
					Debug.WriteLine("conn string = " + res.ReturnValue);
					cStr = res.ReturnValue;
				} else
				{
					Debug.WriteLine("failed ErrorCode " + res.ErrorCode);
					Debug.WriteLine("failed ReturnValue " + res.ReturnValue);
					foreach (IGPMessage msg in res.Messages)
					{
						Debug.WriteLine("failed msg text " + msg.Text);

					}
				}
				
				/*
				// this works
				IReadOnlyList<string> cParams = Geoprocessing.MakeValueArray(new object[] {
					"C:/toolTest",
					"test01"
				});
				IGPResult res = await Geoprocessing.ExecuteToolAsync("management.CreateFolder", cParams);
				if (!res.IsFailed)
				{
					Debug.WriteLine("folder = " + res.ReturnValue);
				}
				else
				{
					Debug.WriteLine("failed!!!! " + res.ErrorCode);
					Debug.WriteLine("failed!!!! " + res.ReturnValue);
					foreach (IGPMessage msg in res.Messages)
					{
						Debug.WriteLine("failed!!!! " + msg.Text);

					}
				}
				*/


				ArcGIS.Core.Data.DatabaseConnectionProperties connectionProperties = new DatabaseConnectionProperties(EnterpriseDatabaseType.PostgreSQL)
				{
					AuthenticationMode = AuthenticationMode.DBMS,
					Instance = @"127.0.0.1",
					Database = "geomapmaker2",
					User = "geomapmaker2",
					Password = "password",
					//Version = "dbo.DEFAULT"
				};

				using (Geodatabase geodatabase = new Geodatabase(connectionProperties))
				{
					//Call fails no matter what I pass as connection string. No error is generated.
					cStr = geodatabase.GetConnectionString();
					//cStr = "ENCRYPTED_PASSWORD=00022e6877513471743162776f4c684e454a3363556157765177616373677361465977513463364c2b314c386165733d2a00;SERVER=127.0.0.1;INSTANCE=sde:postgresql:127.0.0.1;DBCLIENT=postgresql;DB_CONNECTION_PROPERTIES=127.0.0.1;DATABASE=geomapmaker2;USER=geomapmaker2;VERSION=sde.DEFAULT;AUTHENTICATION_MODE=DBMS";
					//TODO: build this connection string using GeoProcessing "management.CreateDatabaseConnectionString"?
					Debug.WriteLine("cStr = " + cStr);
					IReadOnlyList<string> createParams = Geoprocessing.MakeValueArray(new object[] { cStr, "fc01", "POLYGON" });
					//file gdb works:
					//IReadOnlyList<string> createParams = Geoprocessing.MakeValueArray(new object[] { "C:/toolTest", "fc01", "POLYGON" });
					res = await Geoprocessing.ExecuteToolAsync("management.CreateFeatureclass", createParams);
					if (res.IsFailed)
					{
						Debug.WriteLine("feature class fail!!! = " + res.ErrorCode);
						Debug.WriteLine("fc failed ReturnValue " + res.ReturnValue);
						foreach (IGPMessage msg in res.Messages)
						{
							Debug.WriteLine("fc failed msg text " + msg.Text);

						}
					}
				}
			});
			

		}

		protected override /*async*/ Task<bool> OnSketchCompleteAsync(Geometry geometry)
		{
			Debug.WriteLine("OnSketchCompleteAsync, enter");

			// Create an edit operation
			var createOperation = new EditOperation();
			createOperation.Name = string.Format("Create {0}", "polygons");
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
					using (FeatureClass featureClass = geodatabase.OpenDataset<FeatureClass>("geomapmaker2.geomapmaker2.polygon_features"))
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