using Project_3.Models;
using Project_3.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Project_3.Controllers
{
    public class HomeController : Controller
    {
        private string tspDirectory = AppContext.BaseDirectory + "TSPs\\";

        public ActionResult Index()
        {
            var vm = new HomeIndexViewModel();

            //for each file, generate a new problem section
            foreach (var file in Directory.GetFiles(tspDirectory))
            {
                var tspProblem = new TSPProblemModel();
                tspProblem.FileName = Path.GetFileName(file);

                var lines = System.IO.File.ReadAllLines(file);

                //get all lines after the 7th line since that's where coordinates start
                for (var i = 7; i < lines.Count(); i++)
                {
                    //split out line into coordinate class I created
                    var coordsText = lines[i].Split(' ');
                    tspProblem.Coords.Add(new Coordinate() { Id = Convert.ToInt32(coordsText[0]), Latitude = Convert.ToDouble(coordsText[1]), Longitude = Convert.ToDouble(coordsText[2]) });
                }
                //create first node path between first two variables to pass into recursive function
                var newNodePaths = new List<NodePath>() { new NodePath() { FromNode = tspProblem.Coords[0].Id, ToNode = tspProblem.Coords[1].Id, Distance = DistanceBetween(tspProblem.Coords[0], tspProblem.Coords[1]) } };
                //get node paths using the closest insertion heuristic algorithm
                tspProblem.NodePaths = ClosestInsertionHeuristic(tspProblem.Coords, newNodePaths);

                vm.Problems.Add(tspProblem);
            }

            return View(vm);
        }

        //recursive function using closest insertion heuristic algorithm
        private List<NodePath> ClosestInsertionHeuristic(List<Coordinate> allCoords, List<NodePath> currentPaths)
        {
            //create closes node id variable that will be set inside a foreach
            int closestNodeId = -1;
            //track all currently used node ids
            var currentUsedNodeIds = currentPaths.Select(x => x.ToNode).ToList();
            currentUsedNodeIds.AddRange(currentPaths.Select(x => x.FromNode));
            currentUsedNodeIds = currentUsedNodeIds.Distinct().ToList();

            //get all not used node ids
            var notUsedNodeIds = allCoords.Where(x => !currentUsedNodeIds.Any(y => y == x.Id)).Select(x => x.Id).ToList();

            //if all node ids are used, close the loop and return the paths
            if (!notUsedNodeIds.Any())
            {
                var firstNode = allCoords.FirstOrDefault(x => currentPaths.FirstOrDefault(y => y.FromNode == x.Id) != null
                    && currentPaths.FirstOrDefault(y => y.ToNode == x.Id) == null);

                var lastNode = allCoords.FirstOrDefault(x => currentPaths.FirstOrDefault(y => y.ToNode == x.Id) != null
                    && currentPaths.FirstOrDefault(y => y.FromNode == x.Id) == null);

                currentPaths.Add(new NodePath() { FromNode = firstNode.Id, ToNode = lastNode.Id, Distance = DistanceBetween(firstNode, lastNode) });

                return currentPaths;
            }

            //find the closest node id to any current existing node
            double closestNodeDistance = -1;
            foreach (var usedNodeId in currentUsedNodeIds)
            {
                foreach (var notUsedNodeId in notUsedNodeIds)
                {
                    var distance = DistanceBetween(allCoords.First(x => x.Id == usedNodeId), allCoords.First(x => x.Id == notUsedNodeId));
                    if (closestNodeDistance == -1 || distance < closestNodeDistance)
                    {
                        closestNodeDistance = distance;
                        closestNodeId = notUsedNodeId;
                    }
                }
            }

            //variables to track the current distance of the path and what the new shortest distance will be
            var overallDistance = currentPaths.Sum(x => x.Distance);
            double shortestNewDistance = -1;
            List<NodePath> shortestNewNodePaths = new List<NodePath>();
            //try inserting the new node between every existing path and track the shortest overall distance
            foreach (var path in currentPaths)
            {
                var potentialPaths = new List<NodePath>(currentPaths);
                potentialPaths.Remove(path);

                potentialPaths.Add(new NodePath()
                {
                    FromNode = path.FromNode,
                    ToNode = closestNodeId,
                    Distance = DistanceBetween(allCoords.First(x => x.Id == path.FromNode), allCoords.First(x => x.Id == closestNodeId))
                });

                potentialPaths.Add(new NodePath()
                {
                    FromNode = closestNodeId,
                    ToNode = path.ToNode,
                    Distance = DistanceBetween(allCoords.First(x => x.Id == closestNodeId), allCoords.First(x => x.Id == path.ToNode))
                });

                var thisPathDistance = potentialPaths.Sum(x => x.Distance);

                if (shortestNewDistance == -1 || thisPathDistance < shortestNewDistance)
                {
                    shortestNewDistance = thisPathDistance;
                    shortestNewNodePaths = potentialPaths;
                }
            }

            //instead, try to tact the newest coordinate to the beginning of the path and see if this is a shorter option
            var startingNode = allCoords.FirstOrDefault(x => currentPaths.FirstOrDefault(y => y.FromNode == x.Id) != null
                && currentPaths.FirstOrDefault(y => y.ToNode == x.Id) == null);

            if (startingNode != null)
            {
                var potentialPathsAppendAtStart = new List<NodePath>(currentPaths);

                potentialPathsAppendAtStart.Add(new NodePath()
                {
                    FromNode = startingNode.Id,
                    ToNode = closestNodeId,
                    Distance = DistanceBetween(allCoords.First(x => x.Id == startingNode.Id), allCoords.First(x => x.Id == closestNodeId))
                });

                var thisPathDistanceAppendedAtStart = potentialPathsAppendAtStart.Sum(x => x.Distance);

                if (thisPathDistanceAppendedAtStart < shortestNewDistance)
                {
                    shortestNewDistance = thisPathDistanceAppendedAtStart;
                    shortestNewNodePaths = potentialPathsAppendAtStart;
                }
            }

            //instead, try to tact the newest coordinate to the end of the path and see if this is a shorter option
            var endingNode = allCoords.FirstOrDefault(x => currentPaths.FirstOrDefault(y => y.ToNode == x.Id) != null
                && currentPaths.FirstOrDefault(y => y.FromNode == x.Id) == null);

            if (endingNode != null)
            {
                var potentialPathsAppendAtEnd = new List<NodePath>(currentPaths);

                potentialPathsAppendAtEnd.Add(new NodePath()
                {
                    FromNode = endingNode.Id,
                    ToNode = closestNodeId,
                    Distance = DistanceBetween(allCoords.First(x => x.Id == endingNode.Id), allCoords.First(x => x.Id == closestNodeId))
                });

                var thisPathDistanceAppendedAtEnd = potentialPathsAppendAtEnd.Sum(x => x.Distance);

                if (thisPathDistanceAppendedAtEnd < shortestNewDistance)
                {
                    shortestNewDistance = thisPathDistanceAppendedAtEnd;
                    shortestNewNodePaths = potentialPathsAppendAtEnd;
                }
            }

            //recursively call itself
            return ClosestInsertionHeuristic(allCoords, shortestNewNodePaths);
        }

        //use distance formula to find distance between two points
        private static double DistanceBetween(Coordinate coord1, Coordinate coord2)
        {
            return Math.Sqrt(Math.Pow((coord2.Latitude - coord1.Latitude), 2) + Math.Pow((coord2.Longitude - coord1.Longitude), 2));
        }
    }
}