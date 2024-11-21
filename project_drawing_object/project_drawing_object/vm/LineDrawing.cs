using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using project_drawing_object.config;
using project_drawing_object.jig;
using project_drawing_object.utils;



[assembly: Autodesk.AutoCAD.Runtime.ExtensionApplication(null)]
[assembly: CommandClass(typeof(project_drawing_object.vm.LineDrawing))]
namespace project_drawing_object.vm
{
    public class LineDrawing
    {

        [CommandMethod("Dline")]
        public static void DrawingLine()
        {

            /* var doc = ConnectDrawing.DocumentAutoCad;
             Editor ed = doc.Editor;
             List<Line> drawnLines = [];
             List<Point3d> points = [];
             PromptPointOptions ppoStart = new("\nSpecify first point ");
             PromptPointResult pprResult = ed.GetPoint(ppoStart);
             if (pprResult.Status != PromptStatus.OK) return;

             points.Add(pprResult.Value);
             Point3d lastPoint = pprResult.Value;
             Database db = doc.Database;
             Autodesk.AutoCAD.DatabaseServices.TransactionManager tm = db.TransactionManager;

             while (true)
             {
                 using Transaction myT = tm.StartTransaction();
                 try
                 {
                     BlockTable bt = (BlockTable)tm.GetObject(db.BlockTableId, OpenMode.ForRead, false);
                     BlockTableRecord btr = (BlockTableRecord)tm.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite, false);
                     PromptPointOptions ppoNext;
                     if (points.Count == 1)
                     {
                         ppoNext = new PromptPointOptions("\nSpecify next point ");
                     }
                     else if (points.Count < 3)
                     {
                         ppoNext = new PromptPointOptions("\nSpecify next point or");
                         ppoNext.Keywords.Add("Undo");
                     }
                     else
                     {
                         ppoNext = new PromptPointOptions("\nSpecify next point or");
                         ppoNext.Keywords.Add("Cancel");
                         ppoNext.Keywords.Add("Undo");
                     }
                     ppoNext.UseBasePoint = true;
                     ppoNext.BasePoint = lastPoint;
                     PromptPointResult pprNext = ed.GetPoint(ppoNext);

                     if (pprNext.Status == PromptStatus.Cancel || pprNext.Status == PromptStatus.None)
                     {
                         if (points.Count == 0)
                         {
                             points.Clear();
                         }
                         break;
                     }
                     if (pprNext.Status == PromptStatus.Keyword)
                     {
                         if (CommonUtils.EnterKeyIntoCommand("Undo", pprNext.StringResult))
                         {
                             if (drawnLines.Count > 0)
                             {
                                 Line lastLine = drawnLines[drawnLines.Count - 1];
                                 Line lastLineSaved = LineUtils.GetSavedLine(lastLine, OpenMode.ForWrite, myT);
                                 lastLineSaved.Erase();
                                 drawnLines.RemoveAt(drawnLines.Count - 1);
                                 points.RemoveAt(points.Count - 1);
                                 myT.Commit();

                                 lastPoint = points.Count > 0 ? points[points.Count - 1] : lastPoint;
                                 continue;
                             }

                         }
                         if (points.Count > 2)
                         {
                             if (CommonUtils.EnterKeyIntoCommand("Cancel", pprNext.StringResult))
                             {
                                 Line closingLine = LineUtils.CreateLine(lastPoint, points[0], btr, myT);
                                 //LineUtils.SaveLine(closingLine, true, btr, myT);
                                 lastPoint = pprNext.Value;
                                 drawnLines.Add(closingLine);
                                 myT.Commit();
                                 break;
                             }
                         }

                     }
                     if (pprNext.Status == PromptStatus.OK)
                     {
                         ed.WriteMessage("\n Write line to database");
                         points.Add(pprNext.Value);
                         Line line = LineUtils.CreateLine(lastPoint, pprNext.Value, btr, myT);
                         //LineUtils.SaveLine(line, true, btr, myT);
                         lastPoint = pprNext.Value;
                         drawnLines.Add(line);
                         myT.Commit();
                     }
                 }
                 catch (System.Exception ex)
                 {
                     ed.WriteMessage("Error {0}", ex.Message);
                     myT.Abort();
                 }*/
            //}
            LineJig.DoDrawingLine();
        }
        [CommandMethod("CreateRotatedDimension")]
        public static void CreateRotatedDimension()
        {
            // Get the current database
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;

            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Block table for read
                BlockTable acBlkTbl;
                acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId,
                                                OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec;
                acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
                                                OpenMode.ForWrite) as BlockTableRecord;

                // Create the rotated dimension
                using (RotatedDimension acRotDim = new RotatedDimension())
                {
                    acRotDim.XLine1Point = new Point3d(0, 0, 0);
                    acRotDim.XLine2Point = new Point3d(6, 3, 0);
                    acRotDim.Rotation = 0.707;
                    acRotDim.DimLinePoint = new Point3d(0, 5, 0);
                    acRotDim.DimensionStyle = acCurDb.Dimstyle;

                    // Add the new object to Model space and the transaction
                    acBlkTblRec.AppendEntity(acRotDim);
                    acTrans.AddNewlyCreatedDBObject(acRotDim, true);
                }

                // Commit the changes and dispose of the transaction
                acTrans.Commit();
            }
        }
    } 
}
