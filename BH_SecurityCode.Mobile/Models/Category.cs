using BH_SecurityCode.Core;
using Microsoft.Maui.Controls.Shapes;

namespace BH_SecurityCode.Mobile.Models
{
    /// <summary>
    /// 카테고리별 표시 정보. 데스크톱 좌측 메뉴(<see cref="Menus"/> 101~105)를 모바일 타일로 옮긴 것.
    /// 색은 라이트/다크 공통이고 옅은 배경은 알파로 만든다.
    /// </summary>
    public sealed class CategoryMeta
    {
        public required Menus Id { get; init; }
        public required string Title { get; init; }
        public required string Caption { get; init; }

        /// <summary>대표 값의 이름 (목록 카드에서 가려서 보여주는 항목)</summary>
        public required string ValueLabel { get; init; }

        public required Color Accent { get; init; }
        public required Geometry Icon { get; init; }

        /// <summary>타일·아이콘 배경에 쓰는 옅은 색. 알파를 써서 다크 테마에서도 자연스럽게 깔린다.</summary>
        public Color Soft => Accent.WithAlpha(0.13f);

        public Color SoftStrong => Accent.WithAlpha(0.22f);
    }

    public static class Categories
    {
        public static readonly CategoryMeta Code = new()
        {
            Id = Menus.보안코드,
            Title = "보안코드",
            Caption = "보안카드 코드표",
            ValueLabel = "일련번호",
            Accent = Color.FromArgb("#4F5BD5"),
            Icon = Icons.CodeCard,
        };

        public static readonly CategoryMeta Book = new()
        {
            Id = Menus.통장,
            Title = "통장",
            Caption = "계좌·개설일",
            ValueLabel = "계좌번호",
            Accent = Color.FromArgb("#0E8F7E"),
            Icon = Icons.Book,
        };

        public static readonly CategoryMeta Card = new()
        {
            Id = Menus.카드,
            Title = "카드",
            Caption = "카드번호·유효기간",
            ValueLabel = "카드번호",
            Accent = Color.FromArgb("#B93B8F"),
            Icon = Icons.Card,
        };

        public static readonly CategoryMeta Id = new()
        {
            Id = Menus.신분증,
            Title = "신분증",
            Caption = "주민·면허·여권",
            ValueLabel = "번호",
            Accent = Color.FromArgb("#B77400"),
            Icon = Icons.IdCard,
        };

        public static readonly CategoryMeta Site = new()
        {
            Id = Menus.계정,
            Title = "계정",
            Caption = "사이트 아이디·비밀번호",
            ValueLabel = "아이디",
            Accent = Color.FromArgb("#2A6FDB"),
            Icon = Icons.Key,
        };

        public static readonly IReadOnlyList<CategoryMeta> All = new[] { Code, Book, Card, Id, Site };

        public static CategoryMeta Get(Menus menu) => menu switch
        {
            Menus.보안코드 => Code,
            Menus.통장 => Book,
            Menus.카드 => Card,
            Menus.신분증 => Id,
            Menus.계정 => Site,
            _ => Code,
        };
    }
}
