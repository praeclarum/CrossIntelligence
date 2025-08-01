//
//  DotnetMauiIntelligence.swift
//  MauiIntelligence
//
//  Created by .NET MAUI team on 6/18/24.
//

import Foundation

@objc(DotnetMauiIntelligence)
public class DotnetMauiIntelligence : NSObject
{

    @objc
    public static func getString(myString: String) -> String {
        return myString  + " from swift!"
    }

}
