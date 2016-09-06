﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Datamining.Roughset.DecisionTrees
{
    /// <summary>
    /// https://en.wikipedia.org/wiki/Tree_traversal
    /// </summary>
    public static class TreeNodeTraversal
    {
        /// <summary>
        /// Traverse tree in level order and perform Action for every tree node (aka Breadth-startFromIdx search (BFS))
        /// </summary>
        /// <param name="node"></param>
        /// <param name="action"></param>
        public static void TraverseLevelOrder(ITreeNode node, Action<ITreeNode> action)
        {
            Queue<ITreeNode> queue = new Queue<ITreeNode>();
            queue.Enqueue(node);
            while (queue.Count != 0)
            {
                ITreeNode currentNode = queue.Dequeue();
                action.Invoke(currentNode);

                if (currentNode.Children != null)
                    foreach (ITreeNode child in currentNode.Children)
                        if (child != null)
                            queue.Enqueue(child);
            }
        }

        public static void TraversePreOrder(ITreeNode node, Action<ITreeNode> action)
        {
            if (node == null)
                return;
            action.Invoke(node);
            if (node.Children != null)
                foreach (ITreeNode child in node.Children)
                    TreeNodeTraversal.TraversePreOrder(child, action);
        }

        public static void TraverseInOrder(ITreeNode node, Action<ITreeNode> action)
        {
            if (node == null)
                return;

            bool nodeActionFinished = false;

            if (node.Children != null)
                foreach (ITreeNode child in node.Children)
                {
                    TreeNodeTraversal.TraverseInOrder(child, action);
                    if (nodeActionFinished == false)
                    {
                        action.Invoke(node);
                        nodeActionFinished = true;
                    }
                }
        }

        public static void TraversePostOrder(ITreeNode node, Action<ITreeNode> action)
        {
            if (node == null)
                return;
            if (node.Children != null)
                foreach (ITreeNode child in node.Children)
                    TreeNodeTraversal.TraversePostOrder(child, action);
            action.Invoke(node);
        }

        public static void TraverseEulerPath(ITreeNode node, Action<ITreeNode> action)
        {
            if (node == null)
                return;
            action.Invoke(node);
            foreach (ITreeNode child in node.Children)
            {
                TreeNodeTraversal.TraverseEulerPath(child, action);
                action.Invoke(node);
            }
        }
    }
}