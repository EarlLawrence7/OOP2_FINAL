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
    public partial class For_Home : Form
    {
        private string loggedInUsername;
        private const string connectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=C:\Users\bagui\OneDrive\Documents\MOVIES.accdb";

        public For_Home(string username)
        {
            InitializeComponent();
            loggedInUsername = username;
        }

        public For_Home()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {

        }

        private void btnProfile_Click(object sender, EventArgs e)
        {
            For_Profile for_profile = new For_Profile(loggedInUsername);
            for_profile.Show();
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

        private void btnLogOut_Click(object sender, EventArgs e)
        {
            MessageBox.Show("SUCCESSFULLY LOGGED OUT!");
            For_Login for_login = new For_Login();
            for_login.Show();
            this.Hide();
        }
    }
}
