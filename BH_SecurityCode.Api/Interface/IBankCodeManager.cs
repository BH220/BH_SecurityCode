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
    /// 보안카드 - /api/security/bank_code/*  (응답 모델 <see cref="BankCodeInfo"/> 를 그대로 사용)
    /// </summary>
    public interface IBankCodeManager
    {
        /// <summary>목록 (POST /api/security/bank_code/list → <see cref="ResBankCodeList"/>)</summary>
        Task<List<BankCodeInfo>> GetListAsync();

        /// <summary>상세. codes, images 포함 (POST /api/security/bank_code/detail → <see cref="ResBankCodeDetail"/>)</summary>
        Task<BankCodeInfo?> GetDetailAsync(int bankCodeNum);

        /// <summary>등록 (POST /api/security/bank_book/update → <see cref="ResBankBookSave"/>)</summary>
        Task<(bool Success, string Message)> InsertAsync(BankCodeInfo model);

        /// <summary>수정 (POST /api/security/bank_book/update + image/add, image/del)</summary>
        Task<(bool Success, string Message)> UpdateAsync(BankCodeInfo model, List<int> deleteImageIds); 

        /// <summary>삭제 - 사용불가 처리 (POST /api/security/bank_code/delete → <see cref="ResBankCodeDelete"/>)</summary>
        Task<(bool Success, string Message)> DeleteAsync(int bankCodeNum);

        /// <summary>사용상태 일괄 변경 (POST /api/security/bank_code/status → <see cref="ResStatusChange"/>)</summary>
        Task<(bool Success, string Message)> ChangeStatusAsync(IEnumerable<int> bankCodeNums, CdStatus status); 
    }
}
