using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace m039.BasicLocalization
{
    public static class ManagedReferenceUtils
    {
        public static readonly GUIContent Empty = new GUIContent("Empty");

        static readonly Dictionary<object, GUIContent> _sNameLookup = new Dictionary<object, GUIContent>();

        static readonly Dictionary<string, Type> _sTypeLookup = new Dictionary<string, Type>();

        public static Type GetType(string managedReferenceFullTypename)
        {
            if (string.IsNullOrEmpty(managedReferenceFullTypename))
                throw new ArgumentException("String can not be null or empty", nameof(managedReferenceFullTypename));

            if (_sTypeLookup.TryGetValue(managedReferenceFullTypename, out var type))
                return type;

            var typeNames = managedReferenceFullTypename.Split(' ');
            if (typeNames?.Length == 2)
                type = Type.GetType($"{typeNames[1]}, {typeNames[0]}");
            _sTypeLookup[managedReferenceFullTypename] = type;
            return type;
        }

        public static GUIContent GetDisplayName(string managedReferenceFullTypename)
        {
            if (string.IsNullOrEmpty(managedReferenceFullTypename))
                return Empty;

            if (_sNameLookup.TryGetValue(managedReferenceFullTypename, out var name))
                return name;

            var type = GetType(managedReferenceFullTypename);
            if (type == null)
            {
                Debug.LogWarning($"Could not resolve managed reference type {managedReferenceFullTypename}. A Display name could not be found.");
                name = new GUIContent(managedReferenceFullTypename);
                _sNameLookup[managedReferenceFullTypename] = name;
                return name;
            }

            name = GetDisplayName(type);
            _sNameLookup[managedReferenceFullTypename] = name;
            return name;
        }

        public static GUIContent GetDisplayName(Type type)
        {
            if (_sNameLookup.TryGetValue(type, out var name))
                return name;

            // Class name
            var className = ObjectNames.NicifyVariableName(type.Name);
            var guiContent = new GUIContent(className);
            _sNameLookup[type] = guiContent;
            return guiContent;
        }
    }
}
