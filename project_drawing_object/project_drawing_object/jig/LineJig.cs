using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using project_drawing_object.config;
using project_drawing_object.utils;
using System.Windows.Controls;


namespace project_drawing_object.jig
{
    public class LineJig : EntityJig
    {
        private Point3d mStartPoint, mEndPoint;
        private Vector3d mNormal;
        private List<Entity> drawnLines;
        private double angle;
        private double length;
        private DynamicDimensionDataCollection dimensionCollection = new DynamicDimensionDataCollection();
        private DynamicAngularConfig dynamicAngularConfig = new DynamicAngularConfig();
        private DynamicDimensionConfig dynamicDimensionConfig = new DynamicDimensionConfig();
        private enum DimensionIndicator { Length, Angle }
        public LineJig(Point3d startPoint, Vector3d vec, List<Entity> drawnLines) : base(new Line())
        {
            this.mNormal = vec;
            this.mStartPoint = startPoint;
            if (Entity is Line line)
            {
                line.Normal = mNormal;
                line.StartPoint = startPoint;
            }
            this.drawnLines = drawnLines ?? [];
            this.mEndPoint = mStartPoint;
            dynamicAngularConfig.addToDynamicCollection(dimensionCollection);
            dynamicDimensionConfig.addToDynamicCollection(dimensionCollection);
        }
        public LineJig() : base(new Line()) { }
        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            if (Entity is not Line line) return SamplerStatus.NoChange;
            var jigOpts = CreateJigOptions();
            var res = prompts.AcquirePoint(jigOpts);
            switch (res.Status)
            {
                case PromptStatus.OK:
                    if (mouseMovesSignificantly(res, line))
                    {
                        return SamplerStatus.NoChange;
                    }
                    else
                    {
                        this.mEndPoint = res.Value;
                        return SamplerStatus.OK;
                    }
                case PromptStatus.Error:
                    return SamplerStatus.Cancel;
                case PromptStatus.Other:
                    this.mEndPoint = res.Value;
                    return SamplerStatus.OK;
                case PromptStatus.None:
                    return SamplerStatus.NoChange;
                case PromptStatus.Cancel:
                    return SamplerStatus.Cancel;
                default:
                    return SamplerStatus.OK;
            }
        }
        private bool mouseMovesSignificantly(PromptPointResult promptResult, Line line)
        {
            return promptResult.Value.DistanceTo(line.EndPoint) < 1e-3;
        }
        private JigPromptPointOptions CreateJigOptions()
        {
            var jigOpts = new JigPromptPointOptions("\nSpecify endpoint of line or");

            if (drawnLines?.Count < 2)
            {
                jigOpts.Keywords.Add("Undo");
            }
            else
            {
                jigOpts.Keywords.Add("Close");
                jigOpts.Keywords.Add("Undo");
            }
            jigOpts.BasePoint = mStartPoint;
            jigOpts.UseBasePoint = true;
            jigOpts.UserInputControls = UserInputControls.AcceptOtherInputString | UserInputControls.GovernedByUCSDetect |
                UserInputControls.Accept3dCoordinates | UserInputControls.NoNegativeResponseAccepted | UserInputControls.NoZDirectionOrtho;

            return jigOpts;
        }
        protected override bool Update()
        {
            if (Entity is Line line)
            {
                var plane = new Plane(Point3d.Origin, Vector3d.XAxis, Vector3d.YAxis);
                this.angle = (this.mEndPoint - this.mStartPoint).AngleOnPlane(plane);
                this.length = (this.mEndPoint - this.mStartPoint).Length;
                line.EndPoint = this.mEndPoint;
                dynamicDimensionConfig.UpdateLineDimension(line.StartPoint, line.EndPoint);
                dynamicAngularConfig.UpdateAngleDimension(line, length, angle);
                return true;
            }
            return false;
        }
        public  void DoDrawingLine()
        {
            var doc = ConnectDrawing.DocumentAutoCad;
            Editor ed = doc.Editor;
            PromptPointOptions ppoStart = CreateFristPromptOptions();
            PromptPointResult pprResult = ed.GetPoint(ppoStart);
            if (pprResult.Status == PromptStatus.Cancel)
            {
                return;
            }
            switch (pprResult.Status)
            {
                case PromptStatus.OK:
                    if (pprResult.Status == PromptStatus.OK)
                    {
                        Vector3d normal = Vector3d.ZAxis;
                        List<Entity> drawnLines = [];
                        LineJig lineJig = new(pprResult.Value, normal, drawnLines);
                        PromptResult dynamicPoint = ed.Drag(lineJig);
                        HandleDynamic(dynamicPoint, doc, ed, lineJig, normal);
                    }
                    break;
                case PromptStatus.None:
                    var lastPointValue = Application.GetSystemVariable("LASTPOINT");
                    if (lastPointValue is Point3d pointLast)
                    {
                        Vector3d normal = Vector3d.ZAxis;
                        var drawnLines = new List<Entity>();
                        LineJig lineJig = new(pointLast, normal, drawnLines);
                        PromptResult dynamicPoint = ed.Drag(lineJig);
                        HandleDynamic(dynamicPoint, doc, ed, lineJig, normal);
                    }
                    else
                    {
                        CommonUtils.NotificationMessage("\nNo line or arc to continue.");
                        DoDrawingLine();
                    }
                    break;
                case PromptStatus.Keyword:
                    CommonUtils.NotificationMessage("Invalid point.");
                    DoDrawingLine();
                    break;
                default:
                    break;
            }
  
           
        }
        private Entity GetEntity()
        {
            return Entity;
        }
        private static PromptPointOptions CreateFristPromptOptions()
        {
            PromptPointOptions ppoStart = new("\nSpecify first point ");
            ppoStart.AppendKeywordsToMessage = true;
            ppoStart.AllowArbitraryInput = true;
            ppoStart.AllowNone = true;
            ppoStart.UseBasePoint = false;
            ppoStart.UseDashedLine = false;
            return ppoStart;
        }
        private void HandleDynamic(PromptResult dynamicPoint, Document doc, Editor ed, LineJig lineJig, Vector3d normal)
        {
            bool statusContinue = true;
            while (statusContinue)
            {
                using (Transaction tr = doc.Database.TransactionManager.StartTransaction())
                {
                    try
                    {
                        BlockTable bt = (BlockTable)tr.GetObject(doc.Database.BlockTableId, OpenMode.ForRead);

                        BlockTableRecord btr = (BlockTableRecord)tr.GetObject(doc.Database.CurrentSpaceId, OpenMode.ForWrite);
                        switch (dynamicPoint.Status)
                        {
                            case PromptStatus.OK:
                                btr.AppendEntity(lineJig.GetEntity());
                                tr.AddNewlyCreatedDBObject(lineJig.GetEntity(), true);
                                tr.Commit();
                                lineJig.drawnLines.Add(lineJig.GetEntity());
                                lineJig = new LineJig(lineJig.mEndPoint, normal, lineJig.drawnLines);
                                dynamicPoint = ed.Drag(lineJig);
                                break;
                            case PromptStatus.Cancel:
                                statusContinue = false;
                                break;
                            case PromptStatus.Keyword:
                                HandleKeyword(ref dynamicPoint, ref lineJig, ref statusContinue, tr, btr, ed);
                                break;
                            case PromptStatus.Other:
                                //if (lineJig.GetEntity() is Line line)
                                //{
                                //    line.EndPoint = lineJig.mEndPoint;
                                //}
                                //btr.AppendEntity(lineJig.GetEntity());
                                //tr.AddNewlyCreatedDBObject(lineJig.GetEntity(), true);
                                //tr.Commit();
                                //lineJig.drawnLines.Add(lineJig.GetEntity());
                                //lineJig = new LineJig(lineJig.mEndPoint, normal, lineJig.drawnLines);
                                //dynamicPoint = ed.Drag(lineJig);
                                statusContinue = false;
                                break;
                            case PromptStatus.None:
                                statusContinue = false;
                                break;
                            default:
                                statusContinue = false;
                                break;
                        }
                    }
                    catch (Exception e)
                    {
                        ed.WriteMessage("Error {0}", e.Message);
                        tr.Abort();
                        break;
                    }
                }
            }
        }
        private  void HandleKeyword(ref PromptResult dynamicPoint, ref LineJig lineJig, ref bool statusContinue, Transaction tr, BlockTableRecord btr, Editor ed)
        {
            if (lineJig == null)
            {
                statusContinue = false;
                return;
            }
            switch (dynamicPoint.StringResult)
            {
                case "Undo":
                    if (lineJig.drawnLines.Count == 0)
                    {
                        dynamicPoint = ed.Drag(lineJig);
                        return;
                    }
                    var lastLine = (Line)lineJig.drawnLines[^1];
                    lineJig.mEndPoint = lastLine.StartPoint;
                    var normal = Vector3d.ZAxis;
                    Line? lastLineWritable = tr.GetObject(lastLine.ObjectId, OpenMode.ForWrite) as Line;

                    if (lastLineWritable != null)
                    {
                        lastLineWritable.Erase();
                        lineJig.drawnLines.Remove(lastLine);
                        lineJig = new LineJig(lastLine.StartPoint, normal, lineJig.drawnLines);
                        tr.Commit();
                        dynamicPoint = ed.Drag(lineJig);
                    }
                    break;
                case "Close":
                    if (lineJig.drawnLines.Count > 1)
                    {
                        var lineStart = (Line)lineJig.drawnLines[0];
                        var lineLast = (Line)lineJig.drawnLines[^1];
                        LineUtils.CreateLine(lineLast.EndPoint, lineStart.StartPoint, btr, tr);
                        tr.Commit();
                        statusContinue = false;
                    }
                    break;

                default:
                    statusContinue = false;
                    break;
            }
        }



        protected override DynamicDimensionDataCollection GetDynamicDimensionData(double dimScale)
        {
            return dimensionCollection;
        }
    }
}
