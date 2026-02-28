using System;
using System.IO;

namespace liquidclient.Classes
{
	// Token: 0x02000045 RID: 69
	public class WAV
	{
		// Token: 0x060002A4 RID: 676 RVA: 0x0001BD90 File Offset: 0x00019F90
		public WAV(byte[] wavFile)
		{
			using (MemoryStream memoryStream = new MemoryStream(wavFile))
			{
				using (BinaryReader binaryReader = new BinaryReader(memoryStream))
				{
					binaryReader.ReadBytes(22);
					this.ChannelCount = binaryReader.ReadInt16();
					this.Frequency = binaryReader.ReadInt32();
					binaryReader.ReadBytes(6);
					int num = (int)binaryReader.ReadInt16();
					string text = new string(binaryReader.ReadChars(4));
					while (text != "data")
					{
						int num2 = binaryReader.ReadInt32();
						binaryReader.ReadBytes(num2);
						text = new string(binaryReader.ReadChars(4));
					}
					int num3 = binaryReader.ReadInt32();
					byte[] array = binaryReader.ReadBytes(num3);
					int num4 = num / 8;
					this.SampleCount = num3 / num4 / this.ChannelCount;
					float[] array2 = new float[this.SampleCount * this.ChannelCount];
					int num5 = 0;
					for (int i = 0; i < array2.Length; i++)
					{
						int num6 = num4;
						int num7 = num6;
						bool flag = num7 != 1;
						if (flag)
						{
							bool flag2 = num7 == 2;
							if (flag2)
							{
								short num8 = BitConverter.ToInt16(array, num5);
								array2[i] = (float)num8 / 32768f;
								num5 += 2;
							}
						}
						else
						{
							array2[i] = (float)(array[num5] - 128) / 128f;
							num5++;
						}
					}
					bool flag3 = this.ChannelCount == 2;
					bool flag4 = flag3;
					if (flag4)
					{
						this.LeftChannel = new float[this.SampleCount];
						this.RightChannel = new float[this.SampleCount];
						for (int j = 0; j < this.SampleCount; j++)
						{
							this.LeftChannel[j] = array2[j * 2];
							this.RightChannel[j] = array2[j * 2 + 1];
						}
					}
					else
					{
						this.LeftChannel = array2;
						this.RightChannel = null;
					}
				}
			}
		}

		// Token: 0x1700003A RID: 58
		// (get) Token: 0x060002A5 RID: 677 RVA: 0x0001BFC0 File Offset: 0x0001A1C0
		public float[] LeftChannel { get; }

		// Token: 0x1700003B RID: 59
		// (get) Token: 0x060002A6 RID: 678 RVA: 0x0001BFC8 File Offset: 0x0001A1C8
		public float[] RightChannel { get; }

		// Token: 0x1700003C RID: 60
		// (get) Token: 0x060002A7 RID: 679 RVA: 0x0001BFD0 File Offset: 0x0001A1D0
		public int ChannelCount { get; }

		// Token: 0x1700003D RID: 61
		// (get) Token: 0x060002A8 RID: 680 RVA: 0x0001BFD8 File Offset: 0x0001A1D8
		public int SampleCount { get; }

		// Token: 0x1700003E RID: 62
		// (get) Token: 0x060002A9 RID: 681 RVA: 0x0001BFE0 File Offset: 0x0001A1E0
		// (set) Token: 0x060002AA RID: 682 RVA: 0x0001BFE8 File Offset: 0x0001A1E8
		public int Frequency { get; private set; }
	}
}
