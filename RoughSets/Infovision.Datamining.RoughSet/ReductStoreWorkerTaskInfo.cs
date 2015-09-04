using System;

namespace Infovision.Datamining.Roughset
{
    internal class ReductStoreWorkerTaskInfo
    {
        #region Constructors

        public ReductStoreWorkerTaskInfo(Int32 threadIndex, Int32 numberOfThreads, IReduct reduct)
        {
            this.Reduct = reduct;
            this.ThreadIndex = threadIndex;
            this.NumberOfThreads = numberOfThreads;
        }

        #endregion

        #region Properties

        public Int32 ThreadIndex { get; set; }
        public Int32 NumberOfThreads { get; set; }
        public IReduct Reduct { get; set; }

        #endregion
    }
}
