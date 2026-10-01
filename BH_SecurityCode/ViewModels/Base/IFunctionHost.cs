using BH_SecurityCode.Core;

namespace BH_SecurityCode.ViewModels.Base
{
    /// <summary>
    /// 메인 화면의 컨텐츠 영역에 표시되며 상단 기능 버튼(단축키)을 처리하는 화면.
    /// (기존 BhForm 의 InsertData/UpdateData/... 역할)
    /// </summary>
    public interface IFunctionHost
    {
        Menus MenuId { get; }
        string Title { get; }
        FunctionButtonState Buttons { get; }

        Task InitializeAsync();

        /// <summary><see cref="Core.Constants.Functions"/> 의 기능 ID 를 실행한다.</summary>
        Task RunFunctionAsync(string functionId);
    }
}
