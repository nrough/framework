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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.Core
{
    /// <summary>
    /// http://stackoverflow.com/questions/420429/mirroring-console-output-to-a-file
    /// </summary>
    [AssemblyTreeVisible(false)]
    public class ConsoleCopy : IDisposable
    {
        private FileStream fileStream;
        private StreamWriter fileWriter;
        private TextWriter doubleWriter;
        private TextWriter oldOut;
        private bool isDisposed;

        class DoubleWriter : TextWriter
        {
            private TextWriter one;
            private TextWriter two;

            public DoubleWriter(TextWriter one, TextWriter two)
            {
                this.one = one;
                this.two = two;
            }

            public override Encoding Encoding
            {
                get { return one.Encoding; }
            }

            public override void Flush()
            {
                one.Flush();
                two.Flush();
            }

            public override void Write(char value)
            {
                one.Write(value);
                two.Write(value);
            }
        }

        public ConsoleCopy(string path)
        {
            oldOut = Console.Out;

            try
            {
                fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read);
                fileWriter = new StreamWriter(fileStream);
                fileWriter.AutoFlush = true;

                doubleWriter = new DoubleWriter(fileWriter, oldOut);
            }
            catch (Exception e)
            {
                Console.WriteLine("Cannot open file for writing");
                Console.WriteLine(e.Message);
                return;
            }
            Console.SetOut(doubleWriter);
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing)
                {
                    Console.SetOut(oldOut);
                    if (fileWriter != null)
                    {
                        fileWriter.Flush();
                        fileWriter.Close();
                        //fileWriter.Dispose();
                        fileWriter = null;
                    }
                    if (fileStream != null)
                    {
                        fileStream.Close();
                        //fileStream.Dispose();
                        fileStream = null;
                    }

                    if (doubleWriter != null)
                    {
                        doubleWriter.Close();
                        //doubleWriter.Dispose();
                        doubleWriter = null;
                    }
                }
                    
                isDisposed = true;
            }
        }
        
    }
}
