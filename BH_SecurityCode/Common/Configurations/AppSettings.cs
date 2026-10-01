using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH_SecurityCode.Common.Configurations
{
    public partial class AppSettings : ObservableObject, IAppSettings
    {
        public void Apply()
        {

        }

        public void BeginEdit()
        {

        }

        public void Cancel()
        {

        }

        public void Load()
        {

        }

        public void Save()
        {

        }

        public Task SaveAsync()
        {
            return Task.CompletedTask;
        }
    }
}
