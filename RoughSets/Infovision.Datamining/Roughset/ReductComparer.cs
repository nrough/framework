using Infovision.Data;
using Infovision.MachineLearning.Classification;
using Infovision.MachineLearning.Classification.DecisionTables;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Infovision.MachineLearning.Roughset
{
    public abstract class ReductBaseComparer : Comparer<IReduct>, ICloneable
    {
        public object Clone()
        {
            var clone = (ReductBaseComparer)this.MemberwiseClone();
            this.HandleCloned(clone);
            return clone;
        }

        protected virtual void HandleCloned(ReductBaseComparer clone)
        {
        }
    }

    public class ReductLengthComparer : ReductBaseComparer
    {
        public override int Compare(IReduct left, IReduct right)
        {
            if (left.Attributes.Count > right.Attributes.Count)
                return 1;
            else if (left.Attributes.Count < right.Attributes.Count)
                return -1;
            return 0;
        }
    }

    public class ReductAccuracyComparer : ReductBaseComparer
    {
        private DataStore Data { get; set; }
        public ReductAccuracyComparer(DataStore data)
            : base()
        {
            this.Data = data;
        }
        public override int Compare(IReduct left, IReduct right)
        {
            DecisionTable decTabLeft = new DecisionTable();
            decTabLeft.Learn(this.Data, left.Attributes.ToArray());
            decTabLeft.DefaultOutput = null;
            ClassificationResult leftResult = Classifier.DefaultClassifer.Classify(decTabLeft, this.Data, this.Data.Weights);

            DecisionTable decTabRight = new DecisionTable();
            decTabRight.Learn(this.Data, right.Attributes.ToArray());
            decTabRight.DefaultOutput = null;
            ClassificationResult rightResult = Classifier.DefaultClassifer.Classify(decTabRight, this.Data, this.Data.Weights);

            return leftResult.Accuracy.CompareTo(rightResult.Accuracy);
        }
    }

    public class ReductRuleNumberComparer : ReductBaseComparer
    {
        public override int Compare(IReduct left, IReduct right)
        {
            if (left.EquivalenceClasses.Count > right.EquivalenceClasses.Count)
                return 1;
            else if (left.EquivalenceClasses.Count < right.EquivalenceClasses.Count)
                return -1;
            return 0;
        }
    }

    public class BireductSizeComparer : ReductBaseComparer
    {
        public override int Compare(IReduct left, IReduct right)
        {
            if (left.ObjectSetInfo.NumberOfRecords < right.ObjectSetInfo.NumberOfRecords)
                return 1;
            else if (left.ObjectSetInfo.NumberOfRecords > right.ObjectSetInfo.NumberOfRecords)
                return -1;
            return 0;
        }
    }

    public class BireductRelativeComparer : ReductBaseComparer
    {
        private BireductMeasureRelative bireductMeasureRelative = new BireductMeasureRelative();

        public override int Compare(IReduct left, IReduct right)
        {
            double leftResult = bireductMeasureRelative.Calc(left);
            double rightResult = bireductMeasureRelative.Calc(right);

            if (leftResult < rightResult)
                return 1;
            else if (leftResult > rightResult)
                return -1;
            return 0;
        }
    }

    //TODO Implement Reduct DiversityComparer
    public class DiversityComparer : ReductBaseComparer
    {
        public override int Compare(IReduct left, IReduct right)
        {
            
            return 0;
        }
    }

    public abstract class ReductStoreBaseComparer : Comparer<IReductStore>, ICloneable
    {
        public object Clone()
        {
            var clone = (ReductStoreBaseComparer)this.MemberwiseClone();
            this.HandleCloned(clone);
            return clone;
        }

        protected virtual void HandleCloned(ReductStoreBaseComparer clone)
        {
        }
    }

    public class ReductStoreLengthComparer : ReductStoreBaseComparer
    {
        public bool IncludeExceptions { get; set; }

        public ReductStoreLengthComparer(bool includeExceptions)
            : base()
        {
            this.IncludeExceptions = includeExceptions;
        }

        public override int Compare(IReductStore left, IReductStore right)
        {
            double avgLengthLeft = left.GetWeightedAvgMeasure(new ReductMeasureLength(), this.IncludeExceptions);
            double avgLengthRight = right.GetWeightedAvgMeasure(new ReductMeasureLength(), this.IncludeExceptions);

            if (avgLengthLeft > avgLengthRight)
                return 1;
            else if (avgLengthLeft < avgLengthRight)
                return -1;

            return 0;
        }
    }
}