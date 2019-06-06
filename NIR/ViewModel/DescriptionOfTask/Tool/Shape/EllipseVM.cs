using System.Windows.Shapes;
using System.Drawing;

namespace ViewModel.DescriptionOfTask.Tool.Shape
{
    
	public class EllipseVM
	{
        public Ellipse Ellipse;
        public EllipseVM(PointF MidPoint)
        {
            Ellipse = new Ellipse
            {
                Width = 100,
                Height = 100,
                Margin = new System.Windows.Thickness(MidPoint.X, MidPoint.Y, 0, 0)
            };

        }
	}
}

