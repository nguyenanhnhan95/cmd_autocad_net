using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;

namespace project_drawing_object.config
{
    public class DynamicConfig
    {

        public enum DynamicIndicator { Length, Angle }
        //private void UpdateDynamic(Line line, DynamicIndicator dynamicIndicator, double length = 0.0, double angle = 0.0)
        //{
        //    foreach (DynamicDimensionData ddd in dimensionCollection)
        //    {
        //        switch (dynamicIndicator)
        //        {
        //            case DynamicIndicator.Length:
        //                var alignedDim = (AlignedDimension)ddd.Dimension;
        //                dynamicDimensionConfig.UpdateLineDimension(alignedDim, line);
        //                break;

        //            case DynamicIndicator.Angle:
        //                var angularDim = (LineAngularDimension2)ddd.Dimension;
        //                dynamicAngularConfig.UpdateAngleDimension(angularDim, line, length, angle);
        //                break;

        //            default:
        //                break;
        //        }
        //    }
        //}
    }
}
