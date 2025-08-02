#if __IOS__ || __MACOS__ || __MACCATALYST__

using System;
using Foundation;

namespace CrossIntelligence
{
	delegate void IntelligenceResponseHandler(string response, NSError error);

	[BaseType(typeof(NSObject))]
	interface AppleIntelligenceSessionNative
	{
		[Export("initWithInstructions:dotnetTools:")]
		[DesignatedInitializer]
		IntPtr Constructor(string instructions, NSObject[] tools);

		[Static]
		[Export("isAppleIntelligenceAvailable")]
		bool IsAppleIntelligenceAvailable { get; }

		[Export("respond:onComplete:")]
		[Async]
		void Respond(string input, IntelligenceResponseHandler onComplete);
	}

	[BaseType (typeof (NSObject))]
	[Model][Protocol]
	interface DotnetTool {
		[Abstract]
		[Export("toolName")]
		string ToolName { get; }

		[Abstract]
		[Export("toolDescription")]
		string ToolDescription { get; }

		[Abstract]
		[Export("execute:")]
		string Execute(string input);
	}
}

#endif
