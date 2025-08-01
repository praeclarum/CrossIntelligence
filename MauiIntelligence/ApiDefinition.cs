using Foundation;

namespace MauiIntelligence
{
	delegate void IntelligenceResponseHandler(string response, NSError error);

	[BaseType(typeof(NSObject))]
	interface IntelligenceSession
	{
		[Static]
		[Export("isAppleIntelligenceAvailable")]
		bool IsAppleIntelligenceAvailable { get; }


		[Export("respond:onComplete:")]
		[Async]
		void Respond(string input, IntelligenceResponseHandler onComplete);
	}
}
