using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Threading.Tasks;


namespace Xzy.SK.Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class MsgContextController : ControllerBase
    {
        private readonly Kernel _kernel;

        public MsgContextController(Kernel kernel)
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

            var promptRenderer = new KernelPromptTemplateFactory();
            var renderedPrompt = promptRenderer.Create(new PromptTemplateConfig(FunctionDefinition));

            var test = _kernel.CreateFunctionFromPrompt(FunctionDefinition,  new OpenAIPromptExecutionSettings() { MaxTokens = 100 });
            KernelArguments KernelArguments = new ()
            {
                ["$history"] = history,
                ["$input"] = input
            };
            var result = await _kernel.InvokeAsync(test, KernelArguments);

            return Ok(result.GetValue<string>());
        }
    }
}