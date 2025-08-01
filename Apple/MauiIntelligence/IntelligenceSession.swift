//
//  IntelligenceSession.swift
//  MauiIntelligence
//
//  Created by Frank A. Krueger on 8/1/25.
//

import Foundation
import FoundationModels

@objc(TranscriptEntry)
public class TranscriptEntry : NSObject
{
    let text: String
    
    @objc
    public init(text: String) {
        self.text = text
    }
}

@objc(IntelligenceSessionDelegate)
public protocol IntelligenceSessionDelegate
{
}

@objc(IntelligenceSession)
public class IntelligenceSession : NSObject
{
    @objc
    public static var isAppleIntelligenceAvailable: Bool {
        if #available(iOS 26.0, macOS 26.0, *) {
            return SystemLanguageModel.default.isAvailable
        } else {
            return false
        }
    }

    @objc
    public override init() {
        super.init()
    }
    
    @objc
    public func respond(_ prompt: String, onComplete: @escaping (String, NSError?) -> Void) {
        Task.detached {
            if #available(iOS 26.0, macOS 26.0, *) {
                let session = LanguageModelSession()
                do {
                    let response = try await session.respond(to: prompt)
                    onComplete(response.content, nil)
                }
                catch {
                    let error = NSError(domain: "IntelligenceSessionError", code: 0, userInfo: [NSLocalizedDescriptionKey: "Failed to respond to prompt: \(error.localizedDescription)"])
                    onComplete("", error)
                }
            } else {
                let error = NSError(domain: "IntelligenceSessionError", code: 1, userInfo: [NSLocalizedDescriptionKey: "Apple Intelligence is not available on this version of iOS."])
                onComplete("", error)
            }
        }
    }
}
