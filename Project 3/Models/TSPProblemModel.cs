using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Project_3.Models
{
    public class TSPProblemModel
    {
        public string FileName { get; set; }
        public List<Coordinate> Coords { get; set; } = new List<Coordinate>();
        public List<NodePath> NodePaths { get; set; } = new List<NodePath>();
    }
}