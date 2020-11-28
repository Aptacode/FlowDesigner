using System.Numerics;
using System.Text;

namespace Aptacode.FlowDesigner.Core.ViewModels
{
    public static class StringBuilderExtensions
    {
        public static StringBuilder Add(this StringBuilder path, Vector2 point)
        {
            path.Append(point.X * DesignerViewModel.ScaleFactor).Append(' ')
                .Append(point.Y * DesignerViewModel.ScaleFactor);
            path.Append("L ");
            return path;
        }
    }
}