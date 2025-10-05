using CrossIntelligence;

namespace Tests;

public class SessionTests
{
#if __MACOS__ || __IOS__ || __MACCATALYST__ || __TVOS__
    [Fact]
    public void AppleIntelligenceAvailable()
    {
        Assert.True(IntelligenceSession.IsAppleIntelligenceAvailable);
        Assert.Equal(AppleIntelligenceAvailability.Available, IntelligenceSession.AppleIntelligenceAvailability);
    }
#else
    [Fact]
    public void AppleIntelligenceNotAvailable()
    {
        Assert.False(IntelligenceSession.IsAppleIntelligenceAvailable);
        Assert.Equal(AppleIntelligenceAvailability.PlatformNotSupported, IntelligenceSession.AppleIntelligenceAvailability);
    }
#endif

    [Fact]
    public void SessionHasDefaultCtor()
    {
        var session = new IntelligenceSession();
        Assert.NotNull(session);
    }
}
