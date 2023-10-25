using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI;
using Microsoft.SemanticKernel.Plugins.Core;
using Microsoft.SemanticKernel.TemplateEngine.Basic;
using System;
using System.Threading.Tasks;

namespace Xzy.SK.Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class TemplateController : ControllerBase
    {
        private readonly IKernel _kernel;

        public TemplateController(IKernel kernel)
        {
            _kernel = kernel;
        }

        [HttpPost]
        public async Task<IActionResult> CreateTemplate()
        {
            _kernel.ImportFunctions(new TimePlugin(), "time");

            const string FunctionDefinition = @"
今天是： {{time.Date}}
当前时间为：{{time.Time}}

使用JSON语法回答以下问题，包括使用的数据。
是上午、下午、晚上还是晚上（上午/下午/晚上/晚上）？
是周末时间吗（周末/不是周末）？
";

            var promptRenderer = new BasicPromptTemplateEngine();
            var renderedPrompt = await promptRenderer.RenderAsync(FunctionDefinition, _kernel.CreateNewContext());

            var kindOfDay = _kernel.CreateSemanticFunction(FunctionDefinition, requestSettings: new OpenAIRequestSettings() { MaxTokens = 100 });

            var result = await _kernel.RunAsync(kindOfDay);

            return Ok(result.GetValue<string>());
        }
    }
}