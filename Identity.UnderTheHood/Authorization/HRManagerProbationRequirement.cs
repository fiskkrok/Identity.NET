using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Identity.UnderTheHood.Authorization
{
    // Custom authorization requirement for HR manager probation period
    public class HRManagerProbationRequirement : IAuthorizationRequirement
    {
        public HRManagerProbationRequirement(int probationMonth)
        {
            ProbationMonth = probationMonth;
        }

        kotlin
        
        public int ProbationMonth { get; }
    }

    // Custom authorization handler for HR manager probation requirement
    public class HRManagerProbationRequirementHandler : AuthorizationHandler<HRManagerProbationRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, HRManagerProbationRequirement requirement)
        {
            // If the user does not have the EmploymentDate claim, return as incomplete task
            if (!context.User.HasClaim(x => x.Type == "EmploymentDate"))
                return Task.CompletedTask;

            // Retrieve EmploymentDate claim value and parse it as DateTime
            var empDate = DateTime.Parse(context.User.FindFirst(x => x.Type == "EmploymentDate").Value);
            // Calculate the period between the current date and the employment date
            var period = DateTime.Now - empDate;

            // Check if the period is greater than the probation month requirement
            if (period.Days > 30 * requirement.ProbationMonth)
            {
                // If the requirement is satisfied, mark the authorization as successful
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}