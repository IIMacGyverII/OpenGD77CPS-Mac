using System;
using System.Drawing;
using System.Windows.Forms;

namespace DMR
{
	/// <summary>
	/// Tier 2.4 — lightweight busy overlay during Path B batch import/export.
	/// </summary>
	public sealed class AndroidBusyForm : Form
	{
		private readonly Label lblMessage;
		private readonly ProgressBar progress;

		public AndroidBusyForm(IWin32Window owner, string message)
		{
			this.FormBorderStyle = FormBorderStyle.FixedDialog;
			this.ControlBox = false;
			this.StartPosition = FormStartPosition.CenterParent;
			this.ShowInTaskbar = false;
			this.ClientSize = new Size(360, 88);
			this.Font = new Font("Segoe UI", 9.75f);
			Theme.ApplyForkDialog(this);

			this.lblMessage = new Label
			{
				Location = new Point(12, 12),
				Size = new Size(336, 20),
				Text = message
			};

			this.progress = new ProgressBar
			{
				Location = new Point(12, 40),
				Size = new Size(336, 22),
				Style = ProgressBarStyle.Marquee,
				MarqueeAnimationSpeed = 30
			};

			this.Controls.Add(this.lblMessage);
			this.Controls.Add(this.progress);

			if (owner is Control ownerControl)
			{
				this.Owner = ownerControl.FindForm();
			}
		}

		public void SetMessage(string message)
		{
			this.lblMessage.Text = message;
			Application.DoEvents();
		}

		public void ShowBusy()
		{
			base.Show();
			Application.DoEvents();
		}

	}

}