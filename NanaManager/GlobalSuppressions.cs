// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage( "Design", "CA1031:Do not catch general exception types", Justification = "There is no easy way to handle all errors here, and I can't even find a reference to which errors come out of DragDrop.DoDragDrop() so I'm just swallowing them all as a patch until a rework is made", Scope = "member", Target = "~M:NanaManager.TagManager.gb_MouseMove(System.Object,System.Windows.Input.MouseEventArgs)" )]
