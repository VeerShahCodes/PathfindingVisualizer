using System;

namespace PathfindingVisualizer;

public class Edge<T>
{
        public Vertex<T> StartingPoint { get; set; }
        public Vertex<T> EndingPoint { get; set; }
        public float Distance { get; set; }

        public Edge(Vertex<T> startingPoint, Vertex<T> endingPoint, float distance)
        {
            StartingPoint = startingPoint;
            EndingPoint = endingPoint;
            Distance = distance;
        }

}
