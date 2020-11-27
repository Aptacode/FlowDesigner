using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Aptacode.FlowDesigner.Core.ViewModels;
using Microsoft.AspNetCore.Components;

namespace FlowDesigner.Blazor.Demo.Pages
{
    public class ConnectionBase : ComponentBase, IDisposable
    {
        [Parameter] public ConnectionViewModel Connection { get; set; }

        public void Dispose()
        {
            Connection.PropertyChanged -= Item_PropertyChanged;
        }

        protected override async Task OnInitializedAsync()
        {
            Connection.PropertyChanged += Item_PropertyChanged;

            await base.OnInitializedAsync();
        }

        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            InvokeAsync(StateHasChanged);
        }
    }
}