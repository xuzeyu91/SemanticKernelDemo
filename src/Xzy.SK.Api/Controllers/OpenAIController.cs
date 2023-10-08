using Azure.AI.OpenAI;
using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using System;
using System.IO;
using System.Threading.Tasks;
using Xzy.SK.Domain.Common.Options;
using Xzy.SK.Domain.Domain.DTO.Chat;
using System.Linq;
using System.Collections.Generic;

namespace Xzy.SK.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OpenAIController : ControllerBase
    {
        public OpenAIController()
        {

        }

        /// <summary>
        /// OpenAi测试翻译，可以对比与SK写法差异
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Translate(string msg)
        {
            string prompt = $"Translate this into English:\r\n{msg}\r\n";

            OpenAIClient client = new OpenAIClient(new Uri(OpenAIOptions.Endpoint), new AzureKeyCredential(OpenAIOptions.Key), new OpenAIClientOptions());
            ChatCompletionsOptions completionsOptions = new ChatCompletionsOptions();
   
            completionsOptions.Messages.Add(new ChatMessage("user", prompt));          
            completionsOptions.MaxTokens = 300;
            completionsOptions.Temperature = 0;
            var result= await client.GetChatCompletionsAsync(OpenAIOptions.Model, completionsOptions);
            if (result != null)
            {
                return Ok(result.Value.Choices.First().Message.Content);
            }
            else 
            {
                return Ok();
            }
        }

    }
}
