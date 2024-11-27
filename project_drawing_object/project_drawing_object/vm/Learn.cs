using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using project_drawing_object.config;
using project_drawing_object.jig;
[assembly: CommandClass(typeof(project_drawing_object.vm.Learn))]
namespace project_drawing_object.vm
{
    public class Learn
    {
        [CommandMethod("Learn")]
        public  void DLearn()
        {
            var doc = ConnectDrawing.DocumentAutoCad;
            Editor ed = doc.Editor;
           
                using (Transaction tr = doc.TransactionManager.StartTransaction())
                {
                try
                {
                    BlockTable bt = (BlockTable)doc.Database.BlockTableId.GetObject(OpenMode.ForWrite);
                    BlockTableRecord newBlr = new BlockTableRecord();
                    newBlr.Name = "Nhan";
                    bt.Add(newBlr);
                    tr.AddNewlyCreatedDBObject(newBlr, true);
                    tr.Commit();
                    
                }
                catch (System.Exception ex)
                {
                    ed.WriteMessage(ex.Message);
                }
            
            }                      
        }
        [CommandMethod("ADDBLOCK")]
        public void AddBlock()
        {
            var doc = ConnectDrawing.DocumentAutoCad;
            Editor ed = doc.Editor;

            using (Transaction tr = doc.TransactionManager.StartTransaction())
            {
                try
                {
                    BlockTable bt = (BlockTable)doc.Database.BlockTableId.GetObject(OpenMode.ForWrite);
                    BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt["Nhan"], OpenMode.ForWrite);
                    Line line = new Line();
                    line.StartPoint = new Autodesk.AutoCAD.Geometry.Point3d(0, 0, 0);
                    line.EndPoint = new Autodesk.AutoCAD.Geometry.Point3d(15, 15, 0);
                    btr.AppendEntity(line);
                    tr.AddNewlyCreatedDBObject(line, true);
                    tr.Commit();
                }
                catch (System.Exception ex)
                {
                    ed.WriteMessage(ex.Message);
                }

            }
        }
    }
}
