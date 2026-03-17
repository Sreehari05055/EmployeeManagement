using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using EmployeeManagementSyst;
using Microsoft.Data.SqlClient;

namespace EmployeeManagement.Tests
{
    [TestClass]
    public class EmployeeManagerTests
    {
        [TestMethod]
        public void CreateEmployee_ShouldReturnId_WhenValidDataProvided()
        {

            // Initialize ServerConnection using Config. If configuration isn't available (e.g. CI), mark test inconclusive.
            try
            {
                var config = new Config();
                ServerConnection.Initialize(config);
            }
            catch (Exception ex)
            {
                Assert.Inconclusive("ServerConnection could not be initialized: " + ex.Message);
            }

            var manager = new EmployeeManagementSyst.EmployeeManager();

            try
            {
                    // Act - deconstruct the returned tuple
                    var (id, clockPin) = manager.CreateEmployee(
                        fullName: "John Doe", age: "30", phoneNumber: "1234567890",
                        email: "john.doe@example.com", hourlyRate: "15.00", surname: "Doe",
                        userRole: "employee", hireDate: DateTime.Today,
                        cardNumber: "4111111111111111", expiryDate: "12/25", cvv: "123", holderName: "John Doe"
                    );

                    // Assert
                    Assert.IsFalse(string.IsNullOrEmpty(id), "Expected a non-empty Id to be returned after creating an employee.");
                }

                catch (Exception ex)
                {
                    // If the method depends on external resources (DB) this will surface here.
                    Assert.Fail($"CreateEmployee threw an unexpected exception: {ex.Message}");
                }
            
        }
    }
}
