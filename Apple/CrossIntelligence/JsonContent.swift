//
//  JsonContent.swift
//  CrossIntelligence
//
//  Created by Frank A. Krueger on 8/3/25.
//

import Foundation
import FoundationModels

@available(iOS 26.0, macOS 26.0, macCatalyst 26.0, visionOS 26.0, *)
extension GeneratedContent {
    var json: String {
        if let properties = try? properties() {
            var jsonDict = ["{"]
            var head = ""
            for (key, value) in properties {
                jsonDict.append(head)
                jsonDict.append("\"\(key)\":")
                jsonDict.append(value.json)
                head = ","
            }
            jsonDict.append("}")
            return jsonDict.joined()
        }
        else if let str = try? value(String.self) {
            if let jsonData = try? JSONSerialization.data(withJSONObject: str, options: .fragmentsAllowed),
               let json = String(data: jsonData, encoding: .utf8) {
                return json
            } else {
                print("Failed to serialize JSON data from string")
                return "\"\""
            }
        }
        else if let integer = try? value(Int.self) {
            if let jsonData = try? JSONSerialization.data(withJSONObject: integer, options: .fragmentsAllowed),
               let json = String(data: jsonData, encoding: .utf8) {
                return json
            } else {
                print("Failed to serialize JSON data from integer")
                return "0"
            }
        }
        print("Unable to generate JSON from \(self)")
        return "null"
    }
}
