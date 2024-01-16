using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Plugins.Core;

using System;
using System.Threading.Tasks;

namespace Xzy.SK.Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class TemplateController : ControllerBase
    {
        private readonly Kernel _kernel;

        public TemplateController(Kernel kernel)
        {
            _kernel = kernel;
        }

        [HttpPost]
        public async Task<IActionResult> CreateTemplate()
        {
            _kernel.ImportPluginFromObject(new TimePlugin(), "time");

            const string FunctionDefinition = @"
今天是： {{time.Date}}
当前时间为：{{time.Time}}

使用JSON语法回答以下问题，包括使用的数据。
是上午、下午、晚上还是晚上（上午/下午/晚上/晚上）？
是周末时间吗（周末/不是周末）？
";

            var promptTemplateFactory = new KernelPromptTemplateFactory();
            var promptTemplate = promptTemplateFactory.Create(new PromptTemplateConfig(FunctionDefinition));
            var renderedPrompt = await promptTemplate.RenderAsync(_kernel);

            var kindOfDay = _kernel.CreateFunctionFromPrompt(FunctionDefinition, new OpenAIPromptExecutionSettings() { MaxTokens = 100 });

            // Show the result
            Console.WriteLine("--- Prompt Function result");
            var result = await _kernel.InvokeAsync(kindOfDay);
            return Ok(result.GetValue<string>());
        }
    }
}