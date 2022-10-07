using System;

using Modding;

using Osmi.Utils;

namespace NoParticles;

public sealed partial class NoParticles : Mod {
	private static readonly Lazy<string> version = AssemblyUtil
#if DEBUG
		.GetMyDefaultVersionWithHash();
#else
		.GetMyDefaultVersion();
#endif

	public override string GetVersion() => version.Value;

	public override void Initialize() {
	}
}
