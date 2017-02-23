using System;
using System.ComponentModel;

namespace NRough.MRI.DAL
{
    internal interface IMiningObjectViewModel : INotifyPropertyChanged
    {
        Type GetMiningObjectType();
    }
}