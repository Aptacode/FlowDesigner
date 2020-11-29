using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Aptacode.FlowDesigner.Core.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace FlowDesigner.Blazor.Demo.Pages
{
    public class DiagramBase : ComponentBase
    {
        public DesignerViewModel Designer { get; set; }

        protected override async Task OnInitializedAsync()
        {
            Designer = new DesignerViewModel();
            Designer.CreateItem.Execute(("State 1", new Vector2(8, 8), new Vector2(8, 4)));
            Designer.CreateItem.Execute(("State 2", new Vector2(8, 16), new Vector2(8, 4)));
            Designer.CreateItem.Execute(("State 3", new Vector2(25, 30), new Vector2(8, 4)));
            Designer.CreateItem.Execute(("State 4", new Vector2(10, 40), new Vector2(8, 4)));
            Designer.CreateItem.Execute(("State 5", new Vector2(10, 50), new Vector2(8, 4)));
            Designer.CreateItem.Execute(("State 6", new Vector2(25, 60), new Vector2(8, 4)));

            var items = Designer.Items.ToList();
            Designer.Connect.Execute((items[0], items[1]));
            Designer.Connect.Execute((items[0], items[2]));
            Designer.Connect.Execute((items[0], items[3]));
            Designer.Connect.Execute((items[0], items[4]));

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
    }
}