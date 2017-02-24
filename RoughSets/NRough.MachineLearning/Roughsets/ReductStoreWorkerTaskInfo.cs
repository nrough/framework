namespace NRough.MachineLearning.Roughsets
{
    internal class ReductStoreWorkerTaskInfo
    {
        #region Constructors

        public ReductStoreWorkerTaskInfo(int threadIndex, int numberOfThreads, IReduct reduct)
        {
            this.Reduct = reduct;
            this.ThreadIndex = threadIndex;
            this.NumberOfThreads = numberOfThreads;
        }

        #endregion Constructors

        #region Properties

        public int ThreadIndex { get; set; }
        public int NumberOfThreads { get; set; }
        public IReduct Reduct { get; set; }

        #endregion Properties
    }
}