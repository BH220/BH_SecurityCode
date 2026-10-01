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
    /// 카드 - /api/security/card/*
    /// </summary>
    public interface ICardManager
    {
        /// <summary>목록 (POST /api/security/card/list → <see cref="ResCardList"/>)</summary>
        Task<List<CardInfo>> GetListAsync();

        /// <summary>상세. 이미지 포함 (POST /api/security/card/detail → <see cref="ResCardDetail"/>)</summary>
        Task<CardInfo?> GetAsync(int cardNum);

        /// <summary>등록 (POST /api/security/card/update → <see cref="ResCardSave"/>)</summary>
        Task<(bool Success, string Message)> InsertAsync(CardInfo model);

        /// <summary>수정 (POST /api/security/card/update + image/add, image/del)</summary>
        Task<(bool Success, string Message)> UpdateAsync(CardInfo model, List<int> deleteImageIds);

        /// <summary>삭제 - 사용불가 처리 (POST /api/security/card/delete → <see cref="ResCardDelete"/>)</summary>
        Task<(bool Success, string Message)> DeleteAsync(int cardNum);

        /// <summary>사용상태 일괄 변경 (POST /api/security/card/status → <see cref="ResStatusChange"/>)</summary>
        Task<(bool Success, string Message)> ChangeStatusAsync(IEnumerable<int> cardNums, CdStatus status);
    }
}
