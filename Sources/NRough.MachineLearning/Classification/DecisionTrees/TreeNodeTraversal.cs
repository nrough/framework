using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.MachineLearning.Classification.DecisionTrees
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
        public static void TraverseLevelOrder(IDecisionTreeNode node, Action<IDecisionTreeNode> action)
        {
            Queue<IDecisionTreeNode> queue = new Queue<IDecisionTreeNode>();
            queue.Enqueue(node);
            while (queue.Count != 0)
            {
                IDecisionTreeNode currentNode = queue.Dequeue();
                action.Invoke(currentNode);

                if (currentNode.Children != null)
                    foreach (IDecisionTreeNode child in currentNode.Children)
                        if (child != null)
                            queue.Enqueue(child);
            }
        }

        public static void TraversePreOrder(IDecisionTreeNode node, Action<IDecisionTreeNode> action)
        {
            if (node == null)
                return;
            action.Invoke(node);
            if (node.Children != null)
                foreach (IDecisionTreeNode child in node.Children)
                    TreeNodeTraversal.TraversePreOrder(child, action);
        }

        public static void TraverseInOrder(IDecisionTreeNode node, Action<IDecisionTreeNode> action)
        {
            if (node == null)
                return;

            bool nodeActionFinished = false;

            if (node.Children != null)
                foreach (IDecisionTreeNode child in node.Children)
                {
                    TreeNodeTraversal.TraverseInOrder(child, action);
                    if (nodeActionFinished == false)
                    {
                        action.Invoke(node);
                        nodeActionFinished = true;
                    }
                }
        }

        public static void TraversePostOrder(IDecisionTreeNode node, Action<IDecisionTreeNode> action)
        {
            if (node == null)
                return;

            if (node.Children != null)
                foreach (IDecisionTreeNode child in node.Children)
                    TreeNodeTraversal.TraversePostOrder(child, action);

            action.Invoke(node);
        }

        public static void TraverseEulerPath(IDecisionTreeNode node, Action<IDecisionTreeNode> action)
        {
            if (node == null)
                return;
            action.Invoke(node);
            foreach (IDecisionTreeNode child in node.Children)
            {
                TreeNodeTraversal.TraverseEulerPath(child, action);
                action.Invoke(node);
            }
        }

        public static IEnumerator<IDecisionTreeNode> GetEnumeratorTopDown(IDecisionTreeNode node)
        {
            if (node == null)
                yield break;

            var stack = new Stack<IDecisionTreeNode>(new[] { node });
            while (stack.Count != 0)
            {
                IDecisionTreeNode current = stack.Pop();
                yield return current;

                if (current.Children != null)
                    foreach (IDecisionTreeNode child in current.Children)
                        stack.Push(child);
            }
        }

        public static IEnumerator<IDecisionTreeNode> GetEnumeratorBottomUp(IDecisionTreeNode node)
        {
            IEnumerator<IDecisionTreeNode> topDownEnumerator = TreeNodeTraversal.GetEnumeratorTopDown(node);
            List<IDecisionTreeNode> list = new List<IDecisionTreeNode>();
            while (topDownEnumerator.MoveNext())
                list.Add(topDownEnumerator.Current);
            list.Reverse();
            return list.GetEnumerator();
        }
    }
}
