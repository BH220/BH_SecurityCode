using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH_SecurityCode.Common.Configurations
{
    public interface IAppSettings : INotifyPropertyChanged
    {
        // Lifecycle
        void Load();
        void Save();
        Task SaveAsync();

        // Optional (설정창용)
        void BeginEdit();
        void Apply();
        void Cancel();
    }
}
