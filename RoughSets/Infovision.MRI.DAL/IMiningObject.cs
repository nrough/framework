using System;
using System.Xml.Linq;

namespace Infovision.MRI.DAL
{
    public interface IMiningObject
    {
        string Name { get; set; }
        string TypeId { get; set; }
        long Id { get; set; }
        long RefId { get; set; }

        XElement XMLParametersElement { get; }

        void XMLParseParameters(XElement parametersElement);

        event EventHandler<MiningObjectEventArgs> Executing;

        event EventHandler<MiningObjectEventArgs> Executed;

        event EventHandler<MiningObjectEventArgs> Viewing;

        event EventHandler<MiningObjectEventArgs> Viewed;

        void Execute();

        void View();

        void ReloadReferences(MiningProject project);

        void InitFromViewModel(MiningObjectViewModel viewModel);
    }
}