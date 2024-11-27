using Autodesk.AutoCAD.Runtime;
using project_drawing_object.jig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
[assembly: CommandClass(typeof(project_drawing_object.vm.CircleVM))]
namespace project_drawing_object.vm
{
    public class CircleVM
    {
        [CommandMethod("DCircle")]
        public static void DrawingCircle()
        {
            CircleJig.DrawingCircle();
        }
    }
}
