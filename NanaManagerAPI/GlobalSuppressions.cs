// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage( "Design", "CA1031:Do not catch general exception types", Justification = "The error thrown will always be swallowed, because it is used as a checker until a better method is found", Scope = "member", Target = "~M:NanaManagerAPI.IO.ContentFile.CheckValidity~System.Boolean" )]
[assembly: SuppressMessage( "Style", "IDE1006:Naming Styles", Justification = "Readonly is used as an efficiency option, not as a constant definer", Scope = "member", Target = "~F:NanaManagerAPI.UI.Paging.history" )]
[assembly: SuppressMessage( "Style", "IDE1006:Naming Styles", Justification = "Readonly is used as an efficiency option, not as a constant definer", Scope = "member", Target = "~F:NanaManagerAPI.UI.Paging.pages" )]