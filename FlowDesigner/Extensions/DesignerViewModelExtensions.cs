using Aptacode.FlowDesigner.Core.ViewModels;
using Aptacode.PathFinder.Geometry.Neighbours;
using Aptacode.PathFinder.Maps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Aptacode.FlowDesigner.Core.Extensions
{
    public static class DesignerViewModelExtensions
    {
        #region PathFinding

        public static List<Vector2> GetPath(this DesignerViewModel designer, Vector2 startPoint, Vector2 endPoint, INeighbourFinder neighbourFinder = null)
        {
            var points = new List<Vector2>();

            try
            {
                var mapBuilder = new MapBuilder();

                foreach (var item in designer.Items.ToList())
                {
                    mapBuilder.AddObstacle(item.Position - Vector2.One, item.Size + Vector2.One * 2);
                }

                mapBuilder.SetStart(startPoint);
                mapBuilder.SetEnd(endPoint);
                mapBuilder.SetDimensions(designer.Width, designer.Height);
                var mapResult = mapBuilder.Build();
                if (!mapResult.Success)
                {
                    throw new Exception(mapResult.Message);
                }

                var pathFinder =
                    new PathFinder.Algorithm.PathFinder(mapResult.Map, neighbourFinder ?? DefaultNeighbourFinder.Straight(0.5f));

                points.AddRange(pathFinder.FindPath());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return points;
        }

        #endregion
    }
}
