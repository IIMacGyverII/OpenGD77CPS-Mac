using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DMR
{
	public sealed class AndroidAdbDevice
	{
		public string Serial;
		public string Model;
	}

	public sealed class AndroidAdbListResult
	{
		public bool Success;
		public string ErrorMessage = "";
		public List<AndroidAdbDevice> Devices = new List<AndroidAdbDevice>();
		public List<string> BackupFolders = new List<string>();
		public string PhoneBackupPath = "";
	}

	/// <summary>
	/// Tier 2.10 — list and pull DMR_Backups from a USB-debugging phone via adb.
	/// </summary>
	public static class AndroidAdbBackup
	{
		public const string IniAdbPathKey = "AdbPath";

		private static readonly string[] PhoneBackupRoots = new string[]
		{
			"/sdcard/Download/DMR/DMR_Backups",
			"/storage/emulated/0/Download/DMR/DMR_Backups"
		};

		private static readonly Regex BackupFolderPattern = new Regex(@"^\d{8}_\d{6}$", RegexOptions.Compiled);

		public static string GetConfiguredAdbPath()
		{
			return IniFileUtils.getProfileStringWithDefault("Setup", IniAdbPathKey, "");
		}

		public static void SetConfiguredAdbPath(string path)
		{
			IniFileUtils.WriteProfileString("Setup", IniAdbPathKey, path ?? "");
		}

		public static string ResolveAdbExecutable()
		{
			string configured = GetConfiguredAdbPath();
			if (!string.IsNullOrEmpty(configured) && File.Exists(configured))
			{
				return configured;
			}

			string pathEnv = Environment.GetEnvironmentVariable("PATH") ?? "";
			foreach (string dir in pathEnv.Split(';'))
			{
				if (string.IsNullOrWhiteSpace(dir))
				{
					continue;
				}
				try
				{
					string candidate = Path.Combine(dir.Trim(), "adb.exe");
					if (File.Exists(candidate))
					{
						return candidate;
					}
				}
				catch
				{
				}
			}

			string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
			string[] common = new string[]
			{
				Path.Combine(localAppData, "Android", "Sdk", "platform-tools", "adb.exe"),
				Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "AppData", "Local", "Android", "Sdk", "platform-tools", "adb.exe")
			};
			foreach (string candidate in common)
			{
				if (File.Exists(candidate))
				{
					return candidate;
				}
			}

			return null;
		}

		public static bool IsAdbAvailable()
		{
			return !string.IsNullOrEmpty(ResolveAdbExecutable());
		}

		public static Task<AndroidAdbListResult> ListBackupsAsync(string preferredSerial)
		{
			return Task.Run(() => ListBackups(preferredSerial));
		}

		public static Task<string> PullBackupAsync(string deviceSerial, string backupFolderName, Action<string> progress)
		{
			return Task.Run(() => PullBackup(deviceSerial, backupFolderName, progress));
		}

		public static AndroidAdbListResult ListBackups(string preferredSerial)
		{
			AndroidAdbListResult result = new AndroidAdbListResult();
			string adb = ResolveAdbExecutable();
			if (string.IsNullOrEmpty(adb))
			{
				result.ErrorMessage = "adb.exe not found. Install Android platform-tools or set Setup → AdbPath in DockPanel.config.";
				return result;
			}

			result.Devices = ListDevices(adb);
			if (result.Devices.Count == 0)
			{
				result.ErrorMessage = "No adb device found. Enable USB debugging on the phone and authorize this PC.";
				return result;
			}

			AndroidAdbDevice device = PickDevice(result.Devices, preferredSerial);
			if (device == null)
			{
				result.ErrorMessage = "Could not select adb device.";
				return result;
			}

			foreach (string root in PhoneBackupRoots)
			{
				string stdout;
				string stderr;
				int code = RunAdb(adb, BuildAdbArgs(device.Serial, "shell", "ls", root), out stdout, out stderr, 20000);
				if (code != 0)
				{
					continue;
				}
				List<string> folders = ParseBackupFolderNames(stdout);
				if (folders.Count > 0)
				{
					result.PhoneBackupPath = root;
					result.BackupFolders = folders;
					result.Success = true;
					return result;
				}
			}

			result.ErrorMessage = "No backup folders found on the phone at Download/DMR/DMR_Backups.\nExport from the radio app first.";
			return result;
		}

		public static string PullBackup(string deviceSerial, string backupFolderName, Action<string> progress)
		{
			if (string.IsNullOrWhiteSpace(backupFolderName) || !BackupFolderPattern.IsMatch(backupFolderName.Trim()))
			{
				throw new InvalidOperationException("Invalid backup folder name.");
			}

			string adb = ResolveAdbExecutable();
			if (string.IsNullOrEmpty(adb))
			{
				throw new InvalidOperationException("adb.exe not found.");
			}

			AndroidAdbListResult listing = ListBackups(deviceSerial);
			if (!listing.Success || string.IsNullOrEmpty(listing.PhoneBackupPath))
			{
				throw new InvalidOperationException(listing.ErrorMessage);
			}

			string remotePath = listing.PhoneBackupPath + "/" + backupFolderName.Trim();
			string localRoot = Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
				"PriInterPhoneCPS",
				"adb_pull");
			Directory.CreateDirectory(localRoot);
			string localPath = Path.Combine(localRoot, backupFolderName.Trim());
			if (Directory.Exists(localPath))
			{
				try
				{
					Directory.Delete(localPath, true);
				}
				catch
				{
				}
			}

			if (progress != null)
			{
				progress("Pulling " + backupFolderName + " from phone…");
			}

			string stdout;
			string stderr;
			int code = RunAdb(adb, BuildAdbArgs(deviceSerial, "pull", QuoteAdbPath(remotePath), QuoteAdbPath(localPath)),
				out stdout, out stderr, 300000);
			if (code != 0)
			{
				throw new InvalidOperationException("adb pull failed:\n" + MergeOutput(stdout, stderr));
			}

			if (!AndroidBackupFolderPicker.IsReadableBackupFolder(localPath))
			{
				throw new InvalidOperationException("Pull finished but folder is missing or unreadable:\n" + localPath);
			}

			string channels = Path.Combine(localPath, "Channels.csv");
			if (!File.Exists(channels))
			{
				throw new InvalidOperationException("Pull finished but Channels.csv was not found in:\n" + localPath);
			}

			if (progress != null)
			{
				progress("Saved to " + localPath);
			}
			return localPath;
		}

		private static List<AndroidAdbDevice> ListDevices(string adb)
		{
			List<AndroidAdbDevice> devices = new List<AndroidAdbDevice>();
			string stdout;
			string stderr;
			int code = RunAdb(adb, "devices", out stdout, out stderr, 15000);
			if (code != 0)
			{
				return devices;
			}

			using (StringReader reader = new StringReader(stdout ?? ""))
			{
				string line;
				while ((line = reader.ReadLine()) != null)
				{
					line = line.Trim();
					if (line.Length == 0 || line.StartsWith("List of devices", StringComparison.OrdinalIgnoreCase))
					{
						continue;
					}
					string[] parts = line.Split(new char[] { '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);
					if (parts.Length >= 2 && parts[1].Equals("device", StringComparison.OrdinalIgnoreCase))
					{
						AndroidAdbDevice dev = new AndroidAdbDevice { Serial = parts[0] };
						dev.Model = GetDeviceModel(adb, dev.Serial);
						devices.Add(dev);
					}
				}
			}
			return devices;
		}

		private static string GetDeviceModel(string adb, string serial)
		{
			string stdout;
			string stderr;
			int code = RunAdb(adb, BuildAdbArgs(serial, "shell", "getprop", "ro.product.model"), out stdout, out stderr, 10000);
			if (code != 0)
			{
				return "";
			}
			return (stdout ?? "").Trim();
		}

		private static AndroidAdbDevice PickDevice(List<AndroidAdbDevice> devices, string preferredSerial)
		{
			if (devices == null || devices.Count == 0)
			{
				return null;
			}
			if (!string.IsNullOrEmpty(preferredSerial))
			{
				foreach (AndroidAdbDevice dev in devices)
				{
					if (dev.Serial == preferredSerial)
					{
						return dev;
					}
				}
			}
			return devices[0];
		}

		private static List<string> ParseBackupFolderNames(string lsOutput)
		{
			HashSet<string> names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			if (string.IsNullOrEmpty(lsOutput))
			{
				return new List<string>();
			}

			string[] tokens = lsOutput.Split(new char[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
			foreach (string token in tokens)
			{
				string name = token.Trim();
				if (BackupFolderPattern.IsMatch(name))
				{
					names.Add(name);
				}
			}

			List<string> sorted = new List<string>(names);
			sorted.Sort((a, b) => string.Compare(b, a, StringComparison.Ordinal));
			return sorted;
		}

		public static string TryPickPulledFolder(IWin32Window owner)
		{
			using (AndroidAdbPickForm form = new AndroidAdbPickForm())
			{
				if (form.ShowDialog(owner) == System.Windows.Forms.DialogResult.OK)
				{
					return form.LocalFolderPath;
				}
			}
			return null;
		}

		private static int RunAdb(string adbExe, string arguments, out string stdout, out string stderr, int timeoutMs)
		{
			stdout = "";
			stderr = "";
			using (Process process = new Process())
			{
				ProcessStartInfo psi = new ProcessStartInfo
				{
					FileName = adbExe,
					Arguments = arguments,
					UseShellExecute = false,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					CreateNoWindow = true,
					StandardOutputEncoding = Encoding.UTF8,
					StandardErrorEncoding = Encoding.UTF8
				};
				process.StartInfo = psi;
				process.Start();
				Task<string> outTask = Task.Run(() => process.StandardOutput.ReadToEnd());
				Task<string> errTask = Task.Run(() => process.StandardError.ReadToEnd());
				if (!process.WaitForExit(timeoutMs))
				{
					try
					{
						process.Kill();
					}
					catch
					{
					}
					stderr = "adb timed out.";
					return -1;
				}
				stdout = outTask.Result;
				stderr = errTask.Result;
				return process.ExitCode;
			}
		}

		private static string BuildAdbArgs(string serial, params string[] parts)
		{
			StringBuilder sb = new StringBuilder();
			if (!string.IsNullOrEmpty(serial))
			{
				sb.Append("-s ").Append(serial).Append(' ');
			}
			for (int i = 0; i < parts.Length; i++)
			{
				if (i > 0)
				{
					sb.Append(' ');
				}
				sb.Append(parts[i]);
			}
			return sb.ToString();
		}

		private static string QuoteAdbPath(string path)
		{
			if (path.IndexOf(' ') >= 0)
			{
				return "\"" + path + "\"";
			}
			return path;
		}

		private static string MergeOutput(string stdout, string stderr)
		{
			StringBuilder sb = new StringBuilder();
			if (!string.IsNullOrWhiteSpace(stdout))
			{
				sb.AppendLine(stdout.Trim());
			}
			if (!string.IsNullOrWhiteSpace(stderr))
			{
				sb.AppendLine(stderr.Trim());
			}
			return sb.ToString().Trim();
		}
	}

}