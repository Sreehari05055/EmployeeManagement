using Microsoft.Data.SqlClient;
using System;

namespace EmployeeManagementSyst
{
    /// <summary>
    /// Encapsulates employee creation and related operations so forms can reuse the same logic.
    /// </summary>
    public class EmployeeManager
    {
        /// <summary>
        /// Creates an employee record and optionally a card record in a single transaction.
        /// Returns the generated Id and ClockPin on success.
        /// </summary>
        public (string Id, string ClockPin) CreateEmployee(
            string fullName,
            string age,
            string phoneNumber,
            string email,
            string hourlyRate,
            string surname,
            string userRole = "employee",
            DateTime? hireDate = null,
            string accountNumber = null,
            string sortCode = null,
            string holderName = null)
        {
            hireDate ??= DateTime.Today;

            using (var conn = ServerConnection.GetOpenConnection())
            using (var tran = conn.BeginTransaction())
            {
                try
                {
                    string insertEmp = @"INSERT INTO EmployeeDetails(FullName,Age,PhoneNumber,Email,HourlyRate,Surname,UserRole,HireDate) OUTPUT INSERTED.Id, INSERTED.ClockPin VALUES (@fullname,@age,@phonenumber,@email,@hourlyrate,@surname,@userRole,@hireDate);";

                    using (var cmd = new SqlCommand(insertEmp, conn, tran))
                    {
                        cmd.Parameters.AddWithValue("@fullname", (object)fullName ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@age", (object)age ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@phonenumber", (object)phoneNumber ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@email", (object)email ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@hourlyrate", (object)hourlyRate ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@surname", (object)surname ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@userRole", (object)userRole ?? "employee");
                        cmd.Parameters.AddWithValue("@hireDate", hireDate.Value);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (!reader.Read()) throw new Exception("Failed to insert employee");
                            var newId = reader.GetString(reader.GetOrdinal("Id"));
                            var clockPin = reader.IsDBNull(reader.GetOrdinal("ClockPin")) ? null : reader.GetString(reader.GetOrdinal("ClockPin"));
                            reader.Close();

                            if (!string.IsNullOrEmpty(accountNumber))
                            {
                                string insertCard = "INSERT INTO CardInformation(EmployeeId,AccountNumber,SortCode,HolderName) VALUES (@id,@accountNum,@sortCode,@holder);";
                                using (var ccmd = new SqlCommand(insertCard, conn, tran))
                                {
                                    ccmd.Parameters.AddWithValue("@id", newId);
                                    ccmd.Parameters.AddWithValue("@accountNum", (object)accountNumber ?? DBNull.Value);
                                    ccmd.Parameters.AddWithValue("@sortCode", (object)sortCode ?? DBNull.Value);
                                    ccmd.Parameters.AddWithValue("@holder", (object)holderName ?? DBNull.Value);
                                    ccmd.ExecuteNonQuery();
                                }
                            }

                            tran.Commit();
                            return (newId, clockPin);
                        }
                    }
                }
                catch
                {
                    try { tran.Rollback(); } catch { }
                    throw;
                }
            }
        }
    }
}
