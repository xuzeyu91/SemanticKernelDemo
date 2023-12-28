using Microsoft.SemanticKernel;
using System;
using System.ComponentModel;
using System.Globalization;
using Xzy.SK.Domain;

namespace Xzy.SK.Api.plugins.MathPlugin
{
    public class MathSK
    {
        /// <summary>
        /// 得到负数
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        [KernelFunction, Description("得到负数")]
        public string Negative(string number)
        {
            return (number.ConvertToInt32() * -1).ToString();
        }

        /// <summary>
        /// 两个数相减
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        [KernelFunction, Description("两个数相减")]
        [return: Description("减完后的数")]
        public string Subtraction(
        [Description("The value to subtract")] int num1,
        [Description("Amount to subtract")] int num2) =>
        (num1 - num2).ToString();
    
    }
}
