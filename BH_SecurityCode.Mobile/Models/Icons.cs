using Microsoft.Maui.Controls.Shapes;

namespace BH_SecurityCode.Mobile.Models
{
    /// <summary>
    /// 24x24 기준으로 직접 그린 선 아이콘. 아이콘 폰트를 넣지 않고 도형으로 그려서
    /// 테마 색(Stroke)만 바꿔 라이트/다크에 그대로 쓴다. Shapes.Path 로 그린다.
    /// XAML 에서는 Data="{x:Static m:Icons.CodeCard}" 처럼 참조한다.
    /// </summary>
    public static class Icons
    {
        private static Geometry P(string data) =>
            (Geometry)new PathGeometryConverter().ConvertFromInvariantString(data)!;

        /// <summary>보안코드 — 코드칸이 있는 보안카드 (데스크톱 security_card.png)</summary>
        public static readonly Geometry CodeCard = P(
            "M2.5 7 A2 2 0 0 1 4.5 5 H19.5 A2 2 0 0 1 21.5 7 V17 A2 2 0 0 1 19.5 19 " +
            "H4.5 A2 2 0 0 1 2.5 17 Z M2.5 9.2 H21.5 " +
            "M5.5 12.2 H9.5 V16 H5.5 Z M11 12.2 H15 V16 H11 Z M16.5 12.2 H20.5 V16 H16.5 Z");

        /// <summary>통장 — 좌측 제본선이 있는 통장 (데스크톱 bankbook.png)</summary>
        public static readonly Geometry Book = P(
            "M4.5 4.5 A2 2 0 0 1 6.5 2.5 H19.5 V21.5 H6.5 A2 2 0 0 1 4.5 19.5 Z " +
            "M4.5 17.6 H19.5 M8.5 7 H15.5 M8.5 10.5 H13");

        /// <summary>카드 — IC 칩과 번호가 있는 신용카드 (데스크톱 credit_card.png)</summary>
        public static readonly Geometry Card = P(
            "M2.5 6.5 A2 2 0 0 1 4.5 4.5 H19.5 A2 2 0 0 1 21.5 6.5 V17.5 A2 2 0 0 1 19.5 19.5 " +
            "H4.5 A2 2 0 0 1 2.5 17.5 Z M5.5 8.2 H9 V11.2 H5.5 Z M5.5 15.6 H12 M14.8 15.6 H18.5");

        /// <summary>신분증 — 클립 · 사진 · 기재란 (데스크톱 id_card.png)</summary>
        public static readonly Geometry IdCard = P(
            "M10.6 2.5 H13.4 V5.5 H10.6 Z " +
            "M2.5 7.5 A2 2 0 0 1 4.5 5.5 H19.5 A2 2 0 0 1 21.5 7.5 V18.5 A2 2 0 0 1 19.5 20.5 " +
            "H4.5 A2 2 0 0 1 2.5 18.5 Z " +
            "M6.9 11.4 A1.7 1.7 0 0 1 10.3 11.4 A1.7 1.7 0 0 1 6.9 11.4 Z " +
            "M6 16.8 C6.6 14.9 10.6 14.9 11.2 16.8 M14.4 10.8 H18.6 M14.4 14.2 H18.6");

        /// <summary>계정 — 열쇠 (데스크톱 account.png 의 열쇠)</summary>
        public static readonly Geometry Key = P(
            "M4 12 A4 4 0 0 1 12 12 A4 4 0 0 1 4 12 Z M12 12 H21 M17.2 12 V16 M20.2 12 V15.2");

        /// <summary>앱 마크 — 해저드 밴드가 붙은 보안 폴더 (데스크톱 app.ico)</summary>
        public static readonly Geometry Folder = P(
            "M2.5 6.5 A2 2 0 0 1 4.5 4.5 H9.2 L11.7 7 H19.5 A2 2 0 0 1 21.5 9 V17.5 " +
            "A2 2 0 0 1 19.5 19.5 H4.5 A2 2 0 0 1 2.5 17.5 Z " +
            "M16 9 V19.5 M16 11.3 L19.4 14.7 M16 14.8 L19.4 18.2");

        /// <summary>홈</summary>
        public static readonly Geometry Home = P(
            "M3.5 11 L12 3.5 L20.5 11 V20.5 H14.8 V14.5 H9.2 V20.5 H3.5 Z");

        /// <summary>검색</summary>
        public static readonly Geometry Search = P(
            "M10.5 3.5 A7 7 0 0 1 10.5 17.5 A7 7 0 0 1 10.5 3.5 Z M15.6 15.6 L20.5 20.5");

        /// <summary>설정 — 슬라이더</summary>
        public static readonly Geometry Settings = P(
            "M3.5 7.5 H20.5 M3.5 16.5 H20.5 " +
            "M6.8 7.5 A2.2 2.2 0 0 1 11.2 7.5 A2.2 2.2 0 0 1 6.8 7.5 Z " +
            "M12.8 16.5 A2.2 2.2 0 0 1 17.2 16.5 A2.2 2.2 0 0 1 12.8 16.5 Z");

        public static readonly Geometry ChevronRight = P("M9 5.5 L15.5 12 L9 18.5");
        public static readonly Geometry ChevronLeft = P("M15 5.5 L8.5 12 L15 18.5");
        public static readonly Geometry Plus = P("M12 5 V19 M5 12 H19");
        public static readonly Geometry Check = P("M5 13 L10 18 L19.5 6.5");
        public static readonly Geometry Close = P("M6 6 L18 18 M18 6 L6 18");

        /// <summary>복사</summary>
        public static readonly Geometry Copy = P(
            "M9 9 H20 V20.5 H9 Z M16 5.5 V3.5 H4 V15.5 H5.8");

        /// <summary>값 보기</summary>
        public static readonly Geometry Eye = P(
            "M2.5 12 C5.5 6.5 18.5 6.5 21.5 12 C18.5 17.5 5.5 17.5 2.5 12 Z " +
            "M9.7 12 A2.3 2.3 0 0 1 14.3 12 A2.3 2.3 0 0 1 9.7 12 Z");

        /// <summary>값 가리기</summary>
        public static readonly Geometry EyeOff = P(
            "M2.5 12 C5.5 6.5 18.5 6.5 21.5 12 C18.5 17.5 5.5 17.5 2.5 12 Z " +
            "M9.7 12 A2.3 2.3 0 0 1 14.3 12 A2.3 2.3 0 0 1 9.7 12 Z M4 20 L20 4");

        /// <summary>잠금</summary>
        public static readonly Geometry Lock = P(
            "M6.5 10.5 H17.5 V20.5 H6.5 Z M9.2 10.5 V7.6 A2.8 2.8 0 0 1 14.8 7.6 V10.5");

        /// <summary>세션 시간</summary>
        public static readonly Geometry Clock = P(
            "M4 12 A8 8 0 0 1 20 12 A8 8 0 0 1 4 12 Z M12 7.5 V12.4 L15.6 14.6");

        /// <summary>로그아웃</summary>
        public static readonly Geometry Logout = P(
            "M10 4.5 H4.5 V19.5 H10 M14.5 8.2 L18.5 12 L14.5 15.8 M8 12 H18.5");

        /// <summary>서버 주소</summary>
        public static readonly Geometry Server = P(
            "M3.5 5 H20.5 V9.6 H3.5 Z M3.5 14.4 H20.5 V19 H3.5 Z M7 7.3 H7.1 M7 16.7 H7.1");

        /// <summary>보안코드 표</summary>
        public static readonly Geometry Grid = P(
            "M4 4 H20 V20 H4 Z M4 9.3 H20 M4 14.7 H20 M9.3 4 V20 M14.7 4 V20");

        /// <summary>사용자</summary>
        public static readonly Geometry User = P(
            "M8 7 A4 4 0 0 1 16 7 A4 4 0 0 1 8 7 Z M4.5 20.5 C4.5 16.3 8 14 12 14 C16 14 19.5 16.3 19.5 20.5");

        /// <summary>만료 경고</summary>
        public static readonly Geometry Warning = P(
            "M12 4 L21.5 20.5 H2.5 Z M12 10 V15 M12 17.6 H12.1");

        /// <summary>첨부 이미지</summary>
        public static readonly Geometry Image = P(
            "M3.5 5.5 H20.5 V18.5 H3.5 Z M3.5 15.2 L9 10.5 L13.5 14.6 L16.4 12 L20.5 15.6 " +
            "M15.4 9.2 A1.3 1.3 0 0 1 18 9.2 A1.3 1.3 0 0 1 15.4 9.2 Z");

        /// <summary>비고/메모</summary>
        public static readonly Geometry Note = P(
            "M6 3.5 H14 L18.5 8 V20.5 H6 Z M14 3.5 V8 H18.5 M9 12.5 H15.5 M9 16 H13");
    }
}
