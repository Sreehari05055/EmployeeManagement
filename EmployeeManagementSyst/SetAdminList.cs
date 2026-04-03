using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EmployeeManagementSyst
{
    public partial class SetAdminList : Form
    {
       
        public SetAdminList()
        {
            InitializeComponent();

        }


        /// <summary>
        /// Event handler for cell click in DataGridView. Retrieves the selected employee's ID
        /// and opens the SetAdmin form to set the employee as an administrator.
        /// </summary>
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                // Get the current row
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];
                string employeeName = row.Cells["Employee Name"].Value.ToString();
                string code = row.Cells["Id"].Value.ToString();

                using (SqlConnection serverConnect = ServerConnection.GetOpenConnection())
                {

                    string qry = "SELECT Id FROM EmployeeDetails WHERE FullName = @fname OR Id = @id;";
                    SqlCommand mySqlCommand = new SqlCommand(qry, serverConnect);
                    mySqlCommand.Parameters.AddWithValue("@fname", employeeName);
                    mySqlCommand.Parameters.AddWithValue("@id", code);

                    object result = mySqlCommand.ExecuteScalar();

                    if (result != null)
                    {
                        string empId = result.ToString();

                        // Check if there are any existing admins
                        string countAdminsQuery = "SELECT COUNT(*) FROM EmployeeDetails WHERE UserRole = 'admin';";
                        using (SqlCommand countCmd = new SqlCommand(countAdminsQuery, serverConnect))
                        {
                            object countResult = countCmd.ExecuteScalar();
                            int adminCount = countResult != null ? Convert.ToInt32(countResult) : 0;

                            if (adminCount == 0)
                            {
                                // No admins exist, directly promote the employee
                                string employeeNameResolved = EmployeeHelper.GetNameById(empId) ?? empId;
                                var confirmMsg = $"No admins exist. This will grant admin privileges to {employeeNameResolved} (ID: {empId}). Do you want to continue?";
                                var confirm = MessageBox.Show(confirmMsg, "Confirm Promotion", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                                if (confirm != DialogResult.Yes)
                                {
                                    // User cancelled the promotion
                                    serverConnect.Close();
                                    return;
                                }

                                var success = PromoteToAdmin(empId, empId, employeeNameResolved);
                                if (success)
                                {
                                    this.Close();
                                }
                            }
                            else
                            {
                                // Admins exist, require verification
                                // Reuse the existing AdminVerification dialog to verify the acting admin.
                                AdminVerification verify = new AdminVerification();
                                verify.ReturnDialogResultOnSuccess = true;
                                var dr = verify.ShowDialog();
                                if (dr == DialogResult.OK)
                                {
                                    // After successful verification, confirm the action with the acting admin.
                                    string employeeNameResolved = EmployeeHelper.GetNameById(empId) ?? empId;
                                    var confirmMsg = $"This action will grant admin privileges to {employeeNameResolved} (ID: {empId}) and will notify all existing admins. Do you want to continue?";
                                    var confirm = MessageBox.Show(confirmMsg, "Confirm Promotion", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                                    if (confirm != DialogResult.Yes)
                                    {
                                        // User cancelled the promotion
                                        serverConnect.Close();
                                        return;
                                    }

                                    // Verified and confirmed - perform the role update directly from the list
                                    var promotingAdminId = verify.VerifiedAdminId;
                                    var promotingAdminName = verify.VerifiedAdminName;
                                    var success = PromoteToAdmin(empId, promotingAdminId, promotingAdminName);
                                    if (success)
                                    {
                                        // Send notification emails to existing admins about the promotion
                                        // promotingAdminId/name were captured above
                                        var adminEmails = EmployeeHelper.GetAdminEmails();

                                        if (adminEmails != null && adminEmails.Length > 0)
                                        {
                                            var subject = "Employee Role Update Notification";
                                            var body = $"Employee {employeeNameResolved} (ID: {empId}) has been promoted to admin by {promotingAdminName} (ID: {promotingAdminId}).";
                                            var emailer = new EmailConfiguration();
                                            foreach (var admin in adminEmails)
                                            {
                                                try
                                                {
                                                    emailer.SendEmail(admin, subject, body);
                                                }
                                                catch (Exception ex)
                                                {
                                                    MessageBox.Show("Error sending admin notification to: " + admin + "\n" + ex.Message, "Email Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                                }
                                            }
                                        }

                                        this.Close();
                                    }
                                }
                            }
                        }
                    }
                    else { MessageBox.Show("Error Finding Employee ID"); }
                    serverConnect.Close();
                    this.Close();
                }
            }
        }

        /// <summary>
        /// Event handler for text change in the search TextBox. Filters employee data based on the entered text.
        /// </summary>
        private void Changing_Text(object sender, EventArgs s)
        {
            string userInput = textBox1.Text.Trim().ToLower();

            if (string.IsNullOrWhiteSpace(userInput))
            {

                LoadAllData();
                return;
            }
            try
            {
                DataTable dataTable = new DataTable();
                dataTable.Columns.Add("Employee Name", typeof(string));
                dataTable.Columns.Add("Id", typeof(string));


                using (SqlConnection serverConnect = ServerConnection.GetOpenConnection())
                {

                    string qry = "SELECT Id,FullName FROM EmployeeDetails WHERE UserRole = 'employee' AND (Surname = @surname OR Id = @id);";
                    SqlCommand mySqlCommand = new SqlCommand(qry, serverConnect);

                    mySqlCommand.Parameters.AddWithValue("@surname", userInput);
                    mySqlCommand.Parameters.AddWithValue("@id", userInput);
                    SqlDataReader reader = mySqlCommand.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            DataRow row = dataTable.NewRow();
                            row["Id"] = reader["Id"].ToString();
                            row["Employee Name"] = reader["FullName"].ToString();
                            dataTable.Rows.Add(row);
                        }
                        dataGridView1.DataSource = dataTable;
                    }
                    serverConnect.Close();
                }

            }

            catch (Exception ex) { MessageBox.Show("Employee Details Error: " + ex.Message); }
        }

        /// <summary>
        /// Method to load all employee data into the DataGridView.
        /// </summary>
        private void LoadAllData()
        {
            try
            {
                DataTable dataTable = new DataTable();
                dataTable.Columns.Add("Id", typeof(string));
                dataTable.Columns.Add("Employee Name", typeof(string));



                using (SqlConnection connection = ServerConnection.GetOpenConnection())
                {

                    string query = "SELECT Id,FullName FROM EmployeeDetails WHERE UserRole = 'employee';";
                    SqlCommand cmd = new SqlCommand(query, connection);
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            DataRow row = dataTable.NewRow();
                            row["Id"] = reader["Id"].ToString();
                            row["Employee Name"] = reader["FullName"].ToString();

                            dataTable.Rows.Add(row);
                        }
                        dataGridView1.DataSource = dataTable;

                    }

                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading data: " + ex.Message);
            }
        }
        /// <summary>
        /// Event handler for form load to populate employee data when the form is first opened.
        /// </summary>
        private void EmployeeDetailGrid_Load(object sender, EventArgs e)
        {
            LoadAllData();
        }

        /// <summary>
        /// Promote the specified employee id to admin role and return whether it succeeded.
        /// </summary>
        private bool PromoteToAdmin(string empId, string promotingAdminId, string promotingAdminName)
        {
            try
            {
                using (SqlConnection serverConnect = ServerConnection.GetOpenConnection())
                {
                    // Check current role first
                    string checkQuery = "SELECT UserRole FROM EmployeeDetails WHERE Id = @Id";
                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, serverConnect))
                    {
                        checkCmd.Parameters.AddWithValue("@Id", empId);
                        object roleObj = checkCmd.ExecuteScalar();
                        if (roleObj == null || roleObj == DBNull.Value)
                        {
                            MessageBox.Show("No employee found with the provided ID.");
                            return false;
                        }

                        string currentRole = roleObj.ToString();
                        if (string.Equals(currentRole, "admin", StringComparison.OrdinalIgnoreCase))
                        {
                            MessageBox.Show("Employee is already an admin.");
                            return false;
                        }
                    }

                    // Update role to admin
                    string qry = "UPDATE EmployeeDetails SET UserRole = 'admin' WHERE Id = @Id;";
                    using (SqlCommand mySqlCommand = new SqlCommand(qry, serverConnect))
                    {
                        mySqlCommand.Parameters.AddWithValue("@Id", empId);
                        int affected = mySqlCommand.ExecuteNonQuery();
                        if (affected > 0)
                        {
                            MessageBox.Show("Employee was made admin.");
                            return true;
                        }
                        else
                        {
                            MessageBox.Show("No matching employee found to update.");
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Set Admin Error: " + ex.Message);
                return false;
            }
        }
    }
}
