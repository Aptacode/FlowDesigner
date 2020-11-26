using Aptacode.FlowDesigner.Core.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Threading.Tasks;

namespace FlowDesigner.Blazor.Demo.Pages
{
    public class ItemBase : ComponentBase, IDisposable
    {
        [Parameter]
        public ItemViewModel Item { get; set; }

        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }

        protected override async Task OnInitializedAsync() {

            Item.PropertyChanged += Item_PropertyChanged;
            Refresh();

            await base.OnInitializedAsync();
        }

        private void Item_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Position" || e.PropertyName == "Size")
            {
                Refresh();
                InvokeAsync(StateHasChanged);
            }

        }

        public void Dispose()
        {
            Item.PropertyChanged -= Item_PropertyChanged;
        }

        public void Refresh()
        {
            var scaledPosition = Item.Position * 10;
            var scaledSize = Item.Size * 10;

            X = scaledPosition.X + 0.5f;
            Y = scaledPosition.Y + 0.5f;
            Width = scaledSize.X + 0.5f;
            Height = scaledSize.Y + 0.5f;
        }
    }
}
