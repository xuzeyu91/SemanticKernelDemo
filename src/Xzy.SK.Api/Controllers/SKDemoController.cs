using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.Planners;
using Microsoft.SemanticKernel.Plugins.Core;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using Xzy.SK.Api.plugins;
using Xzy.SK.Api.plugins.MathPlugin;

namespace Xzy.SK.Api.Controllers
{
    /// <summary>
    /// SKDemo示例
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class SKDemoController : ControllerBase
    {
        private readonly IKernel _kernel;

        public SKDemoController(IKernel kernel)
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
            var pluginsDirectory = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "plugins");
            var writerPlugin = _kernel
                 .ImportSemanticFunctionsFromDirectory(pluginsDirectory, "Translate");

            var result = await _kernel.RunAsync(input, writerPlugin[language]);

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

            var pluginsDirectory = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "plugins");
            var calculatePlugin = _kernel
                 .ImportSemanticFunctionsFromDirectory(pluginsDirectory, "Calculate");

            var variables = new ContextVariables
            {
                ["num1"] = num1,
                ["num2"] = num2
            };
            var result = await _kernel.RunAsync(variables, calculatePlugin["Addition"]);

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

            var mathPlugin = _kernel.ImportFunctions(new MathSK(), "MathPlugin");

            var variables = new ContextVariables
            {
                ["num1"] = num1,
                ["num2"] = num2
            };
            var result = await _kernel.RunAsync(variables, mathPlugin["Subtraction"]);

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
            var pluginsDirectory = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "plugins");
            var calculatePlugin = _kernel
                .ImportSemanticFunctionsFromDirectory(pluginsDirectory, "Calculate");
            //MathPlugin Multiplication 中可以嵌套其他函数
            var mathPlugin = _kernel.ImportFunctions(new MathSK(), "MathPlugin");

            var variables = new ContextVariables
            {
                ["num1"] = num1,
                ["num2"] = num2
            };
            var result = await _kernel.RunAsync(variables, calculatePlugin["Multiplication"]);

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
            var NativeNested = _kernel.ImportFunctions(new NativeNested(_kernel), "NativeNested");

            var result = await _kernel.RunAsync(msg, NativeNested["Test"]);

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
            var planner = new SequentialPlanner(_kernel);

            var pluginsDirectory = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "plugins");
            var calculatePlugin = _kernel
                .ImportSemanticFunctionsFromDirectory(pluginsDirectory, "Calculate");

            var plan = await planner.CreatePlanAsync(msg);
            Console.WriteLine("Plan:\n");
            Console.WriteLine(JsonConvert.SerializeObject(plan));

            var result = (await _kernel.RunAsync(plan)).GetValue<string>();
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
            _kernel.ImportFunctions(new ConversationSummaryPlugin(_kernel), "ConversationSummarySkill");

            var pluginsDirectory = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "plugins");
            var intentPlugin = _kernel
                 .ImportSemanticFunctionsFromDirectory(pluginsDirectory, "BasePlugin");
            var travelPlugin = _kernel
                 .ImportSemanticFunctionsFromDirectory(pluginsDirectory, "Travel");

            var NativeNested = _kernel.ImportFunctions(new UtilsPlugin(_kernel), "UtilsPlugin");
            var getIntentVariables = new ContextVariables
            {
                ["input"] = msg,
                ["options"] = "Attractions, Delicacy,Traffic,Weather,SendEmail"  //给GPT的意图，通过Prompt限定选用这些里面的
            };
            string intent = (await _kernel.RunAsync(getIntentVariables, intentPlugin["GetIntent"])).GetValue<string>().Trim();
            ISKFunction MathFunction;
            //获取意图后动态调用Fun
            switch (intent)
            {
                case "Attractions":
                    MathFunction = _kernel.Functions.GetFunction("Travel", "Attractions");
                    break;

                case "Delicacy":
                    MathFunction = _kernel.Functions.GetFunction("Travel", "Delicacy");
                    break;

                case "Traffic":
                    MathFunction = _kernel.Functions.GetFunction("Travel", "Traffic");
                    break;

                case "Weather":
                    MathFunction = _kernel.Functions.GetFunction("Travel", "Weather");
                    break;

                case "SendEmail":
                    var sendEmailVariables = new ContextVariables
                    {
                        ["input"] = msg,
                        ["example"] = JsonConvert.SerializeObject(new { send_user = "xzy", receiver_user = "xzy", body = "hello" })
                    };
                    msg = (await _kernel.RunAsync(sendEmailVariables, intentPlugin["JSON"])).GetValue<string>();
                    MathFunction = _kernel.Functions.GetFunction("UtilsPlugin", "SendEmail");
                    break;

                default:
                    return Ok("对不起我不知道");
            }
            var result = await _kernel.RunAsync(msg, MathFunction);

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
            var myText = _kernel.ImportFunctions(new TextPlugin());
            //管道模式的顺序调用
            var myOutput = await _kernel.RunAsync(
                "    i n f i n i t e     s p a c e     ",
                myText["TrimStart"],//清除左边空格
                myText["TrimEnd"],//清除右边空格
                myText["Uppercase"]);//转大写

            return Ok(myOutput.GetValue<string>());
        }
    }
}