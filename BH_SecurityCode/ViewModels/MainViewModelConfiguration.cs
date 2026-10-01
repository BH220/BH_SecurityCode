using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wpf.Ui;

namespace BH_SecurityCode.ViewModels
{
    public class MainViewModelConfiguration(
         INavigationService navigationService,
         IMessenger messenger 
         )
    {
        public INavigationService NavigationService { get; } = navigationService;
        public IMessenger Messenger { get; } = messenger; 
    }
}
