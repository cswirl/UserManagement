using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Constants
{
    public static class ClaimNames
    {
        public static readonly string MembershipTier = "MembershipTier";

        public enum Role
        {
            Admin,
            Moderator,
            Member
        }

        public enum MembershipTierEnum
        {
            Silver,
            Gold,
            Platinum
        }
    }
}
