using System;
using System.Drawing;
using Model.Tool.Shape;
using System.Windows.Shapes;

namespace ViewModel.DescriptionOfTask.Tool.Shape
{
    public class LineVM
    {
        private LineM _lineM;
        public Line _line;


        public LineVM(Point midPoint)
        {
            _lineM = new LineM(midPoint);

        }



        public double X1
        {
            get { return _line.X1; }
            set
            {
                _line.X2 = Math.Abs(_lineM.MiddlePoint.X - _line.X2);
                _line.X1 = value;
            }
        }

        public double X2
        {
            get { return _line.X2; }
            set
            {
                _line.X1 = Math.Abs(_lineM.MiddlePoint.X - _line.X1);
                _line.X2 = value;
            }
        }
        public double Y1
        {
            get { return _line.X1; }
            set
            {
                _line.Y2 = Math.Abs(_lineM.MiddlePoint.Y - _line.Y2);
                _line.Y1 = value;
            }
        }

        public double Y2
        {
            get { return _line.X2; }
            set
            {
                _line.Y1 = Math.Abs(_lineM.MiddlePoint.Y - _line.Y1);
                _line.Y2 = value;
            }
        }



        public PointF MiddlePoint
        {
            get { return _lineM.MiddlePoint; }

        }

    }
}

