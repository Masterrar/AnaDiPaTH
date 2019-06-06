using System.Drawing;

namespace Model.Tool.Shape
{
	public class EllipseM
	{
        public EllipseM(PointF midPoint)
        {
            MiddlePoint = midPoint;
            //Wight = 100;
            //Height = Wight / 2;
        }
        public EllipseM(PointF midPoint, int Wight, int Height)
        {
            _midPoint = midPoint;
            this.Wight = Wight;
            this.Height = Height;
        }
        public int Wight { get; set; }
        public int Height { get; set; }

        private PointF _midPoint;
        public PointF MiddlePoint
        {
            get { return _midPoint; }
            private set { _midPoint = value; }
        }

	}
}

