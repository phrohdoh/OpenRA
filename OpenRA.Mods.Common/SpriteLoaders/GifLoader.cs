using System;
using System.IO;
using OpenRA.Graphics;
using System.Drawing;
using System.Collections.Generic;

namespace OpenRA.Mods.Common.SpriteLoader
{
	public class FrameHeader
	{
		byte[] leftPosition; 
		byte[] rightPosition; 
		byte[] imageWidth; 
		byte[] imageHeight; 
		byte bitField; 
	}

	public class GifLoader : ISpriteLoader
	{
		const byte gTypeImageBlock = 0x2c;
		const byte gTypeTerminator = 0x3b;
		const byte gTypeExtension = 0x21;

		public static int GetSizeOfColorTable(int definedSize)
		{
			if (definedSize > 0)
				return (int)Math.Pow(definedSize + 1, 2);

			return 0;
		}

		public class GifHeader
		{
			public readonly string Signature;
			public readonly string Version;

			public GifHeader(string signature, string version)
			{
				Signature = signature;
				Version = version;
			}
		}

		public class LogicalScreenDescriptor
		{
			public ushort LogicalScreenWidth;
			public ushort LogicalScreenHeight;

			public bool HasFollowingGCT;
			public int ColorResolution;
			public bool IsSorted;
			public int SizeOfGCT;
			public int RealSizeOfGCT;

			public byte BackgroundIndex;
			public float PixelAspectRatio;

			public LogicalScreenDescriptor(ushort w, ushort h, byte packedField, byte backgroundIndex, byte aspectRatio)
			{
				LogicalScreenWidth = w;
				LogicalScreenHeight = h;

				HasFollowingGCT = packedField >> 7 == 1;
				ColorResolution = (packedField >> 4) & 0x7;
				IsSorted = ((packedField >> (8 - 5)) & 1) == 1;
				SizeOfGCT = packedField & 7;
				RealSizeOfGCT = GetSizeOfColorTable(SizeOfGCT);

				BackgroundIndex = backgroundIndex;
				PixelAspectRatio = aspectRatio;

				if (Math.Abs(PixelAspectRatio) > float.Epsilon)
					PixelAspectRatio = (PixelAspectRatio + 15) / 64;
			}
		}

		class GifFrame : ISpriteFrame
		{
			byte[] ISpriteFrame.Data { get { throw new NotImplementedException(); } }
			bool ISpriteFrame.DisableExportPadding { get { throw new NotImplementedException(); } }
			Size ISpriteFrame.FrameSize { get { throw new NotImplementedException(); } }
			float2 ISpriteFrame.Offset { get { throw new NotImplementedException(); } }
			Size ISpriteFrame.Size { get { throw new NotImplementedException(); } }

			public GifFrame(Stream s)
			{
				var offset = new float2(s.ReadUInt16(), s.ReadUInt16());
				var size = new Size(s.ReadUInt16(), s.ReadUInt16());

				var packedField = s.ReadUInt8();
				var hasLct = (packedField & 128) == 128;
				var isInterlaced = (packedField & 64) == 64;
				var isSorted = (packedField & 32) == 32;
				var sizeOfLCT = (packedField << 5) >> 5;
				var realSizeOfLCT = GetSizeOfColorTable(sizeOfLCT);

				// Again, skip palettes defined in the file.
				if (hasLct)
					s.Position += realSizeOfLCT;

				// After the LCT comes the raw image data.
				var minCodeSize = s.ReadUInt8();
				var dataLen = s.ReadUInt8();
				var data = s.ReadBytes(dataLen);
				var zero = s.ReadUInt8();

				if (zero != 0)
					throw new InvalidDataException("Something went wrong!");

				// TODO
			}
		}

		bool ISpriteLoader.TryParseSprite(Stream s, out ISpriteFrame[] frames)
		{
			// http://www.w3.org/Graphics/GIF/spec-gif89a.txt
			// http://www.matthewflickinger.com/lab/whatsinagif/bits_and_bytes.asp
			// http://www.commandlinefanatic.com/cgi-bin/showarticle.cgi?article=art011
			// http://k1.spdns.de/Develop/Libraries/gif/Info/Downs.GIFDecoder.pdf

			var header = new GifHeader(s.ReadASCII(3), s.ReadASCII(3));

			var lsd = new LogicalScreenDescriptor(s.ReadUInt16(), s.ReadUInt16(), s.ReadUInt8(), s.ReadUInt8(), s.ReadUInt8());

			// Skip the GCT entirely, we have our own palettes.
			s.Position += lsd.RealSizeOfGCT;

			var frameList = new List<GifFrame>();

			// This is where the frame data starts.
			while (s.ReadUInt8() == 0x2c)
				frameList.Add(new GifFrame(s));
		}
	}
}
