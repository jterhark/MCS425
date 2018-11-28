using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MCS425_Demo.Models.JwtModels
{
    public class IndexModel
    {

        public string Token { get; set; }

        
        public string Site { get; set; }

        public IEnumerable<SelectListItem> Sites  {get ;set;}

        [Required]
        [Range(5, 120)]
        public int Minutes { get; set; }
    }
}
