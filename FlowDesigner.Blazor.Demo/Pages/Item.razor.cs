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

        protected override async Task OnInitializedAsync() {

            Item.PropertyChanged += Item_PropertyChanged;

            await base.OnInitializedAsync();
        }

        private void Item_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            InvokeAsync(StateHasChanged);
        }

        public void Dispose()
        {
            Item.PropertyChanged -= Item_PropertyChanged;
        }
    }
}
