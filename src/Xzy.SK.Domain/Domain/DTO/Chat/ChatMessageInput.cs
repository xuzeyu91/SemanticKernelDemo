using Azure.AI.OpenAI;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xzy.SK.Domain.Domain.DTO.Chat
{
    public class ChatMessageInput
    {
        [Required]
        [MinLength(1)]
        public List<ChatMessageItem> Prompts { get; set; } = new List<ChatMessageItem>();
    }

    public class ChatMessageItem
    {
        [Required]
        public ChatRole role { get; set; }
        [Required]
        public string Content { get; set; }
    }
}
