using BH_SecurityCode.Api.Model.Response;
using BH_SecurityCode.Core;

namespace BH_SecurityCode.Mobile.Models
{
    /// <summary>
    /// 목록·검색 화면이 종류를 가리지 않고 한 가지 카드 템플릿으로 그릴 수 있게 만든 평면 항목.
    /// 서버 응답 모델(BH_SecurityCode.Api.Model.Response.*)을 그대로 쓰되,
    /// 화면에 필요한 형태로만 옮겨 담는다. 상세 화면은 다시 원본 모델을 조회해서 쓴다.
    /// </summary>
    public sealed class VaultItem
    {
        public required int Key { get; init; }
        public required Menus Category { get; init; }

        /// <summary>카드 제목 (보안코드명 · 통장명 · 카드명 · 신분증 별칭 · 사이트명)</summary>
        public required string Title { get; init; }

        /// <summary>발급/소속 (은행 · 카드사 · 신분증 종류 · 사이트 주소)</summary>
        public required string Subtitle { get; init; }

        public required string OwnerName { get; init; }

        /// <summary>소유자 코드(CdOwner 값). 필터 칩과 목록 정렬 기준. 이름순이 아니라 코드순으로 맞춘다.</summary>
        public required int OwnerCode { get; init; }

        /// <summary>대표 값. 민감하므로 기본은 가려서 보여준다.</summary>
        public required string PrimaryValue { get; init; }

        public string Note { get; init; } = "";

        /// <summary>만료 임박 / 기간만료 같은 상태 꼬리표. 없으면 빈 문자열.</summary>
        public string Flag { get; init; } = "";

        public string SearchText { get; init; } = "";

        public CategoryMeta Meta => Categories.Get(Category);

        // ── 변환 ────────────────────────────────────────────────────

        public static VaultItem From(BankCodeInfo x) => new()
        {
            Key = x.bank_code_num,
            Category = Menus.보안코드,
            Title = Text(x.name),
            Subtitle = $"{x.bank_type} · 코드 {x.code_qty}개",
            OwnerName = x.owner_type.ToString(),
            OwnerCode = (int)x.owner_type,
            PrimaryValue = Text(x.serial),
            Note = Text(x.note),
            SearchText = $"{x.name} {x.bank_type} {x.owner_type} {x.serial} {x.note}",
        };

        public static VaultItem From(BankBookInfo x) => new()
        {
            Key = x.bank_book_num,
            Category = Menus.통장,
            Title = Text(x.name),
            Subtitle = $"{x.bank_type}은행",
            OwnerName = x.owner_type.ToString(),
            OwnerCode = (int)x.owner_type,
            PrimaryValue = Text(x.book_no),
            Note = Text(x.note),
            Flag = x.end_date.HasValue && x.end_date.Value.Date <= DateTime.Today ? "만기" : "",
            SearchText = $"{x.name} {x.bank_type} {x.owner_type} {x.owner} {x.book_no} {x.note}",
        };

        public static VaultItem From(CardInfo x) => new()
        {
            Key = x.card_num,
            Category = Menus.카드,
            Title = Text(x.name),
            Subtitle = $"{x.card_type}카드 · {ExpireText(x.expire_date)}",
            OwnerName = x.owner.ToString(),
            OwnerCode = (int)x.owner,
            PrimaryValue = Text(x.card_no),
            Note = Text(x.note),
            Flag = IsExpired(x.expire_date) ? "기간만료" : "",
            SearchText = $"{x.name} {x.card_type} {x.owner} {x.owner_name} {x.card_no} {x.note}",
        };

        public static VaultItem From(IdCardInfo x) => new()
        {
            Key = x.id_num,
            Category = Menus.신분증,
            Title = Text(x.name),
            Subtitle = $"{x.id_type} · {Text(x.name_kor)}",
            OwnerName = x.owner.ToString(),
            OwnerCode = (int)x.owner,
            PrimaryValue = string.IsNullOrWhiteSpace(x.id_no) ? Text(x.license_no) : Text(x.id_no),
            Note = Text(x.note),
            Flag = ExpireFlag(x.expired_date),
            SearchText = $"{x.name} {x.id_type} {x.owner} {x.name_kor} {x.license_org} {x.note}",
        };

        public static VaultItem From(AccountInfo x) => new()
        {
            Key = x.account_num,
            Category = Menus.계정,
            Title = Text(x.name),
            Subtitle = string.IsNullOrWhiteSpace(x.address) ? "주소 없음" : x.address,
            OwnerName = x.owner.ToString(),
            OwnerCode = (int)x.owner,
            PrimaryValue = string.IsNullOrWhiteSpace(x.id) ? Text(x.items.FirstOrDefault()?.id) : x.id,
            Note = Text(x.note),
            SearchText = $"{x.name} {x.owner} {x.address} {x.id} {x.note}",
        };

        // ── 표시 도우미 ─────────────────────────────────────────────

        /// <summary>서버 응답의 문자열은 null 일 수 있다. (Api 모델이 non-nullable 로 선언돼 있어도)</summary>
        private static string Text(string? value) => value ?? "";

        /// <summary>yyyyMM → MM/YY</summary>
        public static string ExpireText(string? yyyyMM)
        {
            string value = yyyyMM ?? "";
            return value.Length == 6 ? $"{value.Substring(4, 2)}/{value.Substring(2, 2)}" : value;
        }

        public static bool IsExpired(string? yyyyMM)
        {
            string value = yyyyMM ?? "";
            return value.Length == 6
                && int.TryParse(value, out int ym)
                && ym < int.Parse(DateTime.Now.ToString("yyyyMM"));
        }

        private static string ExpireFlag(DateTime? expiredAt)
        {
            if (expiredAt.HasValue == false)
                return "";
            if (expiredAt.Value.Date < DateTime.Today)
                return "기간만료";
            if (expiredAt.Value.Date <= DateTime.Today.AddDays(90))
                return "갱신필요";
            return "";
        }
    }
}
