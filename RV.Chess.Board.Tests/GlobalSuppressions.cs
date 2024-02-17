// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage(
    "Major Code Smell",
    "S4144:Methods should not have identical implementations",
    Justification = "Does not consider different inline test data",
    Scope = "module")]
