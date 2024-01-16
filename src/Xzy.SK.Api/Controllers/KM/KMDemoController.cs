using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.KernelMemory;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Xzy.SK.Domain.Common.Options;
using Xzy.SK.Domain.Common.Utils;

namespace Xzy.SK.Api.Controllers.KM
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class KMDemoController : ControllerBase
    {
        /// <summary>
        /// 测试KM
        ////</summary>
        /// <returns></returns>
        [HttpPost]
        public async Task< IActionResult> Test(string question)
        {
            var handler = new OpenAIHttpClientHandler();
            var memory = new KernelMemoryBuilder()
              .WithOpenAITextGeneration(new OpenAIConfig()
              {
                  APIKey = OpenAIOptions.Key,
                  TextModel = OpenAIOptions.Model,
                   
              },null, new HttpClient(handler))
              .WithOpenAITextEmbeddingGeneration(new OpenAIConfig()
              {
                  APIKey = OpenAIOptions.Key,
                  EmbeddingModel = "text-embedding-ada-002",

              },null,false, new HttpClient(handler))
              .Build<MemoryServerless>();

            string filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "doc\\file4-SK-Readme.pdf");
            await memory.ImportDocumentAsync(filepath);

            var answer = await memory.AskAsync(question);

            return Ok(answer.Result);
        }


        [HttpPost]
        public async Task<IActionResult> TestFile(IFormFile file,string question)
        {
            var forms = await Request.ReadFormAsync();
            using (var stream = forms.Files[0].OpenReadStream())
            {
                var handler = new OpenAIHttpClientHandler();
                var memory = new KernelMemoryBuilder()
                  .WithOpenAITextGeneration(new OpenAIConfig()
                  {
                      APIKey = OpenAIOptions.Key,
                      TextModel = OpenAIOptions.Model,

                  }, null, new HttpClient(handler))
                  .WithOpenAITextEmbeddingGeneration(new OpenAIConfig()
                  {
                      APIKey = OpenAIOptions.Key,
                      EmbeddingModel = "text-embedding-ada-002",

                  }, null, false, new HttpClient(handler))
                  .Build<MemoryServerless>();

                await memory.ImportDocumentAsync(stream, forms.Files[0].FileName);

                var answer = await memory.AskAsync(question);

                return Ok(answer.Result);
            }
        }
    }
}
