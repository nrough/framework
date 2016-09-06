namespace Infovision.Datamining.Roughset
{
    public interface IReductMeasure : IFactoryProduct
    {
        SortDirection SortDirection { get; }

        double Calc(IReduct reduct);
    }
}