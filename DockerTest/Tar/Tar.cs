using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace DockerTest
{

	public class Tar
	{
		/// <summary>
		/// Extracts a <i>.tar.gz</i> archive to the specified directory.
		/// </summary>
		/// <param name="filename">The <i>.tar.gz</i> to decompress and extract.</param>
		/// <param name="outputDir">Output directory to write the files.</param>
		public static void ExtractTarGz(string filename, string outputDir)
		{
			using (var stream = File.OpenRead(filename))
				ExtractTarGz(stream, outputDir);
		}

		/// <summary>
		/// Extracts a <i>.tar.gz</i> archive stream to the specified directory.
		/// </summary>
		/// <param name="stream">The <i>.tar.gz</i> to decompress and extract.</param>
		/// <param name="outputDir">Output directory to write the files.</param>
		public static void ExtractTarGz(Stream stream, string outputDir)
		{
			// A GZipStream is not seekable, so copy it first to a MemoryStream
			using (var gzip = new GZipStream(stream, CompressionMode.Decompress))
			{
				const int chunk = 4096;
				using (var memStr = new MemoryStream())
				{
					int read;
					var buffer = new byte[chunk];
					do
					{
						read = gzip.Read(buffer, 0, chunk);
						memStr.Write(buffer, 0, read);
					} while (read == chunk);

					memStr.Seek(0, SeekOrigin.Begin);
					ExtractTar(memStr, outputDir);
				}
			}
		}

		/// <summary>
		/// Extractes a <c>tar</c> archive to the specified directory.
		/// </summary>
		/// <param name="filename">The <i>.tar</i> to extract.</param>
		/// <param name="outputDir">Output directory to write the files.</param>
		public static void ExtractTar(string filename, string outputDir)
		{
			using (var stream = File.OpenRead(filename))
				ExtractTar(stream, outputDir);
		}

		/// <summary>
		/// Extractes a <c>tar</c> archive to the specified directory.
		/// </summary>
		/// <param name="stream">The <i>.tar</i> to extract.</param>
		/// <param name="outputDir">Output directory to write the files.</param>
		public static void ExtractTar(Stream stream, string outputDir)
		{
			var buffer = new byte[100];
			long pos = 1;
			while (true)
			{
				Console.WriteLine($"Checking for new record at {pos}");

				pos += stream.Read(buffer, 0, 100);
				var name = Encoding.ASCII.GetString(buffer).Trim('\0');
				if (String.IsNullOrWhiteSpace(name))
					break;
				pos += stream.Read(buffer, 0, 8); // Mode
				pos += stream.Read(buffer, 0, 8); // Uid
				pos += stream.Read(buffer, 0, 8); // Gid
				pos += stream.Read(buffer, 0, 12);
				var size = Convert.ToInt64(Encoding.UTF8.GetString(buffer, 0, 12).Trim('\0').Trim(), 8);
				
				Console.WriteLine($"Filename {name} is {size} bytes long");

				pos += stream.Read(buffer, 0, 12); // mtime
				pos += stream.Read(buffer, 0, 8); // chksum
				pos += stream.Read(buffer, 0, 1); // linkflag
				pos += stream.Read(buffer, 0, 100); // linkname
				pos += stream.Read(buffer, 0, 8); // magic
				pos += stream.Read(buffer, 0, 32); // uname
				pos += stream.Read(buffer, 0, 32); // gname
				pos += stream.Read(buffer, 0, 8); // devmajor
				pos += stream.Read(buffer, 0, 8); // devminor

				Console.WriteLine($"Header ends at {pos}");

				var output = Path.Combine(outputDir, name);
				if (!Directory.Exists(Path.GetDirectoryName(output)))
					Directory.CreateDirectory(Path.GetDirectoryName(output));
				if (!name.Equals("./", StringComparison.InvariantCulture))
				{
					using (var str = File.Open(output, FileMode.OpenOrCreate, FileAccess.Write))
					{
						var buf = new byte[size];
						pos += stream.Read(buf, 0, buf.Length);
						str.Write(buf, 0, buf.Length);
					}
				}

				Console.WriteLine($"File entry ends at {pos}");

				var padding = new byte[512];
				int offset = (int)(512 - (pos % 512));
				if (offset == 512)
					offset = 0;

				pos += stream.Read(padding, 0, offset);

			}
		}
	}
}