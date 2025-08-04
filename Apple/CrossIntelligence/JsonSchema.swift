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
    var required: [String]?
}

@available(iOS 26.0, macOS 26.0, macCatalyst 26.0, visionOS 26.0, *)
fileprivate func readDynamicJsonSchema(_ schema: JsonSchema) -> DynamicGenerationSchema? {
    if let properties = schema.properties {
        let props = properties.compactMap { (key, value) -> DynamicGenerationSchema.Property? in
            if let nestedSchema = readDynamicJsonSchema(value) {
                return DynamicGenerationSchema.Property(name: key, schema: nestedSchema)
            }
            else {
                print("Unsupported JSON schema property: \(key) with type \(value.type)")
                return nil
            }
        }
        return DynamicGenerationSchema(name: schema.title ?? "Unnamed", properties: props)
    }
    else {
        // Primitive type
        switch schema.type {
        case "string":
            return DynamicGenerationSchema(type: String.self)
        case "integer":
            return DynamicGenerationSchema(type: Int.self)
        default:
            print("Unsupported JSON schema type: \(schema.type)")
            return nil
        }
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
    let dschema = parseDynamicJsonSchema(json)
    guard let dschema else {
        return nil
    }
    guard let schema = try? GenerationSchema(root: dschema, dependencies: []) else {
        print("Failed to create GenerationSchema from DynamicGenerationSchema: \(json)")
        return nil
    }
    return schema
}
