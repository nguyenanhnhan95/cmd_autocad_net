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
        public void UpdateLineDimension(Line line)
        {
            this.alignedDimension.XLine1Point = line.StartPoint;
            this.alignedDimension.XLine2Point = line.EndPoint;
            
            if (line.StartPoint.Y > line.EndPoint.Y)
            {
                this.alignedDimension.DimLinePoint = line.EndPoint.RotateBy(-Math.PI / 30, Vector3d.ZAxis, line.StartPoint);
            }
            else
            {
                this.alignedDimension.DimLinePoint = line.EndPoint.RotateBy(+Math.PI / 30, Vector3d.ZAxis, line.StartPoint);
            }
        }
        public AlignedDimension GetAlignedDimension()
        {
            return this.alignedDimension;
        }
    }
}
