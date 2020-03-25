﻿using System;
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
	internal class PolyTool : MapTool
	{
		private FeatureLayer featureLayer = null;

		public PolyTool()
		{
			IsSketchTool = true;
			SketchType = SketchGeometryType.Polygon;
			SketchOutputMode = SketchOutputMode.Map;

		}

		protected override Task OnToolDeactivateAsync(bool hasMapViewChanged)
		{
			if (featureLayer != null)
			{
				QueuedTask.Run(() =>
				{
					MapView.Active.Map.RemoveLayer(featureLayer);
				});
			}

			return base.OnToolDeactivateAsync(hasMapViewChanged);
		}

		protected override Task OnToolActivateAsync(bool active)
		{
			addFeatureLayer();

			return base.OnToolActivateAsync(active);
		}

		protected override async Task<bool> OnSketchCompleteAsync(Geometry geometry)
		{
			WKTExportFlags exportFlagsNoZ = WKTExportFlags.wktExportStripZs;
			WKTExportFlags exportFlagsNoM = WKTExportFlags.wktExportStripMs;
			var wktString = GeometryEngine.Instance.ExportToWKT(exportFlagsNoZ | exportFlagsNoM, geometry);

			Debug.WriteLine("geometry = " + wktString);

			var srid = geometry.SpatialReference.GcsWkid;
			Debug.WriteLine("srid = " + srid);

			//String sql = String.Format("insert into public.features (user_id, geom) values({0}, ST_GeomFromText('POINT({1} {2})', {3}))", DataHelper.userID, ((MapPoint)geometry).X, ((MapPoint)geometry).Y, ((MapPoint)geometry).SpatialReference.GcsWkid);
			String sql = String.Format("insert into public.features (user_id, geom) values({0}, ST_GeomFromText('{1}', {2}))", DataHelper.userID, wktString, srid);
			Debug.WriteLine("sql = " + sql);
			Npgsql.NpgsqlConnection conn = new NpgsqlConnection("Server=127.0.0.1;User Id=postgres; " +
			   "Password=postgres;Database=geomapmaker;");
			conn.Open();
			NpgsqlCommand command = new NpgsqlCommand(sql, conn);
			int rowCount = command.ExecuteNonQuery();
			conn.Close();

			await MapView.Active.RedrawAsync(true);

			if (rowCount > 0)
			{
				MessageBox.Show("Polygon added");
			}
			else
			{
				MessageBox.Show("Something went wrong");
			}


			return true;
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
						GeometryType = esriGeometryType.esriGeometryPolygon,
						OIDFields = "OBJECTID",
						Srid = "4326",
						SqlQuery = "select * from public.features where user_id = " + DataHelper.userID + " and ST_GeometryType(geom)='ST_MultiPolygon'",
						Dataset = "features"
					};
					featureLayer = (FeatureLayer)LayerFactory.Instance.CreateLayer(sqldc, MapView.Active.Map, layerName: DataHelper.userName + "'s polygons");

				}
			});
		}
	}
}