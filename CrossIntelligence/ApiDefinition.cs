#if __IOS__ || __MACOS__ || __MACCATALYST__

using System;
using Foundation;

namespace CrossIntelligence
{
	delegate void IntelligenceResponseHandler(string response, NSError error);

	[BaseType(typeof(NSObject))]
	interface AppleIntelligenceSessionNative
	{
		[Export("initWithInstructions:")]
		[DesignatedInitializer]
		IntPtr Constructor(string instructions, NSObject[] tools);

		[Static]
		[Export("isAppleIntelligenceAvailable")]
		bool IsAppleIntelligenceAvailable { get; }

		[Export("respond:onComplete:")]
		[Async]
		void Respond(string input, IntelligenceResponseHandler onComplete);
	}

	[Protocol]
	interface DotnetTool {
		[Abstract]
		[Export("name")]
		string Name { get; }

		[Abstract]
		[Export("description")]
		string Description { get; }

		[Abstract]
		[Export("execute:")]
		string Execute(string input);
	}
}

#endif
