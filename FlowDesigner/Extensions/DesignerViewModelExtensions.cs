using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Aptacode.AppFramework.Scene;
using Aptacode.PathFinder.Geometry.Neighbours;
using Aptacode.PathFinder.Maps;

namespace Aptacode.FlowDesigner.Core.Extensions
{
    public static class DesignerViewModelExtensions
    {
        #region PathFinding

        public static List<Vector2> GetPath(this Scene scene, Vector2 startPoint, Vector2 endPoint)
        {
            var points = new List<Vector2>();

            try
            {
                var mapBuilder = new MapBuilder();

                foreach (var component in scene.Components.Where(c => c.CollisionDetectionEnabled).ToList())
                {
                    mapBuilder.AddObstacle(component);
                }

                mapBuilder.SetStart(startPoint);
                mapBuilder.SetEnd(endPoint);
                mapBuilder.SetDimensions(scene.Size);

                var mapResult = mapBuilder.Build();
                if (mapResult.Success && mapResult.Map != null)
                {
                    var pathFinder =
                        new PathFinder.Algorithm.PathFinder(mapResult.Map,
                            DefaultNeighbourFinder.Straight(1.0f));

                    points.AddRange(pathFinder.FindPath());
                }
                else
                {
                    Console.WriteLine(mapResult.Message);
                }
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