using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.MachineLearning.Classification.DecisionTrees
{
    public static class DecisionTreeMetric
    {
        public static int GetNumberOfRules(IDecisionTree tree)
        {
            int count = 0;
            TreeNodeTraversal.TraversePostOrder(tree.Root, n =>
            {
                if (n.IsLeaf)
                    count++;
            });
            return count;
        }

        public static int GetHeight(IDecisionTree tree)
        {
            int maxHeight = 0;
            TreeNodeTraversal.TraversePostOrder(tree.Root, n =>
            {
                if (n.IsLeaf && n.Level > maxHeight)
                    maxHeight = n.Level;
            });
            return maxHeight;
        }

        public static double GetAvgHeight(IDecisionTree tree)
        {
            double sumHeight = 0.0;
            double count = 0.0;
            TreeNodeTraversal.TraversePostOrder(tree.Root, n =>
            {
                if (n.IsLeaf)
                {
                    sumHeight += n.Level;
                    count++;
                }
            });

            return count > 0 ? sumHeight / count : 0.0;
        }

        public static int GetNumberOfAttributes(IDecisionTree tree)
        {
            return (tree.Root != null) && (tree.Root is DecisionTreeNode)
                ? ((DecisionTreeNode)tree.Root).GetChildUniqueKeys().Count
                : 0;
        }        
    }
}
