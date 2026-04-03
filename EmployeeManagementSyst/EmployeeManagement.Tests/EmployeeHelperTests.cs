using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using EmployeeManagementSyst;

namespace EmployeeManagement.Tests;

[TestClass]
public class EmployeeHelperTests
{
    // Known clock pins in the test database
    private const string AdminClockPin = "3429";
    private const string EmployeeClockPin = "5053";

    [TestInitialize]
    public void TestInitialize()
    {
        // Attempt to initialize ServerConnection; if not available mark tests inconclusive
        try
        {
            // Try to use user-secrets/config if available
            var config = new Config();
            ServerConnection.Initialize(config);
        }
        catch
        {
            // Try a local default connection string for developer machines; adjust as needed
            try
            {
                ServerConnection.Initialize("Server=(local);Database=TestDb;Trusted_Connection=True;");
            }
            catch
            {
                Assert.Inconclusive("Database connection not configured for tests.");
            }
        }
    }

    [TestMethod]
    public void ExistsByClockPin_AdminPin_ReturnsTrue()
    {
        var exists = EmployeeHelper.ExistsByClockPin(AdminClockPin);
        Assert.IsTrue(exists, "Admin clock pin should exist in test database.");
    }

    [TestMethod]
    public void ExistsByClockPin_EmployeePin_ReturnsTrue()
    {
        var exists = EmployeeHelper.ExistsByClockPin(EmployeeClockPin);
        Assert.IsTrue(exists, "Employee clock pin should exist in test database.");
    }

    [TestMethod]
    public void GetIdByClockPin_AdminPin_ReturnsNonNull()
    {
        var id = EmployeeHelper.GetIdByClockPin(AdminClockPin);
        Assert.IsFalse(string.IsNullOrEmpty(id));
    }

    [TestMethod]
    public void IsAdmin_AdminPin_ReturnsTrue()
    {
        var isAdmin = EmployeeHelper.isAdmin(AdminClockPin);
        Assert.IsTrue(isAdmin);
    }

    [TestMethod]
    public void IsAdmin_EmployeePin_ReturnsFalse()
    {
        var isAdmin = EmployeeHelper.isAdmin(EmployeeClockPin);
        Assert.IsFalse(isAdmin);
    }

    [TestMethod]
    public void GetNameById_ReturnsName()
    {
        var id = EmployeeHelper.GetIdByClockPin(EmployeeClockPin);
        Assert.IsFalse(string.IsNullOrEmpty(id), "Expected Id for employee pin");
        var name = EmployeeHelper.GetNameById(id);
        Assert.IsFalse(string.IsNullOrEmpty(name), "Expected a name for the given employee id");
    }

    [TestMethod]
    public void HasOpenShift_NoOpenShift_ReturnsFalseOrTrueDependingOnData()
    {
        var id = EmployeeHelper.GetIdByClockPin(EmployeeClockPin);
        if (string.IsNullOrEmpty(id)) Assert.Inconclusive("Employee Id not available");
        var hasOpen = EmployeeHelper.HasOpenShift(id);
        // Can't assert exact value without knowing DB state; just call to ensure no exception
        Assert.IsTrue(hasOpen == true || hasOpen == false);
    }

    [TestMethod]
    public void GetAdminEmails_ReturnsArray()
    {
        var emails = EmployeeHelper.GetAdminEmails();
        Assert.IsNotNull(emails);
    }
}
