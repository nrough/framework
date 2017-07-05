// 
// NRough Framework
// http://www.nrough.net
// 
// Copyright (c) Sebastian Widz, 2017
// authors at nrough.net
// 
// This file is part of NRough Framework.
// 
// NRough Framework is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 3 of the License, or
// (at your option) any later version.
// 
// NRough Framework is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with NRough Framework; if not, write to the Free Software
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
//

using NRough.Doc;

namespace NRough.MachineLearning.Roughsets
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
    /// Encodes different types of decisionInternalValue rules voting. Relates to right side of the decisionInternalValue rule.
    /// </summary>
    public enum RuleVoteConseqentRating
    {
        Unknown = 0,
        Plain = 1,
        Confidence = 2,
        Coverage = 3
    }

    /// <summary>
    /// When used together with <c>RuleVoteConseqentRating</c> it encodes the decisionInternalValue rule antecedent coefficient. Relates to left side of the decisionInternalValue rule.
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

    [AssemblyTreeVisible(false)]
    public enum SortDirection
    {
        Ascending = 0,
        Descending = 1,
        Random = 2,
        None = 3
    }

    [AssemblyTreeVisible(false)]
    public enum NoYesUnknown
    {
        No = 0,
        Yes = 1,
        Unknown = 2
    }

    public enum WeightingScheme
    {
        Unknown = 0,
        Majority = 1,
        Relative = 2
    }
}