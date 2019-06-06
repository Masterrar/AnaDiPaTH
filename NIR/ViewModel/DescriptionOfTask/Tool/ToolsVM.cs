using System.Drawing;
namespace ViewModel.DescriptionOfTask.Tool
{
    using Shape;
    public class ToolsVM
    {

        public LineVM Line(Point MidPoint)
        {
            return new LineVM(MidPoint);
        }
        public EllipseVM Ellipse(Point MidPoint)
        {
            return new EllipseVM(MidPoint);
        }
    }
}

