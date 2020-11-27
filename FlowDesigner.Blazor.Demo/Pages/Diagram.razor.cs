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

        public ItemViewModel? SelectedItem { get; set; }

        private Vector2 MouseDelta { get; set; }

        protected override async Task OnInitializedAsync()
        {
            Designer = new DesignerViewModel();
            Designer.CreateItem.Execute(("State 1", new Vector2(10, 10), new Vector2(10, 4)));
            Designer.CreateItem.Execute(("State 2", new Vector2(10, 20), new Vector2(10, 4)));
            Designer.CreateItem.Execute(("State 3", new Vector2(25, 30), new Vector2(10, 4)));
            Designer.CreateItem.Execute(("State 4", new Vector2(10, 40), new Vector2(10, 4)));
            Designer.CreateItem.Execute(("State 5", new Vector2(10, 50), new Vector2(10, 4)));
            Designer.CreateItem.Execute(("State 6", new Vector2(25, 60), new Vector2(10, 4)));
            var items = Designer.Items.ToList();
            Designer.Connect.Execute((items[0], items[1]));
            Designer.Connect.Execute((items[0], items[2]));
            Designer.Connect.Execute((items[0], items[3]));
            Designer.Connect.Execute((items[0], items[4]));

            await base.OnInitializedAsync();
        }

        public void MouseDown(MouseEventArgs e)
        {
            if (SelectedItem != null)
            {
                return;
            }

            var mousePosition = new Vector2((int) (e.OffsetX / 10.0f), (int) (e.OffsetY / 10.0f));

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
            MouseDelta = mousePosition - SelectedItem.Position;

            Console.WriteLine($"Click {SelectedItem.Label}{SelectedItem.Position}");
        }

        public void MouseUp(MouseEventArgs e)
        {
            if (SelectedItem == null)
            {
                return;
            }

            var mousePosition = new Vector2((int) (e.OffsetX / 10.0), (int) (e.OffsetY / 10.0));
            SelectedItem.Position = mousePosition - MouseDelta;


            Console.WriteLine($"Release {SelectedItem.Label}{SelectedItem.Position}");

            var timer = new Stopwatch();
            timer.Start();

            foreach (var connection in Designer.Connections.Where(c => c.Item1.Item == SelectedItem || c.Item2.Item == SelectedItem))
            {
                connection.Refresh();
            }

            timer.Stop();
            Console.WriteLine($"Total Elapsed: {timer.ElapsedMilliseconds}ms");

            SelectedItem = null;
        }

        public void MouseOut(MouseEventArgs e) { }

        Vector2 lastDrawPoint = Vector2.Zero;
        public void MouseMove(MouseEventArgs e)
        {
            if (SelectedItem == null)
            {
                return;
            }

            var mouseX = (int) (e.OffsetX / 10.0);
            var mouseY = (int)(e.OffsetY / 10.0);
            var newPosition = new Vector2(mouseX, mouseY) - MouseDelta;

            SelectedItem.Position = newPosition;

            if (!((newPosition - lastDrawPoint).Length() > 5.0f))
            {
                return;
            }

            lastDrawPoint = newPosition;
            var timer = new Stopwatch();
            timer.Start();

            foreach (var connection in Designer.Connections.Where(c => c.Item1.Item == SelectedItem || c.Item2.Item == SelectedItem))
            {
                connection.Refresh();
            }

            timer.Stop();
            Console.WriteLine($"Total Elapsed: {timer.ElapsedMilliseconds}ms");

        }
    }
}