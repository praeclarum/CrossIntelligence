//
//  JsonSchema.swift
//  CrossIntelligence
//
//  Created by Frank A. Krueger on 8/3/25.
//

import Foundation
import FoundationModels

fileprivate class JsonSchema: Codable {
    var type: String
    var title: String?
    var properties: [String: JsonSchema]?
    var items: JsonSchema?
    var required: [String]?
    
    init(type: String) {
        self.type = type
    }
}

@available(iOS 26.0, macOS 26.0, macCatalyst 26.0, visionOS 26.0, *)
fileprivate func readDynamicJsonSchema(_ schema: JsonSchema) -> DynamicGenerationSchema {
    switch schema.type {
    case "array":
        let items = schema.items ?? JsonSchema(type: "string")
        return DynamicGenerationSchema.init(arrayOf: readDynamicJsonSchema(items))
    case "integer":
        return DynamicGenerationSchema(type: Int.self)
    case "number":
        return DynamicGenerationSchema(type: Double.self)
    case "object":
        let properties = schema.properties ?? [:]
        let props = properties.compactMap { (key, value) -> DynamicGenerationSchema.Property? in
            return DynamicGenerationSchema.Property(name: key, schema: readDynamicJsonSchema(value))
        }
        return DynamicGenerationSchema(name: schema.title ?? "Unnamed", properties: props)
    case "string":
        return DynamicGenerationSchema(type: String.self)
    default:
        print("Unsupported JSON schema type: \"\(schema.type)\"")
        return DynamicGenerationSchema(type: String.self)
    }
}

@available(iOS 26.0, macOS 26.0, macCatalyst 26.0, visionOS 26.0, *)
fileprivate func parseDynamicJsonSchema(_ json: String) -> DynamicGenerationSchema? {
    let decoder = JSONDecoder()
    guard let data = json.data(using: .utf8),
          let schema = try? decoder.decode(JsonSchema.self, from: data) else {
        print("Failed to parse JSON Schema: \(json)")
        return nil
    }
    return readDynamicJsonSchema(schema)
}

@available(iOS 26.0, macOS 26.0, macCatalyst 26.0, visionOS 26.0, *)
func parseJsonSchema(_ json: String) -> GenerationSchema? {
    guard let dschema = parseDynamicJsonSchema(json) else {
        return nil
    }
    guard let schema = try? GenerationSchema(root: dschema, dependencies: []) else {
        print("Failed to create GenerationSchema from DynamicGenerationSchema: \(json)")
        return nil
    }
    return schema
}
