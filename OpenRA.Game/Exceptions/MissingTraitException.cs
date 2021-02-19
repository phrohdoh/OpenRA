namespace OpenRA.Exceptions
{
	using System;
	public class MissingTraitException : Exception
	{
		Type traitType;

		public MissingTraitException(
			string actorTypeName,
			Type traitType
		) : base(
				"Actor `{0}` is missing trait `{1}` (`{2}`)".F(
					actorTypeName,
					traitType.Name.Substring(0, traitType.Name.Length - 4),
					traitType.FullName
				)
			)
		{
			this.traitType = traitType;
		}
	}
}
