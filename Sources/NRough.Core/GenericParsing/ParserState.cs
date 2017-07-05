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

namespace GenericParsing
{
    /// <summary>
    /// The current internal state of the parser.
    /// </summary>
    [AssemblyTreeVisible(false)]
    public enum ParserState
    {
        /// <summary>
        ///   Indicates that the parser has no datasource and is not properly setup.
        /// </summary>
        NoDataSource = 0,
        /// <summary>
        ///   Indicates that the parser is ready to begin parsing.
        /// </summary>
        Ready = 1,
        /// <summary>
        ///   Indicates that the parser is currently parsing the datasource.
        /// </summary>
        Parsing = 2,
        /// <summary>
        ///   Indicates that the parser has finished parsing the datasource.
        /// </summary>
        Finished = 3
    }
}
