using System.Drawing;

namespace Model.Tool.Shape
{
    public class LineM
    {
        public LineM(PointF midPoint)
        {
            MiddlePoint = midPoint;
        }


        private PointF _p1;
        private PointF _p2;
        public PointF P1
        {
            get { return _p1; }
        }

        public PointF P2
        {
            get { return _p2; }
        }

        private PointF _midPoint;
        public PointF MiddlePoint
        {
            get { return _midPoint; }
            private set { _midPoint = value; }
        }
    }
}

