namespace NanaManager
{
#pragma warning disable IDE1006 // Naming Styles because const wants all caps in my VS, and I want these to basically be string enums which aren't caps

    internal static class Pages
    {
        public const string Import = "hydroxa.nanaManager:pg_in";
        public const string Error = "hydroxa.nanaManager:pg_err";
        public const string Login = "hydroxa.nanaManager:pg_login";
        public const string Viewer = "hydroxa.nanaManager:pg_view";
        public const string Welcome = "hydroxa.nanaManager:pg_load";
        public const string Register = "hydroxa.nanaManager:pg_reg";
        public const string Search = "hydroxa.nanaManager:pg_search";
        public const string Settings = "hydroxa.nanaManager:pg_sets";
        public const string Fullscreen = "hydroxa.nanaManager:pg_fsc";
        public const string ComingSoon = "hydroxa.nanaManager:pg_soon";
        public const string TagManager = "hydroxa.nanaManager:pg_tagMan";

        public const string ThemesAndColoursSettings = "hydroxa.nanaManager:pg_tacSets";
        public const string InvalidSettings = "hydroxa.nanaManager:pg_invalidSets";
        public const string LanguagesSettings = "hydroxa.nanaManager:pg_langSets";
        public const string AdvancedSettings = "hydroxa.nanaManager:pg_advSets";
        public const string PluginsSettings = "hydroxa.nanaManager:pg_plugSets";
        public const string SoonSettings = "hydroxa.nanaManager:pg_soonSets";
        public const string TagsSettings = "hydroxa.nanaManager:pg_tagSets";
    }

#pragma warning restore IDE1006 // Naming Styles
}