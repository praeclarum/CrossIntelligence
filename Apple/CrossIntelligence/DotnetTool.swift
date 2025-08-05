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
fileprivate func allocTool(dotnetTool: DotnetTool) -> (any Tool & DotnetToolWrapper)? {
    for i in 0..<gTools.count {
        if gTools[i].tool == nil {
            let argumentsJSONSchema = dotnetTool.argumentsJSONSchema
            if let schema = parseJsonSchema(argumentsJSONSchema) {
                gArgsSchemas[i] = schema
            } else {
                print("- Using default schema for tool \(i): \(dotnetTool.toolName)")
                gArgsSchemas[i] = DefaultDotnetArgs.generationSchema
            }
            gTools[i].tool = dotnetTool
            return gTools[i]
        }
    }
    print("No free tool slot found, \(dotnetTool.toolName) will not be available.")
    return nil
}

@available(iOS 26.0, macOS 26.0, macCatalyst 26.0, visionOS 26.0, *)
func allocDotnetTools(_ dotnetTools: [DotnetTool]) -> [any Tool & DotnetToolWrapper] {
    let tools = dotnetTools.compactMap { allocTool(dotnetTool: $0) }
    return tools
}

@available(iOS 26.0, macOS 26.0, macCatalyst 26.0, visionOS 26.0, *)
func freeDotnetToolWrappers(_ tools: [any Tool & DotnetToolWrapper]) {
    for tool in tools {
        gArgsSchemas[tool.index] = DefaultDotnetArgs.generationSchema
        gTools[tool.index].tool = nil
    }
}
