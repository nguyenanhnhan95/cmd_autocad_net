using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: CommandClass(typeof(project_drawing_object.vm.TestJig))]
namespace project_drawing_object.vm
{
    public class TestJig : EntityJig
    {
        #region Fields

        private enum DimensionIndicator { Length, Angle }

        private Point3d start;
        private Point3d currentPoint;
        private bool isAngleLocked = false;
        private double angle;
        private bool isLengthLocked = false;
        private double length;
        private DynamicDimensionDataCollection dimensionCollection = new DynamicDimensionDataCollection();

        #endregion Fields

        public TestJig(Point3d start, Line entity) : base(entity)
        {
            this.start = start;
            this.currentPoint = start;

            LineAngularDimension2 angleDimension = createAngleDimension();
            addToDynamicCollection(angleDimension);

            AlignedDimension dim = createAlignedDimension();
            addToDynamicDollection(dim);
        }

        #region Constructor Helpers

        private void addToDynamicDollection(AlignedDimension dim)
        {
            var dynamicDimension = new DynamicDimensionData(dim, true, true);
            dynamicDimension.ApplicationData = DimensionIndicator.Length;
            dynamicDimension.Editable = true;
            dynamicDimension.Focal = true;

            dimensionCollection.Add(dynamicDimension);
        }

        private static AlignedDimension createAlignedDimension()
        {
            var dim = new AlignedDimension();
            dim.SetDatabaseDefaults();
            dim.Visible = true;
            dim.DynamicDimension = true;
            return dim;
        }

        private void addToDynamicCollection(LineAngularDimension2 angleDimension)
        {
            DynamicDimensionData dynamicDimension = new DynamicDimensionData(angleDimension, true, true);
            dynamicDimension.ApplicationData = DimensionIndicator.Angle;
            dynamicDimension.Editable = true;
            dynamicDimension.Focal = true;
            dimensionCollection.Add(dynamicDimension);
        }

        private static LineAngularDimension2 createAngleDimension()
        {
            LineAngularDimension2 angleDimension = new LineAngularDimension2();
            angleDimension.SetDatabaseDefaults();
            angleDimension.Visible = true;
            angleDimension.DynamicDimension = true;
            return angleDimension;
        }

        #endregion Constructor Helpers

        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            JigPromptPointOptions promptOption = createPromptPointOption();
            var promptPointResult = prompts.AcquirePoint(promptOption);

            switch (promptPointResult.Status)
            {
                case PromptStatus.OK:
                    if (mouseMovesSignificantly(promptPointResult))
                    {
                        return SamplerStatus.NoChange;
                    }
                    else
                    {
                        currentPoint = promptPointResult.Value;
                        return SamplerStatus.OK;
                    }

                case PromptStatus.Error:
                case PromptStatus.Cancel:
                    return SamplerStatus.Cancel;

                default:
                    return SamplerStatus.OK;
            }
        }

        #region Sampler Helpers

        private JigPromptPointOptions createPromptPointOption()
        {
            var promptOption = new JigPromptPointOptions();
            promptOption.UserInputControls = (UserInputControls.Accept3dCoordinates | UserInputControls.NullResponseAccepted);
            promptOption.BasePoint = start;
            promptOption.UseBasePoint = true;
            return promptOption;
        }

        private bool mouseMovesSignificantly(PromptPointResult promptResult)
        {
            return promptResult.Value.DistanceTo(currentPoint) < 1e-3;
        }

        #endregion Sampler Helpers

        protected override bool Update()
        {
            Line line = (Line)Entity;
            var plane = new Plane(Point3d.Origin, Vector3d.XAxis, Vector3d.YAxis);
            if (!isAngleLocked)
                angle = (currentPoint - start).AngleOnPlane(plane);
            if (!isLengthLocked)
                length = (currentPoint - start).Length;
            currentPoint = start + length * Vector3d.XAxis.RotateBy(angle, Vector3d.ZAxis);
            line.EndPoint = currentPoint;

            UpdateDimensions(line);

            return true;
        }

        #region Update Dimensions helpers when mouse cursor moves

        private void UpdateDimensions(Line line)
        {
            foreach (DynamicDimensionData ddd in dimensionCollection)
            {
                switch ((DimensionIndicator)ddd.ApplicationData)
                {
                    case DimensionIndicator.Length:
                        var alignedDim = (AlignedDimension)ddd.Dimension;
                        UpdateLineDimension(alignedDim);
                        break;

                    case DimensionIndicator.Angle:
                        var angularDim = (LineAngularDimension2)ddd.Dimension;
                        UpdateAngleDimension(angularDim);
                        break;

                    default:
                        break;
                }
            }
        }

        private void UpdateAngleDimension(LineAngularDimension2 angularDimension)
        {
            angularDimension.XLine1Start = start;
            angularDimension.XLine1End = start + Vector3d.XAxis * length * 0.001;
            angularDimension.XLine2Start = start;
            angularDimension.XLine2End = currentPoint;
            angularDimension.ArcPoint = start + length * Vector3d.XAxis.RotateBy(angle / 2, Vector3d.ZAxis);
        }

        private void UpdateLineDimension(AlignedDimension alignedDimension)
        {
            alignedDimension.XLine1Point = start;
            alignedDimension.XLine2Point = currentPoint;
            alignedDimension.DimLinePoint = currentPoint.RotateBy(Math.PI / 30, Vector3d.ZAxis, start);
        }

        protected override DynamicDimensionDataCollection GetDynamicDimensionData(double dimScale)
        {
            return dimensionCollection;
        }

        #endregion Update Dimensions helpers when mouse cursor moves

        #region Event Handler should the user manually change the dimensions - update the values to reflect this

        protected override void OnDimensionValueChanged(DynamicDimensionChangedEventArgs e)
        {
            var dynamicDimensionData = dimensionCollection[e.Index];
            var indicator = (DimensionIndicator)dynamicDimensionData.ApplicationData;

            switch (indicator)
            {
                case DimensionIndicator.Angle:
                    var angularDimension = (LineAngularDimension2)dynamicDimensionData.Dimension;
                    angle = e.Value;
                    isAngleLocked = true;
                    isLengthLocked = false;
                    break;

                case DimensionIndicator.Length:
                    var alignedDimension = (AlignedDimension)dynamicDimensionData.Dimension;
                    length = e.Value;
                    isLengthLocked = true;
                    isAngleLocked = false;
                    break;

                default:
                    break;
            }

            Update();
        }


        [CommandMethod("TestCmd")]
        public static void CommandBimJig()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            var ed = doc.Editor;
            var db = doc.Database;

            var ppr = ed.GetPoint("\nStart point");
            if (ppr.Status != PromptStatus.OK)
            {
                return;
            }
            else
            {
                var startPoint = ppr.Value;

                using (var tr = db.TransactionManager.StartTransaction())
                {
                    var btr = (BlockTableRecord)tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite, false);
                    var line = new Line(startPoint, startPoint);
                    var jig = new TestJig(startPoint, line);
                    PromptResult res;
                    do
                    {
                        res = ed.Drag(jig);
                    } while (res.Status == PromptStatus.Other);

                    if (res.Status == PromptStatus.OK)
                    {
                        btr.AppendEntity(line);
                        tr.AddNewlyCreatedDBObject(line, true);
                        tr.Commit();
                    }
                    else
                    {
                        tr.Abort();
                    }
                }
            }
        }
        #endregion Event Handler should the user manually change the dimensions - update the values to reflect this
    }
}
