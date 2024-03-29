using System;
using System.Drawing;
using System.Windows.Forms;

namespace RP_Notify.LoginForm
{

    internal partial class LoginForm : Form
    {
        private readonly Label userNoticeHeader;
        private readonly Label userNotice;
        private readonly Label labelUsername;
        private readonly Label labelPassword;
        private readonly TextBox textBoxUsername;
        private readonly TextBox textBoxPassword;
        private readonly Button buttonLogin;


        internal event EventHandler<LoginInputEvent> LoginInputEventHandler = delegate { };

        public LoginForm()
        {
            InitializeComponent();

            // UserNotice header Label
            userNoticeHeader = new Label();
            userNoticeHeader.Text = "The RP_Notify applet does NOT save or log your password";
            userNoticeHeader.Font = new Font("Helvetica", 10, FontStyle.Bold);
            userNoticeHeader.Location = new Point(40, 30);
            userNoticeHeader.Size = new Size(420, 25);
            userNoticeHeader.BackColor = Color.Transparent;
            userNoticeHeader.ForeColor = Color.FromArgb(216, 161, 56);

            // UserNotice Label
            userNotice = new Label();
            userNotice.Text = @"The widget retains only the identical cookie that is stored by your browser upon logging into the official site. You have the option to erase all stored information through the menu in the system tray (App Settings/Delete app data)";
            userNotice.Font = new Font("Helvetica", 10);
            userNotice.Location = new Point(50, 65);
            userNotice.Size = new Size(400, 70);
            userNotice.BackColor = Color.Transparent;
            userNotice.ForeColor = Color.WhiteSmoke;

            // Username TextBox
            textBoxUsername = new TextBox();
            textBoxUsername.Font = new Font("Helvetica", 10, FontStyle.Bold);
            textBoxUsername.Location = new Point(190, 160);
            textBoxUsername.Size = new Size(120, 20);
            textBoxUsername.BackColor = Color.Gainsboro;
            textBoxUsername.Text = "Username";
            textBoxUsername.TextAlign = HorizontalAlignment.Center;
            textBoxUsername.ForeColor = Color.DarkGray;

            // Password TextBox
            textBoxPassword = new TextBox();
            textBoxPassword.Font = new Font("Helvetica", 10, FontStyle.Bold);
            textBoxPassword.Location = new Point(190, 190);
            textBoxPassword.Size = new Size(120, 20);
            textBoxPassword.BackColor = Color.Gainsboro;
            textBoxPassword.Text = "Password";
            textBoxPassword.TextAlign = HorizontalAlignment.Center;
            textBoxPassword.ForeColor = Color.DarkGray;

            // Login Button
            buttonLogin = new Button();
            buttonLogin.Size = new Size(80, 30);
            buttonLogin.Text = "Login";
            buttonLogin.Font = new Font("Helvetica", 10, FontStyle.Bold);
            buttonLogin.Location = new Point(210, 235);
            buttonLogin.BackColor = Color.Black;
            buttonLogin.ForeColor = Color.FromArgb(216, 161, 56);


            // Add controls to the form
            Controls.Add(new TextBox() { Location = new Point(1000, 1000) });      // Fake textbox to take promptfocus
            Controls.Add(userNoticeHeader);
            Controls.Add(userNotice);
            Controls.Add(labelUsername);
            Controls.Add(textBoxUsername);
            Controls.Add(labelPassword);
            Controls.Add(textBoxPassword);
            Controls.Add(buttonLogin);

            // Set the form's properties
            Text = "Login to RP_Notify";
            Size = new Size(500, 325);
            StartPosition = FormStartPosition.CenterScreen;

            Icon = Properties.Resources.RPIcon;
            BackgroundImage = Properties.Resources.RP_background;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;

            // Login button click event
            buttonLogin.Click += ButtonLogin_Click;
            textBoxUsername.GotFocus += TextBoxUsername_Gotfocus;
            textBoxUsername.Leave += TextBoxUsername_Leave;
            textBoxPassword.GotFocus += TextBoxPassword_Gotfocus;
            textBoxPassword.Leave += TextBoxPassword_Leave;
        }

        private void ButtonLogin_Click(object sender, EventArgs e)
        {
            string username = textBoxUsername.Text;
            string password = textBoxPassword.Text;

            Dispose();

            LoginInputEventHandler.Invoke(this, new LoginInputEvent(username, password));
        }

        private void TextBoxUsername_Gotfocus(object sender, EventArgs e)
        {
            if (textBoxUsername.Text == "Username")
            {
                textBoxUsername.Text = "";
                textBoxUsername.TextAlign = HorizontalAlignment.Left;
                textBoxUsername.ForeColor = Color.Black;
            }
        }

        private void TextBoxUsername_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBoxUsername.Text))
            {
                textBoxUsername.Text = "Username";
                textBoxUsername.TextAlign = HorizontalAlignment.Center;
                textBoxUsername.ForeColor = Color.DarkGray;
            }
        }

        private void TextBoxPassword_Gotfocus(object sender, EventArgs e)
        {
            if (textBoxPassword.Text == "Password")
            {
                textBoxPassword.Text = "";
                textBoxPassword.PasswordChar = '*';
                textBoxPassword.TextAlign = HorizontalAlignment.Left;
                textBoxPassword.ForeColor = Color.Black;
            }
        }

        private void TextBoxPassword_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBoxPassword.Text))
            {
                textBoxPassword.Text = "Password";
                textBoxPassword.TextAlign = HorizontalAlignment.Center;
                textBoxPassword.PasswordChar = '\0';
                textBoxPassword.ForeColor = Color.DarkGray;
            }
        }

    }
}