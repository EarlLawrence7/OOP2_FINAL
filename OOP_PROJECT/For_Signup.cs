using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OOP_PROJECT
{
    public partial class For_Signup : Form
    {
        private const string connectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=C:\Users\bagui\OneDrive\Documents\MOVIES.accdb";
        public For_Signup()
        {
            InitializeComponent();
            tbxPassWord.UseSystemPasswordChar = true;
            tbxConfirmPass.UseSystemPasswordChar = true;
            this.AcceptButton = btnSave;
        }

        private void For_Signup_Load(object sender, EventArgs e)
        {

        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            string username = tbxUserName.Text;
            string password = tbxPassWord.Text;
            string confirmPassword = tbxConfirmPass.Text;
            string firstName = tbxFirstName.Text;
            string lastName = tbxLastName.Text;
            string gender = Gender.Text;
            string phonenumber = tbxPhoneNum.Text;
            string city = tbxCity.Text;
            string emailaddress = tbxEmailAdd.Text;
            string homeaddress = tbxHomeAdd.Text;

            if (string.IsNullOrWhiteSpace(tbxUserName.Text) ||
           string.IsNullOrWhiteSpace(tbxPassWord.Text) ||
           string.IsNullOrWhiteSpace(tbxConfirmPass.Text) ||
           string.IsNullOrWhiteSpace(tbxFirstName.Text) ||
           string.IsNullOrWhiteSpace(tbxLastName.Text) ||
           string.IsNullOrWhiteSpace(Gender.Text) ||
           string.IsNullOrWhiteSpace(tbxPhoneNum.Text) ||
           string.IsNullOrWhiteSpace(tbxCity.Text) ||
           string.IsNullOrWhiteSpace(tbxEmailAdd.Text) ||
           string.IsNullOrWhiteSpace(tbxHomeAdd.Text))
            {
                MessageBox.Show("Please fill in all fields.");
                return;
            }

            if (phonenumber.Length != 11 || !phonenumber.All(char.IsDigit))
            {
                MessageBox.Show("Please enter a valid 11-digit phone number.");
                tbxPhoneNum.Clear();
                tbxPhoneNum.Focus();
                return;
            }

            if (password != confirmPassword)
            {
                MessageBox.Show("Passwords do not match. Please re-enter your password.");
                tbxPassWord.Clear();
                tbxConfirmPass.Clear();
                tbxPassWord.Focus();
                return;
            }

            if (IsUsernameTaken(username))
            {
                MessageBox.Show("Username already exists. Please choose a different username.");
                tbxUserName.Clear();
                tbxUserName.Focus();
                return;
            }

            if (CreateAccount(username, password, firstName, lastName, gender, phonenumber, city, emailaddress, homeaddress))
            {
                MessageBox.Show("Account created successfully!");
                For_Login for_Login = new For_Login();
                for_Login.Show();
                this.Hide();
            }
            else
            {
                MessageBox.Show("Failed to create account. Please try again later.");
            }
        }

        private bool IsUsernameTaken(string username)
        {
            using (OleDbConnection conn = new OleDbConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT COUNT(*) FROM Accounts WHERE Username = ?";
                using (OleDbCommand cmd = new OleDbCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count > 0;
                }
            }
        }

        private bool CreateAccount(string username, string password, string firstName, string lastName, string gender, string phonenumber, string city, string emailaddress, string homeaddress)
        {
            using (OleDbConnection conn = new OleDbConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "INSERT INTO Accounts (Username, [Password], FirstName, LastName, Gender, Phone_no, City, Email_address, Home_address, Status) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";
                    using (OleDbCommand cmd = new OleDbCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        cmd.Parameters.AddWithValue("@password", password);
                        cmd.Parameters.AddWithValue("@firstname", firstName);
                        cmd.Parameters.AddWithValue("@lastname", lastName);
                        cmd.Parameters.AddWithValue("@gender", gender);
                        cmd.Parameters.AddWithValue("@phonenumber", phonenumber);
                        cmd.Parameters.AddWithValue("@city", city);
                        cmd.Parameters.AddWithValue("@emailadd", emailaddress);
                        cmd.Parameters.AddWithValue("@homeadd", homeaddress);
                        cmd.Parameters.AddWithValue("@status", "User"); // Set the default status to "User"
                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
                catch (OleDbException ex)
                {
                    MessageBox.Show("Error creating account: " + ex.Message);
                    return false;
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you want to go back to Login?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                For_Login for_Login = new For_Login();
                for_Login.Show();
                this.Hide();
            }
        }

        private void btnShowHide_Click(object sender, EventArgs e)
        {
            bool showPassword = !tbxPassWord.UseSystemPasswordChar; // Invert the current state
            // Set the UseSystemPasswordChar property for both text boxes
            tbxPassWord.UseSystemPasswordChar = showPassword;
            tbxConfirmPass.UseSystemPasswordChar = showPassword;
        }
    }
}
