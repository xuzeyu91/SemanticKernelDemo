using Humanizer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Planning.Handlebars;
using Microsoft.SemanticKernel.Plugins.Core;
using Newtonsoft.Json;
using RepoUtils;
using System;
using System.IO;
using System.Threading.Tasks;
using Xzy.SK.Api.plugins;
using Xzy.SK.Api.plugins.MathPlugin;
using Xzy.SK.Domain.Common.SK;

namespace Xzy.SK.Api.Controllers
{
    /// <summary>
    /// SKDemo示例
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class SKDemoController : ControllerBase
    {
        private readonly Kernel _kernel;

        public SKDemoController(Kernel kernel)
        {
            _kernel = kernel;
        }

        /// <summary>
        /// 测试Plugins翻译
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Translate(string input, string language)
        {
            //导入本地技能
            var writerPlugin = _kernel
                 .ImportPluginFromPromptDirectory(Path.Combine(RepoFiles.SamplePluginsPath(), "Translate"));

            var result = await _kernel.InvokeAsync( writerPlugin[language],new () { ["input"]= input } );

            return Ok(result.GetValue<string>());
        }

        /// <summary>
        /// 测试Plugins计算
        /// </summary>
        /// <param name="num1"></param>
        /// <param name="num2"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Calculate(string num1, string num2)
        {
            //导入本地技能，多参数
            var calculatePlugin = _kernel
                 .ImportPluginFromPromptDirectory(Path.Combine(RepoFiles.SamplePluginsPath(), "Calculate"));

            var variables = new KernelArguments
            {
                ["num1"] = num1,
                ["num2"] = num2
            };
            var result = await _kernel.InvokeAsync(calculatePlugin["Addition"], variables);

            return Ok(result.GetValue<string>());
        }

        /// <summary>
        /// 原生函数测试
        /// </summary>
        /// <param name="num1"></param>
        /// <param name="num2"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Native(string num1, string num2)
        {
            //导入原生函数

            var mathPlugin = _kernel.ImportPluginFromObject(new MathSK(), "MathPlugin");

            var variables = new KernelArguments
            {
                ["num1"] = num1,
                ["num2"] = num2
            };
            var result = await _kernel.InvokeAsync( mathPlugin["Subtraction"], variables);

            return Ok(result.GetValue<string>());
        }

        /// <summary>
        /// 嵌套函数
        /// </summary>
        /// <param name="num1"></param>
        /// <param name="num2"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Nested(string num1, string num2)
        {
            //嵌套函数使用，在prompty中使用  {{Plugin.Fun}} 可以嵌套调用
     
            var calculatePlugin = _kernel
                .ImportPluginFromPromptDirectory(Path.Combine(RepoFiles.SamplePluginsPath(), "Calculate"));
            //MathPlugin Multiplication 中可以嵌套其他函数
            var mathPlugin = _kernel.ImportPluginFromObject(new MathSK(), "MathPlugin");

            var variables = new KernelArguments
            {
                ["num1"] = num1,
                ["num2"] = num2
            };
            var result = await _kernel.InvokeAsync( calculatePlugin["Multiplication"], variables);

            return Ok(result.GetValue<string>());
        }

        /// <summary>
        /// 原生嵌套，通过自然语义先找到最大和最小的2个值，然后用最大值减去最小值得到结果返回
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>

        [HttpPost]
        public async Task<IActionResult> NativeNested(string msg)
        {
            var NativeNested = _kernel.ImportPluginFromObject(new NativeNested(_kernel), "NativeNested");

            var result = await _kernel.InvokeAsync(NativeNested["Test"], new () { ["input"] = msg });

            return Ok(result.GetValue<string>());
        }

        /// <summary>
        /// 计划
        /// </summary>
        /// <param name="msg">小明有7个冰淇淋，我有2个冰淇淋，他比我多几个冰淇淋？</param>
        /// <param name="msg">小明有7个冰淇淋，我有2个冰淇淋，我们一共有几个冰淇淋？</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Plan(string msg)
        {
            var planner = new HandlebarsPlanner(
                new HandlebarsPlannerOptions()
                {
                    // Change this if you want to test with loops regardless of model selection.
                    AllowLoops = true
                });
            var calculatePlugin = _kernel
                .ImportPluginFromPromptDirectory(Path.Combine(RepoFiles.SamplePluginsPath(), "Calculate"));

            var plan = await planner.CreatePlanAsync(_kernel,msg);
            Console.WriteLine("Plan:\n");
            Console.WriteLine(JsonConvert.SerializeObject(plan));

            var result = await plan.InvokeAsync(_kernel);
            return Ok(result);
        }

        /// <summary>
        /// 意图识别
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Intent(string msg)
        {
            //对话摘要  SK.Skills.Core 核心技能
            _kernel.ImportPluginFromObject(new ConversationSummaryPlugin(), "ConversationSummarySkill");
            var intentPlugin = _kernel
                 .ImportPluginFromPromptDirectory(Path.Combine(RepoFiles.SamplePluginsPath(), "BasePlugin"));
            var travelPlugin = _kernel
                 .ImportPluginFromPromptDirectory(Path.Combine(RepoFiles.SamplePluginsPath(), "Travel"));

            var NativeNested = _kernel.ImportPluginFromObject(new UtilsPlugin(_kernel), "UtilsPlugin");
            var getIntentVariables = new KernelArguments
            {
                ["input"] = msg,
                ["options"] = "Attractions, Delicacy,Traffic,Weather,SendEmail"  //给GPT的意图，通过Prompt限定选用这些里面的
            };
            string intent = (await _kernel.InvokeAsync( intentPlugin["GetIntent"], getIntentVariables)).GetValue<string>().Trim();
            KernelFunction MathFunction;
            //获取意图后动态调用Fun
            switch (intent)
            {
                case "Attractions":
                    MathFunction = _kernel.Plugins.GetFunction("Travel", "Attractions");
                    break;

                case "Delicacy":
                    MathFunction = _kernel.Plugins.GetFunction("Travel", "Delicacy");
                    break;

                case "Traffic":
                    MathFunction = _kernel.Plugins.GetFunction("Travel", "Traffic");
                    break;

                case "Weather":
                    MathFunction = _kernel.Plugins.GetFunction("Travel", "Weather");
                    break;

                case "SendEmail":
                    KernelArguments sendEmailVariables = new ()
                    {
                        ["input"] = msg,
                        ["example"] = JsonConvert.SerializeObject(new { send_user = "xzy", receiver_user = "xzy", body = "hello" })
                    };
                    msg = (await _kernel.InvokeAsync( intentPlugin["JSON"], sendEmailVariables)).GetValue<string>();
                    MathFunction = _kernel.Plugins.GetFunction("UtilsPlugin", "SendEmail");
                    break;

                default:
                    return Ok("对不起我不知道");
            }
            var result = await _kernel.InvokeAsync( MathFunction,new KernelArguments() { ["input"]= msg } );

            return Ok(result.GetValue<string>());
        }

        /// <summary>
        /// 管道
        /// https://learn.microsoft.com/en-us/semantic-kernel/ai-orchestration/plugins/out-of-the-box-plugins?tabs=Csharp#whats-the-ms-graph-connector-kit
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Pipeline()
        {
            var myText = _kernel.ImportPluginFromObject(new TextPlugin());
            KernelFunction pipeline = KernelFunctionCombinators.Pipe(new[] 
            {  
                myText["TrimStart"],//清除左边空格
                myText["TrimEnd"],//清除右边空格
                myText["Uppercase"] 
            }, "pipeline");
            //管道模式的顺序调用
            var myOutput = await pipeline.InvokeAsync(
              _kernel,new KernelArguments() { ["input"]= "     i n f i n i t e     s p a c e     " });//转大写

            return Ok(myOutput.GetValue<string>());
        }
    }
}