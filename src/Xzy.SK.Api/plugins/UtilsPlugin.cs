using Microsoft.SemanticKernel;
using System;
using System.ComponentModel;

namespace Xzy.SK.Api.plugins
{
    public class UtilsPlugin
    {
        /// <summary>
        /// 
        /// </summary>
        private readonly Kernel _kernel;
        public UtilsPlugin(Kernel kernel)
        {
            _kernel = kernel;
        }

        [KernelFunction, Description("发送邮件")]
        public string SendEmail(string input)
        {
            Console.WriteLine(input);
          return "发送成功";
        }
    }
}
