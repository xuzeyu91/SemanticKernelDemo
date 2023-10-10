using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Orchestration;
using System;
using System.ComponentModel;

namespace Xzy.SK.Api.plugins
{
    public class UtilsPlugin
    {
        /// <summary>
        /// 
        /// </summary>
        private readonly IKernel _kernel;
        public UtilsPlugin(IKernel kernel)
        {
            _kernel = kernel;
        }

        [SKFunction, Description("发送邮件")]
        public string SendEmail(SKContext context)
        {
            Console.WriteLine(context.Variables["input"]);
          return "发送成功";
        }
    }
}
