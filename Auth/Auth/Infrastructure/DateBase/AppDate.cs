namespace Auth.Infrastructure.DateBase
{
    public static partial class AppData
    {
        /// <summary>
        /// "SuperSystemAdministrator"
        /// </summary>
        public const string SuperSystemAdministratorRoleName = "SuperSystemAdministrator";

        /// <summary>
        /// "SystemAdministrator"
        /// </summary>
        public const string SystemAdministratorRoleName = "SystemAdministrator";

        /// <summary>
        /// "AccountExecutive"
        /// </summary>
        public const string AccountExecutiveRoleName = "AccountExecutive";

        /// <summary>
        /// "BusinessOwner"
        /// </summary>
        public const string BusinessOwnerRoleName = "BusinessOwner";

        /// <summary>
        /// "Manager"
        /// </summary>
        public const string ManagerRoleName = "Manager";

        /// <summary>
        /// Roles
        /// </summary>
        public static IEnumerable<string> Roles
        {
            get
            {
                yield return SuperSystemAdministratorRoleName;
                yield return SystemAdministratorRoleName;
                yield return AccountExecutiveRoleName;
                yield return BusinessOwnerRoleName;
                yield return ManagerRoleName;
            }
        }

        /// <summary>
        /// IdentityServer4 path
        /// </summary>
        public const string AuthUrl = "/auth";
    }
}
