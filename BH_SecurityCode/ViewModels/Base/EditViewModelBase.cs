using System.Collections.ObjectModel;
using BH_SecurityCode.Api;
using BH_SecurityCode.Api.Manager;
using BH_SecurityCode.Core;
using BH_SecurityCode.Core.Helper;
using BH_SecurityCode.Api.Model;
using BH_SecurityCode.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BH_SecurityCode.Api.Interface;

namespace BH_SecurityCode.ViewModels.Base
{
    /// <summary>
    /// 입력/수정 창 공통 ViewModel. (기존 frmXxxInfo : BhSaveForm)
    /// 저장 / 저장 후 계속 / 닫기 버튼과 소유자 콤보를 공통 제공한다.
    /// </summary>
    public abstract partial class EditViewModelBase : DialogViewModelBase
    {
        protected readonly IDialogService Dialog;
        protected readonly ICodeManager Codes;

        [ObservableProperty]
        private bool _isNew = true;

        /// <summary>한 번이라도 저장에 성공했는지 여부. 닫을 때 목록 갱신 여부로 사용된다.</summary>
        protected bool IsSaved { get; set; }

        public ObservableCollection<CodeInfo> Owners { get; } = new();

        [ObservableProperty]
        private CodeInfo? _selectedOwner;

        protected EditViewModelBase(IDialogService dialog, ICodeManager codes)
        {
            Dialog = dialog;
            Codes = codes;
        }

        /// <summary>필수값 검증. 실패 시 error 에 메시지를 담고 false 반환</summary>
        protected abstract bool Validate(out string error);

        protected abstract Task<(bool Success, string Message)> SaveAsync();

        /// <summary>"저장 후 계속" 이후 새 입력을 위해 화면을 초기화한다.</summary>
        protected virtual void ResetForContinue() { }

        protected async Task LoadOwnersAsync()
        {
            Owners.Clear();
            foreach (var code in await Codes.GetCodesAsync(CodeTypes.Person))
                Owners.Add(code);
        }

        protected void SelectOwner(int cdOwner)
            => SelectedOwner = Owners.FirstOrDefault(x => x.SqCode == cdOwner);

        protected void SelectOwner(CdOwner owner) => SelectOwner((int)owner);

        protected int SelectedOwnerCode => SelectedOwner?.SqCode ?? -1;

        /// <summary>선택된 소유자 코드(CdOwner). 미선택이면 default(0) 이며 서버가 검증한다.</summary>
        protected CdOwner SelectedOwnerType => SelectedOwner == null ? default : (CdOwner)SelectedOwner.SqCode;

        private async Task<bool> TrySaveAsync()
        {
            if (Validate(out string error) == false)
            {
                Dialog.ShowWarning(error);
                return false;
            }

            IsBusy = true;
            try
            {
                var (success, message) = await SaveAsync();
                if (success == false)
                {
                    Dialog.ShowError(string.IsNullOrEmpty(message) ? "저장에 실패하였습니다." : message);
                    return false;
                }
                IsSaved = true;
                // 성공인데 메시지가 있으면 "본문은 저장됐지만 일부 첨부 처리 실패" 같은 경고다. (서버 성공 응답의 msg 는 비어 있다)
                if (string.IsNullOrEmpty(message) == false)
                    Dialog.ShowWarning(message);
                return true;
            }
            catch (ApiNotImplementedException ex)
            {
                // API 연동 전: 오류가 아닌 "구현 필요" 안내
                Dialog.ShowInfo(ex.Message, "구현 필요");
                return false;
            }
            catch (Exception ex)
            {
                Log.Exception(ex, $"{GetType().Name} Save failed");
                Dialog.ShowError($"저장 중 오류가 발생했습니다.\r\n{ex.Message}");
                return false;
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>저장 후 창 닫기</summary>
        [RelayCommand]
        private async Task Save()
        {
            if (await TrySaveAsync())
                DialogResult = true;
        }

        /// <summary>저장 후 계속 입력</summary>
        [RelayCommand]
        private async Task SaveContinue()
        {
            if (await TrySaveAsync())
            {
                IsNew = true;
                ResetForContinue();
            }
        }

        protected override void OnClose() => DialogResult = IsSaved;
    }
}
