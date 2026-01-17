namespace Common;

using System;
using Godot;
/// <summary>
/// Utility extension methods for Godot Nodes.
/// </summary>
public static class Utils
{
    /// <summary>
    /// Adds a new node of type T as a child to the specified parent node.
    /// This allows for easy instantiation and addition of nodes in the scene tree.
    /// </summary>
    /// <typeparam name="T">The type of node to add. Must be a subclass of Node and have a parameterless constructor.</typeparam>
    /// <param name="parent">The parent node to which the new node will be added as a child.</param>
    /// <param name="name">The name to assign to the new node.</param>
    /// <returns>The newly created node of type T.</returns>
    public static T AddNode<T>(this Node parent) where T : Node, new()
    {
        var child = new T();
        GD.Print($"Adding node of type {typeof(T).Name} to parent {parent.Name}");
        parent.AddChild(child);
        return child;
    }
}