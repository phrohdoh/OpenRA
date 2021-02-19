namespace OpenRA.Exceptions
{
	using System;
	public class MissingTraitException : Exception
	{
		Type traitType;
		InvalidOperationException original;

		public MissingTraitException(
			string actorTypeName,
			Type traitType,
			InvalidOperationException original
		) : base(
				"Actor `{0}` is missing trait `{1}`: {2}".F(
					actorTypeName,
					traitType.Name.Substring(0, traitType.Name.Length - 4),
					original.Message
				)
			)
		{
			this.traitType = traitType;
			this.original = original;
		}
	}
}
