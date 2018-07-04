using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using RDD.Domain.Models;

namespace TodoApi.Models
{
    public class TodoItem : EntityBase<TodoItem, long>
    {
        public override long Id { get; set; } 
        [Required]
        public override string Name { get; set; } 
        [DefaultValue(false)]
        public bool IsComplete { get; set; }
    }
}