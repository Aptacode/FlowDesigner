using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Aptacode.FlowDesigner.Core;
using Aptacode.FlowDesigner.Core.ViewModels;
using Microsoft.AspNetCore.Components;

namespace Aptacode.FlowDesigner.Blazor.Components
{
    public class SelectionBase : ComponentBase, IDisposable
    {
        [Parameter] public SelectionViewModel Selection { get; set; }

        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }

        public void Dispose()
        {
            Selection.PropertyChanged -= Item_PropertyChanged;
        }

        protected override async Task OnInitializedAsync()
        {
            Selection.PropertyChanged += Item_PropertyChanged;
            Refresh();

            await base.OnInitializedAsync();
        }

        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "Position" && e.PropertyName != "Size")
            {
                return;
            }

            Refresh();
            InvokeAsync(StateHasChanged);
        }

        public void Refresh()
        {
            var scaledPosition = Selection.Position * Constants.Scale;
            var scaledSize = Selection.Size * Constants.Scale;

            X = scaledPosition.X + 0.5f;
            Y = scaledPosition.Y + 0.5f;
            Width = scaledSize.X + 0.5f;
            Height = scaledSize.Y + 0.5f;
        }
    }
}