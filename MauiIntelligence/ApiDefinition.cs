#if __IOS__ || __MACOS__ || __MACCATALYST__

using System;
using Foundation;

namespace MauiIntelligence
{
	delegate void IntelligenceResponseHandler(string response, NSError error);

	[BaseType(typeof(NSObject))]
	interface AppleIntelligenceSessionNative
	{
		[Export("initWithInstructions:")]
		[DesignatedInitializer]
		IntPtr Constructor(string instructions);

		[Static]
		[Export("isAppleIntelligenceAvailable")]
		bool IsAppleIntelligenceAvailable { get; }

		[Export("respond:onComplete:")]
		[Async]
		void Respond(string input, IntelligenceResponseHandler onComplete);
	}
}

#endif
