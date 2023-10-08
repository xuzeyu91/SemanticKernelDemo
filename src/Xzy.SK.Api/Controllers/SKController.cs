using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.Planning;
using Microsoft.SemanticKernel.SkillDefinition;
using Microsoft.SemanticKernel.Skills.Core;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Xzy.SK.Api.plugins;
using Xzy.SK.Api.plugins.MathPlugin;
using Xzy.SK.Domain.Common.Model;
using Xzy.SK.Domain.Common.Options;

namespace Xzy.SK.Api.Controllers
{
    /// <summary>
    /// SK
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class SKController : ControllerBase
    {
        private readonly IKernel _kernel;
        public SKController(IKernel kernel)
        {
            _kernel = kernel;
        }

        /// <summary>
        /// 测试翻译
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Translate(string input,string language)
        {
            //导入本地技能
            var pluginsDirectory = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "plugins");
            var writerPlugin = _kernel
                 .ImportSemanticSkillFromDirectory(pluginsDirectory, "Translate");

            var result = await _kernel.RunAsync(input, writerPlugin[language]);

            return Ok(result.Result);
        }

        /// <summary>
        /// 测试计算
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
                 .ImportSemanticSkillFromDirectory(pluginsDirectory, "Calculate");

            var variables = new ContextVariables
            {
                ["num1"] = num1,
                ["num2"] = num2
            };
            var result = await _kernel.RunAsync(variables, calculatePlugin["Addition"]);

            return Ok(result.Result);
        }

        /// <summary>
        /// 原生测试
        /// </summary>
        /// <param name="num1"></param>
        /// <param name="num2"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Native(string num1, string num2)
        {
            //导入原生函数

            var mathPlugin = _kernel.ImportSkill(new MathSK(), "MathPlugin");

            var variables = new ContextVariables
            {
                ["num1"] = num1,
                ["num2"] = num2
            };
            var result = await _kernel.RunAsync(variables, mathPlugin["Subtraction"]);

            return Ok(result.Result);
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
                .ImportSemanticSkillFromDirectory(pluginsDirectory, "Calculate");
            //MathPlugin Multiplication 中可以嵌套其他函数
            var mathPlugin = _kernel.ImportSkill(new MathSK(), "MathPlugin");

            var variables = new ContextVariables
            {
                ["num1"] = num1,
                ["num2"] = num2
            };
            var result = await _kernel.RunAsync(variables, calculatePlugin["Multiplication"]);

            return Ok(result.Result);
        }

      

        /// <summary>
        /// 原生嵌套，通过自然语义先找到最大和最小的2个值，然后用最大值减去最小值得到结果返回
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>

        [HttpPost]
        public async Task<IActionResult> NativeNested(string msg)
        {


            var NativeNested = _kernel.ImportSkill(new NativeNested(_kernel), "NativeNested");

            var result = await _kernel.RunAsync(msg, NativeNested["Test"]);

            return Ok(result.Result);
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
                .ImportSemanticSkillFromDirectory(pluginsDirectory, "Calculate");

            var plan = await planner.CreatePlanAsync(msg);
            Console.WriteLine("Plan:\n");
            Console.WriteLine(JsonConvert.SerializeObject(plan));

            var result = (await _kernel.RunAsync(plan)).Result;
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
            _kernel.ImportSkill(new ConversationSummarySkill(_kernel), "ConversationSummarySkill");

            var pluginsDirectory = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "plugins");
            var intentPlugin = _kernel
                 .ImportSemanticSkillFromDirectory(pluginsDirectory, "BasePlugin");
            var travelPlugin = _kernel
                 .ImportSemanticSkillFromDirectory(pluginsDirectory, "Travel");

            var NativeNested = _kernel.ImportSkill(new UtilsPlugin(_kernel), "UtilsPlugin");
            var getIntentVariables = new ContextVariables
            {
                ["input"] = msg,
                ["options"] = "Attractions, Delicacy,Traffic,Weather,SendEmail"
            };
            string intent = (await _kernel.RunAsync(getIntentVariables, intentPlugin["GetIntent"])).Result.Trim();
            ISKFunction MathFunction;
            switch (intent)
            {
                case "Attractions":
                    MathFunction = _kernel.Skills.GetFunction("Travel", "Attractions");
                    break;
                case "Delicacy":
                    MathFunction = _kernel.Skills.GetFunction("Travel", "Delicacy");
                    break;
                case "Traffic":
                    MathFunction = _kernel.Skills.GetFunction("Travel", "Traffic");
                    break;
                case "Weather":
                    MathFunction = _kernel.Skills.GetFunction("Travel", "Weather");
                    break;
                case "SendEmail":
                    var sendEmailVariables = new ContextVariables
                    {
                        ["input"] = msg,
                        ["example"] = JsonConvert.SerializeObject(new { send_user = "xzy", receiver_user = "xzy", body = "hello" })
                    };
                    msg = (await _kernel.RunAsync(sendEmailVariables, intentPlugin["JSON"])).Result;
                    MathFunction = _kernel.Skills.GetFunction("UtilsPlugin", "SendEmail");
                    break;
                default:
                    return Ok("对不起我不知道");
            }
            var result = await _kernel.RunAsync(msg, MathFunction);

            return Ok(result.Result);
        }
    }
}
