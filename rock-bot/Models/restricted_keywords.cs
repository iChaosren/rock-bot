namespace rock_bot.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("rock-bot.restricted_keywords")]
    public partial class restricted_keywords
    {
        [Key]
        [StringLength(64)]
        public string keyword { get; set; }
    }
}
