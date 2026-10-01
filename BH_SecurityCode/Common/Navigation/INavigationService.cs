using BH_SecurityCode.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH_SecurityCode.Common.Navigation
{
    public interface INavigationService
    {
        Task NavigateAsync(Menus type, object? parameter = null);

        /// <summary>
        /// 네비게이션 히스토리 초기화
        /// </summary>
        void Clear();
    }
}
