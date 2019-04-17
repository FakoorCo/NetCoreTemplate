﻿namespace NetCoreTemplate.DAL.Models.General
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    using NetCoreTemplate.DAL.Attributes;
    using NetCoreTemplate.DAL.Models.Base;
    using NetCoreTemplate.DAL.Models.Interfaces;
    using NetCoreTemplate.DAL.Models.Translation;

    public class Language : TrackableEntity, IKeyModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [Index("IX_Code_CultureCode_Unique", 0, IsClustered = false, IsUnique = true)]
        public string Code { get; set; }

        [Required]
        [Index("IX_Code_CultureCode_Unique", 1, IsClustered = false, IsUnique = true)]
        public string CultureCode { get; set; }

        public virtual List<EntityLabel> EntityLabels { get; set; }
        
        public virtual List<TranslationLabel> TranslationLabels { get; set; }
    }
}
