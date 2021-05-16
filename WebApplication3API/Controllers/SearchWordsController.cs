using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using ConsoleTables;
using System.Linq;
using System.Collections.Generic;
using System.Web;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace WebApplication2.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SearchWordsController : ControllerBase
    {
        IConfiguration conf;


        public SearchWordsController(IConfiguration configuration)
        {
            conf = configuration;
        }

        [HttpGet]
        public IEnumerable<SearchElements> Get([FromQuery(Name = "words")] string[] words)
        {
           
            ActThirteen act13 = new ActThirteen();
            string fullPath = conf.GetValue<string>(WebHostDefaults.ContentRootKey);
            string myurl = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}/files/";
            string[] wordsToFind = new string[]{
            "library"
                };
            var test = act13.executeProgram(fullPath, words, true, myurl);
            var rng = new Random();
            return Enumerable.Range(0, test.Count-1).Select(index => new SearchElements
            {   
                filePath = (KeyValuePair<string,int>)test.ToArray().GetValue(index)
            })
            .ToArray();
        }
    }
}
