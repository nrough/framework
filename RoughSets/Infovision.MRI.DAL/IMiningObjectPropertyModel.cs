using System;
using System.ComponentModel;

namespace Raccoon.MRI.DAL
{
    internal interface IMiningObjectViewModel : INotifyPropertyChanged
    {
        Type GetMiningObjectType();
    }
}