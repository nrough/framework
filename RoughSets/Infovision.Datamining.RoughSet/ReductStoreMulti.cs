using System;
using System.Threading;

namespace Infovision.Datamining.Roughset
{
    public class ReductStoreMulti : ReductStore
    {
        #region Globals

        private Int32 numberOfThreads;
        private ManualResetEvent[] resetEvents;
        private Boolean threadResults;

        #endregion

        #region Constructors

        public ReductStoreMulti(Int32 numberOfThreads)
            : base()
        {
            this.numberOfThreads = numberOfThreads;
            resetEvents = new ManualResetEvent[this.numberOfThreads];
        }

        #endregion

        #region Methods

        public override Boolean IsSuperSet(IReduct reduct)
        {
            lock (this.SyncRoot)
            {
                if (this.Count < this.numberOfThreads)
                {
                    return base.IsSuperSet(reduct);
                }

                this.threadResults = false;

                for (Int32 i = 0; i < this.numberOfThreads; i++)
                {
                    this.resetEvents[i] = new ManualResetEvent(false);
                    ThreadPool.QueueUserWorkItem(new WaitCallback(this.ReductCheckWorkItem),
                                                    new ReductStoreWorkerTaskInfo(i, this.numberOfThreads, reduct));
                }

                WaitHandle.WaitAll(this.resetEvents);
            }

            return this.threadResults;
        }

        public void ReductCheckWorkItem(Object obj)
        {
            ReductStoreWorkerTaskInfo taskInfo = (ReductStoreWorkerTaskInfo)obj;
            if (this.threadResults)
            {
                this.resetEvents[taskInfo.ThreadIndex].Set();
                return;
            }

            for (int i = taskInfo.ThreadIndex; i < this.Count; i += taskInfo.NumberOfThreads)
            {
                if (taskInfo.Reduct.AttributeSet.Superset(this.GetReduct(i).AttributeSet))
                {
                    this.threadResults = true;
                    break;
                }
            }

            this.resetEvents[taskInfo.ThreadIndex].Set();
        }

        #endregion
    }
}
