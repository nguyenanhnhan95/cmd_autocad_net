using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace project_drawing_object.config
{
    public class DynamicDimensionConfig
    {
        private AlignedDimension alignedDimension = new AlignedDimension();
        public DynamicDimensionConfig()
        {
            this.alignedDimension.SetDatabaseDefaults();
            this.alignedDimension.Visible = true;
            this.alignedDimension.DynamicDimension = true;
        }

        public void addToDynamicCollection(DynamicDimensionDataCollection dimensionCollection)
        {
            AlignedDimension dim = this.alignedDimension;
            var dynamicDimension = new DynamicDimensionData(dim, true, true);
            dynamicDimension.ApplicationData = DynamicConfig.DynamicIndicator.Length;
            dynamicDimension.Editable = true;
            dynamicDimension.Focal = true;
            dimensionCollection.Add(dynamicDimension);
        }
        public void UpdateLineDimension(Point3d StartPoint, Point3d EndPoint)
        {
            this.alignedDimension.XLine1Point = StartPoint;
            this.alignedDimension.XLine2Point = EndPoint;
            
            if (StartPoint.Y > EndPoint.Y)
            {
                this.alignedDimension.DimLinePoint = EndPoint.RotateBy(-Math.PI / 30, Vector3d.ZAxis, StartPoint);
            }
            else
            {
                this.alignedDimension.DimLinePoint = EndPoint.RotateBy(+Math.PI / 30, Vector3d.ZAxis, StartPoint);
            }
        }
        public AlignedDimension GetAlignedDimension()
        {
            return this.alignedDimension;
        }
    }
}
