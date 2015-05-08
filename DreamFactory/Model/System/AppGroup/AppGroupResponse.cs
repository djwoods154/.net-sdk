﻿// ReSharper disable InconsistentNaming
namespace DreamFactory.Model.System.AppGroup
{
    using DreamFactory.Model.System.App;
    using global::System;
    using global::System.Collections.Generic;

    /// <summary>
    /// AppGroupResponse.
    /// </summary>
    public class AppGroupResponse
    {
        /// <summary>
        /// Identifier of this application group.
        /// </summary>
        public int? id { get; set; }

        /// <summary>
        /// Displayable name of this application group.
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// Description of this application group.
        /// </summary>
        public string description { get; set; }

        /// <summary>
        /// Related apps by app to group assignment.
        /// </summary>
        public List<RelatedApp> apps { get; set; }

        /// <summary>
        /// Date this group was created.
        /// </summary>
        public DateTime? created_date { get; set; }

        /// <summary>
        /// User Id of who created this group.
        /// </summary>
        public int? created_by_id { get; set; }

        /// <summary>
        /// Date this group was last modified.
        /// </summary>
        public DateTime? last_modified_date { get; set; }

        /// <summary>
        /// User Id of who last modified this group.
        /// </summary>
        public int? last_modified_by_id { get; set; }
    }
}