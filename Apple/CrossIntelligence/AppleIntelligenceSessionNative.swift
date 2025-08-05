//
//  IntelligenceSession.swift
//  CrossIntelligence
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

protocol IntelligenceSessionImplementation {
    func respond(to prompt: String) async throws -> String
    @available(iOS 26.0, macOS 26.0, macCatalyst 26.0, visionOS 26.0, *)
    func respond(to prompt: String, schema: GenerationSchema, includeSchemaInPrompt: Bool) async throws -> GeneratedContent
    func freeTools()
}


@available(iOS 26.0, macOS 26.0, macCatalyst 26.0, visionOS 26.0, *)
class AppleIntelligenceSession: IntelligenceSessionImplementation {
    private let session: LanguageModelSession
    private var tools: [any Tool & DotnetToolWrapper] = []
    init(dotnetTools: [DotnetTool], instructions: String?) {
        tools = allocDotnetTools(dotnetTools)
        session = LanguageModelSession(tools: tools, instructions: instructions)
    }
    func respond(to prompt: String) async throws -> String {
        let response = try await session.respond(to: prompt)
        return response.content
    }
    func respond(to prompt: String, schema: GenerationSchema, includeSchemaInPrompt: Bool) async throws -> GeneratedContent {
        let response = try await session.respond(to: prompt, schema: schema, includeSchemaInPrompt: includeSchemaInPrompt)
        return response.content
    }
    public func freeTools() {
        freeDotnetToolWrappers(tools)
        tools.removeAll()
    }
}

@objc(AppleIntelligenceSessionNative)
public class AppleIntelligenceSessionNative : NSObject
{
    private let implementation: IntelligenceSessionImplementation?
    
    @objc
    public init(instructions: String?, dotnetTools: [DotnetTool]) {
        if #available(iOS 26.0, macOS 26.0, macCatalyst 26.0, visionOS 26.0, *) {
            implementation = AppleIntelligenceSession(dotnetTools: dotnetTools, instructions: instructions)
        }
        else {
            implementation = nil
        }
        super.init()
    }
    
    @objc
    public static var isAppleIntelligenceAvailable: Bool {
        if #available(iOS 26.0, macOS 26.0, macCatalyst 26.0, visionOS 26.0, *) {
            return SystemLanguageModel.default.isAvailable
        } else {
            return false
        }
    }

    @objc
    public func respond(_ prompt: String, onComplete: @escaping (String, NSError?) -> Void) {
        if let implementation {
            Task.detached {
                do {
                    let response = try await implementation.respond(to: prompt)
                    onComplete(response, nil)
                }
                catch {
                    let error = NSError(domain: "AppleIntelligenceSession", code: 0, userInfo: [NSLocalizedDescriptionKey: "Failed to respond to prompt: \(error.localizedDescription)"])
                    onComplete("", error)
                }
            }
        } else {
            let error = NSError(domain: "AppleIntelligenceSession", code: 1, userInfo: [NSLocalizedDescriptionKey: "Apple Intelligence is not available."])
            onComplete("", error)
        }
    }
    
    @objc
    public func respond(_ prompt: String, jsonSchema: String, includeSchemaInPrompt: Bool, onComplete: @escaping (String, NSError?) -> Void) {
        if #available(iOS 26.0, macOS 26.0, macCatalyst 26.0, visionOS 26.0, *), let implementation {
            Task.detached {
                if let schema = parseJsonSchema(jsonSchema) {
                    do {
                        let responseObject = try await implementation.respond(to: prompt, schema: schema, includeSchemaInPrompt: includeSchemaInPrompt)
                        onComplete(responseObject.json, nil)
                    }
                    catch {
                        let error = NSError(domain: "AppleIntelligenceSession", code: 0, userInfo: [NSLocalizedDescriptionKey: "Failed to respond to prompt: \(error.localizedDescription)"])
                        onComplete("", error)
                    }
                }
                else {
                    let error = NSError(domain: "AppleIntelligenceSession", code: 0, userInfo: [NSLocalizedDescriptionKey: "Failed to parse schema"])
                    onComplete("", error)
                }
            }
        } else {
            let error = NSError(domain: "AppleIntelligenceSession", code: 1, userInfo: [NSLocalizedDescriptionKey: "Apple Intelligence is not available."])
            onComplete("", error)
        }
    }
    
    @objc
    public func freeTools() {
        implementation?.freeTools()
    }
}
