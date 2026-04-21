// File: Program.cs
using System;
using System.Data.SqlClient;
using Newtonsoft.Json; // 1. Added the vulnerable library reference

namespace VulnerableApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Vulnerable .NET 5 App ===");

            // ❌ Hardcoded database credentials (CWE-798)
            string connectionString = "Server=localhost;Database=TestDB;User Id=sa;Password=P@ssw0rd123;";

            Console.Write("Enter some JSON data to process: ");
            string inputJson = Console.ReadLine();

            // ⚠️ REACHABLE FINDING: Calling the vulnerable library with untrusted input
            // Semgrep's reachability analysis sees this call and links it to the 
            // vulnerable version in your packages.lock.json.
            try 
            {
                var data = JsonConvert.DeserializeObject<dynamic>(inputJson);
                Console.WriteLine($"Processed JSON type: {data?.GetType()}");
            }
            catch (Exception ex) 
            {
                Console.WriteLine($"JSON Error: {ex.Message}");
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    Console.WriteLine("Connected to database successfully!");

                    // ❌ SQL Injection vulnerability (CWE-89)
                    Console.Write("Enter username: ");
                    string username = Console.ReadLine();

                    string query = $"SELECT * FROM Users WHERE Username = '{username}'";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Console.WriteLine($"User: {reader["Username"]}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
