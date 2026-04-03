using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace EmployeeManagementSyst
{
    public partial class AddEmployeeForm : Form
    {
        private string code; //  Unique employee code
        private string surname; //  Employee's surname
        private string fullname; // Employee's full name

        public String SurName
        {
            get { return surname; }
            set { surname = value; }
        }
        public String FullName
        {
            get { return fullname; }
            set { fullname = value; }
        }
        public String Code
        {
            get { return code; }
            set { code = value; }
        }

        /// <summary>
        /// Initializes a new instance of the AddEmp form and sets its properties.
        /// </summary>
        public AddEmployeeForm()
        {
            InitializeComponent(); // Initializes the components of the form

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }
        private void button2_Click(object sender, EventArgs e)
        {
            this.Close(); // Closes the form when the cancel button is clicked
        }
        private void AddEmp_Load(object sender, EventArgs e)
        {
            // Event handler for form load (can be implemented if needed)
        }

        private void label10_Click(object sender, EventArgs e)
        {

        }
        /// <summary>
        /// Event handler for AddEmployee click event to collect employee data,
        /// generate a unique employee code, and insert details into the database.
        /// </summary>
        private void AddEmployee_Click(object sender, EventArgs e)
        {
            FullName = textBox1.Text.Trim().ToLower(); // Gets and formats the full name from input
            string ageInp = textBox2.Text; // Gets the age input
            string phoneInp = textBox3.Text; // Gets the phone number input
            string emailInp = textBox4.Text; // Gets the email input          
            string rateInp = textBox5.Text; // Gets the hourly rate input
            string accountNumInp = textBox6.Text; // Gets the account number input
            string sortCodeInp = textBox7.Text; // Gets the sort code input
            string cardNameInp = textBox8.Text; // Gets the account holder's name input
            GetSurname();


            var manager = new EmployeeManager();
            var (newId, newPin) = manager.CreateEmployee(
                fullName: FullName,
                age: ageInp,
                phoneNumber: phoneInp,
                email: emailInp,
                hourlyRate: rateInp,
                surname: SurName,
                userRole: "employee",
                hireDate: DateTime.Today,
                accountNumber: string.IsNullOrWhiteSpace(accountNumInp) ? null : accountNumInp,
                sortCode: string.IsNullOrWhiteSpace(sortCodeInp) ? null : sortCodeInp,
                holderName: string.IsNullOrWhiteSpace(cardNameInp) ? null : cardNameInp
            );

            if (!string.IsNullOrEmpty(newId))
            {
                if (!string.IsNullOrEmpty(newPin))
                {
                    new EmailConfiguration().SendEmail(emailInp, "Welcome to the Team", $"We are excited to have you on board, {FullName}!\n Your Clock PIN is: {newPin}");
                }
            }
            this.Close();
        }

        /// <summary>
        /// Extracts the surname from the full name.
        /// </summary>
        public void GetSurname()
        {
            try
            {
                string[] aray = FullName.Split();
                string surname = aray[^1];
                SurName = surname;
            }
            catch (Exception ex) { MessageBox.Show("Error (Surname Comprehension): " + ex.Message); }
        }

        private void label9_Click(object sender, EventArgs e)
        {

        }

        private void label12_Click(object sender, EventArgs e)
        {

        }

        // Employee creation is handled by EmployeeManager.CreateEmployee; helper methods removed.

    }
}
