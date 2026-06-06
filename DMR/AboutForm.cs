using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace DMR
{
	public class AboutForm : Form
	{
		// =====================================================================
		// IIMacGyverII fork version. INCREMENT THIS on every build that ships
		// inside a DMRModHooks release. Format: MAJOR.MINOR.PATCH.
		// See phonedmrapp/.github/copilot-instructions.md → OpenGD77 Fork section.
		// =====================================================================
		public const string FORK_VERSION = "1.5.4";
		public const string FORK_NAME    = "DMRModHooks / PriInterPhone fork";

		//private IContainer components;

		private Label lblVersion;

		private Label lblCompany;
		private Label lblForkInfo;
		private Label lblTranslationCredit;

		private Button btnClose;

		public AboutForm()
		{
			
			//base._002Ector();
			this.InitializeComponent();
			this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);// Roger Clark. Added correct icon on main form!
			base.Scale(Settings.smethod_6());
		}

		private void AboutForm_Load(object sender, EventArgs e)
		{
			Settings.smethod_68(this);
#if OpenGD77
			this.lblVersion.Text = "OpenGD77 CPS  \u2014  " + FORK_NAME + "  v" + FORK_VERSION;
#elif CP_VER_3_1_X
			this.lblVersion.Text = "GD-77 CPS 3.1.x Community Edition  \u2014  " + FORK_NAME + "  v" + FORK_VERSION;
#endif
			this.lblCompany.Text += "\n\nRoger VK3KYY/G4KYF\nColin G4EML\nJason VK7ZJA\nMike DL2MF";
			this.lblForkInfo.Text =
				"\u26A0  IIMacGyverII fork \u2014 patched for the PriInterPhone DMR radio\n" +
				"(Ulefone Armor 26 Ultra et al.) running the DMRModHooks LSPosed module.\n" +
				"DO NOT use this build to manage a stock Radioddity/Baofeng GD-77 \u2014\n" +
				"the relay, timeslot, and contact-ID round-trip fixes are specific to\n" +
				"the Android database and will corrupt a real GD-77 codeplug.";
			this.AddForkAboutLinks();
		}

		private void AddForkAboutLinks()
		{
			LinkLabel lnkCps = new LinkLabel();
			lnkCps.Text = "OpenGD77CPS-Mac source on GitHub";
			lnkCps.Location = new Point(15, 348);
			lnkCps.AutoSize = true;
			lnkCps.LinkClicked += (s, e) =>
			{
				e.Link.Visited = true;
				System.Diagnostics.Process.Start("https://github.com/IIMacGyverII/OpenGD77CPS-Mac");
			};
			LinkLabel lnkBuilds = new LinkLabel();
			lnkBuilds.Text = "phonedmrapp OpenGD77Fork builds & release notes";
			lnkBuilds.Location = new Point(15, 368);
			lnkBuilds.AutoSize = true;
			lnkBuilds.LinkClicked += (s, e) =>
			{
				e.Link.Visited = true;
				System.Diagnostics.Process.Start("https://github.com/IIMacGyverII/phonedmrapp/tree/main/OpenGD77Fork");
			};
			this.Controls.Add(lnkCps);
			this.Controls.Add(lnkBuilds);
			lnkCps.BringToFront();
			lnkBuilds.BringToFront();
		}

		private void btnClose_Click(object sender, EventArgs e)
		{
			base.Close();
		}

		protected override void Dispose(bool disposing)
		{
		/*	if (disposing && this.components != null)
			{
				this.components.Dispose();
			}*/
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			this.lblVersion = new System.Windows.Forms.Label();
			this.lblCompany = new System.Windows.Forms.Label();
			this.lblForkInfo = new System.Windows.Forms.Label();
			this.btnClose = new System.Windows.Forms.Button();
			this.lblTranslationCredit = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// lblVersion
			// 
			this.lblVersion.Location = new System.Drawing.Point(31, 20);
			this.lblVersion.Name = "lblVersion";
			this.lblVersion.Size = new System.Drawing.Size(351, 20);
			this.lblVersion.TabIndex = 0;
			this.lblVersion.Text = "v1.0.0";
			this.lblVersion.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// lblCompany
			// 
			this.lblCompany.AutoSize = false;
			this.lblCompany.Location = new System.Drawing.Point(29, 55);
			this.lblCompany.Name = "lblCompany";
			this.lblCompany.Size = new System.Drawing.Size(351, 120);
			this.lblCompany.TabIndex = 0;
			this.lblCompany.Text = "Company";
			this.lblCompany.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// btnClose
			// 
			this.btnClose.Location = new System.Drawing.Point(173, 400);
			this.btnClose.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.btnClose.Name = "btnClose";
			this.btnClose.Size = new System.Drawing.Size(64, 27);
			this.btnClose.TabIndex = 1;
			this.btnClose.Text = "OK";
			this.btnClose.UseVisualStyleBackColor = true;
			this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
			// 
			// lblTranslationCredit
			// 
			this.lblTranslationCredit.Location = new System.Drawing.Point(31, 370);
			this.lblTranslationCredit.Name = "lblTranslationCredit";
			this.lblTranslationCredit.Size = new System.Drawing.Size(351, 20);
			this.lblTranslationCredit.TabIndex = 0;
			this.lblTranslationCredit.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// lblForkInfo
			// 
			this.lblForkInfo.Location = new System.Drawing.Point(15, 185);
			this.lblForkInfo.Name = "lblForkInfo";
			this.lblForkInfo.Size = new System.Drawing.Size(380, 160);
			this.lblForkInfo.TabIndex = 0;
			this.lblForkInfo.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			this.lblForkInfo.Font = new System.Drawing.Font("Arial", 8.5F, System.Drawing.FontStyle.Bold);
			this.lblForkInfo.ForeColor = System.Drawing.Color.DarkRed;
			// 
			// AboutForm
			// 
			this.ClientSize = new System.Drawing.Size(409, 440);
			this.Controls.Add(this.btnClose);
			this.Controls.Add(this.lblCompany);
			this.Controls.Add(this.lblForkInfo);
			this.Controls.Add(this.lblTranslationCredit);
			this.Controls.Add(this.lblVersion);
			this.Font = new System.Drawing.Font("Arial", 10F);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.Name = "AboutForm";
			this.Text = "About \u2014 " + FORK_NAME + " v" + FORK_VERSION;
			this.Load += new System.EventHandler(this.AboutForm_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}
	}
}
