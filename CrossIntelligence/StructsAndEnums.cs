namespace CrossIntelligence;

public enum AppleIntelligenceAvailability: System.Int32 {
    Available = 0,
    PlatformNotSupported = 1,
    PlatformVersionNotSupported = 2,
    NotEnabled = 3,
    DeviceNotEligible = 4,
    ModelNotReady = 5,
    NotAvailableForOtherReasons = 6
}
