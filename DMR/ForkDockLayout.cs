using System;
using System.IO;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace DMR
{
	/// <summary>
	/// Tier 2.3 — PriInterPhone default dock/MDI layout, reset, and persistence.
	/// </summary>
	public static class ForkDockLayout
	{
		private const string IniSection = "Setup";
		private const string IniKeyInitialized = "ForkDockLayoutInitialized";
		private const string ConfigFileName = "DockPanel.config";
		private const string DefaultTemplateFileName = "DockPanel.config.default";

		public static string GetConfigPath()
		{
			return Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), ConfigFileName);
		}

		public static string GetDefaultTemplatePath()
		{
			return Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), DefaultTemplateFileName);
		}

		public static void Save(DockPanel dockPanel)
		{
			if (dockPanel == null)
			{
				return;
			}
			try
			{
				dockPanel.SaveAsXml(GetConfigPath());
			}
			catch (Exception ex)
			{
				Console.WriteLine("ForkDockLayout.Save: " + ex.Message);
			}
		}

		public static void ApplyFirstRunIfNeeded(MainForm main)
		{
			if (main == null)
			{
				return;
			}
			if (IniFileUtils.getProfileStringWithDefault(IniSection, IniKeyInitialized, "no") == "yes")
			{
				return;
			}
			ApplyMdiDefault(main);
			IniFileUtils.WriteProfileString(IniSection, IniKeyInitialized, "yes");
		}

		public static void RestoreDefault(MainForm main)
		{
			if (main == null)
			{
				return;
			}
			string template = GetDefaultTemplatePath();
			if (!File.Exists(template))
			{
				MessageBox.Show(main,
					"Default layout template not found:\n" + template,
					"Restore layout",
					MessageBoxButtons.OK,
					MessageBoxIcon.Warning);
				return;
			}
			try
			{
				File.Copy(template, GetConfigPath(), true);
				main.ForkLayoutReloadDock();
				ApplyMdiDefault(main);
				IniFileUtils.WriteProfileString(IniSection, IniKeyInitialized, "yes");
			}
			catch (Exception ex)
			{
				MessageBox.Show(main,
					"Could not restore default layout:\n" + ex.Message,
					"Restore layout",
					MessageBoxButtons.OK,
					MessageBoxIcon.Error);
			}
		}

		private static void ApplyMdiDefault(MainForm main)
		{
			main.ForkLayoutCloseMdi();
			TreeNode channelsRoot = main.ForkLayoutFindNode(typeof(ChannelsForm));
			if (channelsRoot != null)
			{
				main.ForkLayoutOpenNode(channelsRoot, false);
			}
			TreeNode firstChannel = main.ForkLayoutFindChannelNode(0);
			if (firstChannel != null)
			{
				main.ForkLayoutOpenNode(firstChannel, true);
			}
			main.LayoutMdi(MdiLayout.TileHorizontal);
		}
	}

}