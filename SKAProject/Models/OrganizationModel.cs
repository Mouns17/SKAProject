using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKAProject.Models
{
    public class OrganizationModel
    {
        public string OrgName { get; set; }
        public string OrgAddress { get; set; }
        public string OrgPhone { get; set; }
        public string OrgEmail { get; set; }

        internal async Task GetOrganizationAsync()
        {
            throw new NotImplementedException();
        }
    }
}
