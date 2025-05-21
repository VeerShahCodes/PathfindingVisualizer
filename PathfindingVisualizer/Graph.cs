using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Threading.Tasks;
//using System.Drawing;

namespace PathfindingVisualizer;

public class Graph<T>
{
    private List<Vertex<T>> vertices;
    public IReadOnlyList<Vertex<T>> Vertices => vertices;
    private Dictionary<(Vertex<T>, Vertex<T>), Edge<T>> edgeDictionary = new Dictionary<(Vertex<T>, Vertex<T>), Edge<T>>();
    public List<Vertex<T>> VisitedNodes;
    public IReadOnlyList<Edge<T>> Edges

    {
        get
        {
            List<Edge<T>> edges = new List<Edge<T>>();
            for (int i = 0; i < vertices.Count; i++)
            {
                for (int j = 0; j < vertices[i].NeighborCount; j++)
                {
                    edges.Add(vertices[i].Neighbors[j]);

                }
            }
            return edges;
        }
    }

    public int VertexCount => vertices.Count;

    public Graph()
    {
        VisitedNodes = new List<Vertex<T>>();
        vertices = new List<Vertex<T>>();
    }

    private void AddVertex(Vertex<T>  vertex)
    {
        if(vertex != null && vertex.NeighborCount == 0 && vertex.Owner != this)
        {
            vertices.Add(vertex);
            vertex.Owner = this;
        }
    }

    public void AddVertex(T val)
    {
        AddVertex(new Vertex<T>(val));
    }

    private bool RemoveVertex(Vertex<T> vertex)
    {
        if(vertex == null || vertex.Owner != this) return false;

        for(int i = 0; i < Edges.Count; i++)
        {
            if (Edges[i].EndingPoint == vertex)
            {
                Edges[i].StartingPoint.Neighbors.Remove(Edges[i]);
            }
        }
        vertices.Remove(vertex);
        return true;
    }

    public bool RemoveVertex(T val)
    {
        bool b = RemoveVertex(Search(val));
        return b;
    }

    private void AddEdge(Vertex<T> a, Vertex<T> b, float distance)
    {
        if (a != null && b != null && a.Owner == this && b.Owner == this)
        {
            Edge<T> edge = new Edge<T>(a, b, distance);
            if (!a.Neighbors.Contains(edge))
            {
                a.Neighbors.Add(edge);
                edgeDictionary[(a, b)] = edge; 

            }
        }

    }

    public void AddEdge(T val1, T val2, float distance)
    {
        AddEdge(Search(val1), Search(val2), distance);
    }

    public void AddUndirectedEdge(T val1, T val2, float distance)
    {
        AddEdge(Search(val1), Search(val2), distance);
        AddEdge(Search(val2), Search(val1), distance);
    }



    private bool RemoveEdge(Vertex<T> a, Vertex<T> b)
    {
        if (a == null || b == null) return false;
        bool hasThatEdge = false;
        int index = 0;
        for(int i = 0; i < Edges.Count; i++)
        {
            if (Edges[i].StartingPoint == a && Edges[i].EndingPoint == b)
            {
                hasThatEdge = true;
                index = i;
            }
        }
        if (hasThatEdge == false) return false;

        a.Neighbors.Remove(Edges[index]);
        return true;
    }

    public bool RemoveEdge(T val1, T val2)
    {
        bool b = RemoveEdge(Search(val1), Search(val2));
        return b;
    }

    public Vertex<T>  Search(T  value)
    {
        if (value == null) return null;
        int z = -1;

        for(int i = 0; i < vertices.Count; i++)
        {
            if (value.Equals(vertices[i].Value))
            {
                z = i;
                break;
            }
        }

        if (z == -1) return null;
        else
        {
            return vertices[z];
        }

    }

    public Edge<T>  GetEdge(Vertex<T>  a, Vertex<T>  b)
    {
        if (a == null || b == null) return null;

        edgeDictionary.TryGetValue((a, b), out var edge);
        return edge;
    }
    public float GetDistance(List<Vertex<T>> path)
    {
        if (path == null || path.Count < 2)
        {
            return 0;
        }

        float totalDistance = 0;
        for (int i = 0; i < path.Count - 1; i++)
        {
            for(int j = 0; j < path[i].NeighborCount; j++)
            {
                if (path[i].Neighbors[j].EndingPoint == path[i + 1])
                {
                    totalDistance += path[i].Neighbors[j].Distance;
                }
            }

        }
        return totalDistance;
    }

    public List<Vertex<T>>  PathFindBreadthFirst(Vertex<T>  start, Vertex<T>  end)
    {
        VisitedNodes = new List<Vertex<T>>();
        if (start == null || end == null) return null;
        float pathCost = 0f;

        Dictionary<Vertex<T>, Vertex<T> > previousVertex = new()
        {
            [start] = null
        };

        Queue<Vertex<T>> queue = new Queue<Vertex<T>>();
        queue.Enqueue(start);
        while(queue.Count > 0)
        {
            Vertex<T>  current = queue.Dequeue();
            if(current.Equals(end))
            {
                List<Vertex<T>> path = new List<Vertex<T>>();
                while (current != null)
                {
                    path.Add(current);
                    var edge = GetEdge(previousVertex[current], current);
                    if (edge is not null)
                    {
                        pathCost += edge.Distance;

                    }
                    current = previousVertex[current];
                }
                path.Reverse();
                Console.WriteLine("Breadth First: " + pathCost + " path cost");
                return path;
            }

            for (int i = 0; i < current.NeighborCount; i++)
            {
                if (!previousVertex.ContainsKey(current.Neighbors[i].EndingPoint))
                {
                    VisitedNodes.Add(current.Neighbors[i].EndingPoint);
                    queue.Enqueue(current.Neighbors[i].EndingPoint);
                    previousVertex[current.Neighbors[i].EndingPoint] = current;
                }
            }
        }
        return null;
    }

    public List<Vertex<T>>  PathFindDepthFirst(Vertex<T>  start, Vertex<T>  end)
    {
        VisitedNodes = new List<Vertex<T>>();
        if (start == null || end == null) return null;
        float pathCost = 0f;
        Dictionary<Vertex<T>, Vertex<T> > previousVertex = new()
        {
            [start] = null
        };

        Stack<Vertex<T>> stack = new Stack<Vertex<T>>();
        stack.Push(start);

        while(stack.Count > 0)
        {
            Vertex<T>  current = stack.Pop();
            if(current.Equals(end))
            {
                List<Vertex<T>> path = new List<Vertex<T>>();
                while (current != null)
                {
                    path.Add(current);
                    var edge = GetEdge(previousVertex[current], current);
                    if (edge != null)
                    {
                        pathCost += edge.Distance;

                    }
                    current = previousVertex[current];
                }
                path.Reverse();
                Console.WriteLine("Depth First: " + pathCost + " path cost");
                return path;
            }

            for(int i = current.NeighborCount - 1; i > -1; i--)
            {
                if (!previousVertex.ContainsKey(current.Neighbors[i].EndingPoint))
                {
                    VisitedNodes.Add(current.Neighbors[i].EndingPoint);
                    stack.Push(current.Neighbors[i].EndingPoint);
                    previousVertex[current.Neighbors[i].EndingPoint] = current;
                }
            } 
        }

        return null;
    }
    private class PathfindingInfo
    {
        public Dictionary<Vertex<T>, float> distance;
        public Dictionary<Vertex<T>, float>  finalDistance;
        public Dictionary<Vertex<T>, Vertex<T> > previousVertex;
        public HashSet<Vertex<T>> visited;
        public PathfindingInfo(Dictionary<Vertex<T>, float> distance, Dictionary<Vertex<T>, float> finalDistance, Dictionary<Vertex<T>, Vertex<T> > previousVertex, HashSet<Vertex<T>> visited)
        {
            this.distance = distance;
            this.finalDistance = finalDistance;
            this.previousVertex = previousVertex;
            this.visited = visited;
        }
        public PathfindingInfo(Dictionary<Vertex<T>, float> distance, Dictionary<Vertex<T>, Vertex<T> > previousVertex, HashSet<Vertex<T>> visited)
        {
            this.distance = distance;
            this.previousVertex = previousVertex;
            this.visited = visited;
        }

        public PathfindingInfo(Dictionary<Vertex<T>, float> distance, Dictionary<Vertex<T>, Vertex<T> > previousVertex)
        {
            this.distance = distance;
            this.previousVertex = previousVertex;
            visited = [];
        }
    }

    public List<Vertex<T>>  DijkstraAlgorithm(Vertex<T> start, Vertex<T> end)
    {
        VisitedNodes = new List<Vertex<T>>();
        if (start == null || end == null) return null;

        PathfindingInfo pathInfo = new PathfindingInfo(new Dictionary<Vertex<T>, float>(), new() { [start] = null! }, new HashSet<Vertex<T>>());


        PriorityQueue<Vertex<T>, float> queue = new PriorityQueue<Vertex<T>, float>();   

        for (int i = 0; i < vertices.Count; i++)
        {
            pathInfo.distance[vertices[i]] = float.PositiveInfinity;
            pathInfo.previousVertex[vertices[i]] = null!;
        }
        pathInfo.distance[start] = 0;
        queue.Enqueue(start, pathInfo.distance[start]);

        while(queue.Count > 0)
        {
            Vertex<T> current = queue.Dequeue();
            for(int i = 0; i < current.NeighborCount; i++)
            {
                float tentativeDistance = pathInfo.distance[current] + current.Neighbors[i].Distance;
                if(tentativeDistance < pathInfo.distance[current.Neighbors[i].EndingPoint])
                {
                    VisitedNodes.Add(current.Neighbors[i].EndingPoint);
 
                    pathInfo.distance[current.Neighbors[i].EndingPoint] = tentativeDistance;
                    pathInfo.previousVertex[current.Neighbors[i].EndingPoint] = current!;
                    pathInfo.visited.Remove(current.Neighbors[i].EndingPoint);
                } 
            }

            for(int i = 0; i < current.NeighborCount; i++)
            {
                if (!pathInfo.visited.Contains(current.Neighbors[i].EndingPoint)
                    && !queue.UnorderedItems.AsEnumerable().Contains((current.Neighbors[i].EndingPoint, 2/*gets thrown away by the comparer, soi we don't care what it is*/),
                                                                    new NodeComparer<T>()))
                {
                    queue.Enqueue(current.Neighbors[i].EndingPoint, pathInfo.distance[current.Neighbors[i].EndingPoint]);
                }
            }

            pathInfo.visited.Add(current);
        }
        Vertex<T> theCurrentest = end;
        List<Vertex<T>> path = new List<Vertex<T>>();
        while (pathInfo.previousVertex[theCurrentest!] != null)
        {
            path.Add(theCurrentest!);
            theCurrentest = pathInfo.previousVertex[theCurrentest]!;
            
        
        }
        path.Add(theCurrentest!);

        path.Reverse();

        return path;
        
    }       


    public List<Vertex<T>>  AStarAlgorithm(Vertex<T> start, Vertex<T> end, Func<Vertex<T>, Vertex<T>, double> heuristic)
    {
        VisitedNodes = new List<Vertex<T>>();
        if (start == null || end == null) return null;



        PathfindingInfo pathInfo = new PathfindingInfo(new Dictionary<Vertex<T>, float>(), new Dictionary<Vertex<T>, float>(), new() { [start] = null! }, []) ;


        PriorityQueue<Vertex<T>, float> queue = new PriorityQueue<Vertex<T>, float>();
    
        for (int i = 0; i < vertices.Count; i++)
        {
            pathInfo.distance[vertices[i]] = float.PositiveInfinity;
            pathInfo.previousVertex[vertices[i]] = null;
            pathInfo.finalDistance![vertices[i]] = float.PositiveInfinity;
        }
        pathInfo.distance[start] = 0;
        pathInfo.finalDistance![start] = (float)(heuristic(start, end));
        
        queue.Enqueue(start, pathInfo.finalDistance[start]);

        while(queue.Count > 0)
        {

            Vertex<T> current = queue.Dequeue();

            pathInfo.visited.Add(current);

            for (int i = 0; i < current.NeighborCount; i++)
            {

                float tentativeDistance = pathInfo.distance[current] + current.Neighbors[i].Distance;
                if (tentativeDistance < pathInfo.distance[current.Neighbors[i].EndingPoint])
                {

                    pathInfo.distance[current.Neighbors[i].EndingPoint] = tentativeDistance;
                    pathInfo.previousVertex[current.Neighbors[i].EndingPoint] = current;
                    pathInfo.finalDistance[current] = pathInfo.distance[current] + (float)(heuristic(current, current.Neighbors[i].EndingPoint));
                    
                    pathInfo.visited.Remove(current.Neighbors[i].EndingPoint);
                }
                if (!pathInfo.visited.Contains(current.Neighbors[i].EndingPoint)
                    && !queue.UnorderedItems.Contains((current.Neighbors[i].EndingPoint, 2/*gets thrown away by the comparer*/),
                                                new NodeComparer<T>()))
                {

                    pathInfo.visited.Add(current.Neighbors[i].EndingPoint);
                    queue.Enqueue(current.Neighbors[i].EndingPoint, pathInfo.finalDistance[current.Neighbors[i].EndingPoint]);
                }
            }
        }
        Vertex<T> theCurrentest = end;
        List<Vertex<T>> path = new List<Vertex<T>>();
        while (pathInfo.previousVertex[theCurrentest!] != null)
        {
            path.Add(theCurrentest!);
            theCurrentest = pathInfo.previousVertex[theCurrentest]!;


        }
        path.Add(theCurrentest!);

        path.Reverse();

        return path;
    }

    public void RandomizeGraph() {
        Random rand = new Random();
        for (int i = 0; i < vertices.Count; i++)
        {
            for (int j = 0; j < vertices[i].NeighborCount; j++)
            {
                vertices[i].Neighbors[j].Distance = (float)rand.NextDouble() * 5;
            }
        }
    }





    //HEURISTICS
    public double Manhattan(Vertex<Point> node, Vertex<Point> goal)
    {
        int dx = Math.Abs(node.Value.X - goal.Value.X);
        int dy = Math.Abs(node.Value.Y - goal.Value.Y);
        return 1 * (dx + dy);
    }

    public double Diagonal(Vertex<Point> node, Vertex<Point> goal) 
    {
        int dx = Math.Abs(node.Value.X - goal.Value.X);
        int dy = Math.Abs(node.Value.Y - goal.Value.Y);
        return 1* (dx + dy) + (Math.Sqrt(2) - 2 * 1) * Math.Min(dx, dy);
    }

    public double Euclidean(Vertex<Point> node, Vertex<Point> goal)
    {
        int dx = Math.Abs(node.Value.X - goal.Value.X);
        int dy = Math.Abs(node.Value.Y - goal.Value.Y);
        return 1 * Math.Sqrt(dx * dx + dy * dy);
    }


}



public class NodeComparer<T> : IEqualityComparer<(Vertex<T> Element, float Priority)>
{
    public bool Equals((Vertex<T> Element, float Priority) x, (Vertex<T> Element, float Priority) y) => x.Element.Equals(y.Element);

    public int GetHashCode([DisallowNull] (Vertex<T> Element, float Priority) obj) => obj.Element.GetHashCode();
}


