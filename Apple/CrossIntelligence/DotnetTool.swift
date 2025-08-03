//
//  DotnetToolWrapper.swift
//  CrossIntelligence
//
//  Created by Frank A. Krueger on 8/1/25.
//

import Foundation
import FoundationModels

@objc(DotnetTool)
public protocol DotnetTool: Sendable {
    var toolName: String { get }
    var toolDescription: String { get }
    var argumentsJSONSchema: String { get }
    func execute(_ arguments: String, onDone: @escaping (String) -> Void)
}

protocol DotnetToolWrapper {
    var tool: DotnetTool? { get set }
    var index: Int { get }
}

@available(iOS 26.0, macOS 26.0, macCatalyst 26.0, visionOS 26.0, *)
@Generable
struct DefaultDotnetArgs {
    var input: String
}

@available(iOS 26.0, macOS 26.0, macCatalyst 26.0, visionOS 26.0, *)
fileprivate func allocTool(dotnetTool: DotnetTool) -> any Tool & DotnetToolWrapper {
    for i in 0..<gTools.count {
        if gTools[i].tool == nil {
            let argumentsJSONSchema = dotnetTool.argumentsJSONSchema
            print("Allocating tool \(i): \(dotnetTool.toolName) with schema: \(argumentsJSONSchema)")
            if let schema = parseJsonSchema(argumentsJSONSchema) {
                print("+ Got schema for tool \(i): \(schema)")
                gArgsSchemas[i] = schema
            } else {
                print("- Using default schema for tool \(i): \(dotnetTool.toolName)")
                gArgsSchemas[i] = DefaultDotnetArgs.generationSchema
            }
            gTools[i].tool = dotnetTool
            return gTools[i]
        }
    }
    return gTools[gTools.count - 1] // Return the last tool if no free tool is found
}

@available(iOS 26.0, macOS 26.0, macCatalyst 26.0, visionOS 26.0, *)
func allocTools(dotnetTools: [DotnetTool]) -> [any Tool & DotnetToolWrapper] {
    let tools = dotnetTools.map { allocTool(dotnetTool: $0) }
    return tools
}

@available(iOS 26.0, macOS 26.0, macCatalyst 26.0, visionOS 26.0, *)
fileprivate func freeTool(index: Int) {
    gTools[index].tool = nil
}

@available(iOS 26.0, macOS 26.0, macCatalyst 26.0, visionOS 26.0, *)
func freeTools(tools: [any Tool & DotnetToolWrapper]) {
    for tool in tools {
        gTools[tool.index].tool = nil
    }
}
