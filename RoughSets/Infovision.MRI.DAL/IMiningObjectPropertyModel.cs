using System;
using System.ComponentModel;

namespace Infovision.MRI.DAL
{
    internal interface IMiningObjectViewModel : INotifyPropertyChanged
    {
        Type GetMiningObjectType();
    }
}