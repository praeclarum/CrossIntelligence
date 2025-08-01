using Foundation;

namespace MauiIntelligence
{
	// @interface DotnetMauiIntelligence : NSObject
	[BaseType (typeof(NSObject))]
	interface DotnetMauiIntelligence
	{
		// +(NSString * _Nonnull)getStringWithMyString:(NSString * _Nonnull)myString __attribute__((warn_unused_result("")));
		[Static]
		[Export ("getStringWithMyString:")]
		string GetString (string myString);
	}
}
