namespace NRough.MachineLearning.Roughsets
{
    public interface IReductMeasure : IFactoryProduct
    {
        SortDirection SortDirection { get; }

        double Calc(IReduct reduct);
    }
}