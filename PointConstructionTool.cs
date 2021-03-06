﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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


namespace LoginTest02
{
    internal class PointConstructionTool : MapTool
    {
        private FeatureLayer featureLayer = null;
        private CIMSqlQueryDataConnection sqldc = null;

        public PointConstructionTool()
        {
            IsSketchTool = true;
            UseSnapping = true;
            // Select the type of construction tool you wish to implement.  
            // Make sure that the tool is correctly registered with the correct component category type in the daml 
            SketchType = SketchGeometryType.Point;
            // SketchType = SketchGeometryType.Line;
            // SketchType = SketchGeometryType.Polygon;
            //Gets or sets whether the sketch is for creating a feature and should use the CurrentTemplate.
            UsesCurrentTemplate = true;
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
            Debug.WriteLine("OnToolActivateAsync enter");
            return addFeatureLayer();
        }

        /// <summary>
        /// Called when the sketch finishes. This is where we will create the sketch operation and then execute it.
        /// </summary>
        /// <param name="geometry">The geometry created by the sketch.</param>
        /// <returns>A Task returning a Boolean indicating if the sketch complete event was successfully handled.</returns>
        protected override Task<bool> OnSketchCompleteAsync(Geometry geometry)
        {
            if (CurrentTemplate == null || geometry == null)
                return Task.FromResult(false);

            // Create an edit operation
            var createOperation = new EditOperation();
            createOperation.Name = string.Format("Create {0}", CurrentTemplate.Layer.Name);
            createOperation.SelectNewFeatures = true;

            // Queue feature creation
            createOperation.Create(CurrentTemplate, geometry);

            // Execute the operation
            return createOperation.ExecuteAsync();
        }
        private Task addFeatureLayer()
        {
            Debug.WriteLine("addFeatureLayer enter");

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

                    /*
					string url = @"C:\Users\Douglas\Documents\testCollections\GeneWash.gdb\GeneWash.gdb\CrossSectionB\CSBMapUnitPolys";  //FeatureClass of a FileGeodatabase

					Uri uri = new Uri(url);
					featureLayer = (FeatureLayer)LayerFactory.Instance.CreateLayer(uri, MapView.Active.Map);
					*/
                }
            });
        }
    }
}
