using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Aptacode.FlowDesigner.Core.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace FlowDesigner.Blazor.Demo.Pages
{

    public static class MouseEventArgsExtensions
    {
        public static Vector2 ToPosition(this MouseEventArgs args)
        {
            return new Vector2((int)(args.OffsetX / 10.0f), (int)(args.OffsetY / 10.0f));
        }
    }
}