using System.Drawing;
using System.Windows.Forms;

namespace DMR
{
	/// <summary>
	/// Small picker for channel grid bulk-edit (Tier 2.8).
	/// </summary>
	public class ChannelsBulkPickForm : Form
	{
		private readonly ComboBox cmbValue;

		public string SelectedValue
		{
			get { return this.cmbValue.Text; }
		}

		public ChannelsBulkPickForm(string title, string prompt, string[] options, string defaultValue)
		{
			this.Text = title;
			this.FormBorderStyle = FormBorderStyle.FixedDialog;
			this.StartPosition = FormStartPosition.CenterParent;
			this.ClientSize = new Size(360, 120);
			this.Font = new Font("Segoe UI", 9.75f);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			Theme.ApplyForkDialog(this);

			Label lbl = new Label
			{
				Location = new Point(12, 12),
				Size = new Size(336, 36),
				Text = prompt
			};

			this.cmbValue = new ComboBox
			{
				Location = new Point(12, 52),
				Size = new Size(336, 23),
				DropDownStyle = ComboBoxStyle.DropDownList
			};
			this.cmbValue.Items.AddRange(options);
			if (!string.IsNullOrEmpty(defaultValue))
			{
				int idx = this.cmbValue.Items.IndexOf(defaultValue);
				this.cmbValue.SelectedIndex = idx >= 0 ? idx : 0;
			}
			else if (this.cmbValue.Items.Count > 0)
			{
				this.cmbValue.SelectedIndex = 0;
			}

			Button btnOk = new Button
			{
				Location = new Point(172, 84),
				Size = new Size(80, 28),
				Text = "OK",
				DialogResult = DialogResult.OK
			};
			Button btnCancel = new Button
			{
				Location = new Point(268, 84),
				Size = new Size(80, 28),
				Text = "Cancel",
				DialogResult = DialogResult.Cancel
			};
			this.AcceptButton = btnOk;
			this.CancelButton = btnCancel;

			this.Controls.Add(lbl);
			this.Controls.Add(this.cmbValue);
			this.Controls.Add(btnOk);
			this.Controls.Add(btnCancel);
		}
	}

}