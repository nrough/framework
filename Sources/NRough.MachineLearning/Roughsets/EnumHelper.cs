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

using System;

namespace NRough.MachineLearning.Roughsets
{
    public static class EnumHelper
    {
        /// <summary>
        /// Convertes <c>VoteType</c> enum key to <c>RuleVoteConseqentRating</c> enum key
        /// </summary>
        /// <param name="voteType"></param>
        /// <returns></returns>
        public static RuleVoteConseqentRating VoteType2RuleVoteConseqentRating(VoteType voteType)
        {
            RuleVoteConseqentRating result = RuleVoteConseqentRating.Unknown;

            switch (voteType)
            {
                case VoteType.Support:
                    result = RuleVoteConseqentRating.Confidence;
                    break;

                case VoteType.Confidence:
                    result = RuleVoteConseqentRating.Confidence;
                    break;

                case VoteType.Coverage:
                    result = RuleVoteConseqentRating.Coverage;
                    break;

                case VoteType.Ratio:
                    result = RuleVoteConseqentRating.Coverage;
                    break;

                case VoteType.MajorDecision:
                    result = RuleVoteConseqentRating.Plain;
                    break;

                case VoteType.Strength:
                    result = RuleVoteConseqentRating.Plain;
                    break;

                case VoteType.WeightSupport:
                    result = RuleVoteConseqentRating.Confidence;
                    break;

                case VoteType.WeightConfidence:
                    result = RuleVoteConseqentRating.Confidence;
                    break;

                case VoteType.WeightCoverage:
                    result = RuleVoteConseqentRating.Confidence;
                    break;

                case VoteType.WeightRatio:
                    result = RuleVoteConseqentRating.Coverage;
                    break;

                case VoteType.WeightStrength:
                    result = RuleVoteConseqentRating.Coverage;
                    break;

                case VoteType.ConfidenceRelative:
                    result = RuleVoteConseqentRating.Plain;
                    break;

                default:
                    throw new InvalidOperationException(String.Format("Cannot convert {0} to RuleVoteConseqentRating enum key", voteType));
            }

            return result;
        }

        /// <summary>
        /// Converted <c>VoteType</c> enum key to <c>RuleVoteAntecedentRating</c> enum key.
        /// </summary>
        /// <param name="voteType"></param>
        /// <returns></returns>
        public static RuleVoteAntecedentRating VoteType2RuleVoteAntecedentRating(VoteType voteType)
        {
            RuleVoteAntecedentRating result = RuleVoteAntecedentRating.Unknown;

            switch (voteType)
            {
                case VoteType.Support:
                    result = RuleVoteAntecedentRating.Support;
                    break;

                case VoteType.Confidence:
                    result = RuleVoteAntecedentRating.Single;
                    break;

                case VoteType.Coverage:
                    result = RuleVoteAntecedentRating.Support;
                    break;

                case VoteType.Ratio:
                    result = RuleVoteAntecedentRating.Single;
                    break;

                case VoteType.MajorDecision:
                    result = RuleVoteAntecedentRating.Single;
                    break;

                case VoteType.Strength:
                    result = RuleVoteAntecedentRating.Support;
                    break;

                case VoteType.WeightSupport:
                    result = RuleVoteAntecedentRating.Support;
                    break;

                case VoteType.WeightConfidence:
                    result = RuleVoteAntecedentRating.Single;
                    break;

                case VoteType.WeightCoverage:
                    result = RuleVoteAntecedentRating.Support;
                    break;

                case VoteType.WeightRatio:
                    result = RuleVoteAntecedentRating.Single;
                    break;

                case VoteType.WeightStrength:
                    result = RuleVoteAntecedentRating.Support;
                    break;

                case VoteType.ConfidenceRelative:
                    result = RuleVoteAntecedentRating.Single;
                    break;

                default:
                    throw new InvalidOperationException(String.Format("Cannot convert {0} to RuleVoteAntecedentRating enum key", voteType));
            }

            return result;
        }

        /// <summary>
        /// Converts the combination of <c>RuleVoteConseqentRating</c> and <c>RuleVoteAntecedentRating</c> enum key to <c>VoteType</c> enum newInstance
        /// </summary>
        /// <param name="consequentRating"></param>
        /// <param name="antecedentRating"></param>
        /// <param name="isWeighting"></param>
        /// <returns></returns>
        public static VoteType RuleVoteRating2VoteType(RuleVoteConseqentRating consequentRating, RuleVoteAntecedentRating antecedentRating, bool isWeighting)
        {
            VoteType result = VoteType.Unknown;

            switch (consequentRating)
            {
                case RuleVoteConseqentRating.Confidence:
                    switch (antecedentRating)
                    {
                        case RuleVoteAntecedentRating.Support:
                            result = isWeighting ? VoteType.WeightSupport : VoteType.Support;
                            break;

                        case RuleVoteAntecedentRating.Single:
                            result = isWeighting ? VoteType.WeightConfidence : VoteType.Confidence;
                            break;
                    }
                    break;

                case RuleVoteConseqentRating.Coverage:
                    switch (antecedentRating)
                    {
                        case RuleVoteAntecedentRating.Support:
                            result = isWeighting ? VoteType.WeightCoverage : VoteType.Coverage;
                            break;

                        case RuleVoteAntecedentRating.Single:
                            result = isWeighting ? VoteType.WeightRatio : VoteType.Ratio;
                            break;
                    }
                    break;

                case RuleVoteConseqentRating.Plain:
                    switch (antecedentRating)
                    {
                        case RuleVoteAntecedentRating.Support:
                            result = isWeighting ? VoteType.WeightStrength : VoteType.Strength;
                            break;

                        case RuleVoteAntecedentRating.Single:
                            result = VoteType.MajorDecision;
                            break;
                    }
                    break;
            }

            return result;
        }

        /// <summary>
        /// Converts <c>DecisionIdentificationType</c> enum key to <c>IdentificationType</c> enum key.
        /// </summary>
        /// <param name="decisionIdentyficationType"></param>
        /// <param name="isWeighting"></param>
        /// <returns></returns>
        public static IdentificationType DecisionIdentificationType2IdentificationType(DecisionIdentificationType decisionIdentificationType, bool isWeighting)
        {
            IdentificationType result = IdentificationType.Unknown;

            switch (decisionIdentificationType)
            {
                case DecisionIdentificationType.Confidence:
                    result = isWeighting ? IdentificationType.WeightConfidence : IdentificationType.Confidence;
                    break;

                case DecisionIdentificationType.Coverage:
                    result = isWeighting ? IdentificationType.WeightCoverage : IdentificationType.Coverage;
                    break;

                case DecisionIdentificationType.Support:
                    result = isWeighting ? IdentificationType.WeightSupport : IdentificationType.Support;
                    break;
            }

            return result;
        }

        /// <summary>
        /// Converts <c>IdentificationType</c> enum key to <c>DecisionIdentificationType</c> enum key.
        /// </summary>
        /// <param name="identificationType"></param>
        /// <returns></returns>
        public static DecisionIdentificationType IdentificationType2DecisionIdentificationType(IdentificationType identificationType)
        {
            DecisionIdentificationType result = DecisionIdentificationType.Unknown;

            switch (identificationType)
            {
                case IdentificationType.Confidence:
                    result = DecisionIdentificationType.Confidence;
                    break;

                case IdentificationType.Coverage:
                    result = DecisionIdentificationType.Coverage;
                    break;

                case IdentificationType.Support:
                    result = DecisionIdentificationType.Support;
                    break;

                case IdentificationType.WeightConfidence:
                    result = DecisionIdentificationType.Confidence;
                    break;

                case IdentificationType.WeightCoverage:
                    result = DecisionIdentificationType.Coverage;
                    break;

                case IdentificationType.WeightSupport:
                    result = DecisionIdentificationType.Support;
                    break;
            }

            return result;
        }
    }
}