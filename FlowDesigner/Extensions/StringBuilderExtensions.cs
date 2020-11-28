using System.Numerics;
using System.Text;
using Aptacode.FlowDesigner.Core.ViewModels;

namespace Aptacode.FlowDesigner.Core.Extensions
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