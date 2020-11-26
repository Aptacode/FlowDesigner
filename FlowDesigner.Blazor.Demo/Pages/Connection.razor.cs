using Aptacode.FlowDesigner.Core.ViewModels;
using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

namespace FlowDesigner.Blazor.Demo.Pages
{
    public class ConnectionBase : ComponentBase, IDisposable
    {
        [Parameter]
        public ConnectionViewModel Connection { get; set; }

        protected override async Task OnInitializedAsync() {

            Connection.PropertyChanged += Item_PropertyChanged;

            await base.OnInitializedAsync();
        }

        private void Item_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            InvokeAsync(StateHasChanged);
        }

        public void Dispose()
        {
            Connection.PropertyChanged -= Item_PropertyChanged;
        }
    }
}
