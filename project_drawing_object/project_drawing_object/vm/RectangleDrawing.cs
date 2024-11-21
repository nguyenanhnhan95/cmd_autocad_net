using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using project_drawing_object.config;



[assembly: CommandClass(typeof(project_drawing_object.vm.RectangleDrawing))]
namespace project_drawing_object.vm
{
    public class RectangleDrawing
    {
        [CommandMethod("DRectangle")]
        public static void DrawingRectangle()
        {

            var doc = ConnectDrawing.DocumentAutoCad;
            Editor ed = doc.Editor;
            var dynFormat = Application.GetSystemVariable("DYNPIFORMAT");
            short dynFormatValue = Convert.ToInt16(dynFormat);
            if (dynFormatValue == 0) {
                Application.SetSystemVariable("DYNPIFORMAT", 1);
            }
        }
    }
}
