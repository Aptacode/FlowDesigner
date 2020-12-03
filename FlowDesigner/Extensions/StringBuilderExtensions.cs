using System.Numerics;
using System.Text;
using Aptacode.FlowDesigner.Core.ViewModels;

namespace Aptacode.FlowDesigner.Core.Extensions
{
    public static class StringBuilderExtensions
    {
        public static StringBuilder Add(this StringBuilder path, Vector2 point)
        {
            path.Append(' ')
                .Append(point.X * Constants.Scale)
                .Append(",")
                .Append(point.Y * Constants.Scale);

            return path;
        }
    }
}