using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH_SecurityCode.Core
{
    /// <summary>
    /// 사용여부(code:101)
    /// </summary>
    public enum CdStatus
    {
        사용 = 101001,
        사용불가 = 101002,
    }

    /// <summary>
    /// 은행(code:110)
    /// </summary>
    public enum CdBank
    {
        우리 = 110001,
        하나 = 110002,
        시티 = 110003,
        국민 = 110004,
        외환 = 110005,
        농협 = 110006,
        카카오 = 110007,
        신한 = 110008,
        우체국 = 110009,
        신협 = 110010,
        기업 = 110011,
        삼성생명 = 110012,
        SC = 110013,
    }

    /// <summary>
    /// 신분증 유형(code:112)
    /// </summary>
    public enum CdIdType
    {
        주민등록증 = 112001,
        운전면허증 = 112002,
        여권 = 112003,
    }

    /// <summary>
    /// 소유자(code:113)
    /// </summary>
    public enum CdOwner
    {
        병호 = 113001,
        지애 = 113002,
        민서 = 113003,
        민우 = 113004,
        민아 = 113005,
        엄마 = 113006,
        아빠 = 113007,
        누나 = 113008,
        장인어른 = 113009,
        장모님 = 113010,
        이모 = 113011,
        할아버지 = 113012,
        CTK = 113013,
        지희 = 113014,
        딥노이드 = 113015,
    }

    /// <summary>
    /// 이미지 유형(code:114) - 보안코드 첨부 이미지에 쓰는 값만 정의
    /// </summary>
    public enum CdImageType
    {
        보안카드 = 114001,
        통장 = 114002,
        카드 = 114003,
        신분증 = 114004,
    }

    /// <summary>
    /// 코드표시방향(code:126)
    /// </summary>
    public enum CdAlign
    {
        가로 = 126001,
        세로 = 126002,
    }

    /// <summary>
    /// 카드사(code:127)
    /// </summary>
    public enum CdCardCompany
    {
        하나 = 127001,
        외환 = 127002,
        롯데 = 127003,
        국민 = 127004,
        신한 = 127005,
        우리 = 127006,
        시티 = 127007,
        멤버쉽 = 127008,
        삼성 = 127009,
        현대 = 127010,
        기업BC = 127011,
        SC = 127012,
        농협 = 127013,
        하남시 = 127014,
        기업 = 127015,
        비자 = 127016,
        카카오 = 127017,
    }
}
