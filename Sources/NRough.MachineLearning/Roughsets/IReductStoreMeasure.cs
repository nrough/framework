namespace NRough.MachineLearning.Roughsets
{
    public interface IReductStoreMeasure : IFactoryProduct
    {
        SortDirection SortDirection { get; }

        double Calc(IReductStore reductStore);
    }
}