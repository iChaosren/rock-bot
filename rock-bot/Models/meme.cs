namespace rock_bot.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("rock-bot.meme")]
    public partial class meme
    {
        public int id { get; set; }

        [Required]
        [StringLength(64)]
        public string keyword { get; set; }
        
        [StringLength(65535)]
        public string response { get; set; }

        public bool logusage { get; set; }

        public DateTime? lastusage { get; set; }

        public long? maxspan { get; set; }
    }
}
