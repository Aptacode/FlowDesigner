using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Aptacode.FlowDesigner.Blazor.Extensions;
using Aptacode.FlowDesigner.Core.Enums;
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

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await JSRuntime.InvokeVoidAsync("SetFocusToElement", diagram);
            }
        }

        protected override async Task OnInitializedAsync()
        {
            Designer.RedrawConnections();
            Designer.PropertyChanged += Designer_PropertyChanged;
            await base.OnInitializedAsync();
        }

        private void Designer_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            StateHasChanged();
        }

        private Vector2 _position;
        private Vector2 _lastPositionUpdate;
        public void MouseDown(MouseEventArgs e)
        {
            
            _lastPositionUpdate = _position = e.ToPosition();
            Designer.Interactor.MouseDown(_position);
        }

        public void MouseUp(MouseEventArgs e)
        {
            Designer.Interactor.MouseUp(e.ToPosition());
        }

        public void MouseOut(MouseEventArgs e) { }

        public void MouseMove(MouseEventArgs e)
        {
            _position = e.ToPosition();
            if (_lastPositionUpdate == _position)
            {
                return;
            }

            _lastPositionUpdate = _position;
            Designer.Interactor.MouseMove(_position);

        }

        public void KeyDown(KeyboardEventArgs e)
        {
            Designer.Interactor.KeyDown(e.Key);
        }
        public void KeyUp(KeyboardEventArgs e)
        {
            Designer.Interactor.KeyUp(e.Key);
        }
    }
}