using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Infovision.MRI.DAL
{
    interface IMiningObjectViewModel : INotifyPropertyChanged
    {
        Type GetMiningObjectType();
    }
}
