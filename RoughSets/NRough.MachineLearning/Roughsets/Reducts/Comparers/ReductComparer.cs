using NRough.Data;
using NRough.MachineLearning.Classification;
using NRough.MachineLearning.Classification.DecisionLookup;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NRough.MachineLearning.Roughsets.Reducts.Comparers
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
            DecisionLookup decTabLeft = new DecisionLookup();
            decTabLeft.Learn(this.Data, left.Attributes.ToArray());
            decTabLeft.DefaultOutput = null;
            ClassificationResult leftResult = Classifier.Default.Classify(decTabLeft, this.Data, this.Data.Weights);

            DecisionLookup decTabRight = new DecisionLookup();
            decTabRight.Learn(this.Data, right.Attributes.ToArray());
            decTabRight.DefaultOutput = null;
            ClassificationResult rightResult = Classifier.Default.Classify(decTabRight, this.Data, this.Data.Weights);

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
            if (left.EquivalenceClasses.NumberOfObjects < right.EquivalenceClasses.NumberOfObjects)
                return 1;
            else if (left.EquivalenceClasses.NumberOfObjects > right.EquivalenceClasses.NumberOfObjects)
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