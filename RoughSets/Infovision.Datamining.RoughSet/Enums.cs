namespace Infovision.Datamining.Roughset
{
    public enum VoteType
    {
        Unknown = 0,
        
        Support = 1,
        Confidence = 2,
        Coverage = 3,
        Ratio = 4,
        MajorDecision = 5,
        Strength = 6,

        WeightSupport = 7,
        WeightConfidence = 8,
        WeightCoverage = 9,
        WeightRatio = 10,
        WeightStrength = 11,

        ConfidenceRelative = 12
    }

    public enum IdentificationType
    {
        Unknown = 0,

        Support = 1,
        Confidence = 2,
        Coverage = 3,

        WeightSupport = 4,
        WeightConfidence = 5,
        WeightCoverage = 6
    }

    /// <summary>
    /// Encodes different types of decision rules voting. Relates to right side of the decision rule.
    /// </summary>
    public enum RuleVoteConseqentRating
    {
        Unknown = 0,
        Plain = 1,
        Confidence = 2,
        Coverage = 3
    }

    /// <summary>
    /// When used together with <c>RuleVoteConseqentRating</c> it encodes the decision rule antecedent coefficient. Relates to left side of the decision rule.
    /// </summary>
    public enum RuleVoteAntecedentRating
    {
        Unknown = 0,
        Single = 1,
        Support = 2
    }

    public enum DecisionIdentificationType
    {
        Unknown = 0,
        Support = 1,
        Confidence = 2,
        Coverage = 3
    }

    public enum SortDirection
    {
        Ascending = 0,
        Descending = 1
    }

    public enum NoYesUnknown
    {
        No = 0,
        Yes = 1,
        Unknown = 2
    }

    public enum UpdateWeights
    {
        All = 0,
        CorrectOnly = 1,
        NotCorrectOnly = 2
    }

    public enum WeightingSchema
    {
        Unknown = 0,
        Majority = 1,
        Relative = 2
    }
}