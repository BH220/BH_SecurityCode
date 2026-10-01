using BH_SecurityCode.Core;
using BH_SecurityCode.ViewModels.Base;
using CommunityToolkit.Mvvm.Input;

namespace BH_SecurityCode.ViewModels.IdCard
{
    /// <summary>신분증 추가 시 종류 선택 (기존 frmIdSelector). 종류는 코드 테이블(112) 의 <see cref="CdIdType"/> 를 쓴다.</summary>
    public partial class IdTypeSelectorViewModel : DialogViewModelBase
    {
        public CdIdType SelectedType { get; private set; } = CdIdType.주민등록증;

        public IdTypeSelectorViewModel()
        {
            Title = "추가 항목 선택";
        }

        [RelayCommand]
        private void Select(string? typeName)
        {
            if (Enum.TryParse(typeName, out CdIdType type) == false)
                return;
            SelectedType = type;
            DialogResult = true;
        }
    }
}
