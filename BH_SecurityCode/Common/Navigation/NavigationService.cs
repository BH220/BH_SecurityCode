using BH_SecurityCode.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BH_SecurityCode.Common.Navigation
{
    public class NavigationService(IServiceProvider provider)
        : INavigationService
    {
        private CancellationTokenSource? _cts;
        public event Action<bool>? LoadingChanged;
         
        private readonly Stack<(FrameworkElement view, object? parameter)> _history = new();


        public async Task NavigateAsync(Menus type, object? parameter = null)
        { 
        }

        private async Task InvokeOutAsync(FrameworkElement? view)
        {
            if (view is { DataContext: IViewLifecycleAsync vm })
            {
                await vm.OnNavigatedOutAsync();
            }
        }

        public void Clear()
        {

        }
    }
}
