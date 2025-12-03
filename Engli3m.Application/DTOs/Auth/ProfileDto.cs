using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engli3m.Application.DTOs.Auth
{
    public class ProfileDto
    {
        public IFormFile? ProfileImage { get; set; }
    }
}
