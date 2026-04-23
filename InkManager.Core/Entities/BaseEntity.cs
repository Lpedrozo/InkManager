using System.ComponentModel.DataAnnotations;

namespace InkManager.Core.Entities
{
    public abstract class BaseEntity
    {
        [Key]
        public int Id { get; set; }

        public bool EliminadoLogico { get; set; } = false;

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        public DateTime FechaModificacion { get; set; } = DateTime.UtcNow;
    }
}