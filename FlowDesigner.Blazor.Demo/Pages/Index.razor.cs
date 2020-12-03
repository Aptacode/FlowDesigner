using Aptacode.FlowDesigner.Core.ViewModels;
using Microsoft.AspNetCore.Components;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace FlowDesigner.Blazor.Demo.Pages
{
    public class IndexBase : ComponentBase
    {

        public DesignerViewModel Designer { get; set; }

        protected async override Task OnInitializedAsync()
        {
            Designer = new DesignerViewModel(200,100);
            Designer.CreateItem.Execute(("State 1", new Vector2(10, 8), new Vector2(15, 5)));
            Designer.CreateItem.Execute(("State 2", new Vector2(10, 20), new Vector2(15, 5)));
            Designer.CreateItem.Execute(("State 3", new Vector2(40, 40), new Vector2(15, 5)));
            Designer.CreateItem.Execute(("State 4", new Vector2(40, 60), new Vector2(15, 5)));
            Designer.CreateItem.Execute(("State 5", new Vector2(40, 70), new Vector2(15, 5)));
            Designer.CreateItem.Execute(("State 6", new Vector2(70, 70), new Vector2(15, 5)));

            var items = Designer.Items.ToList();
            Designer.Connect.Execute((items[0], items[1]));
            Designer.Connect.Execute((items[0], items[2]));
            Designer.Connect.Execute((items[0], items[3]));
            Designer.Connect.Execute((items[0], items[4]));

            await base.OnInitializedAsync();
        }
    }
}
