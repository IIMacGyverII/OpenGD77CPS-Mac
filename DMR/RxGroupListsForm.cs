#if OpenGD77
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace DMR
{
	public class RxGroupListsForm : DockContent, IDisp, ISingleRow
	{
		private Panel pnlRxLists;
		private DataGridView dgvRxLists;
		private TextBox txtRxListsFilter;
		private Label lblRxListsFilter;
		private Label lblRxListsHint;
		private static readonly string[] ForkRxListHeaderText = { "#", "Name", "Contacts", "1st contact" };
		private int forkSortColumn = -1;
		private bool forkSortAscending = true;
		private int forkActiveRxListDataIndex = -1;
		private int forkLastSelectionDataIndex = -1;
		private bool forkRxListClickHandled;
		private bool forkKeyboardNavPending;
		private bool forkActivatingRow;

		public TreeNode Node { get; set; }

		public RxGroupListsForm()
		{
			this.InitializeComponent();
			this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
			base.Scale(Settings.smethod_6());
		}

		public void SaveData()
		{
			if (this.dgvRxLists != null)
			{
				this.dgvRxLists.Focus();
			}
		}

		public void DispData()
		{
			if (this.dgvRxLists == null)
			{
				return;
			}
			try
			{
				this.dgvRxLists.Rows.Clear();
				for (int i = 0; i < RxGroupListForm.data.Count; i++)
				{
					if (RxGroupListForm.data.DataIsValid(i))
					{
						int index = this.dgvRxLists.Rows.Add(
							(i + 1).ToString(),
							RxGroupListForm.data[i].Name,
							RxGroupListForm.data.GetContactCntByIndex(i).ToString(),
							this.ForkGetFirstContactName(i));
						this.dgvRxLists.Rows[index].Tag = i;
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
			int preserveActive = this.forkActiveRxListDataIndex;
			int preserveSelection = this.forkLastSelectionDataIndex;
			this.ApplyRxListsFilter();
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
			this.dgvRxLists.CurrentCell = null;
			this.forkLastSelectionDataIndex = -1;
		}

		public void RefreshName()
		{
		}

		public void RefreshSingleRow(int index)
		{
			if (this.dgvRxLists == null)
			{
				return;
			}
			foreach (DataGridViewRow item in (IEnumerable)this.dgvRxLists.Rows)
			{
				if (item.IsNewRow || item.Tag == null || (int)item.Tag != index)
				{
					continue;
				}
				item.Cells[1].Value = RxGroupListForm.data[index].Name;
				item.Cells[2].Value = RxGroupListForm.data.GetContactCntByIndex(index).ToString();
				item.Cells[3].Value = this.ForkGetFirstContactName(index);
				this.dgvRxLists.InvalidateRow(item.Index);
				return;
			}
		}

		private void RxGroupListsForm_Load(object sender, EventArgs e)
		{
			this.EnsureForkRxListsGridUi();
			Settings.smethod_68(this);
			this.pnlRxLists.Resize += this.pnlRxLists_Resize;
			this.DispData();
			Theme.ApplyStandardEditorColors(this);
			this.ApplyForkRxListsLayout();
			this.SyncActiveRxListHighlightFromEditor();
		}

		private void InitializeComponent()
		{
			this.pnlRxLists = new Panel();
			this.pnlRxLists.Dock = DockStyle.Fill;
			this.pnlRxLists.Name = "pnlRxLists";
			base.ClientSize = new Size(720, 420);
			base.Controls.Add(this.pnlRxLists);
			base.Name = "RxGroupListsForm";
			base.Text = "TG / Rx Group Lists";
			base.Load += this.RxGroupListsForm_Load;
		}

		private void EnsureForkRxListsGridUi()
		{
			this.pnlRxLists.AutoSize = false;
			if (this.dgvRxLists == null)
			{
				this.dgvRxLists = new DataGridView();
				this.dgvRxLists.Name = "dgvRxLists";
				this.dgvRxLists.ReadOnly = true;
				this.dgvRxLists.AllowUserToAddRows = false;
				this.dgvRxLists.AllowUserToDeleteRows = false;
				this.dgvRxLists.AllowUserToResizeRows = false;
				this.dgvRxLists.AllowUserToOrderColumns = false;
				this.dgvRxLists.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
				this.dgvRxLists.MultiSelect = false;
				this.dgvRxLists.RowHeadersWidth = 50;
				this.dgvRxLists.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
				ForkGridBadges.EnableGridPolish(this.dgvRxLists);
				for (int i = 0; i < ForkRxListHeaderText.Length; i++)
				{
					DataGridViewTextBoxColumn col = new DataGridViewTextBoxColumn();
					col.HeaderText = ForkRxListHeaderText[i];
					col.SortMode = DataGridViewColumnSortMode.NotSortable;
					this.dgvRxLists.Columns.Add(col);
				}
				this.dgvRxLists.CellMouseDown += this.dgvRxLists_CellMouseDown;
				this.dgvRxLists.KeyDown += this.dgvRxLists_KeyDown;
				this.dgvRxLists.CellDoubleClick += this.dgvRxLists_CellDoubleClick;
				this.dgvRxLists.CellFormatting += this.dgvRxLists_CellFormatting;
				this.dgvRxLists.ColumnHeaderMouseClick += this.dgvRxLists_ColumnHeaderMouseClick;
				this.dgvRxLists.RowPostPaint += this.dgvRxLists_RowPostPaint;
				this.dgvRxLists.SelectionChanged += this.dgvRxLists_SelectionChanged;
				this.dgvRxLists.RowHeaderMouseClick += this.dgvRxLists_RowHeaderMouseClick;
				this.pnlRxLists.Controls.Add(this.dgvRxLists);
			}
			if (this.txtRxListsFilter == null)
			{
				this.lblRxListsFilter = new Label();
				this.lblRxListsFilter.Text = "Filter:";
				this.lblRxListsFilter.AutoSize = true;
				this.txtRxListsFilter = new TextBox();
				this.txtRxListsFilter.Size = new Size(180, 23);
				this.txtRxListsFilter.TextChanged += this.txtRxListsFilter_TextChanged;
				ForkFilterEscape.WireEscapeClear(this.txtRxListsFilter);
				this.pnlRxLists.Controls.Add(this.lblRxListsFilter);
				this.pnlRxLists.Controls.Add(this.txtRxListsFilter);
			}
			if (this.lblRxListsHint == null)
			{
				this.lblRxListsHint = new Label();
				this.lblRxListsHint.Text = "F2 or click row opens TG/Rx editor · Ctrl+F filter · Group contacts only";
				this.lblRxListsHint.AutoSize = false;
				this.lblRxListsHint.Height = 18;
				this.lblRxListsHint.ForeColor = SystemColors.GrayText;
				this.pnlRxLists.Controls.Add(this.lblRxListsHint);
			}
		}

		private void pnlRxLists_Resize(object sender, EventArgs e)
		{
			this.ApplyForkRxListsLayout();
		}

		private void ApplyForkRxListsLayout()
		{
			if (this.dgvRxLists == null)
			{
				return;
			}
			int pad = Theme.Dpi(12);
			int clientW = this.pnlRxLists.ClientSize.Width;
			int clientH = this.pnlRxLists.ClientSize.Height;
			int filterY = Theme.Dpi(8);
			if (this.lblRxListsFilter != null)
			{
				this.lblRxListsFilter.Location = new Point(pad, filterY + Theme.Dpi(2));
			}
			if (this.txtRxListsFilter != null)
			{
				this.txtRxListsFilter.Location = new Point(Theme.Dpi(56), filterY);
				this.txtRxListsFilter.Width = Math.Max(Theme.Dpi(120), clientW - Theme.Dpi(68));
			}
			if (this.lblRxListsHint != null)
			{
				this.lblRxListsHint.Location = new Point(pad, filterY + Theme.Dpi(26));
				this.lblRxListsHint.Width = Math.Max(Theme.Dpi(200), clientW - pad * 2);
			}
			int gridY = filterY + Theme.Dpi(48);
			int gridH = Math.Max(Theme.Dpi(200), clientH - gridY - pad);
			this.dgvRxLists.Location = new Point(pad, gridY);
			this.dgvRxLists.Size = new Size(Math.Max(Theme.Dpi(300), clientW - pad * 2), gridH);
			this.dgvRxLists.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
		}

		private string ForkGetFirstContactName(int listIndex)
		{
			int cnt = RxGroupListForm.data.GetContactCntByIndex(listIndex);
			for (int i = 0; i < cnt; i++)
			{
				ushort contactNum = RxGroupListForm.data[listIndex].ContactList[i];
				int idx = contactNum - 1;
				if (ContactForm.data.DataIsValid(idx) && ContactForm.data.IsGroupCall(idx))
				{
					return ContactForm.data[idx].Name;
				}
			}
			return "";
		}

		private void txtRxListsFilter_TextChanged(object sender, EventArgs e)
		{
			this.ApplyRxListsFilter();
		}

		private void ApplyRxListsFilter()
		{
			if (this.dgvRxLists == null)
			{
				return;
			}
			string query = this.txtRxListsFilter == null ? "" : this.txtRxListsFilter.Text.Trim();
			int visible = 0;
			int total = 0;
			foreach (DataGridViewRow row in this.dgvRxLists.Rows)
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
			if (this.lblRxListsFilter != null)
			{
				this.lblRxListsFilter.Text = string.IsNullOrEmpty(query)
					? "Filter:"
					: "Filter (" + visible + "/" + total + "):";
			}
		}

		private void dgvRxLists_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
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
			this.SortRxListsGrid(e.ColumnIndex, this.forkSortAscending);
			this.RefreshRxListSortHeaderGlyphs();
		}

		private void RefreshRxListSortHeaderGlyphs()
		{
			for (int i = 0; i < this.dgvRxLists.Columns.Count && i < ForkRxListHeaderText.Length; i++)
			{
				string text = ForkRxListHeaderText[i];
				if (i == this.forkSortColumn)
				{
					text += this.forkSortAscending ? " \u25B2" : " \u25BC";
				}
				this.dgvRxLists.Columns[i].HeaderText = text;
			}
		}

		private void SortRxListsGrid(int columnIndex, bool ascending)
		{
			List<DataGridViewRow> rows = new List<DataGridViewRow>();
			foreach (DataGridViewRow row in this.dgvRxLists.Rows)
			{
				if (!row.IsNewRow)
				{
					rows.Add(row);
				}
			}
			rows.Sort((a, b) => this.CompareRxListRows(a, b, columnIndex, ascending));
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
			this.dgvRxLists.Rows.Clear();
			for (int i = 0; i < cellValues.Count; i++)
			{
				int index = this.dgvRxLists.Rows.Add(cellValues[i]);
				this.dgvRxLists.Rows[index].Tag = tags[i];
			}
			this.ApplyRxListsFilter();
			if (this.forkActiveRxListDataIndex >= 0)
			{
				foreach (DataGridViewRow row in this.dgvRxLists.Rows)
				{
					if (!row.IsNewRow && row.Tag != null && (int)row.Tag == this.forkActiveRxListDataIndex)
					{
						this.ForkActivateGridRow(row, 0, false);
						break;
					}
				}
			}
			else
			{
				this.dgvRxLists.CurrentCell = null;
			}
		}

		private int CompareRxListRows(DataGridViewRow a, DataGridViewRow b, int columnIndex, bool ascending)
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

		private void dgvRxLists_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
		{
			if (e.RowIndex < 0)
			{
				return;
			}
			DataGridViewRow row = this.dgvRxLists.Rows[e.RowIndex];
			if (row.IsNewRow)
			{
				return;
			}
			bool isActive = row.Tag != null && (int)row.Tag == this.forkActiveRxListDataIndex;
			if (isActive)
			{
				e.CellStyle.BackColor = ForkGridBadges.ActiveRowBack;
				e.CellStyle.ForeColor = ForkGridBadges.ActiveRowFore;
				e.CellStyle.SelectionBackColor = ForkGridBadges.ActiveRowBack;
				e.CellStyle.SelectionForeColor = ForkGridBadges.ActiveRowFore;
			}
		}

		private void dgvRxLists_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
		{
			try
			{
				DataGridViewRow paintRow = this.dgvRxLists.Rows[e.RowIndex];
				if (!paintRow.IsNewRow && paintRow.Tag != null && (int)paintRow.Tag == this.forkActiveRxListDataIndex)
				{
					Rectangle rowBounds = new Rectangle(
						e.RowBounds.Left, e.RowBounds.Top,
						this.dgvRxLists.Columns.GetColumnsWidth(DataGridViewElementStates.Visible) + this.dgvRxLists.RowHeadersWidth,
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

		private void dgvRxLists_SelectionChanged(object sender, EventArgs e)
		{
			if (this.forkRxListClickHandled)
			{
				this.forkRxListClickHandled = false;
				return;
			}
			if (this.forkActivatingRow)
			{
				return;
			}
			DataGridViewRow row = this.dgvRxLists.CurrentRow;
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

		private void dgvRxLists_KeyDown(object sender, KeyEventArgs e)
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
			if (this.IsDisposed || this.dgvRxLists == null || this.dgvRxLists.IsDisposed)
			{
				return;
			}
			DataGridViewRow liveRow = this.ForkFindRowByDataIndex(dataIndex);
			if (liveRow == null)
			{
				if (dataIndex == this.forkActiveRxListDataIndex)
				{
					this.dgvRxLists.Invalidate();
				}
				return;
			}
			if (openEditor)
			{
				MainForm mainForm = this.GetMainForm();
				if (mainForm != null && mainForm.GetOpenRxGroupListEditorDataIndex() == dataIndex)
				{
					openEditor = false;
				}
			}
			this.ForkActivateGridRow(liveRow, 0, openEditor);
		}

		private DataGridViewRow ForkFindRowByDataIndex(int dataIndex)
		{
			foreach (DataGridViewRow row in this.dgvRxLists.Rows)
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
			if (row.DataGridView == this.dgvRxLists)
			{
				return true;
			}
			row = this.ForkFindRowByDataIndex((int)row.Tag);
			return row != null;
		}

		private void dgvRxLists_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
		{
			if (e.Button != MouseButtons.Left || e.RowIndex < 0 || e.ColumnIndex < 0 || e.RowIndex >= this.dgvRxLists.Rows.Count)
			{
				return;
			}
			this.forkRxListClickHandled = true;
			this.ForkActivateGridRow(this.dgvRxLists.Rows[e.RowIndex], e.ColumnIndex, true);
		}

		private void dgvRxLists_RowHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
		{
			if (e.RowIndex < 0 || e.RowIndex >= this.dgvRxLists.Rows.Count)
			{
				return;
			}
			this.forkRxListClickHandled = true;
			this.ForkActivateGridRow(this.dgvRxLists.Rows[e.RowIndex], 0, true);
		}

		private void dgvRxLists_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
		{
			if (e.RowIndex >= 0 && e.RowIndex < this.dgvRxLists.Rows.Count)
			{
				this.OpenRxListEditorForRow(this.dgvRxLists.Rows[e.RowIndex]);
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
				this.forkActiveRxListDataIndex = (int)row.Tag;
				this.forkLastSelectionDataIndex = this.forkActiveRxListDataIndex;
				if (this.dgvRxLists.SelectedRows.Count != 1 || this.dgvRxLists.SelectedRows[0] != row)
				{
					this.dgvRxLists.ClearSelection();
					if (row.DataGridView == this.dgvRxLists)
					{
						row.Selected = true;
					}
				}
				int cellCol = columnIndex >= 0 && columnIndex < row.Cells.Count ? columnIndex : 0;
				if (row.DataGridView == this.dgvRxLists
					&& (this.dgvRxLists.CurrentCell == null || this.dgvRxLists.CurrentCell.OwningRow != row))
				{
					this.dgvRxLists.CurrentCell = row.Cells[cellCol];
				}
				this.ForkScrollRowIntoView(row.Index);
				this.dgvRxLists.Invalidate();
				if (openEditor)
				{
					this.OpenRxListEditorForRow(row);
				}
			}
			finally
			{
				this.forkActivatingRow = false;
			}
		}

		private void ForkScrollRowIntoView(int rowIndex)
		{
			if (rowIndex < 0 || rowIndex >= this.dgvRxLists.Rows.Count)
			{
				return;
			}
			int first = this.dgvRxLists.FirstDisplayedScrollingRowIndex;
			int visible = this.dgvRxLists.DisplayedRowCount(false);
			if (rowIndex < first || rowIndex >= first + visible)
			{
				this.dgvRxLists.FirstDisplayedScrollingRowIndex = rowIndex;
			}
		}

		private void SyncActiveRxListHighlightFromEditor()
		{
			MainForm mainForm = this.GetMainForm();
			if (mainForm == null)
			{
				return;
			}
			int openIndex = mainForm.GetOpenRxGroupListEditorDataIndex();
			if (openIndex < 0)
			{
				return;
			}
			this.forkActiveRxListDataIndex = openIndex;
			foreach (DataGridViewRow row in this.dgvRxLists.Rows)
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
			this.dgvRxLists.Invalidate();
		}

		private void OpenRxListEditorForRow(DataGridViewRow row)
		{
			if (row == null || row.Tag == null)
			{
				return;
			}
			int dataIndex = (int)row.Tag;
			this.forkActiveRxListDataIndex = dataIndex;
			this.dgvRxLists.Invalidate();
			MainForm mainForm = this.GetMainForm();
			if (mainForm == null)
			{
				return;
			}
			mainForm.OpenRxGroupListEditorByDataIndex(dataIndex);
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if (ForkFilterEscape.TryFocusFilter(ref keyData, this.txtRxListsFilter))
			{
				return true;
			}
			if (keyData == Keys.F2)
			{
				this.OpenSelectedRxListEditor();
				return true;
			}
			return base.ProcessCmdKey(ref msg, keyData);
		}

		private void OpenSelectedRxListEditor()
		{
			if (this.dgvRxLists == null || this.dgvRxLists.CurrentRow == null)
			{
				return;
			}
			this.OpenRxListEditorForRow(this.dgvRxLists.CurrentRow);
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