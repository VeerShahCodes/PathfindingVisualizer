using System;
using System.Collections.Generic;
namespace PathfindingVisualizer;

public class Vertex<T>
{
    public T Value { get; set; }
    public List<Edge<T>> Neighbors { get; set; }

    internal Graph<T> Owner { get; set; }

    public int NeighborCount => Neighbors.Count;
    public Vertex(T value)
    {
        Value = value;
        Neighbors = new List<Edge<T>>();
        Owner = new Graph<T>();
    }
     public override bool Equals(object obj)
    {
        if (obj is Vertex<T> other)
            return EqualityComparer<T>.Default.Equals(this.Value, other.Value);
        return false;
    }

    public override int GetHashCode()
    {
        return EqualityComparer<T>.Default.GetHashCode(this.Value);
    }

}
