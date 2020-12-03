using System.Numerics;
using Aptacode.FlowDesigner.Core;
using Microsoft.AspNetCore.Components.Web;

namespace Aptacode.FlowDesigner.Blazor.Components
{
    public static class MouseEventArgsExtensions
    {
        public static Vector2 ToPosition(this MouseEventArgs args)
        {
            return new Vector2((int)(args.OffsetX / Constants.Scale), (int)(args.OffsetY /Constants.Scale));
        }
    }
}