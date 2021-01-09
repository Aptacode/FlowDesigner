using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Aptacode.FlowDesigner.Core;
using Aptacode.FlowDesigner.Core.ViewModels;
using Aptacode.FlowDesigner.Core.ViewModels.Components;
using Microsoft.AspNetCore.Components;

namespace Aptacode.FlowDesigner.Blazor.Components
{
    public class ItemBase : ComponentBase, IDisposable
    {
        [Parameter] public ConnectedComponentViewModel Item { get; set; }

        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }

        public void Dispose()
        {
            Item.PropertyChanged -= Item_PropertyChanged;
        }

        protected override async Task OnInitializedAsync()
        {
            Item.PropertyChanged += Item_PropertyChanged;
            Refresh();

            await base.OnInitializedAsync();
        }

        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Refresh();
            InvokeAsync(StateHasChanged);
        }

        public void Refresh()
        {
            var scaledPosition = Item.Rectangle.Position * Constants.Scale;
            var scaledSize = Item.Rectangle.Size * Constants.Scale;

            X = scaledPosition.X + 0.5f;
            Y = scaledPosition.Y + 0.5f;
            Width = scaledSize.X + 0.5f;
            Height = scaledSize.Y + 0.5f;
        }
    }
}