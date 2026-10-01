using BH_SecurityCode.Api.Model.Response;
using BH_SecurityCode.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH_SecurityCode.Api.Interface
{
    /// <summary>
    /// 계정(사이트 + 하위 아이디/비밀번호) - /api/security/account/*
    /// </summary>
    public interface IAccountManager
    {
        /// <summary>목록. 대표 아이디(id) 포함 (POST /api/security/account/list → <see cref="ResAccountList"/>)</summary>
        Task<List<AccountInfo>> GetListAsync();

        /// <summary>상세. 하위 계정 items 포함 (POST /api/security/account/detail → <see cref="ResAccountDetail"/>)</summary>
        Task<AccountInfo?> GetAsync(int accountNum);

        /// <summary>등록 (POST /api/security/account/update → <see cref="ResAccountSave"/>, 하위 계정은 sub/update)</summary>
        Task<(bool Success, string Message)> InsertAsync(AccountInfo model);

        /// <summary>수정 (POST /api/security/account/update + sub/update, 삭제된 하위 계정은 status 사용불가)</summary>
        Task<(bool Success, string Message)> UpdateAsync(AccountInfo model, List<int> deleteAccountIds);

        /// <summary>삭제 - 사용불가 처리 (POST /api/security/account/delete → <see cref="ResAccountDelete"/>)</summary>
        Task<(bool Success, string Message)> DeleteAsync(int accountNum);

        /// <summary>사용상태 일괄 변경 (POST /api/security/account/status → <see cref="ResStatusChange"/>)</summary>
        Task<(bool Success, string Message)> ChangeStatusAsync(IEnumerable<int> accountNums, CdStatus status);
    }
}
