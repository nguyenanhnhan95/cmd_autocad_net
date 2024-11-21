using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;


namespace project_drawing_object.utils
{
    public class LineUtils
    {
        public static Line CreateLine(Point3d pointStart, Point3d pointEnd, BlockTableRecord btr, Transaction myT)
        {
            Line line = new(pointStart, pointEnd);
            btr.AppendEntity(line);
            myT.AddNewlyCreatedDBObject(line, true);
            return line;
        }
        public static Line GetSavedLine(Line line,OpenMode openMode, Transaction myT)
        {
            return (Line)myT.GetObject(line.ObjectId, openMode);
        }
        public static void SaveLine(Line line,bool optionSave, BlockTableRecord btr, Transaction myT)
        {
            btr.AppendEntity(line);
            myT.AddNewlyCreatedDBObject(line, optionSave);
        }
    }
}
