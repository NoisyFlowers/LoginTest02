using System;
using System.Collections.Generic;
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
		public PointTool()
		{
			/*
			IsSketchTool = true;
			SketchType = SketchGeometryType.Rectangle;
			SketchOutputMode = SketchOutputMode.Map;
			*/
			IsSketchTool = true;
			SketchType = SketchGeometryType.Point;
			SketchOutputMode = SketchOutputMode.Map;

		}

		protected override Task OnToolActivateAsync(bool active)
		{
			return base.OnToolActivateAsync(active);
		}

		protected override async Task<bool> OnSketchCompleteAsync(Geometry geometry)
		{
			//return base.OnSketchCompleteAsync(geometry);
			String sql = String.Format("insert into public.features (user_id, geom) values({0}, ST_GeomFromText('POINT({1} {2})', {3}))", DataHelper.userID, ((MapPoint)geometry).X, ((MapPoint)geometry).Y, ((MapPoint)geometry).SpatialReference.GcsWkid);
			Npgsql.NpgsqlConnection conn = new NpgsqlConnection("Server=127.0.0.1;User Id=postgres; " +
			   "Password=postgres;Database=geomapmaker;");
			conn.Open();
			NpgsqlCommand command = new NpgsqlCommand(sql, conn);
			int rowCount = command.ExecuteNonQuery();
			conn.Close();

			MapView.Active.RedrawAsync(true);
			if (rowCount > 0)
			{
				MessageBox.Show("Point added");
			} else
			{
				MessageBox.Show("Something went wrong");
			}
			return true;
		}
	}
}
