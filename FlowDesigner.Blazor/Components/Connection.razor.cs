using System;
using System.ComponentModel;
using System.Numerics;
using System.Threading.Tasks;
using Aptacode.FlowDesigner.Core.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Aptacode.FlowDesigner.Blazor.Components
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
            Connection.Point1.Item.PropertyChanged += Item_PropertyChanged;
            Connection.Point2.Item.PropertyChanged += Item_PropertyChanged;

            await base.OnInitializedAsync();
        }

        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            InvokeAsync(StateHasChanged);
        }
    }
}