using System;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace QRMenuAPI.Models
{
    public class Food
    {
        public int Id { get; set; }

        [StringLength(100, MinimumLength = 1)]
        [Column(TypeName = "nvarchar(100)")]
        public string Name { get; set; } = "";

        [Range(0, float.MaxValue)]
        public float Price { get; set; }

        [StringLength(200, MinimumLength = 3)]
        [Column(TypeName = "nvarchar(200)")]
        public string? Description { get; set; }

        public byte StateId { get; set; }
        [ForeignKey("StateId")]
        public State? State { get; set; }

        [ForeignKey(nameof(CategoryId))]
        public int CategoryId { get; set; }
        [JsonIgnore]
        public Category? Category { get; set; }

        public string? ImgPath { get; set; }

    }
}

