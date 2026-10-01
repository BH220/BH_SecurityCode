namespace BH_SecurityCode.Core
{
    public enum UserType
    {
        SuperAdmin = 0,
        Administrator = 1,
        User = 2,
        Device = 3
    }

    public enum StatusTypes
    {
        Enable = 1,
        Disable = 0,
    }

    /// <summary>
    /// 공통코드(BHB_CODE) 의 TY_CODE 구분
    /// </summary>
    public enum CodeTypes
    {
        Bank = 1,
        Card = 2,
        ID_Card = 3,
        Person = 4
    }

    /// <summary>
    /// 이미지 연결 종류 (TY_LINK)
    /// </summary>
    public enum ImageTypes
    {
        보안카드 = 1,
        통장 = 2,
        카드 = 3,
        신분증 = 4
    }

    /// <summary>
    /// 신분증 종류. 공통코드(BHB_CODE)의 SQ_CODE 값과 매핑된다.
    /// </summary>
    public enum IdTypes
    {
        주민등록증 = 16,
        운전면허증 = 17,
        여권 = 18,
        자격증 = 19
    }

    /// <summary>
    /// 보안카드 형식. (자릿수 * 100) + 코드 수
    /// </summary>
    public enum BankCodeCardTypes
    {
        None = 0,
        _3자리25개 = 325,
        _3자리30개 = 330,
        _3자리35개 = 335,
        _3자리40개 = 340,
        _4자리25개 = 425,
        _4자리30개 = 430,
        _4자리35개 = 435,
        _4자리40개 = 440,
    }

    /// <summary>
    /// 보안카드 코드 배열 방향
    /// </summary>
    public enum ViewTypes
    {
        가로,
        세로
    }

    /// <summary>
    /// 보안카드 간편보기 표시 옵션
    /// </summary>
    public enum SimpleViewTypes
    {
        모두보기,
        앞2자리만보기,
        뒤2자리만보기,
    }

    public enum Menus
    {
        보안코드     = 101,
        통장         = 102,
        카드         = 103,
        신분증       = 104,
        계정         = 105,
        시간연장     = 201,
        화면잠금     = 202,
        로그아웃     = 203,
        프로그램종료 = 204,
    }
}
