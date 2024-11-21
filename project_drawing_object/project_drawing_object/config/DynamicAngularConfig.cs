
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using static project_drawing_object.config.DynamicConfig;
namespace project_drawing_object.config
{
    public class DynamicAngularConfig
    {
        private LineAngularDimension2 angleDimension = new LineAngularDimension2();
        public DynamicAngularConfig()
        {
            this.angleDimension.SetDatabaseDefaults();
            this.angleDimension.Visible = true;
            this.angleDimension.DynamicDimension = true;
        }
        public void addToDynamicCollection(DynamicDimensionDataCollection dimensionCollection)
        {
            DynamicDimensionData dynamicDimension = new DynamicDimensionData(this.angleDimension, true, true);
            dynamicDimension.ApplicationData = DynamicIndicator.Angle;
            dynamicDimension.Editable = false;
            dynamicDimension.Focal = true;
            dimensionCollection.Add(dynamicDimension);
        }
        public void UpdateAngleDimension(Line line, double length, double angle)
        {
            this.angleDimension.XLine1Start = line.StartPoint;
            this.angleDimension.XLine1End = line.StartPoint + Vector3d.XAxis * length * 0.001;
            this.angleDimension.XLine2Start = line.StartPoint;
            this.angleDimension.XLine2End = line.EndPoint;
            if (line.StartPoint.Y > line.EndPoint.Y)
            {
                this.angleDimension.ArcPoint = line.StartPoint - length * Vector3d.XAxis.RotateBy(angle / 2, Vector3d.ZAxis);
            }
            else
            {
                this.angleDimension.ArcPoint = line.StartPoint + length * Vector3d.XAxis.RotateBy(angle / 2, Vector3d.ZAxis);
            }
        }
    }
}
