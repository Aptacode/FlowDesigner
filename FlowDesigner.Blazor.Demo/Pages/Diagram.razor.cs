using System;
using System.Diagnostics;
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

        protected override async Task OnInitializedAsync() {

            Designer = new DesignerViewModel();
            Designer.CreateItem.Execute(("State 1", new Vector2(10,10), new Vector2(10,10)));
            Designer.CreateItem.Execute(("State 3", new Vector2(10,10), new Vector2(10,10)));
            Designer.CreateItem.Execute(("State 2", new Vector2(25,25), new Vector2(10,10)));
            var items = Designer.Items.ToList();
            var item1 = items.First();
            var item2 = items.Last();
            Designer.Connect.Execute((item1, item2));

            await base.OnInitializedAsync();
        }

        public ItemViewModel? SelectedItem { get; set; }

        private Vector2 mouseDelta {get;set;}

        public void MouseDown(MouseEventArgs e)
        {
            if (SelectedItem != null)
            {
                return;
            }

            var mousePosition = new Vector2((int)(e.OffsetX / 10.0f), (int)(e.OffsetY / 10.0f));

            foreach (var item in Designer.Items)
            {
                if (!item.CollidesWith(mousePosition))
                {
                    continue;
                }

                SelectedItem = item;
                break;
            }

            if (SelectedItem == null)
            {
                return;
            }

            Designer.BringToFront(SelectedItem);
            mouseDelta = mousePosition - SelectedItem.Position;

            Console.WriteLine($"Click {SelectedItem.Label}{SelectedItem.Position}");
        }

        public void MouseUp(MouseEventArgs e)
        {
            if(SelectedItem == null)
            {
                return;
            }

            var mousePosition = new Vector2((int)(e.OffsetX / 10.0), (int)(e.OffsetY / 10.0));
            SelectedItem.Position = mousePosition - mouseDelta;

            Console.WriteLine($"Release {SelectedItem.Label}{SelectedItem.Position}");

            SelectedItem = null;
        }

        public void MouseOut(MouseEventArgs e)
        {
        }

        public void MouseMove(MouseEventArgs e)
        {
            if(SelectedItem != null)
            {
                var mousePosition = new Vector2((int)(e.OffsetX / 10.0), (int)(e.OffsetY / 10.0));
                SelectedItem.Position = mousePosition - mouseDelta;

                var timer = new Stopwatch();
                timer.Start();

                Designer.Connections.First().Refresh();


                timer.Stop();
                Console.WriteLine($"Total Elapsed: {timer.ElapsedMilliseconds}ms");

            }
        }
    }
}
