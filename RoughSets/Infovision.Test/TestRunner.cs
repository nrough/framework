using System;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;


namespace Infovision.Test
{
    public interface ITestRunable : IEnumerator
    {
        void Run();
        object GetResult();
        string GetResultInfo();
        string GetResultHeader();
    }
    
    public class TestRunner : IDisposable
    {
        #region Globals
        
        private ITestRunable testObject = null;
        private string resultFileName = String.Empty;
        private string autoSaveFilename = String.Empty;

        private bool restore = false;
        //private Timer logWriteTimer = null;
        private int autoSaveTime = 0;
        private ParameterValueVector lastParameterVector = null;
        private ParameterValueVector lastSavedParameterVector = null;
        
        private int testLoop = 0;
        
        private FileStream fileStream = null;
        private StreamWriter resultFile = null;
        private StringBuilder logBuffer = null;

        private bool disposed = false;

        private static object syncRoot = new object();

        #endregion

        #region Constructor

        public TestRunner(string resultFileName, ITestRunable testObject, string autoSaveFileName, int autoSaveTime)
        {
            this.resultFileName = resultFileName;
            this.autoSaveFilename = autoSaveFileName;
            this.autoSaveTime = autoSaveTime;
            this.testLoop = 0;
            this.logBuffer = new StringBuilder();

            //string sAppPath = AppDomain.CurrentDomain.BaseDirectory;
            //stateFileName = sAppPath.Substring(0, sAppPath.LastIndexOf(@"\") + 1) + stateFileName;

            this.restore = false;
            //if (!String.IsNullOrEmpty(autoSaveFilename) && File.Exists(autoSaveFilename))
            //{
            //    this.restore = this.RestoreState(autoSaveFilename);
            //}
            
            if (!this.restore)
            {
                this.testObject = testObject;
            }
        }

        public TestRunner(string testName, ITestRunable testObject, string autoSaveFileName)
            : this(testName, testObject, autoSaveFileName, 60 * 1000)
        {
        }

        public TestRunner(string testName, ITestRunable testObject)
            : this(testName, testObject, "", 0)
        {
        }

        // Implement IDisposable. 
        // Do not make this method virtual. 
        // A derived class should not be able to override this method. 
        public void Dispose()
        {
            Dispose(true);
            // This object will be cleaned up by the Dispose method. 
            // Therefore, you should call GC.SupressFinalize to 
            // take this object off the finalization queue 
            // and prevent finalization code for this object 
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        // Dispose(bool disposing) executes in two distinct scenarios. 
        // If disposing equals true, the method has been called directly 
        // or indirectly by a user's code. Managed and unmanaged resources 
        // can be disposed. 
        // If disposing equals false, the method has been called by the 
        // runtime from inside the finalizer and you should not reference 
        // other objects. Only unmanaged resources can be disposed. 
        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called. 
            if (!this.disposed)
            {
                // If disposing equals true, dispose all managed 
                // and unmanaged resources. 
                if (disposing)
                {
                    // Dispose managed resources.
                    
                    resultFile.Close();
                    resultFile.Dispose();

                    fileStream.Close();
                    fileStream.Dispose();
                }

                // Note disposing has been done.
                disposed = true;
            }
        }
        
        #endregion

        #region Properties

        public ParameterValueVector LastSavedParameterVector
        {
            get { return this.lastSavedParameterVector; }
        }

        public ParameterValueVector CurrentParameterVector
        {
            get { return (ParameterValueVector)testObject.Current; }
        }

        public ITestRunable TestObject
        {
            get { return this.testObject; }
        }
        
        #endregion

        #region Methods

        //private void TimerStart(int interval)
        //{
        //    logWriteTimer = new Timer();
        //    logWriteTimer.Interval = interval;
        //    logWriteTimer.Elapsed += logWriteTimer_Elapsed;
        //    logWriteTimer.AutoReset = true;
        //    logWriteTimer.Start();
        //}

        //private void TimerStop()
        //{
        //    if (logWriteTimer != null)
        //    {
        //        logWriteTimer.Stop();
        //    }
        //}

        public void OpenLog(string fileName)
        {
            try
            {
                fileStream = new FileStream(fileName, FileMode.Append, FileAccess.Write, FileShare.Read);
                resultFile = new StreamWriter(fileStream);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                throw new InvalidOperationException(ex.Message);
            }
        }

        public void CloseLog()
        {
            resultFile.Close();
            fileStream.Close();
        }

        private void LogHeader()
        {
            if (this.testLoop == 1)
            {
                if (!this.restore)
                {
                    if (this.resultFile == null)
                    {
                        this.OpenLog(this.resultFileName);
                    }

                    this.resultFile.WriteLine(testObject.GetResultHeader());
                }
            }
        }

        private void LogResult()
        {
            logBuffer.AppendLine(testObject.GetResultInfo());
        }

        public bool RunSingleTest()
        {
            bool result = false;

            lock (syncRoot)
            {
                if (testObject.MoveNext())
                {
                    testLoop++;

                    ParameterValueVector parameterVector = (ParameterValueVector)testObject.Current;
                    //Console.WriteLine("{0}", parameterVector);

                    testObject.Run();

                    this.LogHeader();
                    this.LogResult();

                    
                    //if (this.autoSaveTime == 0)
                    //{
                    //    this.AutoSave(this.autoSaveFilename, false);
                    //}
                    

                    this.lastParameterVector = new ParameterValueVector((ParameterValueVector)this.testObject.Current);

                    result = true;
                }
            }

            return result;
        }
        
        public void Run()
        {
            this.OpenLog(resultFileName);

            bool runFlag = true;
            while (runFlag)
            {
                runFlag = RunSingleTest();
                if (this.testLoop > 0 && this.testLoop % 100 == 0)
                    this.SaveResults();
            }

            this.SaveResults();

            this.CloseLog();
        }

        //public void logWriteTimer_Elapsed(Object sender, ElapsedEventArgs e)
        //{
        //    this.SaveResults(this.autoSaveFilename);
        //}

        public void SaveResults()
        {
            try
            {
                this.resultFile.Write(logBuffer);
                this.resultFile.Flush();
                this.fileStream.Flush();

                this.logBuffer.Clear();

                this.lastSavedParameterVector = this.lastParameterVector;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                throw new InvalidOperationException(ex.Message);
            }
        }


        public void SaveResults(string autoSaveStateFileName, bool saveState = true)
        {
            try
            {
                lock (syncRoot)
                {
                    if (!saveState || this.SaveState(autoSaveStateFileName))
                    {
                        this.SaveResults();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                throw new InvalidOperationException(ex.Message);
            }
        }


        /// <summary>
        /// Restores the state of the instance (thus allowing you to resume calculations)
        /// </summary>
        public bool RestoreState(string fileName)
        {
            bool bSuccess = false;
            try
            {
                //--- deserialize the object
                using (FileStream stream = new FileStream(fileName, FileMode.Open))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    
                    testObject = (ITestRunable) formatter.Deserialize(stream);
                    lastSavedParameterVector = (ParameterValueVector) testObject.Current;
                    stream.Close();

                    bSuccess = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }

            return bSuccess;
        }

        /// <summary>
        /// Save the state of the class
        /// </summary>
        public bool SaveState(string fileName)
        {
            bool bSuccess = false;
            try
            {
                //TODO save to temporary file
                //TODO overwrite the state file with temporary

                //or copy current state file to temporary and write state file
                
                //--- serialize the instance using a BinaryFormatter
                using (FileStream stream = new FileStream(fileName, FileMode.Create))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(stream, testObject);
                    stream.Close();

                    bSuccess = true;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message);
            }

            return bSuccess;
        }
        
        #endregion
    }
}
