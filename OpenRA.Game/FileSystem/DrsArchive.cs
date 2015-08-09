#region Copyright & License Information
/*
 * Copyright 2007-2015 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation. For more information,
 * see COPYING.
 */
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenRA.FileSystem
{
	public class DrsArchive : IFolder
	{
		readonly string filename;
		readonly int priority;
		readonly Stream stream;

		public int Priority { get { return priority; } }
		public string Name { get { return filename; } }

		public readonly string GameVersion;
		public readonly DrsHeader Header;
		public readonly List<DrsTable> Tables = new List<DrsTable>();

		public readonly Dictionary<string, byte[]> FileData = new Dictionary<string, byte[]>();

		public DrsArchive(string filename, int priority)
		{
			this.filename = filename;
			this.priority = priority;

			stream = GlobalFileSystem.Open(filename);
			try
			{
				Header = new DrsHeader(stream);
				GameVersion = Header.GameVersion;

				for (var i = 0; i < Header.TableCount; i++)
					Tables.Add(new DrsTable(stream));

				foreach (var table in Tables)
				{
					stream.Position = table.DataOffset;

					for (var i = 0; i < table.FileCount; i++)
					{
						var file = new DrsFile(stream, table);

						if (!FileData.ContainsKey(file.GeneratedName))
							FileData.Add(file.GeneratedName, file.Data);
					}
				}

			}
			catch
			{
				Dispose();
				throw;
			}
		}

		public Stream GetContent(string filename)
		{
			if (!FileData.ContainsKey(filename))
				throw new ArgumentException("This DrsArchive does not contain an entry for " + filename);

			return new MemoryStream(FileData[filename]);
		}

		public bool Exists(string filename)
		{
			return FileData.ContainsKey(filename);
		}

		public IEnumerable<uint> ClassicHashes()
		{
			return FileData.Keys.Select(filename => PackageEntry.HashFilename(filename, PackageHashType.Classic));
		}

		public IEnumerable<uint> CrcHashes()
		{
			return Enumerable.Empty<uint>();
		}

		public IEnumerable<string> AllFileNames()
		{
			return FileData.Keys;
		}

		public void Write(Dictionary<string, byte[]> contents)
		{
			throw new NotImplementedException();
		}

		public void Dispose()
		{
			stream.Dispose();
		}
	}

	public class DrsHeader
	{
		public readonly string Copyright;
		public readonly string DrsVersion;
		public readonly string DrsType;
		public readonly int TableCount;
		public readonly int FirstTableOffset;

		public readonly string GameVersion;

		public DrsHeader(Stream stream)
		{
			var pos = stream.Position;
			stream.Seek(64, SeekOrigin.Begin);

			var temp = stream.ReadASCII(4);
			var copyrightSize = temp == "swgb" ? 60 : 40;
			GameVersion = temp == "swgb" ? temp : "aoe";
			stream.Seek(pos, SeekOrigin.Begin);

			Copyright = stream.ReadASCII(copyrightSize);
			DrsVersion = stream.ReadASCII(4);
			DrsType = stream.ReadASCII(12);
			TableCount = stream.ReadInt32();
			FirstTableOffset = stream.ReadInt32();
		}
	}

	public class DrsTable
	{
		public readonly string Filetype;
		public readonly int DataOffset;
		public readonly int FileCount;

		public DrsTable(Stream stream)
		{
			stream.Position += 1;
			Filetype = stream.ReadASCII(3);
			DataOffset = stream.ReadInt32();
			FileCount = stream.ReadInt32();
		}
	}

	public class DrsFile
	{
		public readonly int FileID;
		public readonly int DataOffset;
		public readonly int FileSize;

		public readonly byte[] Data;
		public readonly string GeneratedName;

		public DrsFile(Stream stream, DrsTable table)
		{
			FileID = stream.ReadInt32();
			DataOffset = stream.ReadInt32();
			FileSize = stream.ReadInt32();

			var pos = stream.Position;
			stream.Seek(DataOffset, SeekOrigin.Begin);
			Data = stream.ReadBytes(FileSize);
			stream.Seek(pos, SeekOrigin.Begin);

			GeneratedName = FileID.ToString() + table.Filetype;
		}
	}
}
