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
		private CIMSqlQueryDataConnection sqldc = null;

		public PointTool()
		{
			IsSketchTool = true;
			SketchType = SketchGeometryType.Point;
			SketchOutputMode = SketchOutputMode.Map;

		}

		protected override Task OnToolDeactivateAsync(bool hasMapViewChanged)
		{
			/*
			if (featureLayer != null)
			{
				QueuedTask.Run(() =>
				{
					MapView.Active.Map.RemoveLayer(featureLayer);
				});
			}
			return base.OnToolDeactivateAsync(true);
			*/
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

		protected override async Task<bool> OnSketchCompleteAsync(Geometry geometry)
		{
			var rowCount = await QueuedTask.Run(() =>
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
				int count = command.ExecuteNonQuery();
				conn.Close();

				//ActiveMapView.Redraw(true);// RedrawAsync(true);
				featureLayer.SetDataConnection(this.sqldc);

				return count;
			});

			if (rowCount > 0)
			{
				MessageBox.Show("Point added");
			} 
			else
			{
				MessageBox.Show("Something went wrong");
			}
			
			return true;
		}

        private Task addFeatureLayer()
        {
            return ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
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
                    //CIMSqlQueryDataConnection sqldc = new CIMSqlQueryDataConnection()
					this.sqldc = new CIMSqlQueryDataConnection()
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
