using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using project_drawing_object.config;
using System.Windows.Shapes;


namespace project_drawing_object.jig
{
    public class CircleJig : EntityJig
    {
        public static double mDiameter = 0;
        private Point3d mCenter, mAxisPt, acquiredPoint;
        private Vector3d mNormal;
        private double mRadiusRatio;
        private DynamicDimensionDataCollection dimensionCollection;
        private DynamicAngularConfig dynamicAngularConfig = new DynamicAngularConfig();
        private DynamicDimensionConfig dynamicDimensionConfig = new DynamicDimensionConfig();
        public CircleJig(Point3d center, Vector3d vec) : base(new Circle())
        {
            this.mCenter = center;
            this.mNormal = vec;
            this.mRadiusRatio = 0.00001;
            if (Entity is Circle circle)
            {
                circle.Normal=vec;
                circle.Center = center;
                circle.Radius = 0.00001;
            }
            dimensionCollection = new DynamicDimensionDataCollection();
            //dynamicAngularConfig.addToDynamicCollection(dimensionCollection);
            dynamicDimensionConfig.addToDynamicCollection(dimensionCollection);
        }

        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            JigPromptDistanceOptions jigOpts = CreateJigOptions();
            var res = prompts.AcquireDistance(jigOpts);
            if (res.Status == PromptStatus.OK)
            {
                double newDistance = res.Value;

                // Kiểm tra nếu khoảng cách mới khác với khoảng cách trước
                if (Math.Abs(newDistance - this.mRadiusRatio) > Tolerance.Global.EqualPoint)
                {
                    this.mRadiusRatio = newDistance; // Cập nhật khoảng cách
                    return SamplerStatus.OK; // Kích hoạt vẽ lại
                }
            }
            else if (res.Status == PromptStatus.Cancel)
            {
                return SamplerStatus.Cancel; // Người dùng hủy thao tác
            }

            return SamplerStatus.NoChange;

        }

        protected override bool Update()
        {
            if (Entity is Circle circle) 
            {
                circle.Radius = mRadiusRatio;
                dynamicDimensionConfig.UpdateLineDimension(circle.Center, circle.EndPoint);
                return true;
            }
            return false;
        }
        protected override DynamicDimensionDataCollection GetDynamicDimensionData(double dimScale)
        {
            return dimensionCollection;
        }

        public static void DrawingCircle()
        {
            var doc = ConnectDrawing.DocumentAutoCad;
            Editor ed = doc.Editor;
            PromptPointOptions ppoStart = new("\nSpecify center point for circle or ");
            ppoStart.Keywords.Add("3P");
            ppoStart.Keywords.Add("2P");
            ppoStart.Keywords.Add("Ttr", "Ttr (tan tan radius)");
            PromptPointResult pprResult = ed.GetPoint(ppoStart);
            switch (pprResult.Status)
            {
                case PromptStatus.Cancel:
                    return;
                case PromptStatus.OK:
                    Vector3d normal = Vector3d.ZAxis;
                    CircleJig circle = new CircleJig(pprResult.Value, normal);
                    PromptResult dynamicPoint = ed.Drag(circle);
                    return;
                case PromptStatus.Keyword:
                    return;
                default: return;

            }
        }
        private JigPromptDistanceOptions CreateJigOptions()
        {
            JigPromptDistanceOptions jigOpts = new JigPromptDistanceOptions();
            jigOpts.UserInputControls = (UserInputControls.Accept3dCoordinates | UserInputControls.NoZeroResponseAccepted | UserInputControls.NoNegativeResponseAccepted);
            jigOpts.Cursor = CursorType.RubberBand;
            var doc = ConnectDrawing.DocumentAutoCad;
            Editor ed = doc.Editor;
            ed.WriteMessage(this.mCenter.ToString());
            jigOpts.BasePoint=this.mCenter;
            jigOpts.Message = "\nSpecify radius of circle or";
            return jigOpts;
        }
    }
}
