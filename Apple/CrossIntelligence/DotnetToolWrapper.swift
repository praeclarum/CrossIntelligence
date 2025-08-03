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
    var input: String
}

protocol DotnetToolWrapper {
    var tool: DotnetTool? { get set }
    var index: Int { get }
}

@available(iOS 26.0, macOS 26.0, macCatalyst 26.0, visionOS 26.0, *)
fileprivate func allocTool(dotnetTool: DotnetTool) -> any Tool & DotnetToolWrapper {
    for i in 0..<gTools.count {
        if gTools[i].tool == nil {
            gArgsSchemas[i] = DefaultDotnetArgs.generationSchema
            gTools[i].tool = dotnetTool
            return gTools[i]
        }
    }
    return gTools[gTools.count - 1] // Return the last tool if no free tool is found
}

@available(iOS 26.0, macOS 26.0, macCatalyst 26.0, visionOS 26.0, *)
fileprivate func freeTool(index: Int) {
    gTools[index].tool = nil
}

@objc(DotnetTool)
public protocol DotnetTool: Sendable {
    var toolName: String { get }
    var toolDescription: String { get }
    // var argsSchemaProperties: [String] { get }
    func execute(_ arguments: String, onDone: @escaping (String) -> Void)
}

@available(iOS 26.0, macOS 26.0, macCatalyst 26.0, visionOS 26.0, *)
func getTools(dotnetTools: [DotnetTool]) -> [any Tool & DotnetToolWrapper] {
    let tools = dotnetTools.map { allocTool(dotnetTool: $0) }
    return tools
}

@available(iOS 26.0, macOS 26.0, macCatalyst 26.0, visionOS 26.0, *)
func freeTools(tools: [any Tool & DotnetToolWrapper]) {
    for tool in tools {
        gTools[tool.index].tool = nil
    }
}
