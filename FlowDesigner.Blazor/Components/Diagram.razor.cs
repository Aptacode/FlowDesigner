using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Aptacode.FlowDesigner.Core.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace Aptacode.FlowDesigner.Blazor.Components
{
    public class DiagramBase : ComponentBase
    {
        [Parameter]
        public DesignerViewModel Designer { get; set; }

        protected ElementReference diagram;  // set by the @ref attribute

        [Inject] IJSRuntime JSRuntime { get; set; }

        protected async override Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await JSRuntime.InvokeVoidAsync("SetFocusToElement", diagram);
            }
        }


        protected override async Task OnInitializedAsync()
        {
            Designer.RedrawConnections();

            await base.OnInitializedAsync();
        }

        public void MouseDown(MouseEventArgs e)
        {
            Designer.MouseDown(e.ToPosition());
        }

        public void MouseUp(MouseEventArgs e)
        {
            Designer.MouseUp(e.ToPosition());
        }

        public void MouseOut(MouseEventArgs e) { }

        public void MouseMove(MouseEventArgs e)
        {
            Designer.MouseMove(e.ToPosition());
        }

        public void KeyDown(KeyboardEventArgs e)
        {
            Designer.KeyDown(e.Key);
        }
        public void KeyUp(KeyboardEventArgs e)
        {
            Designer.KeyUp(e.Key);
        }
    }
}