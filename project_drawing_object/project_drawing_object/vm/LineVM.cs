using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using project_drawing_object.config;
using project_drawing_object.jig;
using project_drawing_object.utils;



[assembly: Autodesk.AutoCAD.Runtime.ExtensionApplication(null)]
[assembly: CommandClass(typeof(project_drawing_object.vm.LineVM))]
namespace project_drawing_object.vm
{
    public class LineVM
    {

        [CommandMethod("Dline")]
        public static void DrawingLine()
        {
            LineJig lineJig = new LineJig();
            lineJig.DoDrawingLine();
        }
        
    } 
}
