using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading;
using System.Windows.Forms;
using KeePass.Forms;
using KeePass.Plugins;
using KeePass.UI;
using KeePassLib.Serialization;
using KeePassLib.Utility;

namespace KeePassHIBP
{
	public class KeePassHIBPExt : Plugin
	{
		public override Image SmallIcon
		{
			get { return Properties.Resources.B16x16_Icon; }
		}

		public override string UpdateUrl
		{
			get { return "https://github.com/JanisEst/KeePassHIBP/raw/master/keepass.version"; }
		}

		public override bool Initialize(IPluginHost host)
		{
			//Debugger.Launch();

			// Workaround to support Tsl1.2 on .NET 4.0
			ServicePointManager.Expect100Continue = true;
			ServicePointManager.SecurityProtocol |= (SecurityProtocolType)768 | (SecurityProtocolType)3072;

			GlobalWindowManager.WindowAdded += WindowAddedHandler;

			return true;
		}

		public override void Terminate()
		{
			GlobalWindowManager.WindowAdded -= WindowAddedHandler;
		}

		/// <summary>
		/// Used to modify other form when they load.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void WindowAddedHandler(object sender, GwmWindowEventArgs e)
		{
			if (e.Form is PwEntryForm || e.Form is KeyCreationForm)
			{
				e.Form.Shown += delegate
				{
					var fieldInfo = e.Form.GetType().GetField("m_icgPassword", BindingFlags.Instance | BindingFlags.NonPublic);
					if (fieldInfo != null)
					{
						var icg = fieldInfo.GetValue(e.Form) as PwInputControlGroup;
						if (icg != null)
						{
							var m_tbPassword = e.Form.Controls.Find("m_tbPassword", true).FirstOrDefault() as TextBox;
							if (m_tbPassword != null)
							{
								m_tbPassword.TextChanged += new DelayedEventHandler(TimeSpan.FromMilliseconds(500), delegate
								{
									var pwBytes = icg.GetPasswordUtf8();
									var hash = CreateSha1Hash(pwBytes);
									MemUtil.ZeroByteArray(pwBytes);

									ThreadPool.QueueUserWorkItem(delegate(object oHash)
									{
										var strHash = (string)oHash;
										try
										{
											var knownHashes = RequestPwnedHashes(strHash);

											if (knownHashes.Contains(hash))
											{
												m_tbPassword.Invoke((MethodInvoker)delegate
												{
													var toolTip = new ToolTip();
													var pt = new Point(0, 0);
													pt.Offset(0, m_tbPassword.Height + 1);
													toolTip.Show("Warning: This password has previously appeared in a data breach.", m_tbPassword, pt, 2500);
												});
											}
										}
										catch
										{
											// Service may not be available.
										}
									}, hash);

									MemUtil.ZeroByteArray(pwBytes);
								}).OnDelay;
							}
						}
					}
				};
			}
		}

		private static List<string> RequestPwnedHashes(string hash)
		{
			const string ApiUrl = "https://api.pwnedpasswords.com/range/";

			var first5Chars = hash.Substring(0, 5);

			var result = DownloadString(ApiUrl + first5Chars);

			result = StrUtil.NormalizeNewLines(result, false);

			const int Sha1SuffixLength = 35;

			return result
				.Split('\n')
				.Where(l => l.Length >= Sha1SuffixLength)
				.Select(l => first5Chars + l.Substring(0, Sha1SuffixLength))
				.ToList();
		}

		private static string DownloadString(string url)
		{
			var ioc = IOConnectionInfo.FromPath(url);

			using (var s = IOConnection.OpenRead(ioc))
			{
				if (s == null)
				{
					throw new InvalidOperationException();
				}

				using (var ms = new MemoryStream())
				{
					MemUtil.CopyStream(s, ms);

					return StrUtil.Utf8.GetString(ms.ToArray());
				}
			}
		}

		private static string CreateSha1Hash(byte[] data)
		{
			using (var sha1 = new SHA1Managed())
			{
				var hash = sha1.ComputeHash(data);

				return MemUtil.ByteArrayToHexString(hash);
			}
		}
	}
}
