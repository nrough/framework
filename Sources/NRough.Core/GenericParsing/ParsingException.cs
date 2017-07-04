﻿//
// NRough Framework
// http://www.nrough.net
//
// Copyright © Sebastian Widz, 2017
// authors at nrough.net

//This file is part of NRough.

//Foobar is free software; you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation; either version 3 of the License, or
//(at your option) any later version.
//
//Foobar is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
//GNU General Public License for more details.
//
//You should have received a copy of the GNU General Public License
//along with Foobar; if not, write to the Free Software
//Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
//
#region Using Directives

using NRough.Doc;
using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

#endregion Using Directives

namespace GenericParsing
{
    /// <summary>
    ///   <see cref="ParsingException"/> is an exception class meant for states where
    ///   the parser can no longer continue parsing due to the data found in the
    ///   data-source.
    /// </summary>
    [AssemblyTreeVisible(false)]
    [Serializable]
    public class ParsingException : Exception
    {
        #region Constants

        private const string SERIALIZATION_COLUMN_NUMBER = "ColumnNumber";
        private const string SERIALIZATION_FILE_ROW_NUMBER = "FileRowNumber";

        #endregion Constants

        #region Constructors

        /// <summary>
        ///   Creates a new <see cref="ParsingException"/> with default values.
        /// </summary>
        public ParsingException() : base()
        {
            /* Intentionally left blank */
        }
        /// <summary>
        ///   Creates a new <see cref="ParsingException"/> containing a message and the
        ///   file line number that the error occured.
        /// </summary>
        /// <param name="strMessage">
        ///   The message indicating the root cause of the error.
        /// </param>
        /// <param name="intFileRowNumber">The file line number the error occured on.</param>
        /// <param name="intColumnNumber">The column number the error occured on.</param>
        public ParsingException(string strMessage, int intFileRowNumber, int intColumnNumber)
            : base(strMessage)
        {
            this.m_intFileRowNumber = intFileRowNumber;
            this.m_intColumnNumber = intColumnNumber;
        }
        /// <summary>
        ///   Creates a new <see cref="ParsingException"/> with seralized data.
        /// </summary>
        /// <param name="sInfo">
        ///   The <see cref="SerializationInfo"/> that contains information
        ///   about the exception.
        /// </param>
        /// <param name="sContext">
        ///   The <see cref="StreamingContext"/> that contains information
        ///   about the source/destination of the exception.
        /// </param>
        protected ParsingException(SerializationInfo sInfo, StreamingContext sContext)
            : base(sInfo, sContext)
        {
            this.m_intFileRowNumber = sInfo.GetInt32(SERIALIZATION_FILE_ROW_NUMBER);
            this.m_intColumnNumber = sInfo.GetInt32(SERIALIZATION_COLUMN_NUMBER);
        }

        #endregion Constructors

        #region Public Properties

        /// <summary>
        ///   The line number in the file that the exception was thrown at.
        /// </summary>
        public int FileRowNumber
        {
            get
            {
                return this.m_intFileRowNumber;
            }
        }
        /// <summary>
        ///   The column number in the file that the exception was thrown at.
        /// </summary>
        public int ColumnNumber
        {
            get
            {
                return this.m_intColumnNumber;
            }
        }

        #endregion Public Properties

        #region Private Members

        private int m_intFileRowNumber;
        private int m_intColumnNumber;

        #endregion Private Members

        #region Overridden Methods

        /// <summary>
        ///   When overridden in a derived class, sets the <see cref="SerializationInfo"/> 
        ///   with information about the exception.
        /// </summary>
        /// <param name="info">
        ///   The <see cref="SerializationInfo"/> that holds the serialized object data
        ///   about the exception being thrown.
        /// </param>
        /// <param name="context">
        ///   The <see cref="StreamingContext"/> that contains contextual information about the source
        ///   or destination.
        /// </param>
        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue(SERIALIZATION_FILE_ROW_NUMBER, this.m_intFileRowNumber);
            info.AddValue(SERIALIZATION_COLUMN_NUMBER, this.m_intColumnNumber);
        }

        #endregion Overridden Methods
    }
}
