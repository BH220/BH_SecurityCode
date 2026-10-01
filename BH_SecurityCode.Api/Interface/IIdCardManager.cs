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
    /// 신분증 - /api/security/id/*
    /// </summary>
    public interface IIdCardManager
    {
        /// <summary>목록 (POST /api/security/id/list → <see cref="ResIdCardList"/>)</summary>
        Task<List<IdCardInfo>> GetListAsync();

        /// <summary>상세. 이미지 포함 (POST /api/security/id/detail → <see cref="ResIdCardDetail"/>)</summary>
        Task<IdCardInfo?> GetAsync(int idNum);

        /// <summary>등록 (POST /api/security/id/update → <see cref="ResIdCardSave"/>) + 신규 이미지 image/add</summary>
        Task<(bool Success, string Message)> InsertAsync(IdCardInfo model);

        /// <summary>수정 (POST /api/security/id/update + image/add, image/del)</summary>
        Task<(bool Success, string Message)> UpdateAsync(IdCardInfo model, List<int> deleteImageIds);

        /// <summary>삭제 - 사용불가 처리 (POST /api/security/id/delete → <see cref="ResIdCardDelete"/>)</summary>
        Task<(bool Success, string Message)> DeleteAsync(int idNum);

        /// <summary>사용상태 일괄 변경 (POST /api/security/id/status → <see cref="ResStatusChange"/>)</summary>
        Task<(bool Success, string Message)> ChangeStatusAsync(IEnumerable<int> idNums, CdStatus status);
    }

}
