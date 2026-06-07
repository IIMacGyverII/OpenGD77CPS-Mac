#if OpenGD77
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace DMR
{
	public class ScanListsForm : DockContent, IDisp, ISingleRow
	{
		private Panel pnlScanLists;
		private DataGridView dgvScanLists;
		private TextBox txtScanListsFilter;
		private Label lblScanListsFilter;
		private Label lblScanListsHint;
		private static readonly string[] ForkScanListHeaderText = { "#", "Name", "Channels", "1st channel", "TB" };
		private const int ForkScanTalkbackColumnIndex = 4;
		private int forkSortColumn = -1;
		private bool forkSortAscending = true;
		private int forkActiveScanListDataIndex = -1;
		private int forkLastSelectionDataIndex = -1;
		private bool forkScanListClickHandled;
		private bool forkKeyboardNavPending;
		private bool forkActivatingRow;

		public TreeNode Node { get; set; }

		public ScanListsForm()
		{
			this.InitializeComponent();
			this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
			base.Scale(Settings.smethod_6());
		}

		public void SaveData()
		{
			if (this.dgvScanLists != null)
			{
				this.dgvScanLists.Focus();
			}
		}

		public void DispData()
		{
			if (this.dgvScanLists == null)
			{
				return;
			}
			try
			{
				this.dgvScanLists.Rows.Clear();
				for (int i = 0; i < NormalScanForm.data.Count; i++)
				{
					if (NormalScanForm.data.DataIsValid(i))
					{
						int index = this.dgvScanLists.Rows.Add(
							(i + 1).ToString(),
							NormalScanForm.data[i].Name,
							this.ForkGetScanChannelCount(i).ToString(),
							this.ForkGetFirstScanChannelName(i),
							NormalScanForm.data[i].Talkback ? "TB" : "");
						this.dgvScanLists.Rows[index].Tag = i;
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
			int preserveActive = this.forkActiveScanListDataIndex;
			int preserveSelection = this.forkLastSelectionDataIndex;
			this.ApplyScanListsFilter();
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
			this.dgvScanLists.CurrentCell = null;
			this.forkLastSelectionDataIndex = -1;
		}

		public void RefreshName()
		{
		}

		public void RefreshSingleRow(int index)
		{
			if (this.dgvScanLists == null)
			{
				return;
			}
			foreach (DataGridViewRow item in (IEnumerable)this.dgvScanLists.Rows)
			{
				if (item.IsNewRow || item.Tag == null || (int)item.Tag != index)
				{
					continue;
				}
				item.Cells[1].Value = NormalScanForm.data[index].Name;
				item.Cells[2].Value = this.ForkGetScanChannelCount(index).ToString();
				item.Cells[3].Value = this.ForkGetFirstScanChannelName(index);
				item.Cells[ForkScanTalkbackColumnIndex].Value = NormalScanForm.data[index].Talkback ? "TB" : "";
				this.dgvScanLists.InvalidateRow(item.Index);
				return;
			}
		}

		private void ScanListsForm_Load(object sender, EventArgs e)
		{
			this.EnsureForkScanListsGridUi();
			Settings.smethod_68(this);
			this.pnlScanLists.Resize += this.pnlScanLists_Resize;
			this.DispData();
			Theme.ApplyStandardEditorColors(this);
			this.ApplyForkScanListsLayout();
			this.SyncActiveScanListHighlightFromEditor();
		}

		private void InitializeComponent()
		{
			this.pnlScanLists = new Panel();
			this.pnlScanLists.Dock = DockStyle.Fill;
			this.pnlScanLists.Name = "pnlScanLists";
			base.ClientSize = new Size(720, 420);
			base.Controls.Add(this.pnlScanLists);
			base.Name = "ScanListsForm";
			base.Text = "Scan Lists";
			base.Load += this.ScanListsForm_Load;
		}

		private void EnsureForkScanListsGridUi()
		{
			this.pnlScanLists.AutoSize = false;
			if (this.dgvScanLists == null)
			{
				this.dgvScanLists = new DataGridView();
				this.dgvScanLists.Name = "dgvScanLists";
				this.dgvScanLists.ReadOnly = true;
				this.dgvScanLists.AllowUserToAddRows = false;
				this.dgvScanLists.AllowUserToDeleteRows = false;
				this.dgvScanLists.AllowUserToResizeRows = false;
				this.dgvScanLists.AllowUserToOrderColumns = false;
				this.dgvScanLists.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
				this.dgvScanLists.MultiSelect = false;
				this.dgvScanLists.RowHeadersWidth = 50;
				this.dgvScanLists.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
				ForkGridBadges.EnableGridPolish(this.dgvScanLists);
				for (int i = 0; i < ForkScanListHeaderText.Length; i++)
				{
					DataGridViewTextBoxColumn col = new DataGridViewTextBoxColumn();
					col.HeaderText = ForkScanListHeaderText[i];
					col.SortMode = DataGridViewColumnSortMode.NotSortable;
					this.dgvScanLists.Columns.Add(col);
				}
				this.dgvScanLists.CellMouseDown += this.dgvScanLists_CellMouseDown;
				this.dgvScanLists.KeyDown += this.dgvScanLists_KeyDown;
				this.dgvScanLists.CellDoubleClick += this.dgvScanLists_CellDoubleClick;
				this.dgvScanLists.CellFormatting += this.dgvScanLists_CellFormatting;
				this.dgvScanLists.ColumnHeaderMouseClick += this.dgvScanLists_ColumnHeaderMouseClick;
				this.dgvScanLists.RowPostPaint += this.dgvScanLists_RowPostPaint;
				this.dgvScanLists.SelectionChanged += this.dgvScanLists_SelectionChanged;
				this.dgvScanLists.RowHeaderMouseClick += this.dgvScanLists_RowHeaderMouseClick;
				this.pnlScanLists.Controls.Add(this.dgvScanLists);
			}
			if (this.txtScanListsFilter == null)
			{
				this.lblScanListsFilter = new Label();
				this.lblScanListsFilter.Text = "Filter:";
				this.lblScanListsFilter.AutoSize = true;
				this.txtScanListsFilter = new TextBox();
				this.txtScanListsFilter.Size = new Size(180, 23);
				this.txtScanListsFilter.TextChanged += this.txtScanListsFilter_TextChanged;
				ForkFilterEscape.WireEscapeClear(this.txtScanListsFilter);
				this.pnlScanLists.Controls.Add(this.lblScanListsFilter);
				this.pnlScanLists.Controls.Add(this.txtScanListsFilter);
			}
			if (this.lblScanListsHint == null)
			{
				this.lblScanListsHint = new Label();
				this.lblScanListsHint.Text = "Click a row to open scan list editor · TB = Talkback enabled";
				this.lblScanListsHint.AutoSize = false;
				this.lblScanListsHint.Height = 18;
				this.lblScanListsHint.ForeColor = SystemColors.GrayText;
				this.pnlScanLists.Controls.Add(this.lblScanListsHint);
			}
		}

		private void pnlScanLists_Resize(object sender, EventArgs e)
		{
			this.ApplyForkScanListsLayout();
		}

		private void ApplyForkScanListsLayout()
		{
			if (this.dgvScanLists == null)
			{
				return;
			}
			int pad = Theme.Dpi(12);
			int clientW = this.pnlScanLists.ClientSize.Width;
			int clientH = this.pnlScanLists.ClientSize.Height;
			int filterY = Theme.Dpi(8);
			if (this.lblScanListsFilter != null)
			{
				this.lblScanListsFilter.Location = new Point(pad, filterY + Theme.Dpi(2));
			}
			if (this.txtScanListsFilter != null)
			{
				this.txtScanListsFilter.Location = new Point(Theme.Dpi(56), filterY);
				this.txtScanListsFilter.Width = Math.Max(Theme.Dpi(120), clientW - Theme.Dpi(68));
			}
			if (this.lblScanListsHint != null)
			{
				this.lblScanListsHint.Location = new Point(pad, filterY + Theme.Dpi(26));
				this.lblScanListsHint.Width = Math.Max(Theme.Dpi(200), clientW - pad * 2);
			}
			int gridY = filterY + Theme.Dpi(48);
			int gridH = Math.Max(Theme.Dpi(200), clientH - gridY - pad);
			this.dgvScanLists.Location = new Point(pad, gridY);
			this.dgvScanLists.Size = new Size(Math.Max(Theme.Dpi(300), clientW - pad * 2), gridH);
			this.dgvScanLists.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
		}

		private int ForkGetScanChannelCount(int listIndex)
		{
			int cnt = 0;
			for (int i = 0; i < NormalScanForm.data[listIndex].ChList.Length; i++)
			{
				ushort ch = NormalScanForm.data[listIndex].ChList[i];
				if (ch > 1 && ch <= 1025)
				{
					cnt++;
				}
			}
			return cnt;
		}

		private string ForkGetFirstScanChannelName(int listIndex)
		{
			for (int i = 0; i < NormalScanForm.data[listIndex].ChList.Length; i++)
			{
				ushort chRef = NormalScanForm.data[listIndex].ChList[i];
				if (chRef > 1 && chRef <= 1025)
				{
					int chIndex = chRef - 2;
					if (ChannelForm.data.DataIsValid(chIndex))
					{
						return ChannelForm.data.GetName(chIndex);
					}
				}
			}
			return "";
		}

		private void txtScanListsFilter_TextChanged(object sender, EventArgs e)
		{
			this.ApplyScanListsFilter();
		}

		private void ApplyScanListsFilter()
		{
			if (this.dgvScanLists == null)
			{
				return;
			}
			string query = this.txtScanListsFilter == null ? "" : this.txtScanListsFilter.Text.Trim();
			int visible = 0;
			int total = 0;
			foreach (DataGridViewRow row in this.dgvScanLists.Rows)
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
			if (this.lblScanListsFilter != null)
			{
				this.lblScanListsFilter.Text = string.IsNullOrEmpty(query)
					? "Filter:"
					: "Filter (" + visible + "/" + total + "):";
			}
		}

		private void dgvScanLists_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
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
			this.SortScanListsGrid(e.ColumnIndex, this.forkSortAscending);
			this.RefreshScanListSortHeaderGlyphs();
		}

		private void RefreshScanListSortHeaderGlyphs()
		{
			for (int i = 0; i < this.dgvScanLists.Columns.Count && i < ForkScanListHeaderText.Length; i++)
			{
				string text = ForkScanListHeaderText[i];
				if (i == this.forkSortColumn)
				{
					text += this.forkSortAscending ? " \u25B2" : " \u25BC";
				}
				this.dgvScanLists.Columns[i].HeaderText = text;
			}
		}

		private void SortScanListsGrid(int columnIndex, bool ascending)
		{
			List<DataGridViewRow> rows = new List<DataGridViewRow>();
			foreach (DataGridViewRow row in this.dgvScanLists.Rows)
			{
				if (!row.IsNewRow)
				{
					rows.Add(row);
				}
			}
			rows.Sort((a, b) => this.CompareScanListRows(a, b, columnIndex, ascending));
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
			this.dgvScanLists.Rows.Clear();
			for (int i = 0; i < cellValues.Count; i++)
			{
				int index = this.dgvScanLists.Rows.Add(cellValues[i]);
				this.dgvScanLists.Rows[index].Tag = tags[i];
			}
			this.ApplyScanListsFilter();
			if (this.forkActiveScanListDataIndex >= 0)
			{
				foreach (DataGridViewRow row in this.dgvScanLists.Rows)
				{
					if (!row.IsNewRow && row.Tag != null && (int)row.Tag == this.forkActiveScanListDataIndex)
					{
						this.ForkActivateGridRow(row, 0, false);
						break;
					}
				}
			}
			else
			{
				this.dgvScanLists.CurrentCell = null;
			}
		}

		private int CompareScanListRows(DataGridViewRow a, DataGridViewRow b, int columnIndex, bool ascending)
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

		private void dgvScanLists_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
		{
			if (e.RowIndex < 0)
			{
				return;
			}
			DataGridViewRow row = this.dgvScanLists.Rows[e.RowIndex];
			if (row.IsNewRow)
			{
				return;
			}
			bool isActive = row.Tag != null && (int)row.Tag == this.forkActiveScanListDataIndex;
			if (isActive)
			{
				e.CellStyle.BackColor = ForkGridBadges.ActiveRowBack;
				e.CellStyle.ForeColor = ForkGridBadges.ActiveRowFore;
				e.CellStyle.SelectionBackColor = ForkGridBadges.ActiveRowBack;
				e.CellStyle.SelectionForeColor = ForkGridBadges.ActiveRowFore;
			}
			else if (e.ColumnIndex == ForkScanTalkbackColumnIndex && "TB".Equals(e.Value))
			{
				e.CellStyle.BackColor = Color.FromArgb(0xD6, 0xE8, 0xF7);
				e.CellStyle.ForeColor = Color.FromArgb(0x1A, 0x4A, 0x7A);
				e.CellStyle.Font = Theme.UiFontBold;
			}
		}

		private void dgvScanLists_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
		{
			try
			{
				DataGridViewRow paintRow = this.dgvScanLists.Rows[e.RowIndex];
				if (!paintRow.IsNewRow && paintRow.Tag != null && (int)paintRow.Tag == this.forkActiveScanListDataIndex)
				{
					Rectangle rowBounds = new Rectangle(
						e.RowBounds.Left, e.RowBounds.Top,
						this.dgvScanLists.Columns.GetColumnsWidth(DataGridViewElementStates.Visible) + this.dgvScanLists.RowHeadersWidth,
						e.RowBounds.Height - 1);
					using (Pen pen = new Pen(ForkGridBadges.ActiveRowBorder, 2f))
					{
						e.Graphics.DrawRectangle(pen, rowBounds);
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}

		private void dgvScanLists_SelectionChanged(object sender, EventArgs e)
		{
			if (this.forkScanListClickHandled)
			{
				this.forkScanListClickHandled = false;
				return;
			}
			if (this.forkActivatingRow)
			{
				return;
			}
			DataGridViewRow row = this.dgvScanLists.CurrentRow;
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

		private void dgvScanLists_KeyDown(object sender, KeyEventArgs e)
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
			if (this.IsDisposed || this.dgvScanLists == null || this.dgvScanLists.IsDisposed)
			{
				return;
			}
			DataGridViewRow liveRow = this.ForkFindRowByDataIndex(dataIndex);
			if (liveRow == null)
			{
				if (dataIndex == this.forkActiveScanListDataIndex)
				{
					this.dgvScanLists.Invalidate();
				}
				return;
			}
			if (openEditor)
			{
				MainForm mainForm = this.GetMainForm();
				if (mainForm != null && mainForm.GetOpenScanEditorDataIndex() == dataIndex)
				{
					openEditor = false;
				}
			}
			this.ForkActivateGridRow(liveRow, 0, openEditor);
		}

		private DataGridViewRow ForkFindRowByDataIndex(int dataIndex)
		{
			foreach (DataGridViewRow row in this.dgvScanLists.Rows)
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
			if (row.DataGridView == this.dgvScanLists)
			{
				return true;
			}
			row = this.ForkFindRowByDataIndex((int)row.Tag);
			return row != null;
		}

		private void dgvScanLists_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
		{
			if (e.Button != MouseButtons.Left || e.RowIndex < 0 || e.ColumnIndex < 0 || e.RowIndex >= this.dgvScanLists.Rows.Count)
			{
				return;
			}
			this.forkScanListClickHandled = true;
			this.ForkActivateGridRow(this.dgvScanLists.Rows[e.RowIndex], e.ColumnIndex, true);
		}

		private void dgvScanLists_RowHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
		{
			if (e.RowIndex < 0 || e.RowIndex >= this.dgvScanLists.Rows.Count)
			{
				return;
			}
			this.forkScanListClickHandled = true;
			this.ForkActivateGridRow(this.dgvScanLists.Rows[e.RowIndex], 0, true);
		}

		private void dgvScanLists_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
		{
			if (e.RowIndex >= 0 && e.RowIndex < this.dgvScanLists.Rows.Count)
			{
				this.OpenScanListEditorForRow(this.dgvScanLists.Rows[e.RowIndex]);
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
				this.forkActiveScanListDataIndex = (int)row.Tag;
				this.forkLastSelectionDataIndex = this.forkActiveScanListDataIndex;
				if (this.dgvScanLists.SelectedRows.Count != 1 || this.dgvScanLists.SelectedRows[0] != row)
				{
					this.dgvScanLists.ClearSelection();
					if (row.DataGridView == this.dgvScanLists)
					{
						row.Selected = true;
					}
				}
				int cellCol = columnIndex >= 0 && columnIndex < row.Cells.Count ? columnIndex : 0;
				if (row.DataGridView == this.dgvScanLists
					&& (this.dgvScanLists.CurrentCell == null || this.dgvScanLists.CurrentCell.OwningRow != row))
				{
					this.dgvScanLists.CurrentCell = row.Cells[cellCol];
				}
				this.ForkScrollRowIntoView(row.Index);
				this.dgvScanLists.Invalidate();
				if (openEditor)
				{
					this.OpenScanListEditorForRow(row);
				}
			}
			finally
			{
				this.forkActivatingRow = false;
			}
		}

		private void ForkScrollRowIntoView(int rowIndex)
		{
			if (rowIndex < 0 || rowIndex >= this.dgvScanLists.Rows.Count)
			{
				return;
			}
			int first = this.dgvScanLists.FirstDisplayedScrollingRowIndex;
			int visible = this.dgvScanLists.DisplayedRowCount(false);
			if (rowIndex < first || rowIndex >= first + visible)
			{
				this.dgvScanLists.FirstDisplayedScrollingRowIndex = rowIndex;
			}
		}

		private void SyncActiveScanListHighlightFromEditor()
		{
			MainForm mainForm = this.GetMainForm();
			if (mainForm == null)
			{
				return;
			}
			int openIndex = mainForm.GetOpenScanEditorDataIndex();
			if (openIndex < 0)
			{
				return;
			}
			this.forkActiveScanListDataIndex = openIndex;
			foreach (DataGridViewRow row in this.dgvScanLists.Rows)
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
			this.dgvScanLists.Invalidate();
		}

		private void OpenScanListEditorForRow(DataGridViewRow row)
		{
			if (row == null || row.Tag == null)
			{
				return;
			}
			int dataIndex = (int)row.Tag;
			this.forkActiveScanListDataIndex = dataIndex;
			this.dgvScanLists.Invalidate();
			MainForm mainForm = this.GetMainForm();
			if (mainForm == null)
			{
				return;
			}
			mainForm.OpenScanEditorByDataIndex(dataIndex);
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if (ForkFilterEscape.TryFocusFilter(ref keyData, this.txtScanListsFilter))
			{
				return true;
			}
			if (keyData == Keys.F2)
			{
				this.OpenSelectedScanListEditor();
				return true;
			}
			return base.ProcessCmdKey(ref msg, keyData);
		}

		private void OpenSelectedScanListEditor()
		{
			if (this.dgvScanLists == null || this.dgvScanLists.CurrentRow == null)
			{
				return;
			}
			this.OpenScanListEditorForRow(this.dgvScanLists.CurrentRow);
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
	}
}
#endif