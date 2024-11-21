using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace project_drawing_object.common
{
    public class KTypingDistance:EntityJig
    {
        private Point3d _currentPoint;
        private readonly Point3d _startPoint;
        private AlignedDimension _dimension;
        public KTypingDistance(Line line, Point3d startPoint, AlignedDimension dimension)
        : base(line)
        {
            _startPoint = startPoint;
            _currentPoint = startPoint; // Mặc định ban đầu
            _dimension = dimension;
        }

        protected override bool Update()
        {
            throw new NotImplementedException();
        }

        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            throw new NotImplementedException();
        }
    }
}
