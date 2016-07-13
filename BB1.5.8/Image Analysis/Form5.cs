using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Image_Analysis
{
    public partial class Form5 : Form
    {
        Form1 mainform = new Form1();
        public Form5()
        {
            InitializeComponent();
        }
        private void Form5_Load(object sender, EventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (txtUser.Text.ToString().CompareTo("bme") == 0 & txtPass.Text.ToString().CompareTo("hajni") == 0)
            {
                MessageBox.Show("Login Sucessfull.Click OK to continue", "Login", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Hide();     //hide the login form
                mainform.Show(); // show the main form 
            }
            else
            {
                MessageBox.Show("Wrong Username/Password! \n Please try again!", "Login", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtPass.Text = ""; //delete writed password
                txtUser.Text = ""; //delete writed username
            }
        }

       
    
    
    
    }
}
