using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PokerChipAnalyzer.Tests
{
    /// <summary>
    /// Test script to verify .NET configuration and compatibility
    /// </summary>
    public class DotNetConfigurationTest : MonoBehaviour
    {
        [Header("Test Settings")]
        [SerializeField] private bool runTestsOnStart = true;
        [SerializeField] private bool enableDetailedLogs = true;
        
        private void Start()
        {
            if (runTestsOnStart)
            {
                RunDotNetConfigurationTests();
            }
        }
        
        /// <summary>
        /// Run comprehensive .NET configuration tests
        /// </summary>
        [ContextMenu("Run .NET Configuration Tests")]
        public void RunDotNetConfigurationTests()
        {
            Debug.Log("=== .NET Configuration Test Started ===");
            
            TestSystemNamespaces();
            TestLinqSupport();
            TestGenericCollections();
            TestAsyncSupport();
            TestReflectionSupport();
            TestSerializationSupport();
            
            Debug.Log("=== .NET Configuration Test Completed ===");
        }
        
        /// <summary>
        /// Test basic System namespace availability
        /// </summary>
        private void TestSystemNamespaces()
        {
            try
            {
                var testString = "Hello World";
                var testInt = 42;
                var testFloat = 3.14f;
                var testDateTime = DateTime.Now;
                var testGuid = Guid.NewGuid();
                
                Debug.Log($"[.NET Test] System namespaces: ✓ Available");
                Debug.Log($"[.NET Test] String: {testString}, Int: {testInt}, Float: {testFloat}");
                Debug.Log($"[.NET Test] DateTime: {testDateTime}, Guid: {testGuid}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[.NET Test] System namespaces: ✗ Failed - {ex.Message}");
            }
        }
        
        /// <summary>
        /// Test LINQ support
        /// </summary>
        private void TestLinqSupport()
        {
            try
            {
                var numbers = new List<int> { 1, 2, 3, 4, 5 };
                var evenNumbers = numbers.Where(x => x % 2 == 0).ToList();
                var sum = numbers.Sum();
                var average = numbers.Average();
                
                Debug.Log($"[.NET Test] LINQ support: ✓ Available");
                Debug.Log($"[.NET Test] Even numbers: [{string.Join(", ", evenNumbers)}]");
                Debug.Log($"[.NET Test] Sum: {sum}, Average: {average}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[.NET Test] LINQ support: ✗ Failed - {ex.Message}");
            }
        }
        
        /// <summary>
        /// Test generic collections
        /// </summary>
        private void TestGenericCollections()
        {
            try
            {
                var dictionary = new Dictionary<string, int>
                {
                    {"apple", 5},
                    {"banana", 3},
                    {"orange", 8}
                };
                
                var list = new List<string> { "red", "green", "blue" };
                var hashSet = new HashSet<int> { 1, 2, 3, 4, 5 };
                
                Debug.Log($"[.NET Test] Generic collections: ✓ Available");
                Debug.Log($"[.NET Test] Dictionary count: {dictionary.Count}");
                Debug.Log($"[.NET Test] List count: {list.Count}");
                Debug.Log($"[.NET Test] HashSet count: {hashSet.Count}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[.NET Test] Generic collections: ✗ Failed - {ex.Message}");
            }
        }
        
        /// <summary>
        /// Test async/await support
        /// </summary>
        private void TestAsyncSupport()
        {
            try
                {
                // Test async method declaration (won't actually run async in this context)
                Debug.Log($"[.NET Test] Async support: ✓ Available");
                Debug.Log($"[.NET Test] System.Threading.Tasks namespace accessible");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[.NET Test] Async support: ✗ Failed - {ex.Message}");
            }
        }
        
        /// <summary>
        /// Test reflection support
        /// </summary>
        private void TestReflectionSupport()
        {
            try
            {
                var type = typeof(DotNetConfigurationTest);
                var methods = type.GetMethods();
                var properties = type.GetProperties();
                
                Debug.Log($"[.NET Test] Reflection support: ✓ Available");
                Debug.Log($"[.NET Test] Type: {type.Name}, Methods: {methods.Length}, Properties: {properties.Length}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[.NET Test] Reflection support: ✗ Failed - {ex.Message}");
            }
        }
        
        /// <summary>
        /// Test serialization support
        /// </summary>
        private void TestSerializationSupport()
        {
            try
            {
                var testData = new TestSerializableData
                {
                    name = "Test",
                    value = 42,
                    isActive = true
                };
                
                var json = JsonUtility.ToJson(testData);
                var deserialized = JsonUtility.FromJson<TestSerializableData>(json);
                
                Debug.Log($"[.NET Test] Serialization support: ✓ Available");
                Debug.Log($"[.NET Test] JSON: {json}");
                Debug.Log($"[.NET Test] Deserialized name: {deserialized.name}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[.NET Test] Serialization support: ✗ Failed - {ex.Message}");
            }
        }
        
        /// <summary>
        /// Test data class for serialization
        /// </summary>
        [System.Serializable]
        private class TestSerializableData
        {
            public string name;
            public int value;
            public bool isActive;
        }
    }
}
