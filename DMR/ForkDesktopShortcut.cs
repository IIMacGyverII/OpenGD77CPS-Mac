using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace DMR
{
	/// <summary>Creates Windows shell shortcuts for fork workflows.</summary>
	internal static class ForkDesktopShortcut
	{
		private const string StudioLinkName = "PriInterPhone Codeplug Studio.lnk";

		public static bool TryCreateStudioShortcut(IWin32Window owner, bool quiet = false)
		{
			try
			{
				string exe = Application.ExecutablePath;
				if (string.IsNullOrEmpty(exe) || !File.Exists(exe))
				{
					if (!quiet)
					{
						MessageBox.Show(owner, "Could not locate OpenGD77CPS.exe.", "Desktop shortcut",
							MessageBoxButtons.OK, MessageBoxIcon.Warning);
					}
					return false;
				}

				string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
				string linkPath = Path.Combine(desktop, StudioLinkName);
				Type shellType = Type.GetTypeFromProgID("WScript.Shell");
				if (shellType == null)
				{
					if (!quiet)
					{
						MessageBox.Show(owner, "Windows Script Host is not available on this PC.", "Desktop shortcut",
							MessageBoxButtons.OK, MessageBoxIcon.Warning);
					}
					return false;
				}

				object shell = Activator.CreateInstance(shellType);
				object shortcut = shellType.InvokeMember(
					"CreateShortcut",
					BindingFlags.InvokeMethod,
					null,
					shell,
					new object[] { linkPath });
				Type shortcutType = shortcut.GetType();
				shortcutType.InvokeMember("TargetPath", BindingFlags.SetProperty, null, shortcut, new object[] { exe });
				shortcutType.InvokeMember("Arguments", BindingFlags.SetProperty, null, shortcut, new object[] { "--studio" });
				shortcutType.InvokeMember("WorkingDirectory", BindingFlags.SetProperty, null, shortcut, new object[] { Path.GetDirectoryName(exe) });
				shortcutType.InvokeMember("Description", BindingFlags.SetProperty, null, shortcut,
					new object[] { AboutForm.FORK_NAME + " — CSV-only backup workflow (Path B)" });
				shortcutType.InvokeMember("Save", BindingFlags.InvokeMethod, null, shortcut, null);

				if (!quiet)
				{
					MessageBox.Show(owner,
						"Desktop shortcut created:\n\n" + linkPath + "\n\n"
						+ "Double-click it to open Codeplug Studio. Drag a backup folder onto the shortcut to load that folder.\n\n"
						+ "You can also run CodeplugStudio.cmd from the CPS install folder.",
						"Desktop shortcut",
						MessageBoxButtons.OK,
						MessageBoxIcon.Information);
				}
				return true;
			}
			catch (Exception ex)
			{
				if (!quiet)
				{
					MessageBox.Show(owner, "Could not create shortcut:\n\n" + ex.Message, "Desktop shortcut",
						MessageBoxButtons.OK, MessageBoxIcon.Warning);
				}
				return false;
			}
		}
	}
}