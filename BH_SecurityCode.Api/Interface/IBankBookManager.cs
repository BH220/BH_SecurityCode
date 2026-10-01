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
    /// 통장 - /api/security/bank_book/*
    /// </summary>
    public interface IBankBookManager
    {
        /// <summary>목록 (POST /api/security/bank_book/list → <see cref="ResBankBookList"/>)</summary>
        Task<List<BankBookInfo>> GetListAsync();

        /// <summary>상세. 이미지 포함 (POST /api/security/bank_book/detail → <see cref="ResBankBookDetail"/>)</summary>
        Task<BankBookInfo?> GetAsync(int bankBookNum);

        /// <summary>등록 (POST /api/security/bank_book/update → <see cref="ResBankBookSave"/>)</summary>
        Task<(bool Success, string Message)> InsertAsync(BankBookInfo model);

        /// <summary>수정 (POST /api/security/bank_book/update + image/add, image/del)</summary>
        Task<(bool Success, string Message)> UpdateAsync(BankBookInfo model, List<int> deleteImageIds);

        /// <summary>삭제 - 사용불가 처리 (POST /api/security/bank_book/delete → <see cref="ResBankBookDelete"/>)</summary>
        Task<(bool Success, string Message)> DeleteAsync(int bankBookNum);

        /// <summary>사용상태 일괄 변경 (POST /api/security/bank_book/status → <see cref="ResStatusChange"/>)</summary>
        Task<(bool Success, string Message)> ChangeStatusAsync(IEnumerable<int> bankBookNums, CdStatus status);
    }
}
