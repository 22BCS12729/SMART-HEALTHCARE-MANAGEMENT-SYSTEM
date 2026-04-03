using System;
using System.ComponentModel.DataAnnotations;

namespace SmartHealthcare.Core.Entities
{
    public abstract class BaseEntity
    {
        [Key]
        public int Id { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public string? CreatedBy { get; set; }
        
        public string? UpdatedBy { get; set; }
    }
}
