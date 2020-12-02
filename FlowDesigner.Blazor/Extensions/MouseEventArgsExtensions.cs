using System.Numerics;
using Microsoft.AspNetCore.Components.Web;

namespace FlowDesigner.Blazor.Components
{
    public static class MouseEventArgsExtensions
    {
        public static Vector2 ToPosition(this MouseEventArgs args)
        {
            return new Vector2((int)(args.OffsetX / 10.0f), (int)(args.OffsetY / 10.0f));
        }
    }
}