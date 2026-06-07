using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace DMR
{
#if OpenGD77
	public class ZoneBasicForm : DockContent, IDisp, ISingleRow
#else
	public class ZoneBasicForm : DockContent, IDisp
#endif
	{
		//private IContainer components;

		private CustomCombo cmbMainZone;
		private Label lblCurZone;
		private CustomCombo cmbSubCh;
		private Label lblSubCh;
		private CustomCombo cmbMainCh;
		private Label lblMainCh;
		private CustomCombo cmbSubZone;
		private Label lblSubZone;
		private CustomPanel pnlZoneBasic;
		private GroupBox grpSub;
		private GroupBox grpMain;
#if OpenGD77
		private DataGridView dgvZones;
		private TextBox txtZoneFilter;
		private Label lblZoneFilter;
		private Label lblZoneGridHint;
		private static readonly string[] ForkZoneHeaderText = { "#", "Name", "Ch", "1st channel", "Role" };
		private const int ForkZoneRoleColumnIndex = 4;
		private int forkSortColumn = -1;
		private bool forkSortAscending = true;
		private int forkActiveZoneDataIndex = -1;
		private int forkLastSelectionDataIndex = -1;
		private bool forkZoneClickHandled;
		private bool forkKeyboardNavPending;
		private bool forkActivatingRow;
#endif

		const int ZONE_NAME_LENGTH = 16;
		const int ZONES_IN_USE_DATA_LENGTH = 32;
#if OpenGD77
		const int NUM_CHANNELS_PER_ZONE	= 80;
		const int NUM_ZONES = 68;
#elif CP_VER_3_1_X
		const int NUM_CHANNELS_PER_ZONE	= 16;
		const int NUM_ZONES				= 250;
#endif
		const int UNKNOWN_VAR_OF_32 = NUM_CHANNELS_PER_ZONE + ZONE_NAME_LENGTH;

		public int MainZoneLastSelectedIndex
		{
			get;
			set;
		}

		public int SubZoneLastSelectedIndex
		{
			get;
			set;
		}

		public TreeNode Node
		{
			get;
			set;
		}

		public void SaveData()
		{
			if (ChannelForm.CurCntCh > 128)
			{
				ZoneForm.basicData.CurZone = this.cmbMainZone.method_3();
				ZoneForm.basicData.MainCh = this.cmbMainCh.method_3();
				ZoneForm.basicData.SubCh = this.cmbSubCh.method_3();
				ZoneForm.basicData.SubZone = this.cmbSubZone.method_3();
			}
			else
			{
				ZoneForm.basicData.MainCh = this.cmbMainCh.method_3();
				ZoneForm.basicData.SubCh = this.cmbSubCh.method_3();
			}
			((MainForm)base.MdiParent).RefreshRelatedForm(base.GetType());
		}

		public void DispData()
		{
			this.method_0();
			int num = 0;
			this.method_2(this.cmbMainZone);
			num = ZoneForm.basicData.CurZone;
			this.cmbMainZone.method_2(ZoneForm.basicData.CurZone);
			this.method_3(num);
			this.cmbMainCh.method_2(ZoneForm.basicData.MainCh);
			int num2 = 0;
			this.method_2(this.cmbSubZone);
			num2 = ZoneForm.basicData.SubZone;
			this.cmbSubZone.method_2(ZoneForm.basicData.SubZone);
			this.method_4(num2);
			this.cmbSubCh.method_2(ZoneForm.basicData.SubCh);
#if OpenGD77
			this.ForkDispGridData();
#endif
		}

		public void RefreshName()
		{
		}

		public ZoneBasicForm()
		{
			this.InitializeComponent();
			this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);// Roger Clark. Added correct icon on main form!
			base.Scale(Settings.smethod_6());
		}

		private void method_0()
		{
		}

		private void method_1()
		{
			this.method_2(this.cmbMainZone);
			this.method_2(this.cmbSubZone);
		}

		private void method_2(CustomCombo class4_0)
		{
			int num = 0;
			class4_0.method_0();
			for (num = 0; num <= 250; num++)
			{
				if (ZoneForm.data.ZoneChIsValid(num))
				{
					class4_0.method_1(ZoneForm.data.GetName(num), num);
				}
			}
		}

		private void method_3(int int_0)
		{
			this.method_5(int_0, this.cmbMainCh);
		}

		private void method_4(int int_0)
		{
			this.method_5(int_0, this.cmbSubCh);
		}

		private void method_5(int int_0, CustomCombo class4_0)
		{
			int num = 0;
			int num2 = 0;
			class4_0.method_0();
			for (num = 0; num < NUM_CHANNELS_PER_ZONE; num++)
			{
				num2 = ZoneForm.data[int_0].ChList[num] - 1;
				if (num2 >= 0 && num2 < ChannelForm.CurCntCh && ChannelForm.data.DataIsValid(num2))
				{
					class4_0.method_1(ChannelForm.data.GetName(num2), num);
				}
			}
		}

		private void method_6()
		{
			int num = 0;
			this.cmbSubCh.method_0();
			for (num = 0; num < ChannelForm.CurCntCh; num++)
			{
				if (ChannelForm.data.DataIsValid(num))
				{
					this.cmbSubCh.method_1(ChannelForm.data.GetName(num), num + 1);
				}
			}
		}

		private void ZoneBasicForm_Load(object sender, EventArgs e)
		{
			Settings.smethod_59(base.Controls);
			Settings.smethod_68(this);
#if OpenGD77
			this.EnsureForkZoneGridUi();
			this.pnlZoneBasic.Resize += this.pnlZoneBasic_Resize;
			Theme.ApplyStandardEditorColors(this);
			if (base.ClientSize.Height < Theme.Dpi(420))
			{
				base.ClientSize = new Size(Math.Max(base.ClientSize.Width, Theme.Dpi(720)), Theme.Dpi(480));
			}
#endif
			this.DispData();
#if OpenGD77
			this.ApplyForkZoneBasicLayout();
			this.SyncActiveZoneHighlightFromEditor();
#endif
		}

		private void ZoneBasicForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			this.SaveData();
		}

		private void cmbMainZone_SelectedIndexChanged(object sender, EventArgs e)
		{
			int num = this.cmbMainZone.method_3();
			if (num != this.MainZoneLastSelectedIndex)
			{
				this.method_3(num);
				if (this.cmbMainCh.Items.Count > 0)
				{
					this.cmbMainCh.SelectedIndex = 0;
				}
#if OpenGD77
				this.ForkRefreshAllZoneRoles();
#endif
			}
		}

		private void cmbSubZone_SelectedIndexChanged(object sender, EventArgs e)
		{
			int num = this.cmbSubZone.method_3();
			if (num != this.SubZoneLastSelectedIndex)
			{
				this.method_4(num);
				if (this.cmbSubCh.Items.Count > 0)
				{
					this.cmbSubCh.SelectedIndex = 0;
				}
#if OpenGD77
				this.ForkRefreshAllZoneRoles();
#endif
			}
		}

		private void cmbMainZone_DropDown(object sender, EventArgs e)
		{
			this.MainZoneLastSelectedIndex = this.cmbMainZone.method_3();
		}

		private void cmbSubZone_DropDown(object sender, EventArgs e)
		{
			this.SubZoneLastSelectedIndex = this.cmbSubZone.method_3();
		}

		protected override void Dispose(bool disposing)
		{
            /*
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
             * */
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			this.pnlZoneBasic = new CustomPanel();
			this.grpSub = new GroupBox();
			this.cmbSubCh = new CustomCombo();
			this.lblSubCh = new Label();
			this.cmbSubZone = new CustomCombo();
			this.lblSubZone = new Label();
			this.grpMain = new GroupBox();
			this.cmbMainCh = new CustomCombo();
			this.lblMainCh = new Label();
			this.cmbMainZone = new CustomCombo();
			this.lblCurZone = new Label();
			this.pnlZoneBasic.SuspendLayout();
			this.grpSub.SuspendLayout();
			this.grpMain.SuspendLayout();
			base.SuspendLayout();
			this.pnlZoneBasic.AutoScroll = true;
			this.pnlZoneBasic.AutoSize = true;
			this.pnlZoneBasic.Controls.Add(this.grpSub);
			this.pnlZoneBasic.Controls.Add(this.grpMain);
			this.pnlZoneBasic.Dock = DockStyle.Fill;
			this.pnlZoneBasic.Location = new Point(0, 0);
			this.pnlZoneBasic.Name = "pnlZoneBasic";
			this.pnlZoneBasic.Size = new Size(600, 197);
			this.pnlZoneBasic.TabIndex = 0;

			this.grpSub.Controls.Add(this.cmbSubCh);
			this.grpSub.Controls.Add(this.lblSubCh);
			this.grpSub.Controls.Add(this.cmbSubZone);
			this.grpSub.Controls.Add(this.lblSubZone);
			this.grpSub.Location = new Point(300, 35);
			this.grpSub.Name = "grpSub";
			this.grpSub.Size = new Size(250, 129);
			this.grpSub.TabIndex = 7;
			this.grpSub.TabStop = false;
			this.grpSub.Text = "Down";
			this.cmbSubCh.DropDownStyle = ComboBoxStyle.DropDownList;
			this.cmbSubCh.FormattingEnabled = true;
			this.cmbSubCh.Location = new Point(98, 71);
			this.cmbSubCh.Name = "cmbSubCh";
			this.cmbSubCh.Size = new Size(120, 24);
			this.cmbSubCh.TabIndex = 5;
			this.lblSubCh.Location = new Point(19, 71);
			this.lblSubCh.Name = "lblSubCh";
			this.lblSubCh.Size = new Size(70, 24);
			this.lblSubCh.TabIndex = 4;
			this.lblSubCh.Text = "Channel";
			this.lblSubCh.TextAlign = ContentAlignment.MiddleRight;
			this.cmbSubZone.DropDownStyle = ComboBoxStyle.DropDownList;
			this.cmbSubZone.FormattingEnabled = true;
			this.cmbSubZone.Location = new Point(98, 37);
			this.cmbSubZone.Name = "cmbSubZone";
			this.cmbSubZone.Size = new Size(120, 24);
			this.cmbSubZone.TabIndex = 1;
			this.cmbSubZone.SelectedIndexChanged += this.cmbSubZone_SelectedIndexChanged;
			this.cmbSubZone.DropDown += this.cmbSubZone_DropDown;
			this.lblSubZone.Location = new Point(19, 37);
			this.lblSubZone.Name = "lblSubZone";
			this.lblSubZone.Size = new Size(70, 24);
			this.lblSubZone.TabIndex = 0;
			this.lblSubZone.Text = "Zone";
			this.lblSubZone.TextAlign = ContentAlignment.MiddleRight;
			this.grpMain.Controls.Add(this.cmbMainCh);
			this.grpMain.Controls.Add(this.lblMainCh);
			this.grpMain.Controls.Add(this.cmbMainZone);
			this.grpMain.Controls.Add(this.lblCurZone);
			this.grpMain.Location = new Point(25, 35);
			this.grpMain.Name = "grpMain";
			this.grpMain.Size = new Size(250, 129);
			this.grpMain.TabIndex = 6;
			this.grpMain.TabStop = false;
			this.grpMain.Text = "Up";
			this.cmbMainCh.DropDownStyle = ComboBoxStyle.DropDownList;
			this.cmbMainCh.FormattingEnabled = true;
			this.cmbMainCh.Location = new Point(98, 71);
			this.cmbMainCh.Name = "cmbMainCh";
			this.cmbMainCh.Size = new Size(120, 24);
			this.cmbMainCh.TabIndex = 3;
			this.lblMainCh.Location = new Point(19, 71);
			this.lblMainCh.Name = "lblMainCh";
			this.lblMainCh.Size = new Size(70, 24);
			this.lblMainCh.TabIndex = 2;
			this.lblMainCh.Text = "Channel";
			this.lblMainCh.TextAlign = ContentAlignment.MiddleRight;
			this.cmbMainZone.DropDownStyle = ComboBoxStyle.DropDownList;
			this.cmbMainZone.FormattingEnabled = true;
			this.cmbMainZone.Location = new Point(98, 37);
			this.cmbMainZone.Name = "cmbMainZone";
			this.cmbMainZone.Size = new Size(120, 24);
			this.cmbMainZone.TabIndex = 1;
			this.cmbMainZone.SelectedIndexChanged += this.cmbMainZone_SelectedIndexChanged;
			this.cmbMainZone.DropDown += this.cmbMainZone_DropDown;
			this.lblCurZone.Location = new Point(19, 37);
			this.lblCurZone.Name = "lblCurZone";
			this.lblCurZone.Size = new Size(70, 24);
			this.lblCurZone.TabIndex = 0;
			this.lblCurZone.Text = "Zone";
			this.lblCurZone.TextAlign = ContentAlignment.MiddleRight;
			base.AutoScaleDimensions = new SizeF(7f, 16f);
//			base.AutoScaleMode = AutoScaleMode.Font;
			base.ClientSize = new Size(600, 197);
			base.Controls.Add(this.pnlZoneBasic);
			this.Font = new Font("Arial", 10f, FontStyle.Regular);
			base.Name = "ZoneBasicForm";
			this.Text = "Zone";
			base.Load += this.ZoneBasicForm_Load;
			base.FormClosing += this.ZoneBasicForm_FormClosing;
			this.pnlZoneBasic.ResumeLayout(false);
			this.grpSub.ResumeLayout(false);
			this.grpMain.ResumeLayout(false);
			base.ResumeLayout(false);
			base.PerformLayout();
		}

#if OpenGD77
		public void RefreshSingleRow(int index)
		{
			if (this.dgvZones == null)
			{
				return;
			}
			foreach (DataGridViewRow item in (IEnumerable)this.dgvZones.Rows)
			{
				if (item.IsNewRow || item.Tag == null || (int)item.Tag != index)
				{
					continue;
				}
				item.Cells[1].Value = ZoneForm.data.GetName(index);
				item.Cells[2].Value = ZoneForm.data.GetZoneChCnt(index).ToString();
				item.Cells[3].Value = this.ForkGetZoneFirstChannelName(index);
				item.Cells[4].Value = ForkGridBadges.GetZoneRoleBadge(index);
				this.dgvZones.InvalidateRow(item.Index);
				return;
			}
		}

		private void EnsureForkZoneGridUi()
		{
			this.pnlZoneBasic.AutoSize = false;
			if (this.dgvZones == null)
			{
				this.dgvZones = new DataGridView();
				this.dgvZones.Name = "dgvZones";
				this.dgvZones.ReadOnly = true;
				this.dgvZones.AllowUserToAddRows = false;
				this.dgvZones.AllowUserToDeleteRows = false;
				this.dgvZones.AllowUserToResizeRows = false;
				this.dgvZones.AllowUserToOrderColumns = false;
				this.dgvZones.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
				this.dgvZones.MultiSelect = false;
				this.dgvZones.RowHeadersWidth = 50;
				this.dgvZones.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
				ForkGridBadges.EnableGridPolish(this.dgvZones);
				for (int i = 0; i < ForkZoneHeaderText.Length; i++)
				{
					DataGridViewTextBoxColumn col = new DataGridViewTextBoxColumn();
					col.HeaderText = ForkZoneHeaderText[i];
					col.SortMode = DataGridViewColumnSortMode.NotSortable;
					this.dgvZones.Columns.Add(col);
				}
				this.dgvZones.CellMouseDown += this.dgvZones_CellMouseDown;
				this.dgvZones.KeyDown += this.dgvZones_KeyDown;
				this.dgvZones.CellDoubleClick += this.dgvZones_CellDoubleClick;
				this.dgvZones.CellFormatting += this.dgvZones_CellFormatting;
				this.dgvZones.ColumnHeaderMouseClick += this.dgvZones_ColumnHeaderMouseClick;
				this.dgvZones.RowPostPaint += this.dgvZones_RowPostPaint;
				this.dgvZones.SelectionChanged += this.dgvZones_SelectionChanged;
				this.dgvZones.RowHeaderMouseClick += this.dgvZones_RowHeaderMouseClick;
				this.pnlZoneBasic.Controls.Add(this.dgvZones);
			}
			if (this.txtZoneFilter == null)
			{
				this.lblZoneFilter = new Label();
				this.lblZoneFilter.Text = "Filter:";
				this.lblZoneFilter.AutoSize = true;
				this.txtZoneFilter = new TextBox();
				this.txtZoneFilter.Size = new Size(180, 23);
				this.txtZoneFilter.TextChanged += this.txtZoneFilter_TextChanged;
				ForkFilterEscape.WireEscapeClear(this.txtZoneFilter);
				this.pnlZoneBasic.Controls.Add(this.lblZoneFilter);
				this.pnlZoneBasic.Controls.Add(this.txtZoneFilter);
			}
			if (this.lblZoneGridHint == null)
			{
				this.lblZoneGridHint = new Label();
				this.lblZoneGridHint.Text = "Click a row to open zone editor · Up/Dn = radio main/sub zone";
				this.lblZoneGridHint.AutoSize = false;
				this.lblZoneGridHint.Height = 18;
				this.lblZoneGridHint.ForeColor = System.Drawing.SystemColors.GrayText;
				this.pnlZoneBasic.Controls.Add(this.lblZoneGridHint);
			}
			this.grpMain.Font = Theme.UiFont;
			this.grpSub.Font = Theme.UiFont;
		}

		private void pnlZoneBasic_Resize(object sender, EventArgs e)
		{
			this.ApplyForkZoneBasicLayout();
		}

		private void ApplyForkZoneBasicLayout()
		{
			if (this.dgvZones == null)
			{
				return;
			}
			int pad = Theme.Dpi(12);
			int clientW = this.pnlZoneBasic.ClientSize.Width;
			int clientH = this.pnlZoneBasic.ClientSize.Height;
			int groupsTop = Theme.Dpi(8);
			int groupsH = Theme.Dpi(132);
			int availW = Math.Max(Theme.Dpi(220), (clientW - pad * 3) / 2);
			this.grpMain.SetBounds(pad, groupsTop, availW, groupsH);
			this.grpSub.SetBounds(pad + availW + pad, groupsTop, availW, groupsH);
			int filterY = groupsTop + groupsH + Theme.Dpi(8);
			if (this.lblZoneFilter != null)
			{
				this.lblZoneFilter.Location = new Point(pad, filterY + Theme.Dpi(2));
			}
			if (this.txtZoneFilter != null)
			{
				this.txtZoneFilter.Location = new Point(Theme.Dpi(56), filterY);
				this.txtZoneFilter.Width = Math.Max(Theme.Dpi(120), clientW - Theme.Dpi(68));
			}
			if (this.lblZoneGridHint != null)
			{
				this.lblZoneGridHint.Location = new Point(pad, filterY + Theme.Dpi(26));
				this.lblZoneGridHint.Width = Math.Max(Theme.Dpi(200), clientW - pad * 2);
			}
			int gridY = filterY + Theme.Dpi(48);
			int gridH = Math.Max(Theme.Dpi(160), clientH - gridY - pad);
			this.dgvZones.Location = new Point(pad, gridY);
			this.dgvZones.Size = new Size(Math.Max(Theme.Dpi(300), clientW - pad * 2), gridH);
			this.dgvZones.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			this.dgvZones.BringToFront();
		}

		private void ForkDispGridData()
		{
			if (this.dgvZones == null)
			{
				return;
			}
			try
			{
				this.dgvZones.Rows.Clear();
				for (int i = 0; i < ZoneForm.NUM_ZONES; i++)
				{
					if (ZoneForm.data.DataIsValid(i))
					{
						int index = this.dgvZones.Rows.Add(
							(i + 1).ToString(),
							ZoneForm.data.GetName(i),
							ZoneForm.data.GetZoneChCnt(i).ToString(),
							this.ForkGetZoneFirstChannelName(i),
							ForkGridBadges.GetZoneRoleBadge(i));
						this.dgvZones.Rows[index].Tag = i;
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
			int preserveActive = this.forkActiveZoneDataIndex;
			int preserveSelection = this.forkLastSelectionDataIndex;
			this.ApplyZoneGridFilter();
			if (preserveActive >= 0)
			{
				DataGridViewRow activeRow = this.ForkFindRowByDataIndex(preserveActive);
				if (activeRow != null)
				{
					this.forkLastSelectionDataIndex = preserveSelection >= 0 ? preserveSelection : preserveActive;
					this.ForkActivateGridRow(activeRow, 0, false);
					return;
				}
			}
			this.dgvZones.CurrentCell = null;
			this.forkLastSelectionDataIndex = -1;
		}

		private string ForkGetZoneFirstChannelName(int zoneIndex)
		{
			ushort[] chList = ZoneForm.data.ZoneList[zoneIndex].ChList;
			for (int i = 0; i < chList.Length; i++)
			{
				int ch = chList[i];
				if (ch == 0)
				{
					break;
				}
				int idx = ch - 1;
				if (ChannelForm.data.DataIsValid(idx))
				{
					return ChannelForm.data.GetName(idx);
				}
			}
			return "";
		}

		private void txtZoneFilter_TextChanged(object sender, EventArgs e)
		{
			this.ApplyZoneGridFilter();
		}

		private void ApplyZoneGridFilter()
		{
			if (this.dgvZones == null)
			{
				return;
			}
			string query = this.txtZoneFilter == null ? "" : this.txtZoneFilter.Text.Trim();
			int visible = 0;
			int total = 0;
			foreach (DataGridViewRow row in this.dgvZones.Rows)
			{
				if (row.IsNewRow)
				{
					continue;
				}
				total++;
				if (string.IsNullOrEmpty(query))
				{
					row.Visible = true;
					visible++;
					continue;
				}
				bool match = false;
				for (int c = 0; c < row.Cells.Count; c++)
				{
					string cell = Convert.ToString(row.Cells[c].Value);
					if (cell != null && cell.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
					{
						match = true;
						break;
					}
				}
				row.Visible = match;
				if (match)
				{
					visible++;
				}
			}
			if (this.lblZoneFilter != null)
			{
				this.lblZoneFilter.Text = string.IsNullOrEmpty(query)
					? "Filter:"
					: "Filter (" + visible + "/" + total + "):";
			}
		}

		private void dgvZones_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
		{
			if (e.ColumnIndex < 0)
			{
				return;
			}
			if (this.forkSortColumn == e.ColumnIndex)
			{
				this.forkSortAscending = !this.forkSortAscending;
			}
			else
			{
				this.forkSortColumn = e.ColumnIndex;
				this.forkSortAscending = true;
			}
			this.SortZonesGrid(e.ColumnIndex, this.forkSortAscending);
			this.RefreshZoneSortHeaderGlyphs();
		}

		private void RefreshZoneSortHeaderGlyphs()
		{
			for (int i = 0; i < this.dgvZones.Columns.Count && i < ForkZoneHeaderText.Length; i++)
			{
				string text = ForkZoneHeaderText[i];
				if (i == this.forkSortColumn)
				{
					text += this.forkSortAscending ? " \u25B2" : " \u25BC";
				}
				this.dgvZones.Columns[i].HeaderText = text;
			}
		}

		private void SortZonesGrid(int columnIndex, bool ascending)
		{
			List<DataGridViewRow> rows = new List<DataGridViewRow>();
			foreach (DataGridViewRow row in this.dgvZones.Rows)
			{
				if (!row.IsNewRow)
				{
					rows.Add(row);
				}
			}
			rows.Sort((a, b) => this.CompareZoneRows(a, b, columnIndex, ascending));
			List<object> tags = new List<object>();
			List<object[]> cellValues = new List<object[]>();
			foreach (DataGridViewRow row in rows)
			{
				tags.Add(row.Tag);
				object[] values = new object[row.Cells.Count];
				for (int i = 0; i < row.Cells.Count; i++)
				{
					values[i] = row.Cells[i].Value;
				}
				cellValues.Add(values);
			}
			this.dgvZones.Rows.Clear();
			for (int i = 0; i < cellValues.Count; i++)
			{
				int index = this.dgvZones.Rows.Add(cellValues[i]);
				this.dgvZones.Rows[index].Tag = tags[i];
			}
			this.ApplyZoneGridFilter();
			if (this.forkActiveZoneDataIndex >= 0)
			{
				foreach (DataGridViewRow row in this.dgvZones.Rows)
				{
					if (!row.IsNewRow && row.Tag != null && (int)row.Tag == this.forkActiveZoneDataIndex)
					{
						this.ForkActivateGridRow(row, 0, false);
						break;
					}
				}
			}
			else
			{
				this.dgvZones.CurrentCell = null;
			}
		}

		private int CompareZoneRows(DataGridViewRow a, DataGridViewRow b, int columnIndex, bool ascending)
		{
			string left = Convert.ToString(a.Cells[columnIndex].Value) ?? "";
			string right = Convert.ToString(b.Cells[columnIndex].Value) ?? "";
			int cmp;
			if (columnIndex == 0 || columnIndex == 2)
			{
				int leftNum;
				int rightNum;
				int.TryParse(left, out leftNum);
				int.TryParse(right, out rightNum);
				cmp = leftNum.CompareTo(rightNum);
			}
			else
			{
				cmp = string.Compare(left, right, StringComparison.OrdinalIgnoreCase);
			}
			return ascending ? cmp : -cmp;
		}

		private void dgvZones_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
		{
			if (e.RowIndex < 0 || e.ColumnIndex < 0)
			{
				return;
			}
			DataGridViewRow row = this.dgvZones.Rows[e.RowIndex];
			if (row.IsNewRow)
			{
				return;
			}
			if (e.ColumnIndex == ForkZoneRoleColumnIndex)
			{
				ForkGridBadges.ApplyZoneRoleStyle(e, Convert.ToString(e.Value));
			}
			bool isActive = row.Tag != null && (int)row.Tag == this.forkActiveZoneDataIndex;
			if (isActive)
			{
				e.CellStyle.BackColor = ForkGridBadges.ActiveRowBack;
				e.CellStyle.ForeColor = ForkGridBadges.ActiveRowFore;
				e.CellStyle.SelectionBackColor = ForkGridBadges.ActiveRowBack;
				e.CellStyle.SelectionForeColor = ForkGridBadges.ActiveRowFore;
			}
		}

		private void dgvZones_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
		{
			try
			{
				DataGridViewRow paintRow = this.dgvZones.Rows[e.RowIndex];
				if (!paintRow.IsNewRow)
				{
					bool isActive = paintRow.Tag != null && (int)paintRow.Tag == this.forkActiveZoneDataIndex;
					if (isActive)
					{
						Rectangle rowBounds = new Rectangle(
							e.RowBounds.Left, e.RowBounds.Top,
							this.dgvZones.Columns.GetColumnsWidth(DataGridViewElementStates.Visible) + this.dgvZones.RowHeadersWidth,
							e.RowBounds.Height - 1);
						using (Pen pen = new Pen(ForkGridBadges.ActiveRowBorder, 2f))
						{
							e.Graphics.DrawRectangle(pen, rowBounds);
						}
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}

		private void dgvZones_SelectionChanged(object sender, EventArgs e)
		{
			if (this.forkZoneClickHandled)
			{
				this.forkZoneClickHandled = false;
				return;
			}
			if (this.forkActivatingRow)
			{
				return;
			}
			DataGridViewRow row = this.dgvZones.CurrentRow;
			if (row == null || row.IsNewRow || !row.Selected || row.Tag == null)
			{
				return;
			}
			int dataIndex = (int)row.Tag;
			bool openEditor = this.forkKeyboardNavPending;
			this.forkKeyboardNavPending = false;
			if (openEditor)
			{
				this.forkLastSelectionDataIndex = dataIndex;
			}
			else if (this.forkLastSelectionDataIndex < 0)
			{
				this.forkLastSelectionDataIndex = dataIndex;
			}
			this.BeginInvoke(new Action(() => this.ForkActivateGridRowDeferred(dataIndex, openEditor)));
		}

		private void dgvZones_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down
				|| e.KeyCode == Keys.PageUp || e.KeyCode == Keys.PageDown
				|| e.KeyCode == Keys.Home || e.KeyCode == Keys.End)
			{
				this.forkKeyboardNavPending = true;
			}
		}

		private void ForkActivateGridRowDeferred(int dataIndex, bool openEditor)
		{
			if (this.IsDisposed || this.dgvZones == null || this.dgvZones.IsDisposed)
			{
				return;
			}
			DataGridViewRow liveRow = this.ForkFindRowByDataIndex(dataIndex);
			if (liveRow == null)
			{
				if (dataIndex == this.forkActiveZoneDataIndex)
				{
					this.dgvZones.Invalidate();
				}
				return;
			}
			if (openEditor)
			{
				MainForm mainForm = this.GetMainForm();
				if (mainForm != null && mainForm.GetOpenZoneEditorDataIndex() == dataIndex)
				{
					openEditor = false;
				}
			}
			this.ForkActivateGridRow(liveRow, 0, openEditor);
		}

		private void ForkRefreshAllZoneRoles()
		{
			if (this.dgvZones == null)
			{
				return;
			}
			foreach (DataGridViewRow row in this.dgvZones.Rows)
			{
				if (!row.IsNewRow && row.Tag != null)
				{
					row.Cells[ForkZoneRoleColumnIndex].Value = ForkGridBadges.GetZoneRoleBadge((int)row.Tag);
				}
			}
			this.dgvZones.Invalidate();
		}

		private DataGridViewRow ForkFindRowByDataIndex(int dataIndex)
		{
			foreach (DataGridViewRow row in this.dgvZones.Rows)
			{
				if (!row.IsNewRow && row.Tag != null && (int)row.Tag == dataIndex)
				{
					return row;
				}
			}
			return null;
		}

		private bool ForkEnsureLiveRow(ref DataGridViewRow row)
		{
			if (row == null || row.IsNewRow || row.Tag == null)
			{
				return false;
			}
			if (row.DataGridView == this.dgvZones)
			{
				return true;
			}
			row = this.ForkFindRowByDataIndex((int)row.Tag);
			return row != null;
		}

		private void dgvZones_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
		{
			if (e.Button != MouseButtons.Left || e.RowIndex < 0 || e.ColumnIndex < 0 || e.RowIndex >= this.dgvZones.Rows.Count)
			{
				return;
			}
			this.forkZoneClickHandled = true;
			this.ForkActivateGridRow(this.dgvZones.Rows[e.RowIndex], e.ColumnIndex, true);
		}

		private void dgvZones_RowHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
		{
			if (e.RowIndex < 0 || e.RowIndex >= this.dgvZones.Rows.Count)
			{
				return;
			}
			this.forkZoneClickHandled = true;
			this.ForkActivateGridRow(this.dgvZones.Rows[e.RowIndex], 0, true);
		}

		private void dgvZones_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
		{
			if (e.RowIndex >= 0 && e.RowIndex < this.dgvZones.Rows.Count)
			{
				this.OpenZoneEditorForRow(this.dgvZones.Rows[e.RowIndex]);
			}
		}

		private void ForkActivateGridRow(DataGridViewRow row, int columnIndex, bool openEditor)
		{
			if (!this.ForkEnsureLiveRow(ref row))
			{
				return;
			}
			if (this.forkActivatingRow)
			{
				return;
			}
			this.forkActivatingRow = true;
			try
			{
				this.forkActiveZoneDataIndex = (int)row.Tag;
				this.forkLastSelectionDataIndex = this.forkActiveZoneDataIndex;
				if (this.dgvZones.SelectedRows.Count != 1 || this.dgvZones.SelectedRows[0] != row)
				{
					this.dgvZones.ClearSelection();
					if (row.DataGridView == this.dgvZones)
					{
						row.Selected = true;
					}
				}
				int cellCol = columnIndex >= 0 && columnIndex < row.Cells.Count ? columnIndex : 0;
				if (row.DataGridView == this.dgvZones
					&& (this.dgvZones.CurrentCell == null || this.dgvZones.CurrentCell.OwningRow != row))
				{
					this.dgvZones.CurrentCell = row.Cells[cellCol];
				}
				this.ForkScrollRowIntoView(row.Index);
				this.dgvZones.Invalidate();
				if (openEditor)
				{
					this.OpenZoneEditorForRow(row);
				}
			}
			finally
			{
				this.forkActivatingRow = false;
			}
		}

		private void ForkScrollRowIntoView(int rowIndex)
		{
			if (rowIndex < 0 || rowIndex >= this.dgvZones.Rows.Count)
			{
				return;
			}
			int first = this.dgvZones.FirstDisplayedScrollingRowIndex;
			int visible = this.dgvZones.DisplayedRowCount(false);
			if (rowIndex < first || rowIndex >= first + visible)
			{
				this.dgvZones.FirstDisplayedScrollingRowIndex = rowIndex;
			}
		}

		private void SyncActiveZoneHighlightFromEditor()
		{
			MainForm mainForm = this.GetMainForm();
			if (mainForm == null)
			{
				return;
			}
			int openIndex = mainForm.GetOpenZoneEditorDataIndex();
			if (openIndex < 0)
			{
				return;
			}
			this.forkActiveZoneDataIndex = openIndex;
			foreach (DataGridViewRow row in this.dgvZones.Rows)
			{
				if (row.IsNewRow || row.Tag == null)
				{
					continue;
				}
				if ((int)row.Tag == openIndex)
				{
					this.ForkActivateGridRow(row, 0, false);
					return;
				}
			}
			this.dgvZones.Invalidate();
		}

		private void OpenZoneEditorForRow(DataGridViewRow row)
		{
			if (row == null || row.Tag == null)
			{
				return;
			}
			int dataIndex = (int)row.Tag;
			this.forkActiveZoneDataIndex = dataIndex;
			this.dgvZones.Invalidate();
			MainForm mainForm = this.GetMainForm();
			if (mainForm == null)
			{
				return;
			}
			mainForm.OpenZoneEditorByDataIndex(dataIndex);
		}

		private MainForm GetMainForm()
		{
			MainForm mainForm = base.MdiParent as MainForm;
			if (mainForm != null)
			{
				return mainForm;
			}
			for (Form parent = this.ParentForm; parent != null; parent = parent.ParentForm)
			{
				mainForm = parent as MainForm;
				if (mainForm != null)
				{
					return mainForm;
				}
			}
			foreach (Form openForm in Application.OpenForms)
			{
				mainForm = openForm as MainForm;
				if (mainForm != null)
				{
					return mainForm;
				}
			}
			return null;
		}
#endif
	}
}
