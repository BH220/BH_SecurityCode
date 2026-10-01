using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH_SecurityCode.Common.Navigation
{
    public interface IViewLifecycleAsync
    {
        bool UseLoading { get; }

        Task OnNavigatedInAsync(object? parameter, CancellationToken ct);
        Task OnNavigatedOutAsync();
    }
}
