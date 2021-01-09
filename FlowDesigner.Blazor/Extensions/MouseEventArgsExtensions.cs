using System.Numerics;
using Aptacode.FlowDesigner.Core;
using Microsoft.AspNetCore.Components.Web;

namespace Aptacode.FlowDesigner.Blazor.Extensions
{
    public static class MouseEventArgsExtensions
    {
        public static Vector2 ToPosition(this MouseEventArgs args)
        {
            return new((int)(args.OffsetX / Constants.Scale), (int)(args.OffsetY / Constants.Scale));
        }
    }
}