using System;
using System.Collections.Generic;
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
	internal class SelectTool : MapTool
	{
		private FeatureLayer featureLayer = null;

		public SelectTool()
		{
			IsSketchTool = true;
			SketchType = SketchGeometryType.Point;
			SketchOutputMode = SketchOutputMode.Map;
		}

		protected override Task OnToolActivateAsync(bool active)
		{
			addFeatureLayer();
			return base.OnToolActivateAsync(active);
		}

		protected override Task<bool> OnSketchCompleteAsync(Geometry geometry)
		{
			WKTExportFlags exportFlagsNoZ = WKTExportFlags.wktExportStripZs;
			WKTExportFlags exportFlagsNoM = WKTExportFlags.wktExportStripMs;
			var wktString = GeometryEngine.Instance.ExportToWKT(exportFlagsNoZ | exportFlagsNoM, geometry);

			Debug.WriteLine("geometry = " + wktString);

			var srid = geometry.SpatialReference.GcsWkid;
			Debug.WriteLine("srid = " + srid);

			//String sql = String.Format("select id from public.features where ST_Intersects(geom, ST_GeomFromText('{0}', {1}))", wktString, srid);
			String sql = String.Format("select id from public.features where ST_Intersects(geom, ST_Buffer(ST_GeomFromText('{0}', {1}), 0.05))", wktString, srid);
			Debug.WriteLine("sql = " + sql);
			Npgsql.NpgsqlConnection conn = new NpgsqlConnection("Server=127.0.0.1;User Id=postgres; " +
			   "Password=postgres;Database=geomapmaker;");
			conn.Open();
			NpgsqlCommand command = new NpgsqlCommand(sql, conn);
			NpgsqlDataReader dr = command.ExecuteReader();

			DataTable dT = new DataTable();
			dT.Load(dr);

			foreach (DataRow row in dT.Rows)
			{
				Debug.Write("Hi there \n");
				//Debug.Write("{0} \n", row["name"].ToString());
				Debug.WriteLine(row["id"].ToString());
			}
			conn.Close();

			//return base.OnSketchCompleteAsync(geometry);
			return ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
			{
				// Using the active map view to select
				// the features that intersect the sketch geometry
				ActiveMapView.SelectFeatures(geometry);
				return true;
			});
		}

		private async Task addFeatureLayer()
		{
			await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
			{
				ArcGIS.Core.Data.DatabaseConnectionProperties connectionProperties = new DatabaseConnectionProperties(EnterpriseDatabaseType.PostgreSQL)
				{
					AuthenticationMode = AuthenticationMode.DBMS,
					Instance = @"127.0.0.1",
					Database = "geomapmaker",
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
						SqlQuery = "select * from public.features where user_id = " + DataHelper.userID + " and ST_GeometryType(geom)='ST_Point'",
						Dataset = "features"
					};
					featureLayer = (FeatureLayer)LayerFactory.Instance.CreateLayer(sqldc, MapView.Active.Map, layerName: DataHelper.userName + "'s points");

				}
			});
		}
	}
}
