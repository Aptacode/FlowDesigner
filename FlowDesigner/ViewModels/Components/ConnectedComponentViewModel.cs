using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Threading.Tasks;
using Aptacode.BlazorCanvas;
using Aptacode.Geometry.Blazor.Components.ViewModels.Components.Primitives;
using Aptacode.Geometry.Blazor.Utilities;
using Aptacode.Geometry.Primitives;
using Rectangle = Aptacode.Geometry.Primitives.Polygons.Rectangle;

namespace Aptacode.FlowDesigner.Core.ViewModels.Components
{
    public class ConnectedComponentViewModel : PolygonViewModel
    {
        #region Ctor
        public ConnectedComponentViewModel(Rectangle body) : base(body)
        {
            Body = body;
            FillColor = Color.White;
            Margin = 1;
            ConnectionPoints = new List<ConnectionPointViewModel>();
        }
        #endregion

        #region Prop

        public Rectangle Body { get; private set; }
        public List<ConnectionPointViewModel> ConnectionPoints { get; private set; }
        
        #endregion

        public ConnectionPointViewModel AddConnectionPoint()
        {
            var ellipse = new Ellipse(Body.TopLeft + new Vector2(4,0), new Vector2(1, 1), 0);
            var connectionPoint = new ConnectionPointViewModel(this, ellipse)
            {
                FillColor = FillColor
            };
            ConnectionPoints.Add(connectionPoint);
            Add(connectionPoint);
            return connectionPoint;
        }

        public override async Task Draw(BlazorCanvasInterop ctx)
        {
            OldBoundingRectangle = BoundingRectangle;
            Invalidated = false;

            if (!IsShown)
            {
                return;
            }

            ctx.FillStyle(FillColorName);

            ctx.StrokeStyle(BorderColorName);

            ctx.LineWidth(BorderThickness);

            foreach (var child in Children)
            {
                await child.Draw(ctx);
            }

            await CustomDraw(ctx);

            if (!string.IsNullOrEmpty(Text))
            {
                ctx.TextAlign("center");
                ctx.FillStyle("black");
                ctx.FillText(Text, BoundingRectangle.Center.X * SceneScale.Value, BoundingRectangle.Center.Y * SceneScale.Value);
            }
        }
    }
}
