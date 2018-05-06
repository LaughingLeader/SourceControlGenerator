using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LL.SCG.Util
{
	public static class ProcessHelper
	{
		/// <summary>
		/// Waits asynchronously for the process to exit.
		/// </summary>
		/// <param name="process">The process to wait for cancellation.</param>
		/// <param name="cancellationToken">A cancellation token. If invoked, the task will return 
		/// immediately as canceled.</param>
		/// <returns>A Task representing waiting for the process to end.</returns>
		public static Task WaitForExitAsync(this Process process,
			CancellationToken cancellationToken = default(CancellationToken))
		{
			var tcs = new TaskCompletionSource<object>();
			process.EnableRaisingEvents = true;
			process.Exited += (sender, args) => tcs.TrySetResult(null);
			if (cancellationToken != default(CancellationToken))
				cancellationToken.Register(tcs.SetCanceled);

			return tcs.Task;
		}

		private async static Task<int> RunProcessAsync(Process process, params string[] commands)
		{
			var result = -1;
			//process.OutputDataReceived += (s, ea) => Console.WriteLine(ea.Data);
			//process.ErrorDataReceived += (s, ea) => Console.WriteLine("ERR: " + ea.Data);

			

			bool started = process.Start();
			if (!started)
			{
				//you may allow for the process to be re-used (started = false) 
				//but I'm not sure about the guarantees of the Exited event in such a case
				throw new InvalidOperationException("Could not start process: " + process);
			}
			else
			{
				if(commands != null && commands.Length > 0)
				{
					StreamWriter stream = process.StandardInput;

					for(var i = 0; i < commands.Length; i++)
					{
						stream.WriteLine(commands[i]);
						await Task.Delay(10);
					}

					stream.Close();
				}

				process.WaitForExit();
				result = process.ExitCode;
				Log.Here().Important($"Process exited with code {result}");
				//await process.WaitForExitAsync();
			}

			//process.BeginOutputReadLine();
			//process.BeginErrorReadLine();

			return result;
		}

		public static async Task<int> RunProcessAsync(string filePath, string workingDirectory = "", params string[] commands)
		{
			using (var process = new Process
			{
				EnableRaisingEvents = true,
				StartInfo =
				{
					FileName = filePath,
					UseShellExecute = false,
					RedirectStandardInput = true,
					CreateNoWindow = true,
					WindowStyle = ProcessWindowStyle.Hidden,
					WorkingDirectory = workingDirectory
				}
			})
			{
				return await RunProcessAsync(process, commands).ConfigureAwait(false);
			}
		}

		public static async Task<int> RunCommandLineAsync(string workingDirectory = "", params string[] commands)
		{
			return await RunProcessAsync(Path.Combine(Environment.SystemDirectory, "cmd.exe"), workingDirectory, commands);
		}

		private static int RunProcess(Process process, params string[] commands)
		{
			bool started = process.Start();
			if (!started)
			{
				//you may allow for the process to be re-used (started = false) 
				//but I'm not sure about the guarantees of the Exited event in such a case
				throw new InvalidOperationException("Could not start process: " + process);
			}
			else
			{
				if (commands != null && commands.Length > 0)
				{
					StreamWriter stream = process.StandardInput;

					for (var i = 0; i < commands.Length; i++)
					{
						stream.WriteLine(commands[i]);
					}

					stream.Close();
				}

				process.WaitForExit(1000 * 60 * 5);
				return process.ExitCode;
			}
		}

		public static int RunProcess(string filePath, string workingDirectory = "", params string[] commands)
		{
			using (var process = new Process
			{
				EnableRaisingEvents = true,
				StartInfo =
				{
					FileName = filePath,
					UseShellExecute = false,
					RedirectStandardInput = true,
					CreateNoWindow = true,
					WindowStyle = ProcessWindowStyle.Hidden,
					WorkingDirectory = workingDirectory
				}
			})
			{
				return RunProcess(process, commands);
			}
		}

		public static int RunCommandLine(string workingDirectory = "", params string[] commands)
		{
			return RunProcess(Path.Combine(Environment.SystemDirectory, "cmd.exe"), workingDirectory, commands);
		}
	}
}
