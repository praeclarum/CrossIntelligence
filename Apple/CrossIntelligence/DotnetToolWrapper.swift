//
//  DotnetToolWrapper.swift
//  CrossIntelligence
//
//  Created by Frank A. Krueger on 8/1/25.
//

import Foundation
import FoundationModels

@available(iOS 26.0, macOS 26.0, macCatalyst 26.0, visionOS 26.0, *)
@Generable
struct DefaultDotnetArgs {
    
}

fileprivate let maxTools = 100

@available(iOS 26.0, macOS 26.0, macCatalyst 26.0, visionOS 26.0, *)
var argsSchemas: [GenerationSchema] = Array(repeating: DefaultDotnetArgs.generationSchema, count: maxTools)

protocol DotnetToolWrapper {
    var isFree: Bool { get set }
    var tool: DotnetTool? { get set }
}

@available(iOS 26.0, macOS 26.0, macCatalyst 26.0, visionOS 26.0, *)
struct DotnetToolWrapper0: Tool, DotnetToolWrapper {
    var tool: DotnetTool?
    var isFree: Bool = true
    var name: String { tool?.name ?? "" }
    var description: String { tool?.description ?? "" }
    struct Arguments: Generable {
        let content: GeneratedContent
        static var generationSchema: GenerationSchema { argsSchemas[0] }
        var generatedContent: GeneratedContent { content }
        init(_ content: GeneratedContent) throws {
            self.content = content
        }
    }
    func call(arguments: Arguments) async throws -> some PromptRepresentable {
        let argsJson = "{}"
        return tool?.call(arguments: argsJson)
    }
}

@available(iOS 26.0, macOS 26.0, macCatalyst 26.0, visionOS 26.0, *)
struct DotnetToolWrapper1: Tool, DotnetToolWrapper {
    var tool: DotnetTool?
    var isFree: Bool = true
    var name: String { tool?.name ?? "" }
    var description: String { tool?.description ?? "" }
    struct Arguments: Generable {
        let content: GeneratedContent
        static var generationSchema: GenerationSchema {
            argsSchemas[1]
        }
        init(_ content: GeneratedContent) throws {
            self.content = content
        }
        var generatedContent: GeneratedContent {
            content
        }
    }
    func call(arguments: Arguments) async throws -> some PromptRepresentable {
        let argsJson = "{}"
        return tool?.call(arguments: argsJson)
    }
}

@available(iOS 26.0, macOS 26.0, macCatalyst 26.0, visionOS 26.0, *)
var tools: [any Tool & DotnetToolWrapper] = [
    DotnetToolWrapper0(tool: nil),
    DotnetToolWrapper1(tool: nil),
]

@available(iOS 26.0, macOS 26.0, macCatalyst 26.0, visionOS 26.0, *)
fileprivate func allocTool(dotnetTool: DotnetTool) -> any Tool & DotnetToolWrapper {
    for i in 0..<tools.count {
        if tools[i].isFree {
            argsSchemas[i] = DefaultDotnetArgs.generationSchema
            tools[i].isFree = false
            tools[i].tool = dotnetTool
            return tools[i]
        }
    }
    return tools[tools.count - 1] // Return the last tool if no free tool is found
}

@available(iOS 26.0, macOS 26.0, macCatalyst 26.0, visionOS 26.0, *)
fileprivate func freeTool(index: Int) {
    tools[index].isFree = true
}

@objc(DotnetTool)
public protocol DotnetTool: Sendable {
    var name: String { get }
    var description: String { get }
    var argsSchemaProperties: [String] { get }
    func call(arguments: String) -> String
}

@available(iOS 26.0, macOS 26.0, macCatalyst 26.0, visionOS 26.0, *)
func getTools(dotnetTools: [DotnetTool]) -> [any Tool & DotnetToolWrapper] {
    let tools = dotnetTools.map { allocTool(dotnetTool: $0) }
    return tools
}
