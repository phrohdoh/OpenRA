#region Copyright & License Information
/*
 * Copyright 2013-2020 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using OpenRA.Graphics;
using OpenRA.Mods.Cnc.FileFormats;
using OpenRA.Traits;

namespace OpenRA.Mods.Cnc.Traits
{
	public class VoxelNormalsPaletteInfo : TraitInfo
	{
		[PaletteDefinition]
		public readonly string Name = "normals";

		[Desc("Can be TiberianSun or RedAlert2")]
		public readonly NormalType Type = NormalType.TiberianSun;

		public override object Create(ActorInitializer init) { return new VoxelNormalsPalette(this); }
	}

	public class VoxelNormalsPalette : ILoadsPalettes
	{
		readonly VoxelNormalsPaletteInfo info;

		public VoxelNormalsPalette(VoxelNormalsPaletteInfo info)
		{
			this.info = info;
		}

		public void LoadPalettes(WorldRenderer wr)
		{
			// Rotate vectors to expected orientation
			// Voxel coordinates are x=forward, y=right, z=up
			var channel = new int[] { 2, 1, 0 };
			var n = info.Type == NormalType.RedAlert2 ? RA2Normals : TSNormals;

			// Map normals into color range
			// Introduces a maximum error of ~0.5%
			var data = new uint[Palette.Size];
			for (var i = 0; i < n.Length / 3; i++)
			{
				data[i] = 0xFF000000;
				for (var j = 0; j < 3; j++)
				{
					var t = (n[3 * i + j] + 1) / 2;
					data[i] |= (uint)((byte)(t * 0xFF + 0.5) << (8 * channel[j]));
				}
			}

			wr.AddPalette(info.Name, new ImmutablePalette(data));
		}

		// Normal vector tables from http://www.sleipnirstuff.com/forum/viewtopic.php?t=8048
		static readonly float[] TSNormals =
		{
			0.671214f,  0.198492f, -0.714194f,
			0.269643f,  0.584394f, -0.765360f,
			-0.040546f,  0.096988f, -0.994459f,
			-0.572428f, -0.091914f, -0.814787f,
			-0.171401f, -0.572710f, -0.801639f,
			0.362557f, -0.302999f, -0.881331f,
			0.810347f, -0.348972f, -0.470698f,
			0.103962f,  0.938672f, -0.328767f,
			-0.324047f,  0.587669f, -0.741376f,
			-0.800865f,  0.340461f, -0.492647f,
			-0.665498f, -0.590147f, -0.456989f,
			0.314767f, -0.803002f, -0.506073f,
			0.972629f,  0.151076f, -0.176550f,
			0.680291f,  0.684236f, -0.262727f,
			-0.520079f,  0.827777f, -0.210483f,
			-0.961644f, -0.179001f, -0.207847f,
			-0.262714f, -0.937451f, -0.228401f,
			0.219707f, -0.971301f,  0.091125f,
			0.923808f, -0.229975f,  0.306087f,
			-0.082489f,  0.970660f,  0.225866f,
			-0.591798f,  0.696790f,  0.405289f,
			-0.925296f,  0.366601f,  0.097111f,
			-0.705051f, -0.687775f,  0.172828f,
			0.732400f, -0.680367f, -0.026305f,
			0.855162f,  0.374582f,  0.358311f,
			0.473006f,  0.836480f,  0.276705f,
			-0.097617f,  0.654112f,  0.750072f,
			-0.904124f, -0.153725f,  0.398658f,
			-0.211916f, -0.858090f,  0.467732f,
			0.500227f, -0.674408f,  0.543091f,
			0.584539f, -0.110249f,  0.803841f,
			0.437373f,  0.454644f,  0.775889f,
			-0.042441f,  0.083318f,  0.995619f,
			-0.596251f,  0.220132f,  0.772028f,
			-0.506455f, -0.396977f,  0.765449f,
			0.070569f, -0.478474f,  0.875262f
		};

		static readonly float[] RA2Normals =
		{
			0.526578f, -0.359621f, -0.770317f,
			0.150482f, 0.435984f, 0.887284f,
			0.414195f, 0.738255f, -0.532374f,
			0.075152f, 0.916249f, -0.393498f,
			-0.316149f, 0.930736f, -0.183793f,
			-0.773819f, 0.623334f, -0.112510f,
			-0.900842f, 0.428537f, -0.069568f,
			-0.998942f, -0.010971f, 0.044665f,
			-0.979761f, -0.157670f, -0.123324f,
			-0.911274f, -0.362371f, -0.195620f,
			-0.624069f, -0.720941f, -0.301301f,
			-0.310173f, -0.809345f, -0.498752f,
			0.146613f, -0.815819f, -0.559414f,
			-0.716516f, -0.694356f, -0.066888f,
			0.503972f, -0.114202f, -0.856137f,
			0.455491f, 0.872627f, -0.176211f,
			-0.005010f, -0.114373f, -0.993425f,
			-0.104675f, -0.327701f, -0.938965f,
			0.560412f, 0.752589f, -0.345756f,
			-0.060576f, 0.821628f, -0.566796f,
			-0.302341f, 0.797007f, -0.522847f,
			-0.671543f, 0.670740f, -0.314863f,
			-0.778401f, -0.128357f, 0.614505f,
			-0.924050f, 0.278382f, -0.261985f,
			-0.699773f, -0.550491f, -0.455278f,
			-0.568248f, -0.517189f, -0.640008f,
			0.054098f, -0.932864f, -0.356143f,
			0.758382f, 0.572893f, -0.310888f,
			0.003620f, 0.305026f, -0.952337f,
			-0.060850f, -0.986886f, -0.149511f,
			0.635230f, 0.045478f, -0.770983f,
			0.521705f, 0.241309f, -0.818287f,
			0.269404f, 0.635425f, -0.723641f,
			0.045676f, 0.672754f, -0.738455f,
			-0.180511f, 0.674657f, -0.715719f,
			-0.397131f, 0.636640f, -0.661042f,
			-0.552004f, 0.472515f, -0.687038f,
			-0.772170f, 0.083090f, -0.629960f,
			-0.669819f, -0.119533f, -0.732840f,
			-0.540455f, -0.318444f, -0.778782f,
			-0.386135f, -0.522789f, -0.759994f,
			-0.261466f, -0.688567f, -0.676395f,
			-0.019412f, -0.696103f, -0.717680f,
			0.303569f, -0.481844f, -0.821993f,
			0.681939f, -0.195129f, -0.704900f,
			-0.244889f, -0.116562f, -0.962519f,
			0.800759f, -0.022979f, -0.598546f,
			-0.370275f, 0.095584f, -0.923991f,
			-0.330671f, -0.326578f, -0.885440f,
			-0.163220f, -0.527579f, -0.833679f,
			0.126390f, -0.313146f, -0.941257f,
			0.349548f, -0.272226f, -0.896498f,
			0.239918f, -0.085825f, -0.966992f,
			0.390845f, 0.081537f, -0.916838f,
			0.255267f, 0.268697f, -0.928785f,
			0.146245f, 0.480438f, -0.864749f,
			-0.326016f, 0.478456f, -0.815349f,
			-0.469682f, -0.112519f, -0.875636f,
			0.818440f, -0.258520f, -0.513151f,
			-0.474318f, 0.292238f, -0.830433f,
			0.778943f, 0.395842f, -0.486371f,
			0.624094f, 0.393773f, -0.674870f,
			0.740886f, 0.203834f, -0.639953f,
			0.480217f, 0.565768f, -0.670297f,
			0.380930f, 0.424535f, -0.821378f,
			-0.093422f, 0.501124f, -0.860318f,
			-0.236485f, 0.296198f, -0.925387f,
			-0.131531f, 0.093959f, -0.986849f,
			-0.823562f, 0.295777f, -0.484006f,
			0.611066f, -0.624304f, -0.486664f,
			0.069496f, -0.520330f, -0.851133f,
			0.226522f, -0.664879f, -0.711775f,
			0.471308f, -0.568904f, -0.673957f,
			0.388425f, -0.742624f, -0.545560f,
			0.783675f, -0.480729f, -0.393385f,
			0.962394f, 0.135676f, -0.235349f,
			0.876607f, 0.172034f, -0.449406f,
			0.633405f, 0.589793f, -0.500941f,
			0.182276f, 0.800658f, -0.570721f,
			0.177003f, 0.764134f, 0.620297f,
			-0.544016f, 0.675515f, -0.497721f,
			-0.679297f, 0.286467f, -0.675642f,
			-0.590391f, 0.091369f, -0.801929f,
			-0.824360f, -0.133124f, -0.550189f,
			-0.715794f, -0.334542f, -0.612961f,
			0.174286f, -0.892484f, 0.416049f,
			-0.082528f, -0.837123f, -0.540753f,
			0.283331f, -0.880874f, -0.379189f,
			0.675134f, -0.426627f, -0.601817f,
			0.843720f, -0.512335f, -0.160156f,
			0.977304f, -0.098556f, -0.187520f,
			0.846295f, 0.522672f, -0.102947f,
			0.677141f, 0.721325f, -0.145501f,
			0.320965f, 0.870892f, -0.372194f,
			-0.178978f, 0.911533f, -0.370236f,
			-0.447169f, 0.826701f, -0.341474f,
			-0.703203f, 0.496328f, -0.509081f,
			-0.977181f, 0.063563f, -0.202674f,
			-0.878170f, -0.412938f, 0.241455f,
			-0.835831f, -0.358550f, -0.415728f,
			-0.499174f, -0.693433f, -0.519592f,
			-0.188789f, -0.923753f, -0.333225f,
			0.192254f, -0.969361f, -0.152896f,
			0.515940f, -0.783907f, -0.345392f,
			0.905925f, -0.300952f, -0.297871f,
			0.991112f, -0.127746f, 0.037107f,
			0.995135f, 0.098424f, -0.004383f,
			0.760123f, 0.646277f, 0.067367f,
			0.205221f, 0.959580f, -0.192591f,
			-0.042750f, 0.979513f, -0.196791f,
			-0.438017f, 0.898927f, 0.008492f,
			-0.821994f, 0.480785f, -0.305239f,
			-0.899917f, 0.081710f, -0.428337f,
			-0.926612f, -0.144618f, -0.347096f,
			-0.793660f, -0.557792f, -0.242839f,
			-0.431350f, -0.847779f, -0.308558f,
			-0.005492f, -0.965000f, 0.262193f,
			0.587905f, -0.804026f, -0.088940f,
			0.699493f, -0.667686f, -0.254765f,
			0.889303f, 0.359795f, -0.282291f,
			0.780972f, 0.197037f, 0.592672f,
			0.520121f, 0.506696f, 0.687557f,
			0.403895f, 0.693961f, 0.596060f,
			-0.154983f, 0.899236f, 0.409090f,
			-0.657338f, 0.537168f, 0.528543f,
			-0.746195f, 0.334091f, 0.575827f,
			-0.624952f, -0.049144f, 0.779115f,
			0.318141f, -0.254715f, 0.913185f,
			-0.555897f, 0.405294f, 0.725752f,
			-0.794434f, 0.099406f, 0.599160f,
			-0.640361f, -0.689463f, 0.338495f,
			-0.126713f, -0.734095f, 0.667120f,
			0.105457f, -0.780817f, 0.615795f,
			0.407993f, -0.480916f, 0.776055f,
			0.695136f, -0.545120f, 0.468647f,
			0.973191f, -0.006489f, 0.229908f,
			0.946894f, 0.317509f, -0.050799f,
			0.563583f, 0.825612f, 0.027183f,
			0.325773f, 0.945423f, 0.006949f,
			-0.171821f, 0.985097f, -0.007815f,
			-0.670441f, 0.739939f, 0.054769f,
			-0.822981f, 0.554962f, 0.121322f,
			-0.966193f, 0.117857f, 0.229307f,
			-0.953769f, -0.294704f, 0.058945f,
			-0.864387f, -0.502728f, -0.010015f,
			-0.530609f, -0.842006f, -0.097366f,
			-0.162618f, -0.984075f, 0.071772f,
			0.081447f, -0.996011f, 0.036439f,
			0.745984f, -0.665963f, 0.000762f,
			0.942057f, -0.329269f, -0.064106f,
			0.939702f, -0.281090f, 0.194803f,
			0.771214f, 0.550670f, 0.319363f,
			0.641348f, 0.730690f, 0.234021f,
			0.080682f, 0.996691f, 0.009879f,
			-0.046725f, 0.976643f, 0.209725f,
			-0.531076f, 0.821001f, 0.209562f,
			-0.695815f, 0.655990f, 0.292435f,
			-0.976122f, 0.216709f, -0.014913f,
			-0.961661f, -0.144129f, 0.233314f,
			-0.772084f, -0.613647f, 0.165299f,
			-0.449600f, -0.836060f, 0.314426f,
			-0.392700f, -0.914616f, 0.096247f,
			0.390589f, -0.919470f, 0.044890f,
			0.582529f, -0.799198f, 0.148127f,
			0.866431f, -0.489812f, 0.096864f,
			0.904587f, 0.111498f, 0.411450f,
			0.953537f, 0.232330f, 0.191806f,
			0.497311f, 0.770803f, 0.398177f,
			0.194066f, 0.956320f, 0.218611f,
			0.422876f, 0.882276f, 0.206797f,
			-0.373797f, 0.849566f, 0.372174f,
			-0.534497f, 0.714023f, 0.452200f,
			-0.881827f, 0.237160f, 0.407598f,
			-0.904948f, -0.014069f, 0.425289f,
			-0.751827f, -0.512817f, 0.414458f,
			-0.501015f, -0.697917f, 0.511758f,
			-0.235190f, -0.925923f, 0.295555f,
			0.228983f, -0.953940f, 0.193819f,
			0.734025f, -0.634898f, 0.241062f,
			0.913753f, -0.063253f, -0.401316f,
			0.905735f, -0.161487f, 0.391875f,
			0.858930f, 0.342446f, 0.380749f,
			0.624486f, 0.607581f, 0.490777f,
			0.289264f, 0.857479f, 0.425508f,
			0.069968f, 0.902169f, 0.425671f,
			-0.286180f, 0.940700f, 0.182165f,
			-0.574013f, 0.805119f, -0.149309f,
			0.111258f, 0.099718f, -0.988776f,
			-0.305393f, -0.944228f, -0.123160f,
			-0.601166f, -0.789576f, 0.123163f,
			-0.290645f, -0.812140f, 0.505919f,
			-0.064920f, -0.877163f, 0.475785f,
			0.408301f, -0.862216f, 0.299789f,
			0.566097f, -0.725566f, 0.391264f,
			0.839364f, -0.427387f, 0.335869f,
			0.818900f, -0.041305f, 0.572448f,
			0.719784f, 0.414997f, 0.556497f,
			0.881744f, 0.450270f, 0.140659f,
			0.401823f, -0.898220f, -0.178152f,
			-0.054020f, 0.791344f, 0.608980f,
			-0.293774f, 0.763994f, 0.574465f,
			-0.450798f, 0.610347f, 0.651351f,
			-0.638221f, 0.186694f, 0.746873f,
			-0.872870f, -0.257127f, 0.414708f,
			-0.587257f, -0.521710f, 0.618828f,
			-0.353658f, -0.641974f, 0.680291f,
			0.041649f, -0.611273f, 0.790323f,
			0.348342f, -0.779183f, 0.521087f,
			0.499167f, -0.622441f, 0.602826f,
			0.790019f, -0.303831f, 0.532500f,
			0.660118f, 0.060733f, 0.748702f,
			0.604921f, 0.294161f, 0.739960f,
			0.385697f, 0.379346f, 0.841032f,
			0.239693f, 0.207876f, 0.948332f,
			0.012623f, 0.258532f, 0.965920f,
			-0.100557f, 0.457147f, 0.883688f,
			0.046967f, 0.628588f, 0.776319f,
			-0.430391f, -0.445405f, 0.785097f,
			-0.434291f, -0.196228f, 0.879139f,
			-0.256637f, -0.336867f, 0.905902f,
			-0.131372f, -0.158910f, 0.978514f,
			0.102379f, -0.208767f, 0.972592f,
			0.195687f, -0.450129f, 0.871258f,
			0.627319f, -0.423148f, 0.653771f,
			0.687439f, -0.171583f, 0.705682f,
			0.275920f, -0.021255f, 0.960946f,
			0.459367f, 0.157466f, 0.874178f,
			0.285395f, 0.583184f, 0.760556f,
			-0.812174f, 0.460303f, 0.358461f,
			-0.189068f, 0.641223f, 0.743698f,
			-0.338875f, 0.476480f, 0.811252f,
			-0.920994f, 0.347186f, 0.176727f,
			0.040639f, 0.024465f, 0.998874f,
			-0.739132f, -0.353747f, 0.573190f,
			-0.603512f, -0.286615f, 0.744060f,
			-0.188676f, -0.547059f, 0.815554f,
			-0.026045f, -0.397820f, 0.917094f,
			0.267897f, -0.649041f, 0.712023f,
			0.518246f, -0.284891f, 0.806386f,
			0.493451f, -0.066533f, 0.867225f,
			-0.328188f, 0.140251f, 0.934143f,
			-0.328188f, 0.140251f, 0.934143f,
			-0.328188f, 0.140251f, 0.934143f,
			-0.328188f, 0.140251f, 0.934143f
		};
	}
}
