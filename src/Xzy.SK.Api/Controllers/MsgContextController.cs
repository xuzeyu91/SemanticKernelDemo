using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI;
using System.Threading.Tasks;
using Microsoft.SemanticKernel.TemplateEngine.Basic;
using Microsoft.SemanticKernel.Orchestration;
using Newtonsoft.Json;
using Microsoft.SemanticKernel.Plugins.Core;

namespace Xzy.SK.Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class MsgContextController : ControllerBase
    {
        private readonly IKernel _kernel;

        public MsgContextController(IKernel kernel)
        {
            _kernel = kernel;
        }

        /// <summary>
        /// 聊天上下文
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Test()
        {
            string history = @"User:你好\r\n ChatBot:你好！我是AI助手，有什么需要帮忙的吗？";
            string input = "我刚才说了什么";

            string FunctionDefinition = @$"
{history}
User: {input}";

            var promptRenderer = new BasicPromptTemplateEngine();
            var renderedPrompt = await promptRenderer.RenderAsync(FunctionDefinition, _kernel.CreateNewContext());

            var test = _kernel.CreateSemanticFunction(FunctionDefinition, requestSettings: new OpenAIRequestSettings() { MaxTokens = 100 });
            var contextVariables = new ContextVariables
            {
                ["$history"] = history,
                ["$input"] = input
            };
            var result = await _kernel.RunAsync(contextVariables, test);

            return Ok(result.GetValue<string>());
        }
    }
}