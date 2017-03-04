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
