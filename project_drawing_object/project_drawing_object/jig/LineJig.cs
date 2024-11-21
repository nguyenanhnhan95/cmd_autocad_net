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
                jigOpts.Keywords.Add("Undo");
                jigOpts.Keywords.Add("Close");
            }

            jigOpts.BasePoint = mStartPoint;
            jigOpts.UseBasePoint = true;
            jigOpts.UserInputControls = UserInputControls.Accept3dCoordinates |
                                         UserInputControls.NoZeroResponseAccepted |
                                         UserInputControls.NoNegativeResponseAccepted;

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
                dynamicDimensionConfig.UpdateLineDimension(line);
                dynamicAngularConfig.UpdateAngleDimension(line,length,angle);
                return true;
            }
            return false;
        }
        static public void DoDrawingLine()
        {
            var doc = ConnectDrawing.DocumentAutoCad;
            Editor ed = doc.Editor;
            PromptPointOptions ppoStart = new("\nSpecify first point ");
            PromptPointResult pprResult = ed.GetPoint(ppoStart);
            if (pprResult.Status == PromptStatus.Cancel)
            {
                return;
            }
            if (pprResult.Status == PromptStatus.OK)
            {
                Vector3d normal = Vector3d.ZAxis;
                List<Entity> drawnLines = [];
                LineJig lineJig = new(pprResult.Value, normal, drawnLines);
                PromptResult dynamicPoint = ed.Drag(lineJig);
                bool statusContinue = true;
                while (statusContinue)
                {
                    using (Transaction tr = doc.Database.TransactionManager.StartTransaction())
                    {
                        try
                        {
                            BlockTable bt = (BlockTable)tr.GetObject(doc.Database.BlockTableId, OpenMode.ForRead);
                            BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
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
                                    if (lineJig.GetEntity() is Line line)
                                    {
                                        line.EndPoint = lineJig.mEndPoint;
                                    }
                                    btr.AppendEntity(lineJig.GetEntity());
                                    tr.AddNewlyCreatedDBObject(lineJig.GetEntity(), true);
                                    tr.Commit();
                                    lineJig.drawnLines.Add(lineJig.GetEntity());
                                    lineJig = new LineJig(lineJig.mEndPoint, normal, lineJig.drawnLines);
                                    dynamicPoint = ed.Drag(lineJig);
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

        }


        private Entity GetEntity()
        {
            return Entity;
        }
        private static void HandleKeyword(ref PromptResult dynamicPoint, ref LineJig lineJig, ref bool statusContinue, Transaction tr, BlockTableRecord btr, Editor ed)
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
        //private  LineAngularDimension2 createAngleDimension()
        //{
        //    LineAngularDimension2 angleDimension = new LineAngularDimension2();
        //    angleDimension.SetDatabaseDefaults();
        //    angleDimension.Visible = true;
        //    angleDimension.DynamicDimension = true;
        //    return angleDimension;
        //}
        //private  AlignedDimension createAlignedDimension()
        //{
        //    var dim = new AlignedDimension();
        //    dim.SetDatabaseDefaults();
        //    dim.Visible = true;
        //    dim.DynamicDimension = true;
        //    return dim;
        //}
        //private void addToDynamicCollection(LineAngularDimension2 angleDimension)
        //{
        //    DynamicDimensionData dynamicDimension = new DynamicDimensionData(angleDimension, true, true);
        //    dynamicDimension.ApplicationData = DimensionIndicator.Angle;
        //    dynamicDimension.Editable = false;
        //    dynamicDimension.Focal = true;
        //    dimensionCollection.Add(dynamicDimension);
        //}
        //private void addToDynamicCollection(AlignedDimension dim)
        //{
        //    var dynamicDimension = new DynamicDimensionData(dim, true, true);
        //    dynamicDimension.ApplicationData = DimensionIndicator.Length;
        //    dynamicDimension.Editable = true;
        //    dynamicDimension.Focal = true;
        //    dimensionCollection.Add(dynamicDimension);
        //}
        //private void UpdateLineDimension(AlignedDimension alignedDimension, Line line)
        //{
        //    alignedDimension.XLine1Point = line.StartPoint;
        //    alignedDimension.XLine2Point = line.EndPoint;
        //    alignedDimension.DimLinePoint = line.EndPoint.RotateBy(Math.PI / 30, Vector3d.ZAxis, line.StartPoint);
        //}
        //private void UpdateAngleDimension(LineAngularDimension2 angularDimension, Line line)
        //{
        //    angularDimension.XLine1Start = line.StartPoint;
        //    angularDimension.XLine1End = line.StartPoint + Vector3d.XAxis * length * 0.001;
        //    angularDimension.XLine2Start = line.StartPoint;
        //    angularDimension.XLine2End = line.EndPoint;
        //    if (line.StartPoint.Y > line.EndPoint.Y)
        //    {
        //        angularDimension.ArcPoint = line.StartPoint - length * Vector3d.XAxis.RotateBy(angle / 2, Vector3d.ZAxis);
        //    }
        //    else
        //    {
        //        angularDimension.ArcPoint = line.StartPoint + length * Vector3d.XAxis.RotateBy(angle / 2, Vector3d.ZAxis);
        //    }
        //}
        //private void UpdateDimensions(Line line)
        //{
        //    foreach (DynamicDimensionData ddd in dimensionCollection)
        //    {
        //        switch ((DimensionIndicator)ddd.ApplicationData)
        //        {
        //            case DimensionIndicator.Length:
        //                var alignedDim = (AlignedDimension)ddd.Dimension;
        //                UpdateLineDimension(alignedDim, line);
        //                break;

        //            case DimensionIndicator.Angle:
        //                var angularDim = (LineAngularDimension2)ddd.Dimension;
        //                UpdateAngleDimension(angularDim, line);
        //                break;

        //            default:
        //                break;
        //        }
        //    }
        //}

        protected override DynamicDimensionDataCollection GetDynamicDimensionData(double dimScale)
        {
            return dimensionCollection;
        }
    }
}
