namespace BH_SecurityCode.Messages
{
    /// <summary>로그인 성공</summary>
    public sealed record LoginSucceededMessage;

    /// <summary>사용자 활동 (자동 잠금 타이머 리셋)</summary>
    public sealed record UserActivityMessage;

    /// <summary>하단 상태바 텍스트 변경</summary>
    public sealed record StatusTextMessage(string Text);

    /// <summary>확인 없이 프로그램 종료 요청 (로그인 화면의 종료 버튼 등)</summary>
    public sealed record ExitRequestedMessage;
}
