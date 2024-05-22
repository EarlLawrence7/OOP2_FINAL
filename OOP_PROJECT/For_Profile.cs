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
using static System.Windows.Forms.DataFormats;

namespace OOP_PROJECT
{
    public partial class For_Profile : Form
    {
        private string loggedInUsername;
        OleDbConnection con = new OleDbConnection();
        string dbProvider = "Provider=Microsoft.ACE.OLEDB.12.0;";
        string dbsource = @"Data Source=C:\Users\bagui\OneDrive\Documents\MOVIES.accdb";
        private const string connectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=C:\Users\bagui\OneDrive\Documents\MOVIES.accdb";
        public For_Profile(string username)
        {
            InitializeComponent();
            loggedInUsername = username;
            con.ConnectionString = dbProvider + dbsource; // Set the connection string
            LoadUserInfo(); // Load user information when the form is initialized
            tbxPassWord.UseSystemPasswordChar = true;


            LoadBookingHistory();
        }
        private void For_Profile_Load(object sender, EventArgs e)
        {
            // Enable editing for text boxes and combo boxes
            tbxFirstName.ReadOnly = false;
            tbxLastName.ReadOnly = false;
            comboBox1.Enabled = true;
            tbxEmailAdd.ReadOnly = false;
            tbxHomeAdd.ReadOnly = false;
            tbxCity.ReadOnly = false;
            tbxPhoneNum.ReadOnly = false;
        }

        private void btnHome_Click(object sender, EventArgs e)
        {
            For_Home for_home = new For_Home(loggedInUsername);
            for_home.Show();
            this.Hide();
        }

        private void btnBookTickets_Click(object sender, EventArgs e)
        {
            For_BookTickets for_bookTickets = new For_BookTickets(loggedInUsername);
            for_bookTickets.Show();
            this.Hide();
        }
        private void btnBrowse_Click(object sender, EventArgs e)
        {
            For_BrowseMovies for_browse = new For_BrowseMovies(loggedInUsername);
            for_browse.Show();
            this.Hide();
        }
        private void btnAbout_Click(object sender, EventArgs e)
        {

        }
        private void LoadUserInfo()
        {
            try
            {
                con.Open(); // Open the database connection
                string query = "SELECT * FROM Accounts WHERE Username = ?";
                OleDbCommand cmd = new OleDbCommand(query, con);
                cmd.Parameters.AddWithValue("@username", loggedInUsername);

                OleDbDataAdapter adapter = new OleDbDataAdapter(cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);

                if (dt.Rows.Count > 0)
                {
                    // Populate the profile fields with user information
                    tbxUserName.Text = dt.Rows[0]["Username"].ToString();
                    tbxPassWord.Text = dt.Rows[0]["Password"].ToString();
                    tbxUserID.Text = dt.Rows[0]["UserID"].ToString();
                    tbxFirstName.Text = dt.Rows[0]["FirstName"].ToString();
                    tbxLastName.Text = dt.Rows[0]["LastName"].ToString();
                    comboBox1.Text = dt.Rows[0]["Gender"].ToString();
                    tbxEmailAdd.Text = dt.Rows[0]["Email_address"].ToString();
                    tbxHomeAdd.Text = dt.Rows[0]["Home_address"].ToString();
                    tbxCity.Text = dt.Rows[0]["City"].ToString();
                    long phoneNumber = Convert.ToInt64(dt.Rows[0]["Phone_no"]);
                    tbxPhoneNum.Text = phoneNumber.ToString();
                    if (dt.Rows[0]["Profile_pic"] != DBNull.Value)
                    {
                        // Convert the byte array data from the database to an image
                        byte[] imageData = (byte[])dt.Rows[0]["Profile_pic"];
                        using (MemoryStream ms = new MemoryStream(imageData))
                        {
                            pictureBox2.Image = Image.FromStream(ms);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("User information not found.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message);
            }
            finally
            {
                con.Close(); // Close the database connection
            }
        }


        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (comboBox1.SelectedIndex == -1)
                {
                    MessageBox.Show("Please select a gender.");
                    return; // Exit the method
                }

                con.Open();
                string query = "UPDATE Accounts SET FirstName = ?, LastName = ?, Gender = ?, Email_address = ?, Home_address = ?, City = ?, Phone_no = ?, [Password] = ? WHERE Username = ?";

                OleDbCommand cmd = new OleDbCommand(query, con);
                cmd.Parameters.AddWithValue("@firstName", tbxFirstName.Text);
                cmd.Parameters.AddWithValue("@lastName", tbxLastName.Text);
                cmd.Parameters.AddWithValue("@gender", comboBox1.SelectedItem.ToString()); // Ensure comboBox1 has a selected item
                cmd.Parameters.AddWithValue("@email", tbxEmailAdd.Text);
                cmd.Parameters.AddWithValue("@homeAddress", tbxHomeAdd.Text);
                cmd.Parameters.AddWithValue("@city", tbxCity.Text);
                cmd.Parameters.AddWithValue("@phoneNumber", tbxPhoneNum.Text);
                cmd.Parameters.AddWithValue("@password", tbxPassWord.Text);
                cmd.Parameters.AddWithValue("@username", loggedInUsername);

                int rowsAffected = cmd.ExecuteNonQuery();
                if (rowsAffected > 0)
                {
                    MessageBox.Show("Information updated successfully!");
                }
                else
                {
                    MessageBox.Show("Failed to update information.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message);
            }
            finally
            {
                con.Close();
            }
        }


        private void btnShowHide_Click(object sender, EventArgs e)
        {
            tbxPassWord.UseSystemPasswordChar = !tbxPassWord.UseSystemPasswordChar;
        }

        private void btnLogOut_Click_1(object sender, EventArgs e)
        {
            MessageBox.Show("SUCCESSFULLY LOGGED OUT!");
            For_Login for_login = new For_Login();
            this.Hide();
            for_login.Show();

        }

        private void button1_Click(object sender, EventArgs e)
        {
            {
                // Prompt the user for confirmation
                DialogResult result = MessageBox.Show("Are you sure you want to delete your account, You can never retrieve it after you deleted it?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    // If user confirms deletion, proceed with deleting the account
                    if (DeleteAccount(loggedInUsername))
                    {
                        MessageBox.Show("Account deleted successfully!");

                        // Optionally, navigate back to the login form or close the application
                        For_Login forLogin = new For_Login();
                        forLogin.Show();
                        this.Close(); // Close the profile form
                    }
                    else
                    {
                        MessageBox.Show("Failed to delete account. Please try again later.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
        private bool DeleteAccount(string username)
        {
            using (OleDbConnection conn = new OleDbConnection(connectionString))
            {
                conn.Open();
                string query = "DELETE FROM Accounts WHERE Username = ?";

                using (OleDbCommand cmd = new OleDbCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
        }
        private void AddImagebtn_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string imagePath = openFileDialog.FileName;
                    // Display selected image in pictureBox2
                    pictureBox2.Image = Image.FromFile(imagePath);

                    // Convert the selected image to byte array
                    byte[] imageBytes = ImageToByteArray(pictureBox2.Image);

                    // Update profile picture in the database
                    UpdateProfilePicture(imageBytes);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }

        // Convert Image to byte array
        private byte[] ImageToByteArray(Image image)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                return ms.ToArray();
            }
        }

        // Update profile picture in the database
        private void UpdateProfilePicture(byte[] imageData)
        {
            try
            {
                con.Open();
                string query = "UPDATE Accounts SET Profile_pic = ? WHERE Username = ?";
                OleDbCommand cmd = new OleDbCommand(query, con);
                cmd.Parameters.AddWithValue("@profilePic", imageData);
                cmd.Parameters.AddWithValue("@username", loggedInUsername);
                int rowsAffected = cmd.ExecuteNonQuery();
                if (rowsAffected > 0)
                {
                    MessageBox.Show("Profile picture updated successfully!");
                }
                else
                {
                    MessageBox.Show("Failed to update profile picture.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message);
            }
            finally
            {
                con.Close();
            }
        }
        private void LoadBookingHistory()
        {
            try
            {
                con.Open(); // Open the database connection
                string query = "SELECT * FROM Booking_Records WHERE Username = ?";
                OleDbCommand cmd = new OleDbCommand(query, con);
                cmd.Parameters.AddWithValue("@username", loggedInUsername);

                OleDbDataAdapter adapter = new OleDbDataAdapter(cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);

                dgvBookHistory.DataSource = dt; // Bind the DataTable to the DataGridView
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message);
            }
            finally
            {
                con.Close(); // Close the database connection
            }
        }

        
    }
}